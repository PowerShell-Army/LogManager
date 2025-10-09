# logManager - Epic Breakdown

**Author:** Adam
**Date:** 2025-10-08
**Project Level:** Level 2 (Small complete system)
**Target Scale:** 12-15 stories, 2 epics, 2-3 week MVP timeline

---

## Epic Overview

This project delivers a high-performance PowerShell module for log archival with 8 atomic functions organized in dependency layers. Work is split into 2 epics following the dependency graph:

**Epic 1** builds Foundation → Discovery → Processing layers (12 stories)
**Epic 2** builds Distribution → Orchestration layers plus performance optimization (12 stories)

Implementation follows strict dependency order to ensure each layer builds on stable foundations.

---

## Epic Details

### Epic 1: Core Archival Functions (Foundation + Discovery + Processing Layers)

**Epic Goal:** Implement the 8 core atomic functions with proper dependency layering, enabling basic archival workflows

**Epic Scope:** Foundation layer (FR001, FR009, FR010, FR012, FR013), Discovery layer (FR002, FR003, FR004, FR014), Processing layer (FR005, FR011)

**Success Criteria:**
- All Foundation, Discovery, and Processing layer functions implemented
- Unit tests passing for all functions
- Functions can be composed via PowerShell pipeline
- Compression works in-place with 7-Zip integration
- Check-first workflow prevents redundant file scanning

---

#### Story 1.1: Implement Get-DateRange Function (FR001)

**As a** DevOps engineer
**I want** to calculate date ranges from simple integer day parameters
**So that** I can define archival windows without dealing with complex date objects

**Acceptance Criteria:**
- Function accepts `olderThan` and `youngerThan` integer parameters (days)
- Returns array of DateTime objects representing each day in range
- Supports `DateCriteria` parameter: "CreationDate" or "ModifiedDate"
- Validates parameters (olderThan must be >= youngerThan)
- Returns PowerShell objects compatible with pipeline chaining
- Unit tests cover edge cases (same day, 1 day range, 30+ day range)

**Technical Notes:**
- Foundation layer - no dependencies
- Must follow <500 line constraint
- Return objects must include: Date, DateCriteria properties

---

#### Story 1.2: Implement Path Token Resolution Engine (FR013)

**As a** system function
**I want** to resolve path tokens independently of transfer operations
**So that** both Test-ArchiveExists and Send-Archive can use consistent destination paths

**Acceptance Criteria:**
- Resolves tokens: `{year}`, `{month}`, `{day}`, `{date}`, `{server}`, `{app}`
- Uses log date being archived, NOT current date
- Accepts template string and date/context parameters
- Returns fully resolved path string
- Handles UNC paths (\\\\server\\share), S3 paths (s3://bucket/key), local paths (D:\\path)
- Unit tests validate token replacement accuracy

**Technical Notes:**
- Foundation layer - no dependencies
- Critical for FR002 and FR006
- Must be reusable utility function

---

#### Story 1.3: Implement Archive Naming Pattern Logic (FR010)

**As a** system function
**I want** to generate consistent archive filenames
**So that** all functions use the same naming convention

**Acceptance Criteria:**
- Generates pattern: `AppName-YYYYMMDD.zip`
- Accepts application name and date parameters
- Returns filename string
- Validates application name (no invalid filename characters)
- Unit tests validate naming consistency

**Technical Notes:**
- Foundation layer - no dependencies
- Used by FR002, FR005, FR006

---

#### Story 1.4: Implement PowerShell Pipeline Composition Pattern (FR009)

**As a** PowerShell module developer
**I want** all functions to return pipeline-compatible objects
**So that** users can chain functions in custom workflows

**Acceptance Criteria:**
- Define return object schemas for each function type
- All functions output PSCustomObject or equivalent
- Objects include essential properties for next pipeline stage
- Pipeline chaining validated with example workflows
- Documentation shows pipeline composition examples

**Technical Notes:**
- Foundation layer framework
- Cross-cutting concern for all functions

---

#### Story 1.5: Implement Fail-Fast Error Handling Framework (FR012)

**As a** PowerShell module developer
**I want** functions to throw standard PowerShell exceptions on failures
**So that** calling scripts can implement their own error recovery

**Acceptance Criteria:**
- All functions use standard PowerShell error handling
- Throw terminating errors for failures (no silent failures)
- Error messages are clear and actionable
- No built-in retry logic (caller's responsibility)
- Unit tests validate error conditions trigger exceptions

**Technical Notes:**
- Foundation layer framework
- Cross-cutting concern for all functions
- MVP discipline: No complex error recovery

---

#### Story 1.6: Implement Find-LogFiles Function (FR003)

**As a** DevOps engineer
**I want** to locate individual log files matching date criteria
**So that** I can archive file-based logs efficiently

**Acceptance Criteria:**
- Accepts Path, olderThan, youngerThan parameters
- Accepts DateCriteria: "CreationDate" or "ModifiedDate"
- Supports Recurse parameter for subdirectories
- Returns collection of FileInfo objects with dates
- Filters files based on FR001 date range
- Performance tested with 100K+ files
- Unit tests validate filtering accuracy

**Technical Notes:**
- Discovery layer - depends on FR001
- Must handle massive file counts efficiently
- Returns pipeline-compatible objects (FR009)

---

#### Story 1.7: Implement Find-LogFolders Function (FR004)

**As a** DevOps engineer
**I want** to locate dated folders matching criteria
**So that** I can archive folder-based logs as complete units

**Acceptance Criteria:**
- Accepts Path, olderThan, youngerThan parameters
- Detects folder naming: YYYYMMDD or YYYY-MM-DD formats
- Returns collection of DirectoryInfo objects with parsed dates
- Filters folders based on FR001 date range
- Treats entire folder as archival unit
- Unit tests validate folder name parsing

**Technical Notes:**
- Discovery layer - depends on FR001
- Distinct from FR003 for different use cases

---

#### Story 1.8: Implement Archive Existence Query Protocol (FR014)

**As a** system function
**I want** consistent archive checking across all destination types
**So that** Test-ArchiveExists can work with S3/UNC/local uniformly

**Acceptance Criteria:**
- Provides abstraction for checking archive existence
- Supports S3 (AWS SDK), UNC paths (.NET), local drives (.NET)
- Accepts resolved path from FR013 and archive name from FR010
- Returns boolean: archive exists or not
- Handles authentication for S3 (IAM credentials)
- Unit tests with mocked destinations

**Technical Notes:**
- Discovery layer - depends on FR010, FR013
- Required by FR002
- Multi-destination abstraction layer

---

#### Story 1.9: Implement Test-ArchiveExists Check Function (FR002)

**As a** DevOps engineer
**I want** to query destinations for existing archives before processing
**So that** I can skip already-archived dates and save hours of redundant work

**Acceptance Criteria:**
- Accepts Destination (with tokens), olderThan, youngerThan, AppName parameters
- Uses FR001 to generate date range
- Uses FR013 to resolve destination paths for each date
- Uses FR014 to check archive existence
- Returns filtered list of dates NOT yet archived
- Supports S3, UNC, and local destinations
- Unit tests with various destination types

**Technical Notes:**
- Discovery layer - depends on FR001, FR010, FR013, FR014
- Critical for check-first workflow performance
- Enables FR011 idempotent execution

---

#### Story 1.10: Implement Compress-Logs Function (FR005)

**As a** DevOps engineer
**I want** to compress log files/folders into .zip archives using 7-Zip
**So that** I can reduce storage costs and organize archives

**Acceptance Criteria:**
- Accepts input files/folders from FR003/FR004
- Integrates with 7-Zip (CLI or library)
- Creates .zip archives on source drive (in-place, NFR002)
- Uses naming from FR010
- Accepts optional Compress parameter (compression not always required)
- Returns archive FileInfo objects
- Validates NFR002: Never copies to temp location
- Performance tested with 300K+ file dataset

**Technical Notes:**
- Processing layer - depends on FR003/FR004, FR010
- In-place compression constraint is critical
- 7-Zip integration research needed

---

#### Story 1.11: Implement Idempotent Execution Support (FR011)

**As a** DevOps engineer
**I want** to safely retry archival operations without duplicate work
**So that** failed runs can resume from last checkpoint

**Acceptance Criteria:**
- Integration with FR002 check-first workflow
- Previously archived dates are skipped on retry
- No duplicate archives created
- Integration tests validate resumable behavior
- Documentation shows retry scenarios

**Technical Notes:**
- Processing layer - enabled by FR002
- Cross-functional requirement spanning multiple functions

---

#### Story 1.12: Write Unit Tests for Foundation and Discovery Layer Functions

**As a** developer
**I want** comprehensive unit tests for all Foundation and Discovery functions
**So that** I can ensure quality and prevent regressions

**Acceptance Criteria:**
- Unit tests for FR001, FR009, FR010, FR012, FR013
- Unit tests for FR002, FR003, FR004, FR014
- Code coverage >80% for these functions
- Tests validate edge cases and error conditions
- Tests run in CI/CD pipeline

**Technical Notes:**
- Testing framework: Pester (PowerShell standard)
- Mock external dependencies (file system, S3)

---

### Epic 2: Distribution, Orchestration & Performance Optimization

**Epic Goal:** Complete the archival workflow with multi-destination transfer, cleanup, orchestration, and massive-scale performance optimization

**Epic Scope:** Distribution layer (FR006, FR007), Orchestration layer (FR008), Performance requirements (NFR001, NFR002, NFR003)

**Success Criteria:**
- All 8 core functions fully implemented
- Orchestrator function chains complete workflow
- Performance validated at 300K file scale
- Integration tests passing for end-to-end workflows
- Module documentation complete

---

#### Story 2.1: Implement Send-ToS3 Backend Function (FR015)

**As a** backend transfer function
**I want** to transfer archives to AWS S3 storage
**So that** Send-Archive dispatcher can route S3 destinations to me

**Acceptance Criteria:**
- Accepts archive file path and S3 destination parameters
- Uses AWS SDK for .NET
- Supports IAM credentials (default)
- Supports explicit access key/secret key parameters
- Uses FR013 for path token resolution
- Uploads to S3 with resolved key paths
- Returns success/failure status with details
- Unit tests with S3 mock

**Technical Notes:**
- Distribution layer backend - depends on FR005, FR013
- AWS SDK integration required
- Multipart upload for large files (future enhancement)
- Single responsibility: S3 transfers only

**Testing Configuration (MANDATORY):**
For LTS testing, use these non-mock environment variables ONLY:
```powershell
$env:AWS_Access = ''
$env:AWS_Secret = ''
$env:AWS_Region = ''
$env:AWS_Bucket = ''
```
**NO OTHER S3 LOCATION CAN BE USED FOR TESTING.**

---

#### Story 2.2: Implement Send-ToUNC Backend Function (FR016)

**As a** backend transfer function
**I want** to transfer archives to UNC network shares
**So that** Send-Archive dispatcher can route UNC destinations to me

**Acceptance Criteria:**
- Accepts archive file path and UNC destination parameters
- Uses .NET file I/O for UNC paths (\\\\server\\share)
- Uses FR013 for path token resolution
- Handles network share authentication
- Copies to UNC destination
- Returns success/failure status with details
- Unit tests with UNC mock

**Technical Notes:**
- Distribution layer backend - depends on FR005, FR013
- Network share authentication handling
- Single responsibility: UNC transfers only

**Testing Configuration (MANDATORY):**
For LTS testing, use this non-mock environment variable ONLY:
```powershell
$env:UNC_Drive = '\\10.0.10.10\storage\TEST_DATA'
```
**NO OTHER UNC LOCATION CAN BE USED FOR TESTING.**

---

#### Story 2.3: Implement Send-ToDrive Backend Function (FR017)

**As a** backend transfer function
**I want** to transfer archives to local/external drives
**So that** Send-Archive dispatcher can route local destinations to me

**Acceptance Criteria:**
- Accepts archive file path and local destination parameters
- Uses .NET file I/O for local paths
- Uses FR013 for path token resolution
- Copies/moves to local destination
- Returns success/failure status with details
- Unit tests validate local transfer

**Technical Notes:**
- Distribution layer backend - depends on FR005, FR013
- Simplest destination type
- Single responsibility: local drive transfers only

**Testing Configuration (MANDATORY):**
For LTS testing, use this non-mock environment variable ONLY:
```powershell
$env:Local_Drive = 'D:\TEST_DATA'
```
**NO OTHER LOCAL LOCATION CAN BE USED FOR TESTING.**

---

#### Story 2.4: Implement Send-Archive Dispatcher Function (FR006)

**As a** DevOps engineer
**I want** a unified Send-Archive function that routes to the appropriate backend
**So that** I don't have to manually determine which backend function to call

**Acceptance Criteria:**
- Accepts archive files from FR005 and destination parameter
- Analyzes destination path to determine type:
  - s3:// → routes to Send-ToS3 (FR015)
  - \\\\ → routes to Send-ToUNC (FR016)
  - Local path (D:\\, C:\\, etc.) → routes to Send-ToDrive (FR017)
- Passes through relevant parameters to backend functions
- Returns success/failure status from backend (for FR007)
- Returns explicit success confirmation signal for cleanup
- Unit tests validate routing logic for all destination types

**Technical Notes:**
- Distribution layer dispatcher - depends on FR015, FR016, FR017
- Path pattern recognition for routing
- Critical for FR007 cleanup safety
- Single entry point for all transfer operations

---

#### Story 2.5: Implement Remove-LogSource Cleanup Function (FR007)

**As a** DevOps engineer
**I want** to optionally delete source files/folders after successful archival
**So that** I can free up disk space automatically

**Acceptance Criteria:**
- Accepts DeleteSource boolean parameter
- Only deletes after FR006 success confirmation
- Deletes original files/folders that were archived
- Validates deletion completed successfully
- Fail-safe: Never deletes without confirmed transfer
- Unit tests validate safety logic

**Technical Notes:**
- Distribution layer - depends on FR006 success
- Safety-critical function
- Must be conservative (fail-safe)

---

#### Story 2.6: Implement Invoke-LogArchival Orchestrator Function (FR008)

**As a** DevOps engineer
**I want** an all-in-one orchestrator function
**So that** I can run complete archival workflows with a single command

**Acceptance Criteria:**
- Chains: FR001 → FR002 → FR003/FR004 → FR005 → FR006 → FR007
- Accepts all relevant parameters for each step
- Supports SkipCheck parameter to bypass FR002
- Supports Compress parameter (optional compression)
- Supports DeleteSource parameter
- Returns workflow execution summary
- Integration tests validate complete workflow
- Documentation shows usage examples

**Technical Notes:**
- Orchestration layer - depends on all previous functions
- Convenience wrapper for common workflows
- Must support both individual function and orchestrator patterns

---

#### Story 2.7: Optimize File Enumeration for 1M+ File Scale (NFR001)

**As a** developer
**I want** efficient file enumeration at massive scale
**So that** the module can handle 300K-1M files per day without overwhelming resources

**Acceptance Criteria:**
- Find-LogFiles scans 1M files in <5 minutes (NFR001 target)
- Uses .NET parallel enumeration where appropriate
- Efficient filtering to minimize memory usage
- Performance profiling identifies bottlenecks
- Benchmark tests with realistic datasets

**Technical Notes:**
- Performance optimization for FR003/FR004
- May require .NET DirectoryInfo.EnumerateFiles optimizations
- Target: <5 min for 1M file scan (NFR001)

---

#### Story 2.8: Validate In-Place Compression Constraint Enforcement (NFR002)

**As a** developer
**I want** to ensure compression never uses temp locations
**So that** the module doesn't double disk I/O or storage requirements

**Acceptance Criteria:**
- Compress-Logs verified to compress on source drive only
- Code review confirms no temp file creation
- Integration tests monitor file I/O locations
- Documentation explicitly states constraint

**Technical Notes:**
- Validation of NFR002 constraint
- Critical for handling massive file counts

---

#### Story 2.9: Test Idempotent Execution and Resumability (NFR003)

**As a** DevOps engineer
**I want** to validate safe retry behavior
**So that** I can confidently re-run failed archival operations

**Acceptance Criteria:**
- Integration tests simulate partial failures
- Re-running after failure skips already-archived dates
- No duplicate archives created
- Data integrity maintained across retries
- Documentation shows retry scenarios

**Technical Notes:**
- Validation of NFR003
- Integration testing for FR011

---

#### Story 2.10: Write Integration Tests for Complete Archival Workflows

**As a** developer
**I want** end-to-end integration tests
**So that** I can validate complete workflows work correctly

**Acceptance Criteria:**
- Integration tests for full orchestrator workflow
- Tests cover S3, UNC, local destinations
- Tests validate check-first workflow
- Tests validate compression and transfer
- Tests validate optional cleanup
- Tests run in CI/CD pipeline

**Technical Notes:**
- End-to-end testing
- Use environment variables for real LTS destination testing (see Story 2.1-2.3)
- Mock external dependencies for unit tests only

**Testing Configuration (MANDATORY):**
For LTS integration testing, use these non-mock environment variables ONLY:
```powershell
# AWS S3 Configuration
$env:AWS_Access = ''
$env:AWS_Secret = ''
$env:AWS_Region = ''
$env:AWS_Bucket = ''

# Local and Network Storage Configuration
$env:Local_Drive = 'D:\TEST_DATA'
$env:UNC_Drive = '\\10.0.10.10\storage\TEST_DATA'
```
**NO OTHER LOCATIONS CAN BE USED FOR INTEGRATION TESTING.**

---

#### Story 2.11: Performance Benchmark with 300K File Dataset

**As a** developer
**I want** to benchmark with realistic scale
**So that** I can validate NFR001 performance targets

**Acceptance Criteria:**
- Create 300K file test dataset
- Measure end-to-end archival time
- Identify performance bottlenecks
- Validate <5 min scan time for 1M files (extrapolate from 300K)
- Document performance characteristics

**Technical Notes:**
- Performance validation
- NFR001 verification
- May reveal optimization opportunities

---

#### Story 2.12: Create Module Documentation and Usage Examples

**As a** DevOps engineer
**I want** comprehensive documentation
**So that** I can understand how to use the module effectively

**Acceptance Criteria:**
- README with quick start guide
- Function reference documentation
- Usage examples for common scenarios
- Pipeline composition examples
- Troubleshooting guide
- Performance tuning tips

**Technical Notes:**
- User-facing documentation
- Examples should match user journey from PRD

---

## Implementation Dependencies

**Epic 1 must complete before Epic 2** due to dependency graph:
- Epic 2 functions (FR006, FR007, FR008) depend on Epic 1 functions (FR001-FR005, FR011)

**Within Epic 1, implementation order:**
1. Stories 1.1-1.5: Foundation layer (parallel work possible)
2. Stories 1.6-1.9: Discovery layer (depends on Foundation)
3. Stories 1.10-1.11: Processing layer (depends on Discovery)
4. Story 1.12: Unit tests (can run in parallel with implementation)

**Within Epic 2, implementation order:**
1. Stories 2.1-2.4: Send-Archive variations (can be parallel)
2. Story 2.5: Remove-LogSource (depends on 2.4 confirmation)
3. Story 2.6: Orchestrator (depends on all previous)
4. Stories 2.7-2.12: Optimization and validation (parallel possible)

---

_This epic breakdown follows the dependency graph established in the PRD functional requirements section._
