# Technical Specification: Core Archival Functions (Foundation + Discovery + Processing Layers)

Date: 2025-10-08
Author: Adam
Epic ID: 1
Status: Draft

---

## Overview

Epic 1 delivers the foundational architecture for the logManager PowerShell module, implementing 8 of 11 core atomic functions organized across three dependency layers: Foundation, Discovery, and Processing. This epic establishes the module's architectural pattern as a PowerShell binary module built in C#/.NET 9.0, providing the essential capabilities for date-based log file discovery, archive existence checking, and in-place compression using a hybrid compression strategy (7-Zip CLI with SharpCompress fallback). The scope includes all prerequisite services and abstractions required for Epic 2's distribution and orchestration layers, validating the check-first workflow pattern and pipeline composition framework that enables both custom workflows and convenience orchestration.

## Objectives and Scope

**Primary Objectives:**
1. Establish PowerShell binary module architecture (.NET 9.0, PowerShell 7.2+) with layered service design and dependency injection for testability
2. Implement Foundation layer services (date range calculation, path token resolution, archive naming, error handling framework)
3. Implement Discovery layer functions (file discovery, folder discovery, archive existence checking across S3/UNC/Local destinations)
4. Implement Processing layer compression with hybrid engine strategy (7-Zip CLI detection with SharpCompress .NET library fallback)
5. Validate pipeline composition pattern enabling function chaining for custom workflows
6. Achieve 100% test coverage for all Foundation and Discovery layer components

**In Scope:**
- Get-DateRange cmdlet (FR001: Date range calculation from olderThan/youngerThan parameters)
- Find-LogFiles cmdlet (FR003: File discovery with date filtering and recursive scanning)
- Find-LogFolders cmdlet (FR004: Folder discovery for YYYYMMDD and YYYY-MM-DD patterns)
- Test-ArchiveExists cmdlet (FR002: Pre-flight archive check across S3/UNC/Local)
- Compress-Logs cmdlet (FR005: In-place compression with hybrid 7-Zip/SharpCompress strategy)
- Internal services: DateRangeCalculator, PathTokenResolver, ArchiveNamingService, FileDiscoveryService, FolderDiscoveryService, ArchiveExistenceChecker, CompressionService, SevenZipCompressor, SharpCompressCompressor
- Return object models: DateRangeResult, LogFileInfo, LogFolderInfo, ArchiveInfo
- Abstractions: IFileSystem, ICompressionEngine, IDestinationProvider interfaces
- Exception hierarchy: LogManagerException, CompressionException, TokenResolutionException
- xUnit unit tests for all services (100% coverage)
- Pester integration tests for all cmdlets including compression engine switching scenarios

**Out of Scope (Deferred to Epic 2):**
- Send-Archive dispatcher and backend functions (Send-ToS3, Send-ToUNC, Send-ToDrive)
- Remove-LogSource cleanup function
- Invoke-LogArchival orchestrator function
- Performance optimization and massive-scale benchmarking (300K-1M files)
- AWS.Tools.S3 integration for actual S3 transfers (existence checking only in Epic 1)
- End-to-end workflow integration testing (Epic 2)

## System Architecture Alignment

This epic implements the first three layers of the four-layer architecture defined in the Solution Architecture Document:

**Foundation Layer (No Internal Dependencies):**
- Aligns with ADR-001: PowerShell 7.2+ only, .NET 9.0 single target
- Implements date calculation services using modern C# 11/12 record types
- Establishes path token resolution engine supporting {year}, {month}, {day}, {server}, {app} tokens
- Creates archive naming service implementing AppName-YYYYMMDD.zip pattern (FR010)
- Defines fail-fast error handling framework with custom exception hierarchy (FR012)
- Establishes pipeline composition framework using PowerShell-compatible return objects (FR009)

**Discovery Layer (Depends on Foundation):**
- Implements IFileSystem abstraction for testability (Section 2.1 of Architecture)
- File discovery using .NET 8 streaming enumeration (EnumerateFiles) for memory efficiency
- Folder discovery with date parsing for YYYYMMDD and YYYY-MM-DD folder naming patterns
- Archive existence checking using IDestinationProvider abstraction with S3/UNC/Local implementations (FR014)
- Enables check-first workflow pattern critical for NFR003 (idempotent execution)

**Processing Layer (Depends on Discovery):**
- Aligns with ADR-002: Hybrid compression strategy (7-Zip CLI + SharpCompress fallback)
- Implements ICompressionEngine abstraction with two implementations: SevenZipCompressor (Process.Start CLI invocation) and SharpCompressCompressor (bundled library)
- Enforces NFR002: In-place compression constraint (no temp file staging)
- CompressionService provides engine detection and selection logic with verbose logging
- Returns ArchiveInfo objects compatible with pipeline chaining to Epic 2's distribution layer

**Architectural Constraints Enforced:**
- Each cmdlet < 500 lines (ADR-004: Code maintainability)
- Single responsibility per function and service
- Minimal NuGet dependencies (SharpCompress only - ADR-008)
- Constructor injection for all services enabling comprehensive unit testing (ADR-006: 100% coverage target)

## Detailed Design

### Services and Modules

| Component | Type | Responsibilities | Inputs | Outputs | Owner |
|-----------|------|-----------------|--------|---------|-------|
| **Get-DateRange** | Public Cmdlet | Calculate date ranges from day-based parameters | olderThan (int), youngerThan (int), DateCriteria (enum) | DateRangeResult[] | Foundation Layer |
| **Find-LogFiles** | Public Cmdlet | Locate individual files matching date criteria | Path (string), DateRange (DateRangeResult[]), Recurse (bool) | LogFileInfo[] | Discovery Layer |
| **Find-LogFolders** | Public Cmdlet | Locate dated folders (YYYYMMDD/YYYY-MM-DD) | Path (string), DateRange (DateRangeResult[]) | LogFolderInfo[] | Discovery Layer |
| **Test-ArchiveExists** | Public Cmdlet | Query destinations for existing archives | Destination (string), DateRange (DateRangeResult[]), AppName (string) | DateRangeResult[] (filtered) | Discovery Layer |
| **Compress-Logs** | Public Cmdlet | Create .zip archives with hybrid compression | SourcePath (string/LogFileInfo[]/LogFolderInfo[]), OutputPath (string) | ArchiveInfo[] | Processing Layer |
| **DateRangeCalculator** | Internal Service | Date math and range generation logic | olderThan, youngerThan, criteria | DateRangeResult[] | Foundation Layer |
| **PathTokenResolver** | Internal Service | Replace path tokens ({year}, {month}, etc.) | Template path, TokenResolverContext | Resolved path string | Foundation Layer |
| **ArchiveNamingService** | Internal Service | Generate archive filenames (AppName-YYYYMMDD.zip) | AppName, Date | Archive filename | Foundation Layer |
| **FileDiscoveryService** | Internal Service | File enumeration with date filtering | Path, DateRange, Criteria, Recurse | LogFileInfo stream (yield) | Discovery Layer |
| **FolderDiscoveryService** | Internal Service | Folder parsing and date detection | Path, DateRange | LogFolderInfo stream (yield) | Discovery Layer |
| **ArchiveExistenceChecker** | Internal Service | Multi-destination archive existence checking | Destination path, Archive name | Boolean (exists/not exists) | Discovery Layer |
| **CompressionService** | Internal Service | Compression engine selection and invocation | Source paths, Engine preference | ArchiveInfo | Processing Layer |
| **SevenZipCompressor** | Internal Service | 7-Zip CLI integration via Process.Start | Source, Destination | ArchiveInfo | Processing Layer |
| **SharpCompressCompressor** | Internal Service | .NET library compression (SharpCompress) | Source, Destination | ArchiveInfo | Processing Layer |

### Data Models and Contracts

**Return Object Models (PowerShell Pipeline Compatible):**

```csharp
// Foundation Layer Models
public record DateRangeResult
{
    public DateTime Date { get; init; }                    // The date in the range
    public DateCriteriaType Criteria { get; init; }        // CreationDate or ModifiedDate
}

public enum DateCriteriaType
{
    CreationDate,
    ModifiedDate
}

// Discovery Layer Models
public record LogFileInfo
{
    public FileInfo File { get; init; }                    // .NET FileInfo object
    public DateTime LogDate { get; init; }                 // Extracted date matching criteria
    public DateCriteriaType DateSource { get; init; }      // Which date property was used
}

public record LogFolderInfo
{
    public DirectoryInfo Folder { get; init; }             // .NET DirectoryInfo object
    public DateTime ParsedDate { get; init; }              // Date extracted from folder name
    public string FolderNamePattern { get; init; }         // "YYYYMMDD" or "YYYY-MM-DD"
}

// Processing Layer Models
public record ArchiveInfo
{
    public FileInfo ArchiveFile { get; init; }             // Created .zip file
    public string CompressionEngine { get; init; }         // "7-Zip" or "SharpCompress"
    public long CompressedSize { get; init; }              // Archive size in bytes
    public long OriginalSize { get; init; }                // Source size before compression
    public TimeSpan CompressionDuration { get; init; }     // How long compression took
}

// Internal Configuration Models
public record TokenResolverContext
{
    public DateTime LogDate { get; init; }                 // Date being processed (NOT current date)
    public string ServerName { get; init; }                // {server} token value
    public string ApplicationName { get; init; }           // {app} token value
}

public record CompressionOptions
{
    public bool UseCompression { get; init; } = true;
    public string PreferredEngine { get; init; }           // "Auto", "SevenZip", "DotNet"
    public bool FailIfPreferredUnavailable { get; init; } = false;
}
```

**Exception Hierarchy:**

```csharp
public class LogManagerException : Exception
{
    public LogManagerException(string message) : base(message) { }
    public LogManagerException(string message, Exception inner) : base(message, inner) { }
}

public class CompressionException : LogManagerException
{
    public CompressionException(string message) : base(message) { }
    public CompressionException(string message, Exception inner) : base(message, inner) { }
}

public class TokenResolutionException : LogManagerException
{
    public TokenResolutionException(string message) : base(message) { }
}
```

### APIs and Interfaces

**Public Cmdlet Signatures:**

```csharp
// Foundation Layer
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
}

// Discovery Layer
[Cmdlet(VerbsCommon.Find, "LogFiles")]
[OutputType(typeof(LogFileInfo[]))]
public class FindLogFilesCmdlet : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)]
    public string Path { get; set; }

    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public DateRangeResult[] DateRange { get; set; }

    [Parameter(Mandatory = false)]
    public DateCriteriaType DateCriteria { get; set; } = DateCriteriaType.ModifiedDate;

    [Parameter(Mandatory = false)]
    public SwitchParameter Recurse { get; set; }
}

[Cmdlet(VerbsCommon.Find, "LogFolders")]
[OutputType(typeof(LogFolderInfo[]))]
public class FindLogFoldersCmdlet : PSCmdlet
{
    [Parameter(Mandatory = true, Position = 0)]
    public string Path { get; set; }

    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public DateRangeResult[] DateRange { get; set; }
}

[Cmdlet(VerbsDiagnostic.Test, "ArchiveExists")]
[OutputType(typeof(DateRangeResult[]))]  // Returns FILTERED dates (those NOT already archived)
public class TestArchiveExistsCmdlet : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public string Destination { get; set; }  // s3://bucket/path or \\server\share or D:\path

    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public DateRangeResult[] DateRange { get; set; }

    [Parameter(Mandatory = true)]
    public string AppName { get; set; }

    // S3-specific optional parameters
    [Parameter(Mandatory = false)]
    public string AwsAccessKey { get; set; }

    [Parameter(Mandatory = false)]
    public string AwsSecretKey { get; set; }

    [Parameter(Mandatory = false)]
    public string AwsRegion { get; set; }
}

// Processing Layer
[Cmdlet(VerbsData.Compress, "Logs")]
[OutputType(typeof(ArchiveInfo[]))]
public class CompressLogsCmdlet : PSCmdlet
{
    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public object[] SourcePath { get; set; }  // Accepts: string[], LogFileInfo[], LogFolderInfo[]

    [Parameter(Mandatory = false)]
    public string OutputPath { get; set; }  // If omitted, creates archive next to source

    [Parameter(Mandatory = false)]
    public string AppName { get; set; }  // For archive naming (AppName-YYYYMMDD.zip)

    [Parameter(Mandatory = false)]
    public string PreferredEngine { get; set; } = "Auto";  // "Auto", "SevenZip", "DotNet"
}
```

**Abstraction Interfaces (For Dependency Injection and Testing):**

```csharp
// File system abstraction
public interface IFileSystem
{
    IEnumerable<FileInfo> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
    IEnumerable<DirectoryInfo> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption);
    bool FileExists(string path);
    bool DirectoryExists(string path);
    long GetFileSize(string path);
    void DeleteFile(string path);
    void DeleteDirectory(string path, bool recursive);
}

// Compression engine abstraction
public interface ICompressionEngine
{
    bool IsAvailable { get; }
    ArchiveInfo Compress(string sourcePath, string destinationPath);
}

// Destination provider abstraction (for archive existence checking)
public interface IDestinationProvider
{
    bool ArchiveExists(string archiveName);
    string DestinationType { get; }  // "S3", "UNC", "Local"
}

// S3 destination implementation (uses AWS.Tools.S3 cmdlets)
public class S3DestinationProvider : IDestinationProvider
{
    public S3DestinationProvider(string bucketName, string keyPrefix,
        string accessKey = null, string secretKey = null, string region = null);
}

// UNC destination implementation
public class UncDestinationProvider : IDestinationProvider
{
    public UncDestinationProvider(string uncPath);
}

// Local destination implementation
public class LocalDestinationProvider : IDestinationProvider
{
    public LocalDestinationProvider(string localPath);
}
```

### Workflows and Sequencing

**Epic 1 Usage Pattern: Custom Workflow with Pipeline Chaining**

```powershell
# Example: User builds custom workflow using individual functions

# Step 1: Calculate date range (last 7 days)
$dateRange = Get-DateRange -olderThan 7

# Step 2: Check which dates are NOT yet archived (check-first workflow)
$needsArchival = Test-ArchiveExists `
    -Destination "s3://my-bucket/logs/{year}/{month}" `
    -DateRange $dateRange `
    -AppName "MyApp"

# Step 3: Find folders for dates that need archival
$foldersToArchive = Find-LogFolders `
    -Path "D:\Logs\MyApp" `
    -DateRange $needsArchival

# Step 4: Compress found folders
$archives = Compress-Logs `
    -SourcePath $foldersToArchive `
    -AppName "MyApp" `
    -PreferredEngine "SevenZip"

# At this point, Epic 1 is complete
# Epic 2 will add Send-Archive and Remove-LogSource for the final steps
```

**Sequence Diagram: Test-ArchiveExists Check-First Workflow**

```
User → Get-DateRange: olderThan=7
Get-DateRange → DateRangeCalculator: Calculate(7, 0, ModifiedDate)
DateRangeCalculator → Get-DateRange: [2025-10-01, 2025-10-02, ..., 2025-10-07]

User → Test-ArchiveExists: Destination="s3://bucket/logs/{year}/{month}", DateRange, AppName="MyApp"
Test-ArchiveExists → PathTokenResolver: ResolveTokens("s3://bucket/logs/{year}/{month}", LogDate=2025-10-01)
PathTokenResolver → Test-ArchiveExists: "s3://bucket/logs/2025/10"

Test-ArchiveExists → ArchiveNamingService: GetArchiveName("MyApp", 2025-10-01)
ArchiveNamingService → Test-ArchiveExists: "MyApp-20251001.zip"

Test-ArchiveExists → S3DestinationProvider: ArchiveExists("MyApp-20251001.zip")
S3DestinationProvider → AWS.Tools.S3: Get-S3Object -BucketName "bucket" -Key "logs/2025/10/MyApp-20251001.zip"
AWS.Tools.S3 → S3DestinationProvider: (exists=true or false)

Test-ArchiveExists: Repeat for each date in DateRange
Test-ArchiveExists → User: [2025-10-02, 2025-10-05] (only dates NOT yet archived)
```

**Compression Engine Selection Logic:**

```
User → Compress-Logs: SourcePath, PreferredEngine="Auto"
Compress-Logs → CompressionService: Compress(sourcePath, destPath, "Auto")

CompressionService → SevenZipCompressor: IsAvailable?
SevenZipCompressor: Check PATH for "7z" or "7z.exe"
SevenZipCompressor: Check C:\Program Files\7-Zip\7z.exe (Windows)
SevenZipCompressor: Check /usr/bin/7z, /usr/local/bin/7z (Linux)
SevenZipCompressor → CompressionService: true (found) or false (not found)

If SevenZipCompressor.IsAvailable == true:
    CompressionService → SevenZipCompressor: Compress(source, dest)
    SevenZipCompressor: Process.Start("7z", "a -tzip dest.zip source")
    SevenZipCompressor → CompressionService: ArchiveInfo { Engine="7-Zip", ... }

Else:
    CompressionService → SharpCompressCompressor: Compress(source, dest)
    SharpCompressCompressor: Use SharpCompress library API
    SharpCompressCompressor → CompressionService: ArchiveInfo { Engine="SharpCompress", ... }

CompressionService → User: ArchiveInfo
```

**Data Flow: Find-LogFolders**

```
User → Find-LogFolders: Path="D:\Logs\MyApp", DateRange=[2025-10-01, 2025-10-02]
Find-LogFolders → FolderDiscoveryService: DiscoverFolders(path, dateRange)

FolderDiscoveryService → IFileSystem: EnumerateDirectories("D:\Logs\MyApp", "*", TopDirectoryOnly)
IFileSystem → FolderDiscoveryService: Stream of DirectoryInfo objects

For each DirectoryInfo:
    FolderDiscoveryService: Parse folder name (e.g., "20251001" or "2025-10-01")
    FolderDiscoveryService: Extract DateTime from folder name
    FolderDiscoveryService: Check if parsed date matches any date in DateRange

    If match:
        FolderDiscoveryService: yield return LogFolderInfo { Folder, ParsedDate, Pattern }

FolderDiscoveryService → User: Stream of LogFolderInfo objects
```

## Non-Functional Requirements

### Performance

**NFR001: Massive-Scale Performance (Preliminary Implementation)**
- File enumeration using .NET 8 streaming APIs (`EnumerateFiles`, `EnumerateDirectories`) with `yield return` pattern for memory efficiency
- Discovery layer must use streaming to avoid loading entire file/folder lists into memory
- Target for Epic 1: Validate streaming approach works correctly (no specific timing targets yet)
- Epic 2 will include performance benchmarks for 300K-1M file scale with <5 minute target

**NFR002: In-Place Compression Constraint**
- Compress-Logs cmdlet MUST create archives on source drive only
- Validation logic: Compare source path volume with destination path volume before compression
- Throw CompressionException if destination is on different volume
- No usage of `Path.GetTempPath()` or temporary staging locations
- Both compression engines (7-Zip CLI and SharpCompress) configured for in-place operation

**Compression Performance Considerations:**
- 7-Zip CLI: Subprocess overhead (~100-200ms per invocation) acceptable for folder-level archival
- SharpCompress: In-process library, faster startup but potentially slower compression ratio
- Epic 1 validates both engines work correctly; Epic 2 will benchmark compression throughput

### Security

**Credential Handling (Test-ArchiveExists S3 Support):**
- Optional `-AwsAccessKey` and `-AwsSecretKey` parameters for explicit credential passing
- If credentials provided, pass directly to AWS.Tools.S3 cmdlets (Get-S3Object for existence checks)
- If credentials omitted, AWS.Tools.S3 uses default credential chain (IAM role, environment variables)
- Never log credential values in verbose output or error messages
- Epic 1 scope: Archive existence checking only (no file uploads)

**Path Traversal Prevention:**
- Validate all user-provided paths in cmdlet parameter setters
- Reject paths containing `..` traversal sequences
- Use `Path.GetFullPath()` to normalize paths before file system operations
- Throw `ArgumentException` for invalid or unsafe paths

**File System Access:**
- Read-only access required for Discovery layer (Find-LogFiles, Find-LogFolders)
- Write access required for Processing layer compression (creates .zip on source drive)
- No deletion operations in Epic 1 (deferred to Epic 2's Remove-LogSource)

**Minimal Privilege Principle:**
- Cmdlets run with user's current security context (no elevation required)
- S3 access requires appropriate IAM permissions (s3:GetObject for Test-ArchiveExists)
- UNC/Local access uses current process credentials

### Reliability/Availability

**NFR003: Idempotent Execution Foundation**
- Test-ArchiveExists enables idempotent workflow by filtering already-processed dates
- Safe retry pattern: Re-run with same parameters skips dates already archived
- Epic 1 validates check-first logic works correctly across S3/UNC/Local destinations
- Epic 2 will add full end-to-end idempotent workflow testing

**FR012: Fail-Fast Error Handling**
- All cmdlets throw terminating errors (`ThrowTerminatingError`) on failures
- No automatic retry logic (user scripts control retry behavior)
- Clear, actionable error messages with specific failure context
- Exception hierarchy enables targeted catch blocks in user scripts

**Error Scenarios Handled:**
- Path not found → `ItemNotFoundException` with specific path
- Permission denied → `UnauthorizedAccessException` with operation details
- Invalid date range (olderThan < youngerThan) → `ArgumentException` with validation message
- Compression failure → `CompressionException` with engine details and stderr output
- S3 connection failure → `TransferException` (wrapping AWS.Tools.S3 error)
- 7-Zip not found when required → `CompressionException` with installation guidance (if FailIfPreferredUnavailable=true)

**Availability:**
- Stateless cmdlets (no background services or persistent connections)
- No external dependencies beyond .NET 8 runtime and PowerShell 7.2+
- Compression works offline (SharpCompress fallback when 7-Zip unavailable)
- S3 archive checks require network connectivity (degrades gracefully with clear errors)

### Observability

**Verbose Logging (PowerShell -Verbose Support):**
- All cmdlets use `WriteVerbose()` for operational visibility
- Key verbose messages:
  - Compression engine selection: "Using 7-Zip CLI" or "7-Zip not found, using built-in compression"
  - Date range calculation: "Calculated date range: 2025-10-01 to 2025-10-07 (7 days)"
  - Discovery results: "Found 15 folders matching date criteria"
  - Archive existence checks: "Checking s3://bucket/logs/2025/10/MyApp-20251001.zip... Not found"
  - Compression progress: "Compressing D:\Logs\MyApp\20251001 -> MyApp-20251001.zip using 7-Zip"

**Error Stream (PowerShell Error Output):**
- All exceptions written to error stream via `ThrowTerminatingError()`
- Error records include:
  - ErrorId: Unique identifier (e.g., "DateRangeError", "CompressionError")
  - Category: Standard PowerShell error category
  - TargetObject: The object being processed when error occurred

**Warning Stream:**
- `WriteWarning()` for non-fatal issues:
  - "7-Zip not found in PATH, using SharpCompress compression"
  - "Archive already exists, skipping: MyApp-20251001.zip"
  - "No folders found matching date criteria"

**Return Object Metrics:**
- ArchiveInfo includes: CompressionEngine, CompressedSize, OriginalSize, CompressionDuration
- Enables user scripts to track compression ratios and performance
- Pipeline-compatible for aggregation and reporting

**No Built-In Metrics/Monitoring (KISS Principle):**
- No built-in logging to files (user scripts handle with Start-Transcript)
- No metrics export (user scripts aggregate return objects)
- No progress bars for long operations (future enhancement)
- External monitoring via user script wrappers recommended

## Dependencies and Integrations

**NuGet Dependencies (Bundled with Module):**

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| SharpCompress | 0.36.0+ | Fallback compression engine when 7-Zip CLI unavailable | MIT |
| PowerShellStandard.Library | 5.1.1 | PowerShell SDK for cmdlet development | MIT |

**External PowerShell Module Dependencies (User Must Install):**

| Module | Version | Purpose | Installation Command |
|--------|---------|---------|---------------------|
| AWS.Tools.S3 | 4.1.0.0+ | S3 archive existence checking (Get-S3Object cmdlet) | `Install-Module AWS.Tools.S3 -Scope CurrentUser` |

**Optional External Tools:**

| Tool | Purpose | Detection Logic |
|------|---------|----------------|
| 7-Zip CLI | Primary compression engine (better compression ratio) | Check PATH for `7z`/`7z.exe`, check `C:\Program Files\7-Zip\7z.exe` (Windows), `/usr/bin/7z` (Linux) |

**Framework Dependencies:**

- .NET 9.0 SDK (development)
- .NET 9.0 Runtime (deployment)
- PowerShell 7.2+ (host environment)

**Integration Points:**

**1. AWS S3 (via AWS.Tools.S3 PowerShell Module):**
- Epic 1 Usage: Archive existence checking only (Test-ArchiveExists cmdlet)
- Cmdlet Invoked: `Get-S3Object -BucketName <bucket> -Key <key> -ErrorAction SilentlyContinue`
- Authentication: IAM credentials (default credential chain) or explicit AccessKey/SecretKey parameters
- Network Requirement: HTTPS connectivity to S3 endpoints
- Epic 2 Scope: Actual file uploads via `Write-S3Object`

**2. 7-Zip CLI (Optional Primary Compression):**
- Process execution: `Process.Start("7z", "a -tzip <destination.zip> <source>")`
- Working directory: Source file/folder parent directory (enforces in-place compression)
- Exit code checking: 0=success, non-zero=error
- Stderr capture for error diagnostics
- Fallback: If detection fails or process errors, CompressionService uses SharpCompress

**3. SharpCompress Library (Bundled Fallback Compression):**
- In-process .NET library (no subprocess overhead)
- Used when: 7-Zip not found OR user specifies `PreferredEngine="DotNet"` OR 7-Zip execution fails
- Always available (bundled dependency)

**4. File System:**
- .NET 8 System.IO APIs via IFileSystem abstraction
- Supports Windows (NTFS, ReFS), Linux (ext4, xfs), macOS (APFS, HFS+)
- UNC path support: Windows network shares (\\\\server\\share)
- No special mount requirements for Epic 1 (read + write only)

**No Project Dependency Manifests Found:**
- Epic 1 implementation will create .csproj with above NuGet packages
- Development environment setup deferred to implementation phase

## Acceptance Criteria (Authoritative)

**AC1: Get-DateRange Cmdlet**
- Given olderThan=7 and youngerThan=0, when cmdlet executes, then returns 8 DateRangeResult objects (days 7 through 0 inclusive)
- Given olderThan=7 and youngerThan=7, when cmdlet executes, then returns 1 DateRangeResult object (day 7 only)
- Given olderThan=5 and youngerThan=7 (invalid), when cmdlet executes, then throws ArgumentException with validation message
- Given DateCriteria=CreationDate, when cmdlet executes, then DateRangeResult.Criteria is set to CreationDate
- Given DateCriteria=ModifiedDate (default), when cmdlet executes, then DateRangeResult.Criteria is set to ModifiedDate

**AC2: Path Token Resolution**
- Given template "s3://bucket/{year}/{month}/{day}" and LogDate=2025-10-08, when PathTokenResolver resolves, then returns "s3://bucket/2025/10/08"
- Given template "\\\\server\\{app}\\{year}-{month}" and AppName="MyApp", LogDate=2025-10-08, when resolved, then returns "\\\\server\\MyApp\\2025-10"
- Given template with {server} token and ServerName="PROD-SVR01", when resolved, then replaces {server} with "PROD-SVR01"
- Given template with no tokens, when resolved, then returns template unchanged

**AC3: Archive Naming**
- Given AppName="MyApp" and Date=2025-10-08, when ArchiveNamingService generates name, then returns "MyApp-20251008.zip"
- Given AppName with spaces "My App" and Date=2025-10-08, when generated, then returns "My App-20251008.zip" (spaces preserved)

**AC4: Find-LogFiles Cmdlet**
- Given path with 100 files, date range with 3 dates, and files matching those dates exist, when cmdlet executes, then returns only LogFileInfo for files matching date criteria
- Given DateCriteria=CreationDate, when cmdlet executes, then filters files by CreationTime property
- Given DateCriteria=ModifiedDate, when cmdlet executes, then filters files by LastWriteTime property
- Given Recurse switch, when cmdlet executes, then searches all subdirectories recursively
- Given no Recurse switch, when cmdlet executes, then searches top directory only
- Given path with no matching files, when cmdlet executes, then returns empty array (no error)

**AC5: Find-LogFolders Cmdlet**
- Given path with folders named "20251001", "20251002", "20251003" and date range matching first two, when cmdlet executes, then returns 2 LogFolderInfo objects
- Given path with folders named "2025-10-01" (hyphenated format), when cmdlet executes, then successfully parses date and returns LogFolderInfo with FolderNamePattern="YYYY-MM-DD"
- Given path with folders named "20251001" (no hyphens), when cmdlet executes, then successfully parses date and returns LogFolderInfo with FolderNamePattern="YYYYMMDD"
- Given path with folders not matching date pattern (e.g., "logs", "archive"), when cmdlet executes, then skips those folders
- Given path with no matching folders, when cmdlet executes, then returns empty array (no error)

**AC6: Test-ArchiveExists Cmdlet**
- Given S3 destination "s3://bucket/archives/{year}", date range with 5 dates, and 2 archives exist, when cmdlet executes, then returns 3 DateRangeResult objects (dates NOT archived)
- Given UNC destination "\\\\server\\share\\{app}", date range with 3 dates, and all 3 archives exist, when cmdlet executes, then returns empty array
- Given Local destination "D:\\Archives\\{year}\\{month}", date range with 7 dates, and 0 archives exist, when cmdlet executes, then returns all 7 DateRangeResult objects unchanged
- Given S3 destination with explicit AwsAccessKey/AwsSecretKey parameters, when cmdlet executes, then passes credentials to Get-S3Object cmdlet
- Given S3 destination without credentials, when cmdlet executes, then Get-S3Object uses default AWS credential chain
- Given invalid destination path (not s3://, \\\\, or local), when cmdlet executes, then throws ArgumentException

**AC7: Compress-Logs Cmdlet (7-Zip Available)**
- Given 7-Zip installed and SourcePath pointing to folder, when cmdlet executes with PreferredEngine="Auto", then uses SevenZipCompressor
- Given source folder D:\\Logs\\App\\20251001 with 1000 files totaling 500 MB, when compressed, then creates archive on same drive (D:\\) with CompressionEngine="7-Zip"
- Given OutputPath on different drive than source, when cmdlet executes, then throws CompressionException with in-place constraint violation message
- Given compression succeeds, when cmdlet executes, then returns ArchiveInfo with CompressedSize, OriginalSize, CompressionDuration populated

**AC8: Compress-Logs Cmdlet (7-Zip Unavailable)**
- Given 7-Zip NOT installed and PreferredEngine="Auto", when cmdlet executes, then fallback to SharpCompressCompressor with WriteWarning message
- Given SharpCompress compression succeeds, when cmdlet executes, then returns ArchiveInfo with CompressionEngine="SharpCompress"
- Given PreferredEngine="SevenZip" and FailIfPreferredUnavailable=true, when 7-Zip unavailable, then throws CompressionException with installation guidance

**AC9: Compress-Logs Cmdlet (Pipeline Input)**
- Given LogFolderInfo[] from Find-LogFolders piped to Compress-Logs, when cmdlet executes, then accepts pipeline input and compresses each folder
- Given LogFileInfo[] from Find-LogFiles piped to Compress-Logs, when cmdlet executes, then accepts pipeline input and compresses files
- Given string[] paths piped to Compress-Logs, when cmdlet executes, then accepts pipeline input and compresses each path

**AC10: Error Handling (Fail-Fast)**
- Given any cmdlet encounters file not found error, when error occurs, then throws ItemNotFoundException via ThrowTerminatingError
- Given any cmdlet encounters permission denied, when error occurs, then throws UnauthorizedAccessException with operation context
- Given compression fails (7-Zip exits non-zero), when error occurs, then throws CompressionException with stderr output
- Given S3 connection fails in Test-ArchiveExists, when error occurs, then throws TransferException wrapping AWS error

**AC11: Pipeline Composition**
- Given Get-DateRange output piped to Find-LogFolders, when pipeline executes, then Find-LogFolders accepts DateRangeResult[] from pipeline
- Given Get-DateRange piped to Test-ArchiveExists piped to Find-LogFolders, when pipeline executes, then filters dates through entire chain
- Given Find-LogFolders output piped to Compress-Logs, when pipeline executes, then Compress-Logs accepts LogFolderInfo[] from pipeline

**AC12: Verbose Logging**
- Given any cmdlet executes with -Verbose, when operation completes, then writes verbose messages for key operations (engine selection, discovery results, compression progress)
- Given Compress-Logs selects 7-Zip, when executed with -Verbose, then outputs "Using 7-Zip CLI"
- Given Compress-Logs falls back to SharpCompress, when executed with -Verbose, then outputs warning and verbose message about fallback

## Traceability Mapping

| AC | Spec Section(s) | Component(s)/API(s) | Functional Req | Test Idea |
|----|----------------|---------------------|----------------|-----------|
| AC1 | APIs and Interfaces → Get-DateRange | GetDateRangeCmdlet, DateRangeCalculator | FR001 | xUnit: DateRangeCalculatorTests with [Theory] for various day ranges |
| AC2 | Services and Modules → PathTokenResolver | PathTokenResolver.ResolveTokens() | FR013 | xUnit: PathTokenResolverTests with token replacement scenarios |
| AC3 | Services and Modules → ArchiveNamingService | ArchiveNamingService.GetArchiveName() | FR010 | xUnit: ArchiveNamingServiceTests with various app names and dates |
| AC4 | APIs and Interfaces → Find-LogFiles | FindLogFilesCmdlet, FileDiscoveryService | FR003 | Pester: Create test files, run cmdlet, verify filtered results |
| AC5 | APIs and Interfaces → Find-LogFolders | FindLogFoldersCmdlet, FolderDiscoveryService | FR004 | Pester: Create test folders with YYYYMMDD/YYYY-MM-DD patterns, verify parsing |
| AC6 | APIs and Interfaces → Test-ArchiveExists | TestArchiveExistsCmdlet, ArchiveExistenceChecker, IDestinationProvider | FR002, FR014 | Pester: Mock S3/UNC/Local destinations, verify filtering logic |
| AC7 | APIs and Interfaces → Compress-Logs, Workflows → Compression Engine Selection | CompressLogsCmdlet, CompressionService, SevenZipCompressor | FR005, NFR002 | Pester: Verify 7-Zip usage when available, check in-place constraint |
| AC8 | APIs and Interfaces → Compress-Logs, Workflows → Compression Engine Selection | CompressLogsCmdlet, CompressionService, SharpCompressCompressor | FR005 | Pester: Temporarily hide 7-Zip, verify SharpCompress fallback |
| AC9 | Data Models → Pipeline Objects, APIs → ValueFromPipeline | All cmdlets with ValueFromPipeline parameters | FR009 | Pester: Chain cmdlets via pipeline, verify data flows correctly |
| AC10 | Detailed Design → Exception Hierarchy, NFR → Reliability | All cmdlets using ThrowTerminatingError | FR012 | xUnit: Mock file system to trigger errors, verify exception types |
| AC11 | Workflows and Sequencing → Epic 1 Usage Pattern | Pipeline parameter bindings across cmdlets | FR009 | Pester: Full pipeline test (Get-DateRange  Test-ArchiveExists  Find-LogFolders  Compress-Logs) |
| AC12 | NFR → Observability → Verbose Logging | WriteVerbose() calls in all cmdlets | Observability | Pester: Capture verbose stream, verify expected messages present |

**Requirements Coverage Summary:**

| Functional Requirement | Acceptance Criteria | Implementation Status |
|------------------------|---------------------|----------------------|
| FR001: Date Range Calculation | AC1 | Epic 1 - Core |
| FR002: Pre-Flight Archive Check | AC6 | Epic 1 - Core |
| FR003: File Discovery | AC4 | Epic 1 - Core |
| FR004: Folder Discovery | AC5 | Epic 1 - Core |
| FR005: In-Place Compression | AC7, AC8 | Epic 1 - Core |
| FR009: Pipeline Composition | AC9, AC11 | Epic 1 - Core |
| FR010: Archive Naming | AC3 | Epic 1 - Core |
| FR011: Idempotent Execution | Partial (AC6 enables it) | Epic 1 foundation, Epic 2 full testing |
| FR012: Fail-Fast Error Handling | AC10 | Epic 1 - Core |
| FR013: Path Token Resolution | AC2 | Epic 1 - Core |
| FR014: Archive Existence Protocol | AC6 | Epic 1 - Core |
| NFR001: Performance | Streaming implementation | Epic 1 foundation, Epic 2 benchmarks |
| NFR002: In-Place Compression | AC7 enforcement | Epic 1 - Core |
| NFR003: Idempotent | AC6 check-first | Epic 1 foundation, Epic 2 full validation |

## Risks, Assumptions, Open Questions

**RISK-1: 7-Zip CLI Detection Across Platforms**
- **Type:** Technical Risk
- **Description:** 7-Zip installation paths vary across Windows/Linux/macOS, detection logic may miss valid installations
- **Impact:** Medium - Falls back to SharpCompress (works but suboptimal compression)
- **Mitigation:** Test on all target platforms, document common installation paths, provide clear verbose logging when 7-Zip not found
- **Owner:** Implementation team

**RISK-2: AWS.Tools.S3 Module Not Installed**
- **Type:** Dependency Risk
- **Description:** Users may not have AWS.Tools.S3 module installed, Test-ArchiveExists will fail for S3 destinations
- **Impact:** Medium - Cmdlet fails with clear error message
- **Mitigation:** Check module availability in BeginProcessing(), provide installation command in error message, document prerequisite in README
- **Owner:** Implementation team

**RISK-3: In-Place Compression Volume Detection**
- **Type:** Technical Risk
- **Description:** Volume comparison logic may not work correctly for network paths or mounted drives
- **Impact:** Low - May incorrectly allow cross-volume compression or reject valid same-volume operations
- **Mitigation:** Test with UNC paths, mapped drives, symbolic links; document known limitations
- **Owner:** Implementation team

**RISK-4: SharpCompress Compression Ratio**
- **Type:** Performance Risk
- **Description:** SharpCompress may produce larger archives than 7-Zip, impacting storage costs
- **Impact:** Low - Only affects users without 7-Zip; workaround is to install 7-Zip
- **Mitigation:** Epic 2 benchmarks will quantify compression ratio difference, recommend 7-Zip installation for production use
- **Owner:** Epic 2 performance testing

**ASSUMPTION-1: PowerShell 7.2+ Availability**
- **Description:** Target servers have PowerShell 7.2 or later installed (not Windows PowerShell 5.1)
- **Validation:** Document prerequisite, check in CI/CD, provide installation guidance
- **Impact if Invalid:** Module will not load on PowerShell 5.1

**ASSUMPTION-2: .NET 8 Runtime Availability**
- **Description:** Target servers have .NET 8 runtime installed
- **Validation:** Module manifest requires PowerShell 7.2+, which includes .NET runtime
- **Impact if Invalid:** Module will fail to load with runtime version error

**ASSUMPTION-3: Test Environment Configuration**
- **Description:** All integration testing uses environment variables per PRD (AWS_Access, AWS_Bucket, Local_Drive, UNC_Drive)
- **Validation:** CI/CD setup validates environment variables are set before running integration tests
- **Impact if Invalid:** Integration tests fail

**ASSUMPTION-4: Date Folder Naming Patterns**
- **Description:** Log folders use YYYYMMDD or YYYY-MM-DD naming patterns only
- **Validation:** Find-LogFolders ignores folders not matching these patterns
- **Impact if Invalid:** Folders with other date formats will be skipped (user can still use Find-LogFiles)

**QUESTION-1: Should Compress-Logs support multiple compression formats?**
- **Status:** OPEN
- **Context:** PRD specifies .zip only for MVP
- **Impact:** If users request .7z or .tar.gz support, requires architecture changes
- **Decision Needed:** Post-MVP based on user feedback
- **Owner:** Product team after MVP deployment

**QUESTION-2: Should archive existence checking cache results?**
- **Status:** OPEN
- **Context:** Test-ArchiveExists queries S3/UNC/Local for each date, potentially slow for 30+ day ranges
- **Impact:** Performance for large date ranges
- **Decision Needed:** Epic 1 implements no caching (simplicity), Epic 2 benchmarks will reveal if needed
- **Owner:** Epic 2 performance analysis

**QUESTION-3: Should Find-LogFiles support glob patterns?**
- **Status:** OPEN
- **Context:** Currently uses date filtering only, no file name pattern matching (e.g., "*.log")
- **Impact:** Users must filter by extension separately if needed
- **Decision Needed:** Post-MVP enhancement if user feedback indicates need
- **Owner:** Product team after MVP deployment

## Post-Review Follow-ups

**Story 1.1 - Get-DateRange Implementation (2025-10-08):**

1. ✅ **[Medium] Framework Version Alignment** - **RESOLVED**: Architecture documentation (ADR-001) updated to specify .NET 9.0 instead of .NET 8.0 LTS. Rationale: Latest C# 12 features, improved I/O and memory performance, internal deployment environment supports latest runtime. .NET 9.0 is stable and production-ready (released November 2024). See Solution Architecture Document ADR-001 for complete justification.

2. **[Low][Optional] Constructor Injection Pattern** - Add internal constructor to GetDateRangeCmdlet accepting DateRangeCalculator for improved testability, aligning with Solution Architecture section 6.2 cmdlet implementation pattern. Current direct instantiation is acceptable given comprehensive service-layer testing. **Status**: Optional enhancement, non-blocking. File: src/logManager/Cmdlets/GetDateRangeCmdlet.cs:19

## Test Strategy Summary

**Test Coverage Target: 100% for All Epic 1 Components**

**Test Levels:**

**1. Unit Tests (xUnit - C#)**
- **Scope:** All internal services and helper classes
- **Coverage:** 100% code coverage for Foundation, Discovery, and Processing layer services
- **Approach:**
  - Mock all external dependencies (IFileSystem, IDestinationProvider, ICompressionEngine)
  - Use [Theory] and [InlineData] for parameterized testing
  - Test all parameter combinations and edge cases
- **Key Test Suites:**
  - DateRangeCalculatorTests: Date math validation (AC1)
  - PathTokenResolverTests: Token replacement scenarios (AC2)
  - ArchiveNamingServiceTests: Archive name generation (AC3)
  - FileDiscoveryServiceTests: File enumeration and filtering logic
  - FolderDiscoveryServiceTests: Folder parsing (YYYYMMDD, YYYY-MM-DD patterns)
  - CompressionServiceTests: Engine selection logic, fallback behavior
  - SevenZipCompressorTests: 7-Zip CLI invocation (mocked Process.Start)
  - SharpCompressCompressorTests: SharpCompress library usage
- **Execution:** `dotnet test --collect:"XPlat Code Coverage"`

**2. Integration Tests (Pester - PowerShell)**
- **Scope:** All public cmdlets with real file system (test directories)
- **Coverage:** All cmdlet parameter combinations, pipeline scenarios, error handling
- **Approach:**
  - Use $env:Local_Drive for test file/folder creation
  - Mock S3 operations where appropriate (or use real test bucket)
  - Test both 7-Zip and SharpCompress paths (hide 7-Zip temporarily for fallback testing)
- **Key Test Suites:**
  - Get-DateRange.Tests.ps1: Cmdlet parameter validation (AC1)
  - Find-LogFiles.Tests.ps1: File discovery integration (AC4)
  - Find-LogFolders.Tests.ps1: Folder discovery integration (AC5)
  - Test-ArchiveExists.Tests.ps1: Archive checking across S3/UNC/Local (AC6)
  - Compress-Logs.Tests.ps1: Compression with both engines (AC7, AC8)
  - Pipeline-Composition.Tests.ps1: End-to-end pipeline chaining (AC9, AC11)
  - Error-Handling.Tests.ps1: Fail-fast exception verification (AC10)
  - Verbose-Logging.Tests.ps1: Verbose output validation (AC12)
- **Execution:** `Invoke-Pester ./tests/Integration -Output Detailed`

**3. Edge Case Tests**
- **Large File Counts:** Test with 10K+ files to validate streaming (preliminary scale testing)
- **Permission Errors:** Test read-only folders, write-protected destinations
- **Network Failures:** Simulate S3 connectivity issues, UNC share unavailability
- **Invalid Inputs:** Malformed paths, negative day values, cross-volume compression attempts
- **Compression Failures:** Corrupt source files, disk full scenarios, 7-Zip process crashes

**4. Regression Testing**
- All tests run on every PR via GitHub Actions CI
- Both Windows and Linux runners (cross-platform validation)
- Test matrix: PowerShell 7.2, 7.3, 7.4

**Test Data Setup:**
- **BeforeAll:** Create test directory structure with dated folders and files
- **AfterAll:** Clean up test artifacts
- **Test Fixtures:**
  - `D:\TEST_DATA\Epic1\Logs\App1\20251001\app.log` (100 files per folder)
  - `D:\TEST_DATA\Epic1\Logs\App2\2025-10-01\service.log` (hyphenated format)
  - `D:\TEST_DATA\Epic1\Archives\` (pre-created archives for Test-ArchiveExists tests)

**Success Criteria:**
- All unit tests pass (100% coverage)
- All integration tests pass
- All edge case scenarios handled with appropriate exceptions
- CI/CD pipeline green on Windows and Linux
- No regressions from PRD functional requirements
