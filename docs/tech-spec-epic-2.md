# Technical Specification: Distribution, Orchestration & Performance Optimization

Date: 2025-10-08
Author: Adam
Epic ID: 2
Status: Draft

---

## Overview

Epic 2 completes the logManager module by implementing the Distribution and Orchestration layers, enabling end-to-end automated log archival workflows. This epic delivers multi-destination file transfer capabilities (AWS S3, UNC network shares, local drives), safe source cleanup after confirmed transfers, and the all-in-one Invoke-LogArchival orchestrator function that chains all operations from Epic 1. Additionally, Epic 2 validates the module's massive-scale performance claims through comprehensive benchmarking with 300K-1M file datasets, ensuring NFR001 targets are met (<5 minutes for 1M file scan), and proves idempotent execution and resumability through integration testing. The epic transforms Epic 1's foundational building blocks into a production-ready archival solution capable of handling massive daily log volumes efficiently.

## Objectives and Scope

**Primary Objectives:**
1. Implement Distribution layer with multi-destination transfer support (S3, UNC, local drives) using AWS.Tools.S3 PowerShell module integration
2. Implement safe source cleanup function (Remove-LogSource) with confirmed transfer validation
3. Implement Invoke-LogArchival orchestrator function chaining all Epic 1 + Epic 2 operations with configurable steps
4. Validate massive-scale performance (300K-1M files) against NFR001 target (<5 minutes for 1M file scan)
5. Prove idempotent execution and resumability through end-to-end integration testing
6. Achieve 100% test coverage for Distribution and Orchestration layers
7. Create comprehensive module documentation and usage examples for MVP deployment

**In Scope:**
- Send-ToS3 backend function (FR015: AWS S3 transfer using Write-S3Object cmdlet)
- Send-ToUNC backend function (FR016: UNC network share transfer using .NET file I/O)
- Send-ToDrive backend function (FR017: Local drive transfer using .NET file I/O)
- Send-Archive dispatcher cmdlet (FR006: Route to appropriate backend based on destination path, return success confirmation)
- Remove-LogSource cmdlet (FR007: Safe deletion of source files/folders after confirmed transfer)
- Invoke-LogArchival orchestrator cmdlet (FR008: All-in-one workflow chaining all operations)
- TransferResult and WorkflowResult models
- S3TransferService, UncTransferService, LocalTransferService, CleanupService (internal services)
- Performance benchmarks: 300K file dataset, 1M file scan extrapolation, compression throughput
- Idempotent execution integration tests (full workflow retry scenarios)
- End-to-end integration tests (complete archival workflows with all destination types)
- Module documentation: README.md, USAGE.md, function reference

**Out of Scope (Post-MVP Enhancements):**
- WhatIf/Confirm parameters for dry-run testing
- Parallel processing for multiple days simultaneously
- Progress bars for long operations
- Multipart upload for large S3 files
- Archive restoration/extraction capabilities
- Built-in scheduling (users implement via Task Scheduler/cron)
- Complex error recovery or automatic retry logic beyond idempotent execution
- Built-in logging/metrics/monitoring (user scripts handle via external tooling)
- Encryption of archives

## System Architecture Alignment

This epic implements the final two layers of the four-layer architecture defined in the Solution Architecture Document:

**Distribution Layer (Depends on Processing Layer from Epic 1):**
- Aligns with ADR-003: Use AWS.Tools.S3 PowerShell module (no AWS SDK bundling)
- Implements Send-ToS3, Send-ToUNC, Send-ToDrive backend functions as internal services
- Send-Archive dispatcher cmdlet analyzes destination path pattern and routes to appropriate backend:
  - `s3://bucket/path` → S3TransferService → Write-S3Object cmdlet
  - `\\server\share\path` → UncTransferService → .NET File.Copy()
  - Local path (drive letter or absolute path) → LocalTransferService → .NET File.Copy()
- Aligns with ADR-004: Direct credential passing (no AWS profiles) via -AwsAccessKey/-AwsSecretKey parameters
- Returns TransferResult objects with success confirmation signal required for FR007 cleanup
- Path token resolution using Epic 1's PathTokenResolver service (tokens resolved for each log date)

**Orchestration Layer (Depends on All Layers):**
- Implements Invoke-LogArchival cmdlet as the "convenience orchestrator"
- Chains operations in strict order:
  1. Get-DateRange (Epic 1 Foundation)
  2. Test-ArchiveExists (Epic 1 Discovery - check-first workflow)
  3. Find-LogFiles OR Find-LogFolders (Epic 1 Discovery)
  4. Compress-Logs (Epic 1 Processing - optional via -Compress switch)
  5. Send-Archive (Epic 2 Distribution)
  6. Remove-LogSource (Epic 2 Distribution - optional via -DeleteSource switch)
- Configurable step inclusion: `-SkipCheck`, `-Compress`, `-DeleteSource` switches
- Returns WorkflowResult aggregate object with metrics from all operations
- Implements idempotent execution pattern using Test-ArchiveExists filtering (NFR003)

**Performance Validation (NFR001, NFR002, NFR003):**
- Benchmarks with 300K file dataset to validate <2 minute discovery target (extrapolates to <5 min for 1M files)
- Validates in-place compression enforcement (NFR002) across all scenarios
- Proves idempotent execution via retry integration tests (partial failure → retry → completes without duplicate work)
- Validates streaming memory efficiency (no loading entire file lists into memory)

**Architectural Patterns Maintained:**
- Each cmdlet < 500 lines (ADR-004)
- Constructor injection for all new services
- Pipeline composition extended to Distribution layer (ArchiveInfo → Send-Archive)
- Fail-fast error handling with TransferException and CleanupException
- 100% test coverage target (ADR-006)

## Detailed Design

### Services and Modules

| Component | Type | Responsibilities | Inputs | Outputs | Owner |
|-----------|------|-----------------|--------|---------|-------|
| **Send-Archive** | Public Cmdlet | Dispatcher routing to appropriate transfer backend based on destination path | ArchiveFile (FileInfo/ArchiveInfo), Destination (string with tokens), AppName, AWS credentials (optional) | TransferResult[] | Distribution Layer |
| **Remove-LogSource** | Public Cmdlet | Safe deletion of source files/folders after confirmed transfer | SourcePath (string/LogFileInfo[]/LogFolderInfo[]), TransferResult[] (confirmation) | CleanupResult | Distribution Layer |
| **Invoke-LogArchival** | Public Cmdlet | All-in-one orchestrator chaining date calculation → check → find → compress → transfer → cleanup | Path, olderThan/youngerThan, AppName, Destination, switches (-Compress, -DeleteSource, -SkipCheck) | WorkflowResult | Orchestration Layer |
| **S3TransferService** | Internal Service | AWS S3 uploads via AWS.Tools.S3 Write-S3Object cmdlet | FilePath, BucketName, S3Key, AWS credentials (optional) | TransferResult | Distribution Layer |
| **UncTransferService** | Internal Service | UNC network share transfers using .NET File.Copy | SourcePath, UNC destination path | TransferResult | Distribution Layer |
| **LocalTransferService** | Internal Service | Local/external drive transfers using .NET File.Copy | SourcePath, Local destination path | TransferResult | Distribution Layer |
| **CleanupService** | Internal Service | Safe file/folder deletion with validation | Source paths, Transfer confirmations | CleanupResult | Distribution Layer |

### Data Models and Contracts

**Distribution Layer Models:**

```csharp
// Transfer result from Send-Archive operations
public record TransferResult
{
    public bool Success { get; init; }                     // Transfer success confirmation
    public string SourcePath { get; init; }                // Original archive file path
    public string Destination { get; init; }               // Full resolved destination path
    public string DestinationType { get; init; }           // "S3", "UNC", "Local"
    public long BytesTransferred { get; init; }            // Archive file size in bytes
    public TimeSpan TransferDuration { get; init; }        // How long transfer took
    public string ErrorMessage { get; init; }              // Populated if Success=false
}

// Cleanup result from Remove-LogSource operations
public record CleanupResult
{
    public int FilesDeleted { get; init; }                 // Count of files removed
    public int FoldersDeleted { get; init; }               // Count of folders removed
    public long BytesFreed { get; init; }                  // Disk space freed in bytes
    public List<string> DeletedPaths { get; init; }        // Paths successfully deleted
    public List<string> FailedPaths { get; init; }         // Paths that failed to delete
}

// Orchestration result from Invoke-LogArchival
public record WorkflowResult
{
    public int FilesProcessed { get; init; }               // Total files/folders processed
    public int ArchivesCreated { get; init; }              // Count of .zip archives created
    public long TotalBytesCompressed { get; init; }        // Original size before compression
    public long TotalBytesTransferred { get; init; }       // Compressed size transferred
    public int FilesDeleted { get; init; }                 // Source files deleted (if -DeleteSource)
    public TimeSpan TotalDuration { get; init; }           // End-to-end workflow duration
    public List<string> Errors { get; init; }              // Error messages from failures
    public List<string> Warnings { get; init; }            // Warning messages
    public List<string> SkippedDates { get; init; }        // Dates skipped (already archived)
}
```

**Exception Additions:**

```csharp
public class TransferException : LogManagerException
{
    public TransferException(string message) : base(message) { }
    public TransferException(string message, Exception inner) : base(message, inner) { }
    public string DestinationType { get; set; }            // "S3", "UNC", "Local"
}

public class CleanupException : LogManagerException
{
    public CleanupException(string message) : base(message) { }
    public CleanupException(string message, Exception inner) : base(message, inner) { }
    public List<string> FailedPaths { get; set; }          // Paths that failed to delete
}
```

### APIs and Interfaces

**Public Cmdlet Signatures:**

```csharp
// Distribution Layer
[Cmdlet(VerbsCommunications.Send, "Archive")]
[OutputType(typeof(TransferResult[]))]
public class SendArchiveCmdlet : PSCmdlet
{
    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public object[] ArchiveFile { get; set; }  // Accepts: FileInfo[], ArchiveInfo[] from Compress-Logs

    [Parameter(Mandatory = true)]
    public string Destination { get; set; }  // s3://bucket/path, \\server\share\path, or D:\path (with tokens)

    [Parameter(Mandatory = true)]
    public string AppName { get; set; }

    // S3-specific parameters
    [Parameter(Mandatory = false)]
    public string AwsAccessKey { get; set; }

    [Parameter(Mandatory = false)]
    public string AwsSecretKey { get; set; }

    [Parameter(Mandatory = false)]
    public string AwsRegion { get; set; }
}

[Cmdlet(VerbsCommon.Remove, "LogSource")]
[OutputType(typeof(CleanupResult))]
public class RemoveLogSourceCmdlet : PSCmdlet
{
    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public object[] SourcePath { get; set; }  // Accepts: string[], LogFileInfo[], LogFolderInfo[]

    [Parameter(Mandatory = true)]
    public TransferResult[] TransferConfirmation { get; set; }  // Must have matching successful transfers

    [Parameter(Mandatory = false)]
    public SwitchParameter Force { get; set; }  // Skip confirmation prompts (use with caution)
}

// Orchestration Layer
[Cmdlet(VerbsLifecycle.Invoke, "LogArchival")]
[OutputType(typeof(WorkflowResult))]
public class InvokeLogArchivalCmdlet : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public string Path { get; set; }

    [Parameter(Mandatory = true)]
    public int OlderThan { get; set; }

    [Parameter(Mandatory = false)]
    public int YoungerThan { get; set; } = 0;

    [Parameter(Mandatory = true)]
    public string AppName { get; set; }

    [Parameter(Mandatory = true)]
    public string Destination { get; set; }

    // Workflow configuration switches
    [Parameter(Mandatory = false)]
    public SwitchParameter SkipCheck { get; set; }  // Skip Test-ArchiveExists (re-process all dates)

    [Parameter(Mandatory = false)]
    public SwitchParameter Compress { get; set; }  // Compress before transfer

    [Parameter(Mandatory = false)]
    public SwitchParameter DeleteSource { get; set; }  // Delete source after transfer

    [Parameter(Mandatory = false)]
    public string PreferredEngine { get; set; } = "Auto";  // For compression if -Compress used

    // Discovery mode
    [Parameter(Mandatory = false)]
    public SwitchParameter UseFolders { get; set; }  // Use Find-LogFolders instead of Find-LogFiles

    // S3 parameters (passed through to Send-Archive)
    [Parameter(Mandatory = false)]
    public string AwsAccessKey { get; set; }

    [Parameter(Mandatory = false)]
    public string AwsSecretKey { get; set; }

    [Parameter(Mandatory = false)]
    public string AwsRegion { get; set; }
}
```

**Internal Service Interfaces:**

```csharp
// Transfer service abstraction (for testing)
public interface ITransferService
{
    TransferResult Transfer(string sourcePath, string destination);
    string DestinationType { get; }
}

// S3 transfer implementation
public class S3TransferService : ITransferService
{
    private readonly IPowerShellInvoker _psInvoker;

    public string DestinationType => "S3";

    public TransferResult Transfer(string sourcePath, string s3Destination)
    {
        // Parse s3://bucket/key from destination
        // Invoke Write-S3Object via _psInvoker
        // Return TransferResult with success confirmation
    }
}

// UNC transfer implementation
public class UncTransferService : ITransferService
{
    private readonly IFileSystem _fileSystem;

    public string DestinationType => "UNC";

    public TransferResult Transfer(string sourcePath, string uncDestination)
    {
        // Validate UNC path exists
        // Use File.Copy via _fileSystem
        // Return TransferResult
    }
}

// Local transfer implementation
public class LocalTransferService : ITransferService
{
    private readonly IFileSystem _fileSystem;

    public string DestinationType => "Local";

    public TransferResult Transfer(string sourcePath, string localDestination)
    {
        // Validate local path exists
        // Use File.Copy via _fileSystem
        // Return TransferResult
    }
}

// PowerShell invoker abstraction (for testing AWS.Tools.S3 calls)
public interface IPowerShellInvoker
{
    PSDataCollection<PSObject> InvokeCommand(string commandName, Dictionary<string, object> parameters);
}
```

### Workflows and Sequencing

**End-to-End Orchestration: Invoke-LogArchival**

```powershell
# Production Usage Example
Invoke-LogArchival `
    -Path "D:\Logs\MyApp" `
    -olderThan 7 `
    -AppName "MyApp" `
    -Destination "s3://archive-bucket/logs/{year}/{month}" `
    -Compress `
    -DeleteSource `
    -Verbose
```

**Sequence Diagram: Complete Archival Workflow**

```
User → Invoke-LogArchival: Full parameters with -Compress and -DeleteSource

1. Date Range Calculation
Invoke-LogArchival → Get-DateRange: olderThan=7, youngerThan=0
Get-DateRange → Invoke-LogArchival: [2025-10-01, 2025-10-02, ..., 2025-10-08] (8 dates)

2. Check-First Workflow (unless -SkipCheck)
Invoke-LogArchival → Test-ArchiveExists: Destination, DateRange, AppName
Test-ArchiveExists → PathTokenResolver: Resolve tokens for each date
Test-ArchiveExists → S3DestinationProvider: Check "MyApp-20251001.zip", "MyApp-20251002.zip", etc.
S3DestinationProvider → AWS.Tools.S3: Get-S3Object queries
Test-ArchiveExists → Invoke-LogArchival: [2025-10-02, 2025-10-05] (only NOT archived dates)

3. Discovery (UseFolders or Files)
If UseFolders:
    Invoke-LogArchival → Find-LogFolders: Path, FilteredDateRange
    Find-LogFolders → Invoke-LogArchival: LogFolderInfo[] (folders matching dates)
Else:
    Invoke-LogArchival → Find-LogFiles: Path, FilteredDateRange
    Find-LogFiles → Invoke-LogArchival: LogFileInfo[] (files matching dates)

4. Compression (if -Compress switch)
Invoke-LogArchival → Compress-Logs: SourcePaths, AppName, PreferredEngine
Compress-Logs → CompressionService: Select engine (7-Zip or SharpCompress)
Compress-Logs → Invoke-LogArchival: ArchiveInfo[] (created .zip files with metrics)

5. Transfer
Invoke-LogArchival → Send-Archive: ArchiveFiles, Destination, AppName, AWS credentials
Send-Archive → PathTokenResolver: Resolve "{year}/{month}" for each archive's log date
Send-Archive: Analyze destination pattern → Route to appropriate backend
Send-Archive → S3TransferService: Upload to s3://archive-bucket/logs/2025/10/MyApp-20251002.zip
S3TransferService → AWS.Tools.S3: Write-S3Object cmdlet
Send-Archive → Invoke-LogArchival: TransferResult[] (with Success=true confirmations)

6. Cleanup (if -DeleteSource and transfers succeeded)
Invoke-LogArchival → Remove-LogSource: SourcePaths, TransferConfirmations
Remove-LogSource: Validate all transfers succeeded for given sources
Remove-LogSource → CleanupService: Delete source folders/files
Remove-LogSource → Invoke-LogArchival: CleanupResult (files deleted, bytes freed)

7. Aggregation and Return
Invoke-LogArchival: Aggregate metrics from all steps
Invoke-LogArchival → User: WorkflowResult {
    FilesProcessed: 15000,
    ArchivesCreated: 2,
    TotalBytesCompressed: 5000000000,
    TotalBytesTransferred: 1200000000,
    FilesDeleted: 15000,
    TotalDuration: 00:08:45,
    SkippedDates: ["2025-10-01", "2025-10-03", "2025-10-04", "2025-10-06", "2025-10-07", "2025-10-08"]
}
```

**Send-Archive Dispatcher Logic:**

```
Input: Destination="s3://bucket/logs/{year}/{month}", ArchiveFile, AppName="MyApp", LogDate=2025-10-02

1. PathTokenResolver.ResolveTokens(Destination, LogDate)
   → "s3://bucket/logs/2025/10"

2. Destination Path Pattern Analysis:
   If starts with "s3://" → Parse bucket and key prefix → S3TransferService
   Else if starts with "\\\\" → UncTransferService
   Else (local path like "D:\\" or "/mnt/") → LocalTransferService

3. Selected Service: S3TransferService
   S3TransferService.Transfer(
       sourcePath: "D:\\Logs\\MyApp-20251002.zip",
       s3Destination: "s3://bucket/logs/2025/10/MyApp-20251002.zip",
       credentials: AwsAccessKey/AwsSecretKey if provided
   )

4. S3TransferService Implementation:
   - Parse bucket name: "bucket"
   - Parse S3 key: "logs/2025/10/MyApp-20251002.zip"
   - Invoke Write-S3Object via IPowerShellInvoker:
     PowerShell.Create()
       .AddCommand("Write-S3Object")
       .AddParameter("BucketName", "bucket")
       .AddParameter("File", "D:\\Logs\\MyApp-20251002.zip")
       .AddParameter("Key", "logs/2025/10/MyApp-20251002.zip")
       .AddParameter("AccessKey", accessKey)  // if provided
       .AddParameter("SecretKey", secretKey)  // if provided
       .Invoke()

5. Return TransferResult:
   {
       Success: true,
       SourcePath: "D:\\Logs\\MyApp-20251002.zip",
       Destination: "s3://bucket/logs/2025/10/MyApp-20251002.zip",
       DestinationType: "S3",
       BytesTransferred: 600000000,
       TransferDuration: 00:02:15
   }
```

**Remove-LogSource Safety Logic:**

```
Input: SourcePaths=["D:\\Logs\\MyApp\\20251002", "D:\\Logs\\MyApp\\20251005"],
       TransferConfirmations=[TransferResult{Success=true, SourcePath="D:\\Logs\\MyApp-20251002.zip"}, ...]

1. Validation: Match source folders/files to transfer confirmations
   - For each SourcePath, find corresponding TransferResult
   - Extract original source from archive name (MyApp-20251002.zip → date 2025-10-02 → folder 20251002)
   - Verify TransferResult.Success == true for that source

2. Safety Check: If ANY transfer failed or missing confirmation → ABORT with CleanupException
   "Cannot delete sources: Some transfers did not complete successfully"

3. If all validations pass:
   For each source:
       CleanupService.DeletePath(source)
       → File.Delete() or Directory.Delete(recursive=true)
       → Track deleted files/folders count and bytes freed

4. Return CleanupResult:
   {
       FilesDeleted: 15000,
       FoldersDeleted: 2,
       BytesFreed: 5000000000,
       DeletedPaths: ["D:\\Logs\\MyApp\\20251002", "D:\\Logs\\MyApp\\20251005"],
       FailedPaths: []
   }
```

## Non-Functional Requirements

### Performance

**NFR001: Massive-Scale Performance Validation (CRITICAL)**
- **Target:** File enumeration completes in <5 minutes for 1M file scan
- **Epic 2 Benchmark Suite:**
  1. **300K File Dataset Test:**
     - Create test directory with 300,000 files across multiple dated folders
     - Run full Invoke-LogArchival workflow (discover → compress → transfer → cleanup)
     - Measure: Discovery time, compression time, transfer time, total end-to-end duration
     - Success Criteria: Discovery <90 seconds, Total workflow <10 minutes for 300K files
  2. **1M File Extrapolation:**
     - Run Find-LogFiles on test directory with 300K files, measure duration
     - Extrapolate to 1M files (linear scaling assumption)
     - Validate extrapolated time <5 minutes for 1M file scan
  3. **Compression Throughput Benchmark:**
     - Measure 7-Zip compression rate: GB/minute
     - Measure SharpCompress compression rate: GB/minute
     - Compare compression ratios (archive size / original size)
     - Document performance trade-offs for ADR-002 hybrid strategy

**Transfer Performance Targets:**
- **S3 Upload:** Limited by network bandwidth, target >50 MB/sec for large files (use Write-S3Object defaults)
- **UNC Transfer:** Limited by network bandwidth, target >100 MB/sec for gigabit networks
- **Local Transfer:** Limited by disk I/O, target >500 MB/sec for SSD-to-SSD

**Orchestration Overhead:**
- Invoke-LogArchival cmdlet coordination overhead: <5 seconds for typical workflows
- Negligible compared to discovery/compression/transfer durations

### Security

**S3 Transfer Security:**
- AWS.Tools.S3 Write-S3Object uses HTTPS by default (encrypted in transit)
- Credential handling: Direct parameter passing (no profile caching)
- If AwsAccessKey/AwsSecretKey provided, passed securely to Write-S3Object cmdlet
- If credentials omitted, AWS.Tools.S3 uses default credential chain (IAM role preferred for production)
- Never log credential values in verbose output or error messages

**UNC Transfer Security:**
- Uses current process credentials for UNC share authentication
- No credential storage or caching
- Requires user to have write permissions on target UNC share
- File transfers use Windows NTFS ACLs (preserves security descriptors if supported)

**Cleanup Safety (Remove-LogSource):**
- CRITICAL: Only deletes after ALL transfers confirmed successful (TransferResult.Success == true)
- Validation logic: Match source paths to transfer confirmations before ANY deletion
- If ANY transfer failed or missing confirmation → Abort entire cleanup with CleanupException
- No partial cleanup (all-or-nothing approach)
- -Force switch available but discouraged (bypasses additional safety checks)

**Path Validation:**
- All destination paths validated before transfer operations
- Reject paths with `..` traversal attempts
- S3 bucket/key validation (no invalid characters)
- UNC path reachability check before transfer
- Local path existence validation

**Audit Trail Recommendations:**
- WorkflowResult includes full operation log (files processed, skipped dates, errors/warnings)
- User scripts should capture WorkflowResult for audit logging
- Recommend external logging infrastructure for compliance requirements

### Reliability/Availability

**NFR003: Idempotent Execution and Resumability (CRITICAL)**
- **End-to-End Idempotent Workflow:**
  - First run: Invoke-LogArchival archives 7 days of logs → 5 succeed, 2 fail (network issue)
  - Retry (same command): Test-ArchiveExists filters out 5 already-archived dates → only retries 2 failed dates
  - Result: No duplicate work, no duplicate archives, safe retry
- **Epic 2 Validation:**
  - Integration test: Simulate partial failure (mock S3 to fail specific dates)
  - Retry workflow with same parameters
  - Assert: Already-archived dates skipped, failed dates re-attempted, no duplicates created

**Fail-Fast with Actionable Errors:**
- **Transfer Failures:**
  - S3 connection timeout → TransferException with "Check network connectivity to S3 endpoints"
  - S3 permission denied → TransferException with "Verify IAM permissions for s3:PutObject"
  - UNC share not reachable → TransferException with "UNC path \\\\server\\share not accessible"
  - Disk full (local transfer) → TransferException with "Insufficient disk space on destination drive"
- **Cleanup Failures:**
  - File in use → CleanupException with "Cannot delete D:\\Logs\\App\\20251001: File in use by another process"
  - Permission denied → CleanupException with "Cannot delete: Access denied (check permissions)"

**Partial Failure Handling:**
- Invoke-LogArchival continues processing remaining dates if one date fails
- Errors collected in WorkflowResult.Errors list
- Final WorkflowResult indicates partial success (some dates succeeded, some failed)
- User script can inspect Errors list and decide retry strategy

**Transaction-Like Cleanup:**
- Remove-LogSource validates ALL transfers succeeded before deleting ANY sources
- If validation fails → CleanupException, NO files deleted
- Guarantees: Either all sources deleted OR none deleted (no partial cleanup)

**Availability:**
- No long-running background processes or services
- Stateless cmdlets (every invocation independent)
- S3 transfers leverage AWS.Tools.S3 resilience (automatic retries within Write-S3Object)
- Graceful degradation: If S3 unavailable, fail with clear error (no hanging or infinite loops)

### Observability

**Verbose Logging (Invoke-LogArchival -Verbose):**
- Step-by-step workflow visibility:
  - "Calculated date range: 2025-10-01 to 2025-10-08 (8 dates)"
  - "Checking for existing archives at s3://bucket/logs/..."
  - "Skipping already archived: 2025-10-01, 2025-10-03 (2 dates)"
  - "Discovering log folders for 6 remaining dates..."
  - "Found 6 folders matching criteria"
  - "Compressing D:\\Logs\\App\\20251002 using 7-Zip..."
  - "Compression complete: 500 MB → 120 MB (76% reduction) in 45 seconds"
  - "Uploading MyApp-20251002.zip to s3://bucket/logs/2025/10/..."
  - "Transfer complete: 120 MB in 8 seconds (15 MB/sec)"
  - "Deleting source: D:\\Logs\\App\\20251002 (15000 files, 500 MB freed)"
  - "Workflow complete: 6 archives created, 90000 files deleted, total duration 12:34"

**WorkflowResult Metrics (Structured Logging):**
```powershell
$result = Invoke-LogArchival -Path "D:\Logs\App" -olderThan 7 -Destination "s3://..." -Compress -DeleteSource

# User script can log metrics:
Write-Log "Archive workflow: $($result.ArchivesCreated) archives, $($result.FilesDeleted) files deleted, $($result.TotalDuration)"

# Example WorkflowResult output:
FilesProcessed       : 90000
ArchivesCreated      : 6
TotalBytesCompressed : 3000000000  # 3 GB original
TotalBytesTransferred: 720000000   # 720 MB compressed
FilesDeleted         : 90000
TotalDuration        : 00:12:34
Errors               : []
Warnings             : ["7-Zip not found, using SharpCompress"]
SkippedDates         : ["2025-10-01", "2025-10-03"]  # Already archived
```

**Error Stream Detail:**
- All failures include:
  - Operation context: "Transfer failed for MyApp-20251002.zip"
  - Root cause: "S3 bucket 'archive-bucket' does not exist"
  - Remediation: "Create bucket or verify bucket name"
  - Error ID: "TransferError-S3BucketNotFound"

**Warning Stream:**
- Non-fatal issues logged:
  - "Archive already exists at destination, skipping upload"
  - "No files found matching date criteria for 2025-10-06"
  - "Compression using SharpCompress instead of 7-Zip (compression ratio may be lower)"

**No Built-In Monitoring (KISS):**
- No metrics export to external systems (CloudWatch, Prometheus, etc.)
- No built-in alerting
- User scripts wrap Invoke-LogArchival with custom monitoring:
  ```powershell
  try {
      $result = Invoke-LogArchival -Path $path -olderThan 7 -Destination $dest -DeleteSource
      if ($result.Errors.Count -gt 0) {
          Send-Alert "Log archival completed with errors: $($result.Errors -join ', ')"
      }
  } catch {
      Send-Alert "Log archival FAILED: $_"
  }
  ```

## Dependencies and Integrations

**Epic 2 Dependencies (Same as Epic 1):**
- NuGet: SharpCompress 0.36.0+, PowerShellStandard.Library 5.1.1
- External Module: AWS.Tools.S3 4.1.0.0+ (for S3 transfers)
- Optional Tool: 7-Zip CLI (for optimal compression)
- Framework: .NET 9.0, PowerShell 7.2+

**New Integration Points in Epic 2:**

**1. AWS S3 File Uploads (via AWS.Tools.S3):**
- Epic 2 Usage: Actual file uploads via Write-S3Object cmdlet (Epic 1 only did existence checks with Get-S3Object)
- Cmdlet: `Write-S3Object -BucketName <bucket> -File <local path> -Key <s3 key>`
- Authentication: Same as Epic 1 (IAM default chain or explicit AccessKey/SecretKey)
- Network: HTTPS to S3 endpoints, requires outbound port 443
- Error Handling: Catch AWS.Tools.S3 exceptions, wrap in TransferException

**2. UNC Network Share I/O:**
- .NET File.Copy() for UNC paths (`\\server\share\path`)
- Requires: SMB/CIFS network connectivity, write permissions on target share
- Authentication: Current process credentials (no explicit credential passing)
- Error Scenarios: Network unreachable, permission denied, disk full on remote share

**3. Local/External Drive I/O:**
- .NET File.Copy() for local paths
- Supports: Local drives (C:\, D:\), mounted external drives, mapped network drives
- No special authentication (file system permissions apply)

**4. File System Deletion:**
- .NET File.Delete() and Directory.Delete(recursive=true)
- Used by Remove-LogSource for cleanup after successful transfers
- Requires: Write/delete permissions on source files/folders
- Safety: Only invoked after transfer confirmation validation

## Acceptance Criteria (Authoritative)

**AC13: Send-Archive to S3**
- Given archive file and destination "s3://bucket/logs/{year}/{month}", when Send-Archive executes, then resolves tokens, uploads to S3 via Write-S3Object, returns TransferResult with Success=true
- Given S3 destination with explicit AwsAccessKey/AwsSecretKey, when Send-Archive executes, then passes credentials to Write-S3Object cmdlet
- Given S3 destination without credentials, when Send-Archive executes, then Write-S3Object uses default AWS credential chain
- Given S3 upload fails (network timeout), when Send-Archive executes, then throws TransferException with actionable error message
- Given AWS.Tools.S3 module not installed, when Send-Archive executes, then throws PSInvalidOperationException with installation instructions

**AC14: Send-Archive to UNC**
- Given archive file and destination "\\\\server\\share\\{app}\\{year}", when Send-Archive executes, then resolves tokens and copies file to UNC path using File.Copy
- Given UNC destination not reachable, when Send-Archive executes, then throws TransferException with "UNC path not accessible" message
- Given UNC transfer succeeds, when Send-Archive executes, then returns TransferResult with DestinationType="UNC" and Success=true

**AC15: Send-Archive to Local Drive**
- Given archive file and destination "D:\\Archives\\{year}\\{month}", when Send-Archive executes, then resolves tokens and copies file to local path
- Given destination directory does not exist, when Send-Archive executes, then creates directory hierarchy and completes transfer
- Given local transfer succeeds, when Send-Archive executes, then returns TransferResult with DestinationType="Local" and Success=true

**AC16: Remove-LogSource Safety**
- Given source paths and ALL corresponding TransferResults with Success=true, when Remove-LogSource executes, then deletes all sources and returns CleanupResult
- Given source paths but ANY TransferResult with Success=false, when Remove-LogSource executes, then throws CleanupException and deletes NOTHING
- Given source paths but missing TransferResult for one path, when Remove-LogSource executes, then throws CleanupException and deletes NOTHING
- Given successful deletions, when Remove-LogSource executes, then returns CleanupResult with FilesDeleted, FoldersDeleted, BytesFreed populated

**AC17: Invoke-LogArchival End-to-End (S3 Destination)**
- Given full parameters with -Compress and -DeleteSource, when Invoke-LogArchival executes, then completes workflow: date calc → check → find → compress → transfer → cleanup, returns WorkflowResult
- Given -SkipCheck switch, when Invoke-LogArchival executes, then skips Test-ArchiveExists and processes all dates in range
- Given NO -Compress switch, when Invoke-LogArchival executes, then skips compression, transfers raw files/folders directly
- Given NO -DeleteSource switch, when Invoke-LogArchival executes, then skips cleanup, returns WorkflowResult with FilesDeleted=0

**AC18: Invoke-LogArchival Idempotent Execution**
- Given first run archives 5 dates successfully and 2 fail, when second run with same parameters, then Test-ArchiveExists filters out 5 archived dates, only processes 2 failed dates
- Given all dates already archived, when Invoke-LogArchival executes, then Test-ArchiveExists returns empty list, WorkflowResult shows ArchivesCreated=0, SkippedDates contains all dates

**AC19: Invoke-LogArchival Partial Failure Handling**
- Given workflow where 3 dates succeed and 1 date fails (S3 upload error), when Invoke-LogArchival executes, then continues processing remaining dates, returns WorkflowResult with Errors list containing failure details
- Given partial failures with -DeleteSource, when Invoke-LogArchival executes, then only deletes sources for successful transfers (safe partial cleanup)

**AC20: Performance Benchmark - 300K Files**
- Given test directory with 300,000 files across dated folders, when Invoke-LogArchival executes full workflow, then discovery completes in <90 seconds
- Given 300K file benchmark, when extrapolated to 1M files, then estimated discovery time <5 minutes (validates NFR001)

**AC21: Performance Benchmark - Compression Throughput**
- Given test data folder (1 GB), when compressed with 7-Zip via Compress-Logs, then measure compression rate (GB/min) and compression ratio
- Given same test data, when compressed with SharpCompress, then measure compression rate and ratio, document difference vs 7-Zip

**AC22: Orchestrator Verbose Logging**
- Given Invoke-LogArchival executed with -Verbose, when workflow runs, then outputs step-by-step messages for all operations (date calc, check, find, compress, transfer, cleanup)
- Given transfer completes, when executed with -Verbose, then outputs transfer duration and throughput (e.g., "Transfer complete: 120 MB in 8 seconds (15 MB/sec)")

## Traceability Mapping

| AC | Spec Section(s) | Component(s)/API(s) | Functional Req | Test Idea |
|----|----------------|---------------------|----------------|-----------|
| AC13 | APIs → Send-Archive, Services → S3TransferService | S3TransferService, IPowerShellInvoker | FR015, FR006 | Pester: Mock AWS.Tools.S3, verify Write-S3Object called with correct parameters |
| AC14 | APIs → Send-Archive, Services → UncTransferService | UncTransferService, IFileSystem | FR016, FR006 | Pester: Create UNC test share ($env:UNC_Drive), verify file copied |
| AC15 | APIs → Send-Archive, Services → LocalTransferService | LocalTransferService, IFileSystem | FR017, FR006 | Pester: Use $env:Local_Drive, verify file copied to destination |
| AC16 | APIs → Remove-LogSource, Services → CleanupService | RemoveLogSourceCmdlet, CleanupService | FR007 | Pester: Test with successful/failed TransferResults, verify safety logic |
| AC17 | Workflows → End-to-End Orchestration, APIs → Invoke-LogArchival | InvokeLogArchivalCmdlet | FR008 | Pester: Full workflow integration test with all switches |
| AC18 | NFR → Idempotent Execution, Workflows → Check-First | Invoke-LogArchival + Test-ArchiveExists | FR011, NFR003 | Pester: Run twice, verify second run skips archived dates |
| AC19 | NFR → Partial Failure Handling | Invoke-LogArchival error aggregation | FR008 | Pester: Mock S3 to fail specific dates, verify partial success handling |
| AC20 | NFR → Performance | File discovery at scale | NFR001 | Pester: Create 300K files, measure discovery time, extrapolate to 1M |
| AC21 | NFR → Performance | Compression engines | NFR001 | Pester: Benchmark 7-Zip vs SharpCompress (rate, ratio) |
| AC22 | NFR → Observability | Invoke-LogArchival verbose logging | Observability | Pester: Capture verbose stream, verify expected messages |

**Requirements Coverage Summary (Epic 2):**

| Functional Requirement | Acceptance Criteria | Implementation Status |
|------------------------|---------------------|----------------------|
| FR006: Send-Archive Dispatcher | AC13, AC14, AC15 | Epic 2 - Core |
| FR007: Source Cleanup | AC16 | Epic 2 - Core |
| FR008: Orchestrated Workflow | AC17, AC19, AC22 | Epic 2 - Core |
| FR011: Idempotent Execution | AC18 | Epic 2 - Complete (end-to-end validation) |
| FR015: Send-ToS3 Backend | AC13 | Epic 2 - Core |
| FR016: Send-ToUNC Backend | AC14 | Epic 2 - Core |
| FR017: Send-ToDrive Backend | AC15 | Epic 2 - Core |
| NFR001: Performance | AC20, AC21 | Epic 2 - Validation with benchmarks |
| NFR002: In-Place Compression | Validated in Epic 1 | Revalidated in Epic 2 integration tests |
| NFR003: Idempotent/Resumable | AC18, AC19 | Epic 2 - Complete validation |

## Risks, Assumptions, Open Questions

**RISK-5: S3 Upload Failures at Scale**
- **Type:** Operational Risk
- **Description:** Large file uploads to S3 may timeout or fail due to network issues during 300K file archival operations
- **Impact:** Medium - Workflow fails for affected dates, requires retry (idempotent execution mitigates)
- **Mitigation:** Leverage AWS.Tools.S3 built-in retry logic, document timeout scenarios, user can retry workflow (check-first skips successful uploads)
- **Owner:** Epic 2 integration testing

**RISK-6: Cleanup Deletes Wrong Files**
- **Type:** Data Loss Risk
- **Description:** Bug in Remove-LogSource validation logic could delete source files before transfers complete
- **Impact:** HIGH - Permanent data loss
- **Mitigation:** Comprehensive safety testing (AC16), validation logic must be 100% covered by tests, code review focus area, consider adding -WhatIf parameter post-MVP
- **Owner:** Epic 2 implementation + testing

**RISK-7: Performance Benchmarks Don't Meet NFR001**
- **Type:** Performance Risk
- **Description:** 300K file benchmark may reveal discovery takes >90 seconds, failing to meet <5 min for 1M files target
- **Impact:** Medium - Requires performance optimization work
- **Mitigation:** If benchmark fails, profile bottlenecks (use .NET diagnostics), optimize streaming enumeration, consider parallel directory processing
- **Owner:** Epic 2 performance testing

**RISK-8: UNC Share Connectivity**
- **Type:** Infrastructure Risk
- **Description:** UNC share authentication or connectivity issues in production environments
- **Impact:** Low - User can use S3 or local destinations instead
- **Mitigation:** Document UNC share requirements, test with various network configurations, provide clear error messages for connectivity issues
- **Owner:** Epic 2 integration testing

**ASSUMPTION-5: 300K File Benchmark Represents Real Workload**
- **Description:** Test dataset with 300K files accurately represents production log file characteristics (size, count, distribution)
- **Validation:** Consult with early users about actual log file sizes and counts, adjust benchmark dataset if needed
- **Impact if Invalid:** Performance targets may not reflect real-world behavior

**ASSUMPTION-6: AWS.Tools.S3 Performance Acceptable**
- **Description:** Write-S3Object cmdlet performance is sufficient for archival workflow (no multipart upload optimization needed for MVP)
- **Validation:** Benchmark S3 upload speeds with typical archive sizes (100 MB - 1 GB files)
- **Impact if Invalid:** May need to switch to AWS SDK direct usage for multipart uploads (post-MVP)

**ASSUMPTION-7: Cleanup is All-or-Nothing**
- **Description:** Users prefer all-or-nothing cleanup (delete all sources or none) vs partial cleanup (delete only successfully transferred)
- **Validation:** Early user feedback during MVP deployment
- **Impact if Invalid:** May need to add partial cleanup mode (post-MVP enhancement)

**QUESTION-4: Should Invoke-LogArchival support parallel date processing?**
- **Status:** OPEN
- **Context:** Currently processes dates sequentially, could parallelize for faster workflows
- **Impact:** Performance improvement potential (30-50% faster for multi-date ranges)
- **Decision Needed:** Post-MVP based on performance benchmarks
- **Owner:** Epic 2 performance analysis

**QUESTION-5: Should we add WhatIf/Confirm parameters?**
- **Status:** OPEN
- **Context:** Standard PowerShell pattern for destructive operations (Remove-LogSource deletes files)
- **Impact:** Better user experience, dry-run testing capability
- **Decision Needed:** High priority post-MVP enhancement
- **Owner:** Product team after MVP deployment

**QUESTION-6: Should WorkflowResult include detailed per-date breakdown?**
- **Status:** OPEN
- **Context:** Currently aggregates all dates, could provide per-date metrics (which dates succeeded/failed, bytes per date)
- **Impact:** Better observability and troubleshooting
- **Decision Needed:** Post-MVP based on user feedback
- **Owner:** Product team after MVP deployment

## Test Strategy Summary

**Test Coverage Target: 100% for All Epic 2 Components**

**Test Levels:**

**1. Unit Tests (xUnit - C#)**
- **Scope:** All Epic 2 internal services
- **Coverage:** 100% code coverage for Distribution and Orchestration layer services
- **Key Test Suites:**
  - S3TransferServiceTests: Mock IPowerShellInvoker, verify Write-S3Object invocation, test credential passing
  - UncTransferServiceTests: Mock IFileSystem, verify File.Copy calls, test UNC path parsing
  - LocalTransferServiceTests: Mock IFileSystem, verify local file copy operations
  - CleanupServiceTests: Test validation logic (match sources to transfer confirmations), verify safety checks
  - InvokeLogArchivalCmdletTests: Test orchestration logic, parameter validation, workflow chaining

**2. Integration Tests (Pester - PowerShell)**
- **Scope:** End-to-end workflows with real infrastructure (test S3 bucket, UNC share, local drive)
- **Coverage:** All cmdlet parameter combinations, all destination types, error scenarios
- **Key Test Suites:**
  - Send-Archive-S3.Tests.ps1: Real S3 uploads using $env:AWS_Access/$env:AWS_Bucket (AC13)
  - Send-Archive-UNC.Tests.ps1: Real UNC transfers using $env:UNC_Drive (AC14)
  - Send-Archive-Local.Tests.ps1: Real local transfers using $env:Local_Drive (AC15)
  - Remove-LogSource.Tests.ps1: Cleanup safety validation (AC16)
  - Invoke-LogArchival-EndToEnd.Tests.ps1: Complete workflow S3 destination (AC17)
  - Idempotent-Execution.Tests.ps1: Retry scenarios, verify check-first skips archived dates (AC18)
  - Partial-Failure-Handling.Tests.ps1: Mock S3 failures, test partial success (AC19)

**3. Performance Benchmarks (Pester - Performance Tests)**
- **300K File Benchmark (AC20):**
  ```powershell
  Describe "Performance: 300K File Discovery and Archival" {
      BeforeAll {
          # Generate 300,000 test files across 10 dated folders
          1..10 | ForEach-Object {
              $folder = "D:\TEST_DATA\PerfTest\2025100$_"
              New-Item $folder -ItemType Directory -Force
              1..30000 | ForEach-Object {
                  "test" | Out-File "$folder\file_$_.log"
              }
          }
      }

      It "Discovery completes in <90 seconds" {
          $stopwatch = [Diagnostics.Stopwatch]::StartNew()
          $result = Find-LogFolders -Path "D:\TEST_DATA\PerfTest" -DateRange (Get-DateRange -olderThan 10)
          $stopwatch.Stop()
          $stopwatch.Elapsed.TotalSeconds | Should -BeLessThan 90
      }

      It "Full workflow completes in <10 minutes" {
          $stopwatch = [Diagnostics.Stopwatch]::StartNew()
          Invoke-LogArchival -Path "D:\TEST_DATA\PerfTest" -olderThan 10 `
              -AppName "PerfTest" -Destination "$env:Local_Drive\Archives" `
              -Compress -DeleteSource
          $stopwatch.Stop()
          $stopwatch.Elapsed.TotalMinutes | Should -BeLessThan 10
      }
  }
  ```

- **Compression Throughput Benchmark (AC21):**
  - Create 1 GB test folder with representative log files
  - Measure 7-Zip: compression time, archive size, throughput (GB/min)
  - Measure SharpCompress: same metrics
  - Document comparison: "7-Zip: 5 GB/min, 80% ratio vs SharpCompress: 3 GB/min, 75% ratio"

**4. Edge Case Tests**
- **Transfer Failures:**
  - S3 bucket doesn't exist → verify TransferException
  - UNC share unreachable → verify TransferException
  - Disk full on destination → verify TransferException
  - Network interruption mid-transfer → verify failure handling
- **Cleanup Failures:**
  - File in use during cleanup → verify CleanupException
  - Permission denied → verify CleanupException
  - Partial transfer success (3 succeed, 1 fail) → verify only 3 sources deleted
- **Orchestration Edge Cases:**
  - All dates already archived (SkippedDates = all dates)
  - No files found matching criteria (WorkflowResult shows 0 processed)
  - Compression disabled but no raw file transfer support → verify behavior

**5. Regression Testing**
- All Epic 1 + Epic 2 tests run on every PR
- GitHub Actions CI matrix: Windows + Linux, PowerShell 7.2/7.3/7.4
- Performance regression detection: Compare benchmark results to baseline

**Success Criteria:**
- All unit tests pass (100% coverage for Epic 2 components)
- All integration tests pass across S3/UNC/Local destinations
- Performance benchmarks meet NFR001 targets (300K files <90s discovery, extrapolates to <5 min for 1M)
- Idempotent execution validated (retry test passes)
- No regressions from Epic 1 functionality
- CI/CD pipeline green on all platforms
