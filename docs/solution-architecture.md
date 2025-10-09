# logManager Solution Architecture Document

**Project:** logManager
**Date:** 2025-10-08
**Author:** Adam
**Architecture Type:** PowerShell Binary Module (.NET 8.0)

## Executive Summary

logManager is a high-performance PowerShell module for log archival built in C#/.NET 8.0, designed to handle massive-scale operations (300K-1M files per day). The module provides 11 cmdlet functions organized in dependency layers, with a hybrid compression strategy (7-Zip CLI with SharpCompress fallback) and multi-destination support (AWS S3, UNC shares, local drives).

**Key Architectural Decisions:**
- **Target Platform:** PowerShell 7.2+ only (cross-platform)
- **Framework:** .NET 8.0 (LTS, single target)
- **Architecture Pattern:** Modular binary module with layered services
- **Repository Strategy:** Monorepo on GitHub
- **Distribution:** Internal use, GitHub releases
- **AWS Integration:** Leverages existing AWS.Tools.S3 PowerShell module (no SDK bundling)
- **Compression:** Hybrid approach (7-Zip CLI detection with SharpCompress .NET library fallback)
- **Dependencies:** Minimal (SharpCompress only)
- **Test Coverage:** 100% including all parameters and edge cases

**Performance Characteristics:**
- In-place compression (no temp files) - NFR002
- Check-first workflow (skip already-archived dates) - NFR001
- Idempotent execution (safe retry) - NFR003
- Target: <5 minutes for 1M file scan

## 1. Technology Stack and Decisions

### 1.1 Technology and Library Decision Table

| Category | Technology | Version | Rationale |
|----------|------------|---------|-----------|
| **Runtime** | PowerShell | 7.2+ | Cross-platform support, modern performance, .NET 8 compatibility |
| **Framework** | .NET | 8.0 (LTS) | Latest LTS, superior I/O performance, Span\<T\> for zero-allocation processing, modern C# features |
| **Language** | C# | 11/12 | Record types, pattern matching, modern async/await, file-scoped namespaces |
| **Module Type** | Binary PowerShell Module | PSv7.2+ | Compiled performance for massive file operations, better dependency management than script modules |
| **Compression (Primary)** | 7-Zip CLI | Latest | Maximum compression ratio, familiar to ops teams, widely installed |
| **Compression (Fallback)** | SharpCompress | 0.36.0 | Self-contained fallback when 7-Zip unavailable, .NET 8 compatible, MIT license |
| **AWS Integration** | AWS.Tools.S3 | 4.1.0.0+ | Leverage existing PowerShell modules, consistent credential chain, lighter module bundle |
| **Testing (C# Unit)** | xUnit | 2.6.0+ | Modern .NET test framework, parallel execution, excellent VS integration |
| **Testing (PowerShell)** | Pester | 5.5.0+ | Standard PowerShell test framework, integration testing, BDD style |
| **Build Tool** | dotnet CLI | 8.0 SDK | Standard .NET build tooling, GitHub Actions support |
| **CI/CD** | GitHub Actions | Latest | Integrated with repository, free for public repos, cross-platform runners |
| **Version Control** | Git | Latest | Industry standard, GitHub integration |
| **Package Manager** | NuGet | Latest | .NET package ecosystem |

**External Dependencies (Required on Target):**
- PowerShell 7.2+ (host environment)
- AWS.Tools.S3 or AWSPowerShell.NetCore (for S3 functionality)
- 7-Zip CLI (optional - enhances compression, not required)

**Bundled Dependencies:**
- SharpCompress 0.36.0 (compression fallback)

## 2. Architecture Overview

### 2.1 Module Architecture Pattern

logManager follows a **layered cmdlet architecture** with dependency injection for testability:

```
┌────────────────────────────────────────────────────────┐
│           PUBLIC API LAYER (11 Cmdlets)                 │
│  PowerShell-facing commands (Get-*, Find-*, Send-*, etc)│
├────────────────────────────────────────────────────────┤
│             SERVICES LAYER (Internal)                   │
│  - DateRangeCalculator                                  │
│  - PathTokenResolver                                    │
│  - ArchiveNamingService                                 │
│  - FileDiscoveryService / FolderDiscoveryService        │
│  - CompressionService (ICompressionEngine abstraction)  │
│  - S3TransferService / UncTransferService / LocalTransferService│
│  - CleanupService                                       │
├────────────────────────────────────────────────────────┤
│        ABSTRACTIONS LAYER (Interfaces for DI)           │
│  - IFileSystem (file ops abstraction)                   │
│  - IDestinationProvider (S3/UNC/Local)                  │
│  - ICompressionEngine (7-Zip/SharpCompress)            │
│  - IPowerShellInvoker (AWS cmdlet wrapper)              │
├────────────────────────────────────────────────────────┤
│           EXTERNAL DEPENDENCIES                         │
│  File System | AWS.Tools.S3 | 7-Zip | SharpCompress   │
└────────────────────────────────────────────────────────┘
```

### 2.2 Dependency Layers

The module implements strict dependency layering matching the PRD functional requirements:

**Foundation Layer** (no internal dependencies):
- Get-DateRange cmdlet → DateRangeCalculator service
- PathTokenResolver service (internal)
- ArchiveNamingService (internal)
- Error handling framework (custom exceptions)
- Pipeline composition framework (return object models)

**Discovery Layer** (depends on Foundation):
- Find-LogFiles cmdlet → FileDiscoveryService
- Find-LogFolders cmdlet → FolderDiscoveryService
- ArchiveExistenceChecker (IDestinationProvider implementations)
- Test-ArchiveExists cmdlet

**Processing Layer** (depends on Discovery):
- Compress-Logs cmdlet → CompressionService (ICompressionEngine)
- Idempotent execution pattern (uses Test-ArchiveExists)

**Distribution Layer** (depends on Processing):
- Send-ToS3/Send-ToUNC/Send-ToDrive (internal functions)
- Send-Archive cmdlet (dispatcher)
- Remove-LogSource cmdlet

**Orchestration Layer** (depends on all):
- Invoke-LogArchival cmdlet (chains all operations)

### 2.3 Compression Engine Strategy

**Hybrid Compression Architecture:**

```csharp
public interface ICompressionEngine
{
    ArchiveInfo Compress(string sourcePath, string destinationPath);
    bool IsAvailable { get; }
}

// Primary: 7-Zip CLI
public class SevenZipCompressor : ICompressionEngine
{
    public bool IsAvailable => Find7ZipExecutable() != null;
    // Uses Process.Start("7z.exe", "a archive.zip source")
}

// Fallback: SharpCompress
public class SharpCompressCompressor : ICompressionEngine
{
    public bool IsAvailable => true; // Always available (bundled)
    // Uses SharpCompress library API
}

// Service selector
public class CompressionService
{
    public ArchiveInfo Compress(...)
    {
        var engine = TryGet7Zip() ?? GetSharpCompress();
        return engine.Compress(...);
    }
}
```

**Detection Logic:**
1. Check PATH environment variable for `7z` / `7z.exe`
2. Check common installation paths:
   - Windows: `C:\Program Files\7-Zip\7z.exe`
   - Linux: `/usr/bin/7z`, `/usr/local/bin/7z`
3. If found → use SevenZipCompressor
4. If not found → fallback to SharpCompressCompressor (always works)
5. Log engine selection: `Write-Verbose "Using 7-Zip CLI"` or `Write-Warning "7-Zip not found, using built-in compression"`

### 2.4 AWS S3 Integration Strategy

**PowerShell Module Invocation Pattern:**

Instead of bundling AWSSDK.S3, the module calls AWS PowerShell cmdlets:

```csharp
using System.Management.Automation;

public class S3TransferService
{
    public TransferResult UploadToS3(
        string filePath, string bucketName, string s3Key,
        string accessKey = null, string secretKey = null, string region = null)
    {
        using (var ps = PowerShell.Create())
        {
            ps.AddCommand("Write-S3Object")
              .AddParameter("BucketName", bucketName)
              .AddParameter("File", filePath)
              .AddParameter("Key", s3Key);

            // Direct credential passing (no profiles)
            if (!string.IsNullOrEmpty(accessKey))
            {
                ps.AddParameter("AccessKey", accessKey);
                ps.AddParameter("SecretKey", secretKey);
            }

            if (!string.IsNullOrEmpty(region))
                ps.AddParameter("Region", region);

            var results = ps.Invoke();

            if (ps.HadErrors)
                throw new TransferException($"S3 upload failed: {ps.Streams.Error[0]}");

            return new TransferResult { Success = true, Destination = $"s3://{bucketName}/{s3Key}" };
        }
    }
}
```

**Benefits:**
- No 10+ MB AWS SDK DLLs bundled (~500 KB vs ~15 MB module size)
- Leverages existing AWS PowerShell module installation
- Consistent AWS credential chain with other scripts
- Automatic AWS module updates (separate from logManager)

**Prerequisite Check:**
```csharp
protected override void BeginProcessing()
{
    if (!ModuleAvailable("AWS.Tools.S3"))
    {
        throw new PSInvalidOperationException(
            "AWS.Tools.S3 module required for S3 destinations. " +
            "Install with: Install-Module AWS.Tools.S3 -Scope CurrentUser");
    }
}
```

## 3. Data Architecture

### 3.1 Return Object Models

All cmdlets return PowerShell-compatible objects for pipeline chaining:

**DateRangeResult:**
```csharp
public record DateRangeResult
{
    public DateTime Date { get; init; }
    public DateCriteriaType Criteria { get; init; } // CreationDate or ModifiedDate
}
```

**LogFileInfo:**
```csharp
public record LogFileInfo
{
    public FileInfo File { get; init; }
    public DateTime LogDate { get; init; }
    public DateCriteriaType DateSource { get; init; }
}
```

**LogFolderInfo:**
```csharp
public record LogFolderInfo
{
    public DirectoryInfo Folder { get; init; }
    public DateTime ParsedDate { get; init; }
    public string FolderNamePattern { get; init; } // YYYYMMDD or YYYY-MM-DD
}
```

**ArchiveInfo:**
```csharp
public record ArchiveInfo
{
    public FileInfo ArchiveFile { get; init; }
    public string CompressionEngine { get; init; } // "7-Zip" or "SharpCompress"
    public long CompressedSize { get; init; }
    public long OriginalSize { get; init; }
    public TimeSpan CompressionDuration { get; init; }
}
```

**TransferResult:**
```csharp
public record TransferResult
{
    public bool Success { get; init; }
    public string SourcePath { get; init; }
    public string Destination { get; init; }
    public string DestinationType { get; init; } // "S3", "UNC", "Local"
    public long BytesTransferred { get; init; }
    public TimeSpan TransferDuration { get; init; }
}
```

**WorkflowResult:**
```csharp
public record WorkflowResult
{
    public int FilesProcessed { get; init; }
    public int ArchivesCreated { get; init; }
    public long TotalBytesCompressed { get; init; }
    public long TotalBytesTransferred { get; init; }
    public int FilesDeleted { get; init; }
    public TimeSpan TotalDuration { get; init; }
    public List<string> Errors { get; init; }
    public List<string> Warnings { get; init; }
}
```

### 3.2 Configuration Models

**Path Token Context:**
```csharp
public record TokenResolverContext
{
    public DateTime LogDate { get; init; }
    public string ServerName { get; init; }
    public string ApplicationName { get; init; }
}
```

**Compression Options:**
```csharp
public record CompressionOptions
{
    public bool UseCompression { get; init; } = true;
    public string PreferredEngine { get; init; } // "Auto", "SevenZip", "DotNet"
    public bool FailIfPreferredUnavailable { get; init; } = false;
}
```

### 3.3 Exception Hierarchy

```csharp
public class LogManagerException : Exception { }
public class CompressionException : LogManagerException { }
public class TransferException : LogManagerException { }
public class CleanupException : LogManagerException { }
public class TokenResolutionException : LogManagerException { }
```

## 4. Component and Integration Overview

### 4.1 Public Cmdlets (11 Total)

**Foundation Layer:**
- `Get-DateRange` - Calculate date ranges from day offsets

**Discovery Layer:**
- `Find-LogFiles` - Locate files by date criteria
- `Find-LogFolders` - Locate dated folders
- `Test-ArchiveExists` - Check for existing archives (pre-flight check)

**Processing Layer:**
- `Compress-Logs` - Create archives with hybrid compression

**Distribution Layer:**
- `Send-Archive` - Dispatcher to appropriate backend (S3/UNC/Local)
- `Remove-LogSource` - Safe cleanup after confirmed transfer

**Orchestration Layer:**
- `Invoke-LogArchival` - All-in-one workflow orchestrator

### 4.2 Internal Services (Not Exposed as Cmdlets)

**Foundation Services:**
- `DateRangeCalculator` - Date math and range generation
- `PathTokenResolver` - Token replacement ({year}, {month}, etc.)
- `ArchiveNamingService` - AppName-YYYYMMDD.zip pattern

**Discovery Services:**
- `FileDiscoveryService` - File enumeration with date filtering
- `FolderDiscoveryService` - Folder parsing and date detection
- `ArchiveExistenceChecker` - Multi-destination existence checking

**Processing Services:**
- `CompressionService` - Compression engine selection and invocation
- `SevenZipCompressor` - 7-Zip CLI integration
- `SharpCompressCompressor` - .NET library compression

**Distribution Services:**
- `S3TransferService` - AWS S3 uploads via AWS.Tools.S3
- `UncTransferService` - UNC network share transfers
- `LocalTransferService` - Local/external drive transfers
- `CleanupService` - Safe file/folder deletion

### 4.3 Integration Points

**External Systems:**
- **AWS S3** - Via AWS.Tools.S3 PowerShell module (Write-S3Object, Test-S3Bucket cmdlets)
- **7-Zip** - Via Process.Start() CLI invocation (optional)
- **SharpCompress** - Via bundled NuGet package (fallback)
- **File System** - Via IFileSystem abstraction (.NET 8 System.IO)
- **UNC Shares** - Via .NET network path support

**Authentication Flows:**

*S3 Authentication:*
1. If user provides `-AwsAccessKey` and `-AwsSecretKey` → pass directly to Write-S3Object
2. If omitted → AWS.Tools.S3 uses default credential chain (IAM role, environment variables, etc.)
3. No AWS profiles used (explicit credential passing only)

*UNC Authentication:*
- Uses current process credentials
- If share requires different credentials, user must map network drive first

### 4.4 Data Flow Examples

**Example: Invoke-LogArchival -Path "D:\Logs\App" -olderThan 7 -Destination "s3://bucket/archives/{year}/{month}"**

```
1. Get-DateRange (olderThan: 7)
   → Returns [2025-09-30, 2025-09-29, ...] (7 days)

2. Test-ArchiveExists (destination: s3://..., dates from step 1)
   → PathTokenResolver: "s3://bucket/archives/2025/09" for each date
   → S3TransferService: Check if "App-20250930.zip" exists
   → Returns filtered dates NOT yet archived: [2025-09-30, 2025-09-28]

3. Find-LogFolders (path: D:\Logs\App, dates from step 2)
   → Finds: D:\Logs\App\20250930, D:\Logs\App\20250928
   → Returns LogFolderInfo[]

4. Compress-Logs (folders from step 3)
   → CompressionService selects engine (7-Zip or SharpCompress)
   → Creates: D:\Logs\App-20250930.zip, D:\Logs\App-20250928.zip
   → Returns ArchiveInfo[]

5. Send-Archive (archives from step 4, destination: s3://...)
   → PathTokenResolver: Resolve tokens for each date
   → S3TransferService: Upload to s3://bucket/archives/2025/09/App-20250930.zip
   → Returns TransferResult[]

6. Remove-LogSource (if -DeleteSource specified)
   → CleanupService: Delete D:\Logs\App\20250930, D:\Logs\App\20250928
   → Only if step 5 succeeded

7. Return WorkflowResult (summary of all operations)
```

## 5. Architecture Decision Records

### ADR-001: PowerShell 7+ Only (No Cross-Version Compatibility)

**Status:** Accepted

**Context:**
- Original consideration: Support both PowerShell 5.1 (.NET Framework) and PowerShell 7+ (.NET Core/8)
- Would require multi-targeting (.NET Standard 2.0 + .NET 8)

**Decision:**
- Target PowerShell 7.2+ exclusively
- Use .NET 8.0 single target framework

**Consequences:**
- **Positive:**
  - Simpler build (one target)
  - Modern .NET 8 performance (Span\<T\>, modern I/O)
  - Better async/await support
  - Cross-platform ready (Windows/Linux/macOS)
  - Smaller codebase (no compatibility shims)
- **Negative:**
  - Requires PowerShell 7+ installation on target servers
  - Not usable on Windows PowerShell 5.1 systems
- **Mitigation:**
  - PowerShell 7+ is modern standard for new development
  - Internal team is knowledgeable (can install pwsh 7+)
  - Greenfield project allows modern baseline

### ADR-002: Hybrid Compression (7-Zip CLI + SharpCompress Fallback)

**Status:** Accepted

**Context:**
- Need compression for 300K-1M files per day
- 7-Zip offers maximum compression ratio
- Not all servers may have 7-Zip installed
- Process spawn overhead vs. in-process library

**Decision:**
- Detect and use 7-Zip CLI if available (primary)
- Fallback to SharpCompress .NET library if 7-Zip not found
- User can override with `-CompressionEngine` parameter

**Consequences:**
- **Positive:**
  - Works everywhere (SharpCompress fallback)
  - Optimal when 7-Zip available (better compression)
  - Self-contained deployment (no external dependencies required)
  - Flexible for different environments
- **Negative:**
  - Added complexity (two compression code paths)
  - SharpCompress bundled even when 7-Zip used
  - Need to test both engines
- **Mitigation:**
  - ICompressionEngine abstraction makes testing straightforward
  - Bundled SharpCompress is only ~500 KB
  - Detection logic is simple (PATH + common locations)

### ADR-003: Use AWS PowerShell Modules (No AWS SDK Bundling)

**Status:** Accepted

**Context:**
- Need S3 upload capability
- Could bundle AWSSDK.S3 NuGet package (~10-15 MB)
- AWS PowerShell modules already installed in ops environments

**Decision:**
- Call AWS.Tools.S3 cmdlets from C# (PowerShell.Create().AddCommand())
- Do not bundle AWSSDK.S3
- Pass credentials directly to cmdlets (no profiles)

**Consequences:**
- **Positive:**
  - Much smaller module (~500 KB vs ~15 MB)
  - Leverages existing infrastructure
  - Consistent credential chain with other scripts
  - Automatic AWS module updates
- **Negative:**
  - External prerequisite (AWS.Tools.S3 must be installed)
  - Slight performance overhead (PowerShell invocation from C#)
  - Need to check for module availability at runtime
- **Mitigation:**
  - Check AWS.Tools.S3 availability in BeginProcessing()
  - Clear error message with installation instructions
  - Ops team likely already has AWS modules

### ADR-004: Direct Credential Passing (No AWS Profiles)

**Status:** Accepted

**Context:**
- AWS authentication can use profiles, environment variables, IAM roles, or explicit keys
- Need clarity on credential handling for automated scripts

**Decision:**
- Support explicit `-AwsAccessKey` and `-AwsSecretKey` parameters
- If provided, pass directly to Write-S3Object cmdlet
- If omitted, let AWS.Tools.S3 use default credential chain
- Never use AWS profile names

**Consequences:**
- **Positive:**
  - Explicit control (user sees exactly what credentials are used)
  - Works with environment variables (`$env:AWS_Access`)
  - Works with IAM roles (when no keys provided)
  - No profile configuration needed
- **Negative:**
  - Credentials may be visible in script history/logs
  - User responsible for secure credential handling
- **Mitigation:**
  - Document secure practices (use SecureString in automation)
  - Support IAM roles for production (no keys needed)
  - Align with testing configuration (environment variables)

### ADR-005: Monorepo with Single Module

**Status:** Accepted

**Context:**
- Single PowerShell module with 11 cmdlets
- Could split into multiple modules or keep unified

**Decision:**
- Single repository (GitHub)
- Single module output (logManager.psd1)
- All cmdlets in one assembly

**Consequences:**
- **Positive:**
  - Simpler development (one codebase)
  - Easier dependency management
  - Single versioning (all cmdlets always compatible)
  - Simpler CI/CD (one build, one release)
- **Negative:**
  - All cmdlets installed together (no granular installation)
  - Larger module size than per-cmdlet splitting
- **Mitigation:**
  - Module is small enough (~500 KB) that size isn't an issue
  - All cmdlets are related (log archival domain)

### ADR-006: 100% Test Coverage Including Edge Cases

**Status:** Accepted

**Context:**
- Production tool handling file deletion
- Safety-critical operations (cleanup after archival)
- Performance-critical (300K-1M files)

**Decision:**
- Target 100% code coverage
- Test all parameter combinations
- Test all error conditions and edge cases
- Both xUnit (C# unit tests) and Pester (PowerShell integration tests)

**Consequences:**
- **Positive:**
  - High confidence in reliability
  - Safety for destructive operations (Remove-LogSource)
  - Easier refactoring (comprehensive safety net)
  - Documents expected behavior
- **Negative:**
  - Higher initial development time
  - More test maintenance
  - Slower CI/CD (more tests to run)
- **Mitigation:**
  - Parallel test execution
  - Mock external dependencies (IFileSystem, IPowerShellInvoker)
  - Tiered testing (unit → integration → performance)

### ADR-007: Semantic Versioning with GitHub Releases

**Status:** Accepted

**Context:**
- Need versioning strategy for internal distribution
- Not publishing to PowerShell Gallery

**Decision:**
- Use Semantic Versioning (SemVer: Major.Minor.Patch)
- Manual tagging in Git (e.g., `git tag v1.0.0`)
- GitHub Actions auto-builds and attaches module ZIP to release

**Consequences:**
- **Positive:**
  - Standard versioning (familiar to developers)
  - Clear breaking change communication (major version bump)
  - Automated build artifacts
  - Simple distribution (download ZIP from GitHub release)
- **Negative:**
  - Manual version bumping (no auto-increment)
  - Manual changelog maintenance
- **Mitigation:**
  - Document versioning guidelines
  - Use conventional commits for changelog generation
  - Simple process for internal team

### ADR-008: Minimal NuGet Dependencies

**Status:** Accepted

**Context:**
- Need compression library
- Could add other helper libraries for convenience

**Decision:**
- Bundle only SharpCompress (compression fallback)
- No other NuGet dependencies
- Use .NET 8 BCL for everything else

**Consequences:**
- **Positive:**
  - Minimal external dependencies (lower supply chain risk)
  - Smaller module size
  - Fewer version conflicts
  - Simpler dependency tree
- **Negative:**
  - May need to implement some utilities ourselves
  - Can't leverage community libraries for edge cases
- **Mitigation:**
  - .NET 8 BCL is very comprehensive
  - Acceptable to add well-vetted libraries if needed
  - Re-evaluate if clear benefit emerges

## 6. Implementation Guidance

### 6.1 Development Environment Setup

**Prerequisites:**
```bash
# Install .NET 8 SDK
winget install Microsoft.DotNet.SDK.8

# Install PowerShell 7.4+
winget install Microsoft.PowerShell

# Install AWS PowerShell module
Install-Module AWS.Tools.S3 -Scope CurrentUser

# Install 7-Zip (optional, for testing primary compression)
winget install 7zip.7zip

# Clone repository
git clone https://github.com/PowerShell-Army/logManager.git
cd logManager
```

**Build and Test:**
```bash
# Restore and build
dotnet restore
dotnet build

# Run C# unit tests
dotnet test

# Import module for testing
Import-Module ./src/logManager/bin/Debug/net8.0/logManager.psd1

# Run Pester integration tests
Invoke-Pester ./tests/Integration
```

### 6.2 Cmdlet Implementation Pattern

All cmdlets follow this pattern:

```csharp
[Cmdlet(VerbsCommon.Get, "DateRange")]
[OutputType(typeof(DateRangeResult[]))]
public class GetDateRangeCmdlet : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public int OlderThan { get; set; }

    [Parameter(Mandatory = false)]
    public int YoungerThan { get; set; } = 0;

    [Parameter(Mandatory = false)]
    public DateCriteriaType DateCriteria { get; set; } = DateCriteriaType.ModifiedDate;

    private readonly DateRangeCalculator _calculator;

    public GetDateRangeCmdlet()
        : this(new DateRangeCalculator()) { }

    // Constructor injection for testing
    internal GetDateRangeCmdlet(DateRangeCalculator calculator)
    {
        _calculator = calculator;
    }

    protected override void ProcessRecord()
    {
        try
        {
            var results = _calculator.Calculate(OlderThan, YoungerThan, DateCriteria);
            foreach (var result in results)
            {
                WriteObject(result);
            }
        }
        catch (Exception ex)
        {
            ThrowTerminatingError(new ErrorRecord(
                ex, "DateRangeError", ErrorCategory.InvalidOperation, null));
        }
    }
}
```

**Key Patterns:**
- Constructor injection for testability
- `ThrowTerminatingError` for fail-fast (FR012)
- `WriteObject` for pipeline compatibility (FR009)
- Input validation in setters
- Service-based logic (not in cmdlet)

### 6.3 Service Implementation Pattern

```csharp
public class FileDiscoveryService
{
    private readonly IFileSystem _fileSystem;

    public FileDiscoveryService(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public IEnumerable<LogFileInfo> DiscoverFiles(
        string path,
        DateRangeResult[] dateRange,
        DateCriteriaType criteria,
        bool recurse)
    {
        var searchOption = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        // Use .NET 8 streaming enumeration (NFR001 performance)
        foreach (var file in _fileSystem.EnumerateFiles(path, "*", searchOption))
        {
            var fileDate = criteria == DateCriteriaType.CreationDate
                ? file.CreationTime
                : file.LastWriteTime;

            if (dateRange.Any(dr => dr.Date.Date == fileDate.Date))
            {
                yield return new LogFileInfo
                {
                    File = file,
                    LogDate = fileDate.Date,
                    DateSource = criteria
                };
            }
        }
    }
}
```

**Key Patterns:**
- Dependency injection via constructor
- Interface abstraction for testability
- `yield return` for streaming (memory efficient)
- .NET 8 `EnumerateFiles` for performance

### 6.4 Testing Strategy Details

**Unit Tests (xUnit):**
```csharp
public class DateRangeCalculatorTests
{
    [Theory]
    [InlineData(7, 0, 8)] // olderThan=7, youngerThan=0 → 8 days (inclusive)
    [InlineData(7, 7, 1)] // olderThan=7, youngerThan=7 → 1 day
    [InlineData(30, 0, 31)] // olderThan=30, youngerThan=0 → 31 days
    public void Calculate_ReturnsCorrectDateCount(int olderThan, int youngerThan, int expectedCount)
    {
        var calculator = new DateRangeCalculator();
        var results = calculator.Calculate(olderThan, youngerThan, DateCriteriaType.ModifiedDate);
        Assert.Equal(expectedCount, results.Length);
    }

    [Fact]
    public void Calculate_ThrowsWhen_OlderThanLessThanYoungerThan()
    {
        var calculator = new DateRangeCalculator();
        Assert.Throws<ArgumentException>(() =>
            calculator.Calculate(olderThan: 5, youngerThan: 7, DateCriteriaType.ModifiedDate));
    }
}
```

**Integration Tests (Pester):**
```powershell
Describe "Invoke-LogArchival Integration" {
    BeforeAll {
        # Setup test environment
        $testPath = "D:\TEST_DATA\Logs\TestApp"
        New-Item -Path $testPath -ItemType Directory -Force

        # Create test log folders
        @("20250930", "20250929", "20250928") | ForEach-Object {
            $folder = New-Item "$testPath\$_" -ItemType Directory -Force
            "test log" | Out-File "$folder\app.log"
        }
    }

    It "Archives logs older than 7 days to local destination" {
        $result = Invoke-LogArchival `
            -Path $testPath `
            -olderThan 7 `
            -AppName "TestApp" `
            -Destination "$env:Local_Drive\Archives\{year}\{month}" `
            -DeleteSource

        $result.ArchivesCreated | Should -BeGreaterThan 0
        $result.FilesDeleted | Should -BeGreaterThan 0
        Test-Path "$env:Local_Drive\Archives\2025\09\TestApp-20250930.zip" | Should -Be $true
    }
}
```

### 6.5 Performance Optimization Guidelines

**For NFR001 (1M file scan <5 min):**

1. **Use streaming enumeration:**
   ```csharp
   // Good: Streaming (low memory)
   foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))

   // Bad: Load all into memory
   var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
   ```

2. **Filter early:**
   ```csharp
   // Filter during enumeration, not after
   foreach (var file in _fileSystem.EnumerateFiles(path, "*", searchOption))
   {
       if (MatchesDateCriteria(file)) // Early filter
           yield return file;
   }
   ```

3. **Use Span\<T\> for string operations:**
   ```csharp
   // Token replacement with minimal allocations
   ReadOnlySpan<char> template = pathTemplate.AsSpan();
   // Process with Span-based string operations
   ```

4. **Parallel enumeration for very large directories:**
   ```csharp
   // Only if single directory >100K files
   Parallel.ForEach(directories, dir => {
       foreach (var file in Directory.EnumerateFiles(dir))
           // Process
   });
   ```

**For NFR002 (In-place compression):**
- Always compress to same drive as source
- Validate destination path is on same volume
- Never use `Path.GetTempPath()` for staging

## 7. Proposed Source Tree

```
logManager/
├── .github/
│   └── workflows/
│       ├── ci.yml                      # Build and test on PR
│       ├── release.yml                 # Build and attach ZIP on tag
│       └── test-coverage.yml           # Coverage reporting
│
├── src/
│   ├── logManager/
│   │   ├── Cmdlets/
│   │   │   ├── GetDateRangeCmdlet.cs
│   │   │   ├── FindLogFilesCmdlet.cs
│   │   │   ├── FindLogFoldersCmdlet.cs
│   │   │   ├── TestArchiveExistsCmdlet.cs
│   │   │   ├── CompressLogsCmdlet.cs
│   │   │   ├── SendArchiveCmdlet.cs
│   │   │   ├── RemoveLogSourceCmdlet.cs
│   │   │   └── InvokeLogArchivalCmdlet.cs
│   │   │
│   │   ├── Services/
│   │   │   ├── DateRangeCalculator.cs
│   │   │   ├── PathTokenResolver.cs
│   │   │   ├── ArchiveNamingService.cs
│   │   │   ├── FileDiscoveryService.cs
│   │   │   ├── FolderDiscoveryService.cs
│   │   │   ├── ArchiveExistenceChecker.cs
│   │   │   ├── CompressionService.cs
│   │   │   ├── SevenZipCompressor.cs
│   │   │   ├── SharpCompressCompressor.cs
│   │   │   ├── S3TransferService.cs
│   │   │   ├── UncTransferService.cs
│   │   │   ├── LocalTransferService.cs
│   │   │   └── CleanupService.cs
│   │   │
│   │   ├── Interfaces/
│   │   │   ├── IFileSystem.cs
│   │   │   ├── ICompressionEngine.cs
│   │   │   ├── IDestinationProvider.cs
│   │   │   └── IPowerShellInvoker.cs
│   │   │
│   │   ├── Models/
│   │   │   ├── DateRangeResult.cs
│   │   │   ├── LogFileInfo.cs
│   │   │   ├── LogFolderInfo.cs
│   │   │   ├── ArchiveInfo.cs
│   │   │   ├── TransferResult.cs
│   │   │   ├── WorkflowResult.cs
│   │   │   ├── TokenResolverContext.cs
│   │   │   └── CompressionOptions.cs
│   │   │
│   │   ├── Exceptions/
│   │   │   ├── LogManagerException.cs
│   │   │   ├── CompressionException.cs
│   │   │   ├── TransferException.cs
│   │   │   ├── CleanupException.cs
│   │   │   └── TokenResolutionException.cs
│   │   │
│   │   ├── logManager.csproj            # .NET 8.0 project
│   │   └── logManager.psd1              # Module manifest
│   │
│   └── logManager.sln                   # Solution file
│
├── tests/
│   ├── Unit/
│   │   ├── Services/
│   │   │   ├── DateRangeCalculatorTests.cs
│   │   │   ├── PathTokenResolverTests.cs
│   │   │   ├── FileDiscoveryServiceTests.cs
│   │   │   ├── CompressionServiceTests.cs
│   │   │   └── ...
│   │   │
│   │   ├── Cmdlets/
│   │   │   ├── GetDateRangeCmdletTests.cs
│   │   │   ├── FindLogFilesCmdletTests.cs
│   │   │   └── ...
│   │   │
│   │   └── Unit.Tests.csproj
│   │
│   ├── Integration/
│   │   ├── FullWorkflow.Tests.ps1       # Pester: End-to-end tests
│   │   ├── S3Integration.Tests.ps1      # Pester: S3 with real env vars
│   │   ├── UncIntegration.Tests.ps1     # Pester: UNC share tests
│   │   ├── CompressionEngine.Tests.ps1  # Pester: Both engines
│   │   └── IdempotentExecution.Tests.ps1# Pester: Retry scenarios
│   │
│   └── Performance/
│       ├── LargeScaleBenchmark.Tests.ps1# 300K file dataset
│       └── FileEnumeration.Tests.ps1    # 1M file scan timing
│
├── docs/
│   ├── PRD.md                           # Product requirements (from planning)
│   ├── epic-stories.md                  # Epic breakdown (from planning)
│   ├── solution-architecture.md         # This document
│   ├── tech-spec-epic-1.md             # Technical spec Epic 1
│   ├── tech-spec-epic-2.md             # Technical spec Epic 2
│   ├── README.md                        # User documentation
│   ├── USAGE.md                         # Usage examples
│   └── CONTRIBUTING.md                  # Development guidelines
│
├── build/
│   └── package-module.ps1               # Build script to package module ZIP
│
├── .gitignore
├── LICENSE
└── README.md
```

**Key Structure Notes:**

- **Cmdlets/** - Only public PowerShell commands (11 cmdlets)
- **Services/** - Internal business logic (not exposed to users)
- **Interfaces/** - Abstractions for dependency injection and testing
- **Models/** - Return objects and data structures
- **Exceptions/** - Custom exception hierarchy
- **tests/Unit/** - xUnit C# unit tests (mock external dependencies)
- **tests/Integration/** - Pester PowerShell integration tests (use real env vars)
- **tests/Performance/** - Performance validation tests (NFR001)

## 8. Testing Strategy

### 8.1 Test Coverage Requirements

**Target: 100% code coverage including all parameters and edge cases**

**Coverage Breakdown:**
- **Unit Tests (xUnit):** 100% of service layer logic
- **Integration Tests (Pester):** All cmdlet parameter combinations
- **Edge Case Tests:** All error conditions, boundary values, failure scenarios
- **Performance Tests:** Validate NFR001 (<5 min for 1M files), NFR002 (in-place), NFR003 (idempotent)

### 8.2 Test Pyramid

```
       ┌─────────────────┐
       │  Performance    │  ← 5-10 tests (300K file datasets)
       │   Tests         │
       ├─────────────────┤
       │   Integration   │  ← 30-50 tests (Pester, full workflows)
       │     Tests       │
       ├─────────────────┤
       │   Unit Tests    │  ← 200-300 tests (xUnit, all services)
       └─────────────────┘
```

### 8.3 Test Categories

**Unit Tests (xUnit - C#):**
- All service layer methods
- All exception paths
- All parameter validation
- All token resolution patterns
- All compression engine logic
- All transfer service logic
- Mock all external dependencies (IFileSystem, IPowerShellInvoker)

**Integration Tests (Pester - PowerShell):**
- Full workflow scenarios (Invoke-LogArchival with various parameter combinations)
- Individual cmdlet integration (with real file system, test directories)
- S3 integration (using $env:AWS_Access, $env:AWS_Bucket)
- UNC integration (using $env:UNC_Drive)
- Local drive integration (using $env:Local_Drive)
- Compression engine switching (7-Zip present/absent scenarios)
- Idempotent execution (retry after partial failure)
- Error handling (network failures, permission issues)

**Performance Tests (Pester):**
- 300K file discovery benchmark (measure against NFR001 target)
- 1M file scan extrapolation test
- In-place compression validation (no temp file creation)
- Memory usage profiling (streaming vs. bulk operations)

### 8.4 Test Environment Configuration

**Unit Tests:**
```csharp
// Use mocks - no real file system, S3, or compression
var mockFileSystem = new Mock<IFileSystem>();
var mockPowerShell = new Mock<IPowerShellInvoker>();
var service = new S3TransferService(mockPowerShell.Object);
```

**Integration Tests:**
```powershell
# Use real environment variables (from PowerShell profile)
$env:AWS_Access = 'AKIA...'
$env:AWS_Secret = 'secret...'
$env:AWS_Region = 'us-east-1'
$env:AWS_Bucket = 'test-bucket'
$env:Local_Drive = 'D:\TEST_DATA'
$env:UNC_Drive = '\\10.0.10.10\storage\TEST_DATA'

# NO OTHER LOCATIONS CAN BE USED FOR INTEGRATION TESTING
```

**Performance Tests:**
```powershell
# Create test dataset
Describe "Performance: 300K File Discovery" {
    BeforeAll {
        # Generate 300,000 test files
        1..300000 | ForEach-Object {
            "test" | Out-File "D:\TEST_DATA\PerfTest\file_$_.log"
        }
    }

    It "Completes within 2 minutes" {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

        $result = Find-LogFiles -Path "D:\TEST_DATA\PerfTest" -olderThan 7

        $stopwatch.Stop()
        $stopwatch.Elapsed.TotalMinutes | Should -BeLessThan 2
    }
}
```

### 8.5 Continuous Integration

**GitHub Actions Workflow:**

**.github/workflows/ci.yml:**
```yaml
name: CI

on:
  pull_request:
    branches: [main]
  push:
    branches: [main]

jobs:
  test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET 8
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Setup PowerShell
        shell: pwsh
        run: |
          Install-Module AWS.Tools.S3 -Force -Scope CurrentUser
          Install-Module Pester -Force -Scope CurrentUser

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Run unit tests with coverage
        run: dotnet test --no-build --collect:"XPlat Code Coverage"

      - name: Run integration tests
        shell: pwsh
        run: Invoke-Pester ./tests/Integration -Output Detailed

      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          file: ./coverage.xml
```

**.github/workflows/release.yml:**
```yaml
name: Release

on:
  push:
    tags:
      - 'v*.*.*'

jobs:
  build-and-release:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET 8
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Build Release
        run: dotnet build -c Release

      - name: Package Module
        shell: pwsh
        run: ./build/package-module.ps1

      - name: Create GitHub Release
        uses: softprops/action-gh-release@v1
        with:
          files: ./dist/logManager-${{ github.ref_name }}.zip
          generate_release_notes: true
```

## 9. Deployment and Operations

### 9.1 Deployment Strategy

**Distribution Method:** GitHub Releases

**Build Process:**
1. Developer creates Git tag: `git tag v1.0.0 && git push --tags`
2. GitHub Actions triggers release workflow
3. Workflow builds module in Release configuration
4. Workflow packages module folder as ZIP
5. Workflow creates GitHub Release and attaches ZIP
6. Users download ZIP from release page

**Module Structure in ZIP:**
```
logManager-v1.0.0.zip
└── logManager/
    ├── logManager.dll              # Compiled cmdlets
    ├── logManager.psd1             # Module manifest
    ├── SharpCompress.dll           # Bundled dependency
    └── en-US/
        └── about_logManager.help.txt
```

### 9.2 Installation Instructions

**For End Users:**

```powershell
# Method 1: Download and extract to user module path
$release = Invoke-RestMethod "https://api.github.com/repos/PowerShell-Army/logManager/releases/latest"
$zipUrl = $release.assets[0].browser_download_url
$modulePath = "$env:USERPROFILE\Documents\PowerShell\Modules"

Invoke-WebRequest $zipUrl -OutFile "$env:TEMP\logManager.zip"
Expand-Archive "$env:TEMP\logManager.zip" -DestinationPath $modulePath -Force

Import-Module logManager
```

**For Automated Deployment:**

```powershell
# Method 2: Deploy to shared network location
$networkModulePath = "\\fileserver\PowerShellModules"
Expand-Archive "logManager-v1.0.0.zip" -DestinationPath $networkModulePath

# Update PowerShell profile to add to PSModulePath
$env:PSModulePath += ";\\fileserver\PowerShellModules"
```

### 9.3 Prerequisites

**Required on Target Systems:**
- PowerShell 7.2 or later
- AWS.Tools.S3 module (for S3 functionality)
  ```powershell
  Install-Module AWS.Tools.S3 -Scope CurrentUser
  ```

**Optional on Target Systems:**
- 7-Zip CLI (for optimal compression)
  ```powershell
  # Windows
  winget install 7zip.7zip

  # Linux
  sudo apt-get install p7zip-full
  ```

### 9.4 Configuration

**Environment Variables (for testing/automation):**
```powershell
# AWS Configuration (optional - for explicit credentials)
$env:AWS_Access = 'AKIA...'
$env:AWS_Secret = 'secret...'
$env:AWS_Region = 'us-east-1'
$env:AWS_Bucket = 'my-archive-bucket'

# Test Locations (for integration testing only)
$env:Local_Drive = 'D:\TEST_DATA'
$env:UNC_Drive = '\\10.0.10.10\storage\TEST_DATA'
```

**No configuration files needed** - all configuration via cmdlet parameters

### 9.5 Operational Monitoring

**Built-in Diagnostics:**
```powershell
# Verbose logging
Invoke-LogArchival -Path "D:\Logs" -olderThan 7 -Verbose

# Example verbose output:
# VERBOSE: Using 7-Zip CLI for compression
# VERBOSE: Found 15 folders matching date criteria
# VERBOSE: Skipping already-archived: 2025-09-30
# VERBOSE: Compressing: D:\Logs\App\20250929 -> App-20250929.zip
# VERBOSE: Uploading to s3://bucket/archives/2025/09/App-20250929.zip
# VERBOSE: Transfer complete: 1.2 GB in 45 seconds
# VERBOSE: Deleted source: D:\Logs\App\20250929
```

**Error Logging (external):**
```powershell
# Recommended: Wrap in try/catch for external logging
try {
    $result = Invoke-LogArchival -Path $path -olderThan 7 -Destination $dest -DeleteSource
    Write-Log "Archival completed: $($result.ArchivesCreated) archives, $($result.FilesDeleted) files deleted"
} catch {
    Write-Log "Archival failed: $_" -Level Error
    Send-AlertEmail -Subject "Log archival failed" -Body $_
}
```

**Performance Metrics:**
```powershell
$result = Invoke-LogArchival -Path "D:\Logs" -olderThan 7 -Destination "s3://bucket/archives"

# WorkflowResult contains metrics:
# - FilesProcessed: 450000
# - ArchivesCreated: 15
# - TotalBytesCompressed: 25000000000 (25 GB)
# - TotalBytesTransferred: 5000000000 (5 GB after compression)
# - TotalDuration: 00:18:30
```

### 9.6 Rollback Procedures

**Module Rollback:**
```powershell
# Uninstall current version
Remove-Item "$env:USERPROFILE\Documents\PowerShell\Modules\logManager" -Recurse -Force

# Install previous version from GitHub release
$previousVersion = "v1.0.0"
$zipUrl = "https://github.com/PowerShell-Army/logManager/releases/download/$previousVersion/logManager-$previousVersion.zip"
# ... (download and extract as above)
```

**No state to rollback** - module is stateless, all operations are on-demand

## 10. Security

### 10.1 Security Considerations

**Credential Handling:**
- Credentials passed as cmdlet parameters (visible in script history)
- **Recommendation:** Use SecureString for automation:
  ```powershell
  $secureKey = Read-Host "AWS Secret Key" -AsSecureString
  # Convert to plain text only when passing to AWS cmdlet (internal to module)
  ```
- **Best Practice:** Use IAM roles in production (no credentials in scripts)
- Never log credential parameters in verbose output

**File Deletion Safety:**
- `Remove-LogSource` only deletes after **confirmed successful transfer**
- Transfer confirmation required before cleanup
- Fail-safe design: if unsure, don't delete
- **Recommendation:** Test with `-WhatIf` simulation first (future enhancement)

**Path Traversal Prevention:**
- Validate all user-provided paths
- Reject paths with `..` traversal attempts
- Normalize paths before operations

**Privilege Requirements:**
- Read access to source log folders
- Write access to destination (S3/UNC/Local)
- Delete access to source (if using `-DeleteSource`)
- **Recommendation:** Run with minimum required privileges

### 10.2 Secrets Management

**Current Approach:**
- Environment variables for testing (documented in PRD)
- Direct parameter passing for automation
- AWS credential chain for production (IAM roles)

**Future Enhancements (Post-MVP):**
- Azure Key Vault integration for credential retrieval
- AWS Secrets Manager integration
- Support for PowerShell SecureString parameters

### 10.3 Audit Trail

**Current Approach:**
- Module writes verbose output (can be captured)
- No built-in audit logging (KISS principle)

**Recommended External Auditing:**
```powershell
# Capture all operations to audit log
$transcript = Start-Transcript -Path "C:\Logs\archival-audit-$(Get-Date -Format 'yyyyMMdd').log"

Invoke-LogArchival -Path "D:\Logs" -olderThan 7 -Verbose

Stop-Transcript
```

### 10.4 Dependency Supply Chain

**NuGet Package Verification:**
- SharpCompress 0.36.0 (MIT license, verified)
- Use `dotnet restore --locked-mode` to ensure exact versions
- Dependabot enabled on GitHub for security updates

**AWS.Tools.S3 (External):**
- User responsibility to install from PowerShell Gallery
- Recommend: Pin version in team documentation
- Verify publisher: Amazon Web Services

---

## Specialist Sections

### Testing (Inline)
Covered in Section 8 (Testing Strategy).

**Summary:**
- 100% code coverage target with xUnit (C# unit tests) and Pester (PowerShell integration tests)
- Tiered testing: Unit → Integration → Performance
- CI/CD with GitHub Actions
- Performance benchmarks for NFR001 validation

### DevOps (Inline)
Covered in Section 9 (Deployment and Operations).

**Summary:**
- GitHub Releases distribution (manual tag, auto-build)
- Semantic versioning (SemVer)
- Simple deployment (download ZIP, extract to module path)
- No infrastructure required (stateless module)
- Monitoring via external logging (module outputs WorkflowResult metrics)

### Security (Inline)
Covered in Section 10 (Security).

**Summary:**
- Direct credential passing (no profiles)
- IAM role support for production
- File deletion safety (confirmed transfer required)
- Path traversal prevention
- Minimal attack surface (no network listeners, no services)

---

_Generated using BMAD Solution Architecture Workflow for Level 2 Projects_
