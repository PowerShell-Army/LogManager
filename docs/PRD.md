# logManager Product Requirements Document (PRD)

**Author:** Adam
**Date:** 2025-10-08
**Project Level:** Level 2 (Small complete system)
**Project Type:** Library/package (PowerShell module)
**Target Scale:** 5-15 stories, 1-2 epics, 2-3 week MVP timeline

---

## Description, Context and Goals

### Description

**logManager** is a high-performance PowerShell module for log file archival built in C#/.NET. The module provides 8 atomic functions designed to handle massive-scale log archival operations (300K-1M files per day) for custom in-house developed applications.

**Core Capabilities:**

The module follows a modular architecture with two usage patterns:
1. **Individual atomic functions** for granular control and custom pipeline workflows
2. **Orchestrator convenience function** for all-in-one archival operations

**8 Core Functions:**
- **Get-DateRange** - Calculate date ranges from day-based parameters (olderThan/youngerThan)
- **Test-ArchiveExists** - Query destinations for existing archives to avoid reprocessing (check-first workflow)
- **Find-LogFiles** - Locate individual files matching date criteria
- **Find-LogFolders** - Locate folders matching date criteria (YYYYMMDD or YYYY-MM-DD format)
- **Compress-Logs** - Create .zip archives using 7-Zip compression (optional, in-place compression only)
- **Send-Archive** - Dispatcher function that routes to appropriate backend (Send-ToS3, Send-ToUNC, Send-ToDrive) based on destination path, with path token replacement
- **Remove-LogSource** - Remove source files/folders after successful archival (optional cleanup)
- **Invoke-LogArchival** - Orchestrator function chaining all operations with configurable steps

**Backend Transfer Functions** (called by Send-Archive):
- **Send-ToS3** - AWS S3 transfer using AWS SDK for .NET
- **Send-ToUNC** - UNC network share transfer using .NET file I/O
- **Send-ToDrive** - Local drive transfer using .NET file I/O

**Key Features:**
- **Multi-destination support:** AWS S3 (IAM credentials default + explicit keys), UNC network paths, local drives
- **Path token replacement system:** `{year}`, `{month}`, `{day}`, `{server}`, `{app}` for flexible storage organization
- **Performance at scale:** Check-first workflow, in-place compression, date-based grouping, idempotent execution
- **MVP discipline (KISS principle):** No built-in logging, metrics, monitoring, scheduling, or complex error recovery - fail-fast with PowerShell exceptions

**Architecture Constraints:**
- Each function < 500 lines of code
- Single responsibility per function
- Pipeline chaining via return objects
- In-place compression on source drive (no temp copy overhead)
- .zip format only

### Deployment Intent

**MVP for early users** - Minimum viable product for initial internal teams to validate the approach and gather real-world feedback. Focus on core archival capabilities that work reliably at scale, with opportunity to enhance based on actual usage patterns and team needs.

### Context

Custom in-house applications generate massive volumes of log files daily (300K-1M files per day), consuming critical disk space on production servers. Without automated archival, operations teams face storage exhaustion, manual cleanup burden, and risk of data loss. Existing solutions either lack the performance to handle this scale efficiently or require complex infrastructure that's overkill for internal use cases. The logManager module addresses this gap by providing a purpose-built, high-performance PowerShell toolset that integrates seamlessly with existing DevOps workflows, enabling automated archival to S3 or network storage while avoiding redundant processing of already-archived data through intelligent check-first workflows.

### Goals

**Primary Goals:**

1. **Validate automated log archival approach at scale** - Deliver MVP tooling to archive 300K-1M log files per day with early internal teams, proving the check-first workflow and in-place compression architecture can handle real-world scale efficiently

2. **Reduce storage costs through intelligent archival** - Compress and transfer old logs to cost-effective long-term storage (S3, UNC network shares, or archive drives) with path token organization, gathering feedback on storage patterns and token needs from actual usage

3. **Provide flexible, composable MVP tooling** - Deliver 8 core atomic functions that work independently AND an orchestrator function for convenience, enabling early users to experiment with both custom workflows and all-in-one automation while identifying missing capabilities

## Requirements

### Functional Requirements

**Foundation Layer** (no dependencies):

**FR001: Date Range Calculation** - System must calculate date ranges from integer day parameters (olderThan/youngerThan) to define archival windows, supporting both creation date and last modified date criteria

**FR009: Pipeline Composition** - Each atomic function must return objects compatible with PowerShell pipeline chaining, enabling custom workflows beyond the orchestrator

**FR010: Application-Based Archive Naming** - System must generate archive files with pattern AppName-YYYYMMDD.zip using application name parameter for organization

**FR012: Fail-Fast Error Handling** - System must throw standard PowerShell exceptions on failures, allowing calling scripts to implement custom error recovery without built-in retry logic

**FR013: Path Token Resolution Engine** - System must resolve path tokens ({year}, {month}, {day}, {server}, {app}) independently of transfer operations, using log date being archived (not current date), to enable both archive checking and transfer using consistent destination paths

**Discovery Layer** (depends on Foundation):

**FR003: File Discovery with Date Filtering** - System must locate individual log files matching date criteria (olderThan/youngerThan window from FR001) with optional recursive subdirectory scanning

**FR004: Folder Discovery with Date Filtering** - System must locate dated folders (YYYYMMDD or YYYY-MM-DD format) matching date criteria (from FR001), treating entire folder as archival unit

**FR014: Archive Existence Query Protocol** - System must provide consistent archive existence checking across all destination types (S3/UNC/local) using naming pattern from FR010 and resolved paths from FR013

**FR002: Pre-Flight Archive Check** - System must query destination storage (S3/UNC/local) for existing archives using FR014 protocol and return filtered date list to avoid re-processing already-archived data, enabling FR011 idempotent execution

**Processing Layer** (depends on Discovery):

**FR005: In-Place Compression** - System must compress log files/folders (from FR003/FR004) into .zip archives using 7-Zip on the source drive without copying to temporary locations, with optional compression (not required for all workflows), applying naming from FR010

**FR011: Idempotent Execution** - System must support safe retry of archival operations without duplicate work, leveraging FR002 check-first workflow to skip already-processed dates before invoking FR003/FR004 discovery

**Distribution Layer** (depends on Processing):

**FR015: Send-ToS3 Backend Function** - System must provide backend function to transfer archives to AWS S3 using AWS SDK for .NET, supporting both IAM credentials (default) and explicit access key/secret key, with path token resolution from FR013

**FR016: Send-ToUNC Backend Function** - System must provide backend function to transfer archives to UNC network paths using .NET file I/O, with path token resolution from FR013 and network share authentication

**FR017: Send-ToDrive Backend Function** - System must provide backend function to transfer archives to local drives using .NET file I/O, with path token resolution from FR013

**FR006: Send-Archive Dispatcher Function** - System must provide dispatcher function that analyzes destination path and routes to appropriate backend function (FR015 for s3://, FR016 for \\\\, FR017 for local paths), returning explicit success confirmation signal for FR007

**FR007: Source Cleanup** - System must provide optional deletion of source files/folders after successful archive transfer confirmation from FR006, with configurable DeleteSource parameter

**Orchestration Layer** (depends on all layers):

**FR008: Orchestrated Workflow** - System must provide all-in-one orchestrator function (Invoke-LogArchival) that chains: FR001 date calculation → FR002 check → FR003/FR004 find → FR005 compress → FR006 transfer → FR007 cleanup, with configurable step inclusion and skip logic

### Non-Functional Requirements

**NFR001: Massive-Scale Performance** - System must efficiently handle 300K-1M files per day without overwhelming system resources. File enumeration operations must complete within acceptable timeframes (target: <5 minutes for 1M file scan). Check-first workflow must prevent redundant processing of already-archived dates.

**NFR002: In-Place Compression Constraint** - System must compress files/folders on source drive only, never copying to temporary locations regardless of available temp space. This constraint is critical for handling massive file counts without doubling disk I/O or storage requirements.

**NFR003: Idempotent and Resumable** - All operations must be safely retryable without duplicate work or data corruption. Failed archival runs must be resumable from last successful checkpoint. Check-first workflow ensures previously archived dates are skipped on retry.

**NFR004: Code Maintainability** - Each function must be <500 lines of code with single responsibility. Functions must be independently testable and composable via PowerShell pipeline. Code must follow PowerShell best practices for module development.

**NFR005: CLI Usability** - Functions must accept simple, intuitive parameters suitable for manual command-line use. Date parameters must use integer days (olderThan/youngerThan) rather than complex date objects. Error messages must be actionable and clear for operations teams.

## User Journeys

### Primary Journey: DevOps Engineer Automates Daily Log Archival

**Actor:** DevOps Engineer (Sarah)
**Goal:** Set up automated nightly archival of application logs older than 7 days to S3 storage

**Journey:**

1. **Initial Setup**
   - Sarah reviews her application's log output: 300K+ files accumulating daily in `D:\Logs\MyApp\YYYYMMDD\` folders
   - Disk space alerts indicate she needs to archive logs older than 7 days
   - She needs logs organized in S3 as: `s3://archive-bucket/logs/{year}/{month}/MyApp-{date}.zip`

2. **Testing with Individual Functions**
   - Sarah first tests discovery: `Find-LogFolders -Path "D:\Logs\MyApp" -olderThan 7` to see what would be archived
   - She validates the date range looks correct before proceeding
   - She tests the check function: `Test-ArchiveExists -Destination "s3://archive-bucket/logs/{year}/{month}/MyApp-{date}.zip" -olderThan 7` to see which dates are already archived
   - She's relieved to see it returns a filtered list excluding already-processed dates

3. **First Manual Run**
   - Sarah runs the orchestrator for a small test: `Invoke-LogArchival -Path "D:\Logs\MyApp" -olderThan 8 -youngerThan 7 -AppName "MyApp" -Destination "s3://archive-bucket/logs/{year}/{month}/MyApp-{date}.zip" -SkipCheck`
   - This archives just 1 day (day 8) as a proof of concept
   - She verifies the .zip appears in S3 with correct path: `s3://archive-bucket/logs/2025/09/MyApp-20250930.zip`

4. **Production Automation**
   - Confident in the approach, Sarah creates a scheduled task that runs nightly:
   - `Invoke-LogArchival -Path "D:\Logs\MyApp" -olderThan 7 -AppName "MyApp" -Destination "s3://archive-bucket/logs/{year}/{month}/MyApp-{date}.zip" -DeleteSource -Compress`
   - The check-first workflow prevents re-processing already archived dates
   - If the script fails one night, she can re-run it the next day without duplicate work (idempotent execution)

5. **Monitoring Results**
   - Sarah checks disk space after 1 week - freed up 50GB from source drive
   - She verifies S3 has organized archives: `logs/2025/09/`, `logs/2025/10/` folders with daily .zip files
   - Operations team can easily find and restore specific dates when needed
   - Sarah shares the approach with other DevOps engineers managing different applications

## Epics

### Epic 1: Core Archival Functions (Foundation + Discovery + Processing Layers)

**Goal:** Implement the 8 core atomic functions with proper dependency layering, enabling basic archival workflows

**Scope:** Foundation layer (FR001, FR009, FR010, FR012, FR013), Discovery layer (FR002, FR003, FR004, FR014), Processing layer (FR005, FR011)

**Stories:**
1. Implement Get-DateRange function (FR001)
2. Implement path token resolution engine (FR013)
3. Implement archive naming pattern logic (FR010)
4. Implement PowerShell pipeline composition pattern (FR009)
5. Implement fail-fast error handling framework (FR012)
6. Implement Find-LogFiles with date filtering (FR003)
7. Implement Find-LogFolders with date filtering (FR004)
8. Implement archive existence query protocol (FR014)
9. Implement Test-ArchiveExists check function (FR002)
10. Implement Compress-Logs with 7-Zip integration (FR005)
11. Implement idempotent execution support (FR011)
12. Write unit tests for all Foundation and Discovery layer functions

**Estimated Stories:** 12

### Epic 2: Distribution, Orchestration & Performance Optimization

**Goal:** Complete the archival workflow with multi-destination transfer, cleanup, orchestration, and massive-scale performance optimization

**Scope:** Distribution layer (FR006, FR007), Orchestration layer (FR008), Performance requirements (NFR001, NFR002, NFR003)

**Stories:**
1. Implement Send-Archive with S3 support (IAM credentials)
2. Implement Send-Archive with UNC network path support
3. Implement Send-Archive with local drive support
4. Implement transfer success confirmation signaling (FR006)
5. Implement Remove-LogSource cleanup function (FR007)
6. Implement Invoke-LogArchival orchestrator function (FR008)
7. Optimize file enumeration for 1M+ file scale (NFR001)
8. Validate in-place compression constraint enforcement (NFR002)
9. Test idempotent execution and resumability (NFR003)
10. Write integration tests for complete archival workflows
11. Performance benchmark with 300K file dataset
12. Create module documentation and usage examples

**Estimated Stories:** 12

**Total Estimated Stories:** 12-15 stories across 2 epics

_Detailed story breakdown with acceptance criteria available in epic-stories.md_

## Assumptions and Dependencies

### External Dependencies

**Required Software:**
- 7-Zip (CLI or library) - For .zip compression functionality
- AWS SDK for .NET - For S3 transfer operations
- PowerShell SDK - For C# PowerShell module development
- .NET Runtime - Target version TBD in solution architecture

**Required Access:**
- AWS account with S3 bucket access
- IAM credentials or explicit access key/secret key for S3 operations
- UNC network share access (for testing and UNC destination support)
- Local drive access for testing

### Testing Environment Configuration (MANDATORY)

**CRITICAL:** All LTS (Long-Term Storage) testing MUST use the following environment variables. NO OTHER LOCATIONS CAN BE USED.

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

**Testing Constraints:**
- S3 testing: Use only the configured AWS bucket via environment variables
- UNC testing: Use only `\\10.0.10.10\storage\TEST_DATA`
- Local testing: Use only `D:\TEST_DATA`
- No mock destinations for integration testing (use real LTS destinations above)
- Mock destinations allowed for unit tests only

### User Setup Requirements

**Before Development:**
- [ ] AWS account created/configured with S3 access
- [ ] IAM user or role configured with S3 permissions
- [ ] S3 bucket created and tested (use bucket specified in $env:AWS_Bucket)
- [ ] 7-Zip installed locally for development/testing
- [ ] Network share `\\10.0.10.10\storage\TEST_DATA` accessible
- [ ] Local test directory `D:\TEST_DATA` created
- [ ] Environment variables configured in PowerShell profile

### Technical Assumptions

**Deferred to Solution Architecture:**
- 7-Zip integration approach (CLI vs library)
- .NET version target
- Credential storage mechanism
- CI/CD pipeline design
- Error logging strategy

**MVP Constraints:**
- .zip format only (no other compression formats)
- Fail-fast error handling (no complex retry logic)
- No built-in observability (external monitoring required)

## Out of Scope

The following features are explicitly excluded from the MVP to maintain focus on core archival capabilities:

**Built-in Observability:**
- No built-in logging, metrics, or monitoring
- No progress bars for long operations (future enhancement)
- No built-in alerting or notifications

**Advanced Features:**
- No automatic scheduling (users implement via Task Scheduler/cron)
- No complex error recovery or automatic retry logic (fail-fast only)
- No archive restoration/extraction capabilities
- No compression level options (uses 7-Zip defaults)
- No parallel processing for multiple days simultaneously

**Security Features:**
- No encryption of archives (can be added post-MVP)
- No built-in credentials management beyond IAM/explicit keys
- No audit trail or compliance logging

**File Organization:**
- No Sort-LogFiles function to organize files into dated folders (post-MVP)
- No alternative folder naming patterns beyond YYYYMMDD and YYYY-MM-DD

**User Interface:**
- No WhatIf/Confirm parameters for dry-run testing (future)
- No date range expression shortcuts ("Last 30 days", "This week")

**Performance Optimizations:**
- No multipart upload for large S3 files (future enhancement)
- No compression streaming optimizations (uses 7-Zip standard behavior)

These features may be considered for future releases based on early user feedback and actual usage patterns.

---

## Next Steps

### Immediate Action: Solution Architecture and Technical Design

Since this is a **Level 2** project, you need detailed technical design and architecture before implementation.

**Start a new session and provide:**
1. This PRD: `D:\projects\logManager\docs\PRD.md`
2. Epic structure: `D:\projects\logManager\docs\epic-stories.md`
3. Brainstorming results: `D:\projects\logManager\docs\brainstorming-session-results-2025-10-08.md`

**Request the solutioning workflow to:**
- Run `3-solutioning` workflow
- Generate solution-architecture.md covering:
  - C#/.NET PowerShell module structure
  - Class hierarchy and interfaces
  - 7-Zip integration approach (CLI vs library)
  - AWS SDK integration patterns
  - Pipeline object design
  - Error handling patterns
- Create per-epic tech specs (tech-spec-epic-1.md, tech-spec-epic-2.md)

---

## Complete Implementation Roadmap

### Phase 1: Solution Architecture and Design ⬅️ **START HERE**

- [ ] **Run solutioning workflow** (REQUIRED)
  - Command: Start new chat with architect agent or solutioning workflow
  - Input: PRD.md, epic-stories.md, brainstorming-session-results-2025-10-08.md
  - Output: solution-architecture.md, tech-spec-epic-1.md, tech-spec-epic-2.md
  - Focus areas: C# module structure, 7-Zip integration, AWS SDK patterns, dependency injection

### Phase 2: Development Environment Setup

- [ ] **Set up C# PowerShell module project**
  - Initialize .NET project with PowerShell SDK
  - Configure build pipeline
  - Set up Pester testing framework
  - Add AWS SDK for .NET NuGet package
  - Add 7-Zip dependency (CLI or library based on solution architecture)

- [ ] **Create repository structure**
  - /src - C# source code
  - /tests - Pester unit and integration tests
  - /docs - Documentation and examples
  - /build - Build scripts

### Phase 3: Epic 1 Implementation (2 weeks)

- [ ] **Foundation Layer** (Stories 1.1-1.5)
  - Implement core utility functions
  - Establish PowerShell cmdlet patterns
  - Set up error handling framework

- [ ] **Discovery Layer** (Stories 1.6-1.9)
  - Implement Find-LogFiles and Find-LogFolders
  - Implement archive existence checking
  - Performance optimize for massive scale

- [ ] **Processing Layer** (Stories 1.10-1.11)
  - Integrate 7-Zip compression
  - Validate in-place compression constraint
  - Test idempotent execution

- [ ] **Unit Testing** (Story 1.12)
  - Achieve >80% code coverage
  - Mock external dependencies

### Phase 4: Epic 2 Implementation (2 weeks)

- [ ] **Distribution Layer** (Stories 2.1-2.4)
  - Implement Send-ToS3, Send-ToUNC, Send-ToDrive
  - Implement Send-Archive dispatcher
  - Test all destination types

- [ ] **Orchestration** (Story 2.6)
  - Implement Invoke-LogArchival
  - Test complete workflows

- [ ] **Performance & Validation** (Stories 2.7-2.11)
  - Benchmark with 300K file dataset
  - Optimize file enumeration
  - Integration testing

- [ ] **Documentation** (Story 2.12)
  - Module README and quick start
  - Function reference documentation
  - Usage examples

### Phase 5: MVP Release

- [ ] **Final validation**
  - All unit tests passing
  - Integration tests passing
  - Performance benchmarks meet NFR001 targets
  - Documentation complete

- [ ] **Internal deployment**
  - Deploy to early user teams
  - Gather feedback
  - Monitor usage patterns

- [ ] **Iterate based on feedback**
  - Identify missing capabilities
  - Prioritize post-MVP enhancements

## Document Status

- [ ] Goals and context validated with stakeholders
- [ ] All functional requirements reviewed
- [ ] User journeys cover all major personas
- [ ] Epic structure approved for phased delivery
- [ ] Ready for architecture phase

_Note: See technical-decisions.md for captured technical context_

---

_This PRD adapts to project level Level 2 - providing appropriate detail without overburden._
