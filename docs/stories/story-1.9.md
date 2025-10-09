# Story 1.9: Implement Test-ArchiveExists Check Function (FR002)

Status: Draft

## Story

As a DevOps engineer,
I want to query destinations for existing archives before processing,
so that I can skip already-archived dates and save hours of redundant work.

## Acceptance Criteria

1. Accepts Destination (with tokens), olderThan, youngerThan, AppName parameters
2. Uses FR001 to generate date range
3. Uses FR013 to resolve destination paths for each date
4. Uses FR014 to check archive existence
5. Returns filtered list of dates NOT yet archived
6. Supports S3, UNC, and local destinations
7. Unit tests with various destination types

## Tasks / Subtasks

- [ ] Implement TestArchiveExistsCmdlet (AC: #1, #5, #6)
  - [ ] Create cmdlet with Destination, DateRange, AppName parameters
  - [ ] Add optional AWS credential parameters (AwsAccessKey, AwsSecretKey, AwsRegion)
  - [ ] Accept DateRange from pipeline (ValueFromPipeline)
  - [ ] Call ArchiveExistenceChecker service
  - [ ] Return filtered DateRangeResult array
- [ ] Implement ArchiveExistenceChecker service (AC: #2, #3, #4, #5)
  - [ ] Create service with PathTokenResolver and ArchiveNamingService dependencies
  - [ ] For each date in range, resolve destination path using tokens
  - [ ] Generate archive filename using ArchiveNamingService
  - [ ] Determine destination type (S3/UNC/Local) from path pattern
  - [ ] Create appropriate IDestinationProvider instance
  - [ ] Check if archive exists
  - [ ] Return only dates where archive does NOT exist
- [ ] Create unit and integration tests (AC: #7)
  - [ ] xUnit: ArchiveExistenceChecker with mocked dependencies
  - [ ] Test S3 destination filtering
  - [ ] Test UNC destination filtering
  - [ ] Test Local destination filtering
  - [ ] Test all dates archived returns empty array
  - [ ] Test no dates archived returns all dates
  - [ ] Pester: Integration test with real AWS.Tools.S3 module

## Dev Notes

- Discovery layer - depends on FR001, FR010, FR013, FR014
- Critical for check-first workflow performance
- Enables FR011 idempotent execution
- S3 requires AWS.Tools.S3 PowerShell module installed
- Uses Get-S3Object cmdlet for S3 existence checking (Epic 1 scope)
- Write-S3Object for uploads deferred to Epic 2

### Project Structure Notes

- Cmdlet: `/src/logManager/Cmdlets/TestArchiveExistsCmdlet.cs`
- Service: `/src/logManager/Services/ArchiveExistenceChecker.cs`
- Tests:
  - `/tests/logManager.Tests/Services/ArchiveExistenceCheckerTests.cs`
  - `/tests/logManager.Tests/Integration/Test-ArchiveExists.Tests.ps1`

### References

- [Source: docs/tech-spec-epic-1.md#Services and Modules - Test-ArchiveExists]
- [Source: docs/tech-spec-epic-1.md#AC6: Test-ArchiveExists Cmdlet]
- [Source: docs/tech-spec-epic-1.md#Workflows - Test-ArchiveExists Check-First Workflow]
- [Source: docs/epic-stories.md#Story 1.9]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
