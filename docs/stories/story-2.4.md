# Story 2.4: Implement Send-Archive Dispatcher Function (FR006)

Status: Draft

## Story

As a DevOps engineer,
I want a unified Send-Archive function that routes to the appropriate backend,
so that I don't have to manually determine which backend function to call.

## Acceptance Criteria

1. Accepts archive files from FR005 and destination parameter
2. Analyzes destination path to determine type (s3://, \\\\, local)
3. Routes to appropriate backend: Send-ToS3 (FR015), Send-ToUNC (FR016), Send-ToDrive (FR017)
4. Passes through relevant parameters to backend functions
5. Returns success/failure status from backend (for FR007)
6. Returns explicit success confirmation signal for cleanup
7. Unit tests validate routing logic for all destination types

## Tasks / Subtasks

- [ ] Implement SendArchiveCmdlet (AC: #1, #4, #5, #6)
  - [ ] Create cmdlet with ArchiveFile, Destination, AppName parameters
  - [ ] Add AWS credential parameters (optional)
  - [ ] Accept pipeline input: FileInfo[], ArchiveInfo[]
  - [ ] Call dispatcher service
  - [ ] Return TransferResult array
- [ ] Implement dispatcher logic (AC: #2, #3, #4)
  - [ ] Analyze destination path pattern
  - [ ] If starts with "s3://" → route to S3TransferService
  - [ ] If starts with "\\\\" → route to UncTransferService
  - [ ] Otherwise (local path) → route to LocalTransferService
  - [ ] Resolve path tokens using PathTokenResolver for each archive
  - [ ] Pass credentials to S3 service if provided
- [ ] Create unit and integration tests (AC: #7)
  - [ ] xUnit: Test routing logic with mocked services
  - [ ] Test S3 destination routing
  - [ ] Test UNC destination routing
  - [ ] Test local destination routing
  - [ ] Test path token resolution per archive
  - [ ] Pester: Integration test with all destination types
  - [ ] Test pipeline input handling

## Dev Notes

- Distribution layer dispatcher - depends on FR015, FR016, FR017
- Path pattern recognition for routing
- Critical for FR007 cleanup safety
- Single entry point for all transfer operations
- Each archive may have different log date, so tokens resolved per archive
- Uses PathTokenResolver from Epic 1

### Project Structure Notes

- Cmdlet: `/src/logManager/Cmdlets/SendArchiveCmdlet.cs`
- Tests:
  - `/tests/logManager.Tests/Cmdlets/SendArchiveCmdletTests.cs`
  - `/tests/logManager.Tests/Integration/Send-Archive.Tests.ps1`

### References

- [Source: docs/tech-spec-epic-2.md#Services and Modules - Send-Archive]
- [Source: docs/tech-spec-epic-2.md#Workflows - Send-Archive Dispatcher Logic]
- [Source: docs/tech-spec-epic-2.md#AC13, AC14, AC15]
- [Source: docs/epic-stories.md#Story 2.4]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
