# Story 2.3: Implement Send-ToDrive Backend Function (FR017)

Status: Draft

## Story

As a backend transfer function,
I want to transfer archives to local/external drives,
so that Send-Archive dispatcher can route local destinations to me.

## Acceptance Criteria

1. Accepts archive file path and local destination parameters
2. Uses .NET file I/O for local paths
3. Uses FR013 for path token resolution
4. Copies/moves to local destination
5. Returns success/failure status with details
6. Unit tests validate local transfer

## Tasks / Subtasks

- [ ] Implement LocalTransferService (AC: #1, #2, #3, #4, #5)
  - [ ] Create LocalTransferService implementing ITransferService
  - [ ] Validate local path format (drive letter or absolute path)
  - [ ] Use IFileSystem.CopyFile for transfer
  - [ ] Create destination directory if it doesn't exist
  - [ ] Return TransferResult with success confirmation
  - [ ] Measure transfer duration and bytes transferred
- [ ] Create unit and integration tests (AC: #6)
  - [ ] xUnit: LocalTransferService with mocked IFileSystem
  - [ ] Test local path validation
  - [ ] Test directory creation
  - [ ] Test file copy operation
  - [ ] Pester: Integration test using $env:Local_Drive
  - [ ] Test disk full error handling
  - [ ] Test permission denied error handling

## Dev Notes

- Distribution layer backend - depends on FR005 (Compress-Logs), FR013 (PathTokenResolver)
- Simplest destination type (no network or cloud authentication)
- Single responsibility: local drive transfers only
- **Testing Configuration:** Use environment variable: $env:Local_Drive = 'D:\TEST_DATA'
- **NO OTHER LOCAL LOCATION CAN BE USED FOR TESTING**

### Project Structure Notes

- Service: `/src/logManager/Services/LocalTransferService.cs`
- Interface: `/src/logManager/Abstractions/ITransferService.cs` (already exists)
- Tests:
  - `/tests/logManager.Tests/Services/LocalTransferServiceTests.cs`
  - `/tests/logManager.Tests/Integration/Send-Archive-Local.Tests.ps1`

### References

- [Source: docs/tech-spec-epic-2.md#Services and Modules - LocalTransferService]
- [Source: docs/tech-spec-epic-2.md#AC15: Send-Archive to Local Drive]
- [Source: docs/tech-spec-epic-2.md#Data Models - TransferResult]
- [Source: docs/epic-stories.md#Story 2.3]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
