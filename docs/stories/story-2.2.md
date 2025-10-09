# Story 2.2: Implement Send-ToUNC Backend Function (FR016)

Status: Draft

## Story

As a backend transfer function,
I want to transfer archives to UNC network shares,
so that Send-Archive dispatcher can route UNC destinations to me.

## Acceptance Criteria

1. Accepts archive file path and UNC destination parameters
2. Uses .NET file I/O for UNC paths (\\\\server\\share)
3. Uses FR013 for path token resolution
4. Handles network share authentication
5. Copies to UNC destination
6. Returns success/failure status with details
7. Unit tests with UNC mock

## Tasks / Subtasks

- [ ] Implement UncTransferService (AC: #1, #2, #3, #4, #5, #6)
  - [ ] Create UncTransferService implementing ITransferService
  - [ ] Validate UNC path format (starts with \\\\)
  - [ ] Use IFileSystem.CopyFile for transfer
  - [ ] Create destination directory if it doesn't exist
  - [ ] Handle network share authentication (current process credentials)
  - [ ] Return TransferResult with success confirmation
  - [ ] Measure transfer duration and bytes transferred
- [ ] Extend IFileSystem interface (AC: #2)
  - [ ] Add CopyFile method
  - [ ] Add CreateDirectory method
  - [ ] Update FileSystemWrapper implementation
- [ ] Create unit and integration tests (AC: #7)
  - [ ] xUnit: UncTransferService with mocked IFileSystem
  - [ ] Test UNC path validation
  - [ ] Test directory creation
  - [ ] Test file copy operation
  - [ ] Pester: Integration test with real UNC share using $env:UNC_Drive
  - [ ] Test unreachable share error handling

## Dev Notes

- Distribution layer backend - depends on FR005 (Compress-Logs), FR013 (PathTokenResolver)
- Network share authentication handling via current process credentials
- Single responsibility: UNC transfers only
- **Testing Configuration:** Use environment variable: $env:UNC_Drive = '\\10.0.10.10\storage\TEST_DATA'
- **NO OTHER UNC LOCATION CAN BE USED FOR TESTING**

### Project Structure Notes

- Service: `/src/logManager/Services/UncTransferService.cs`
- Interface: `/src/logManager/Abstractions/ITransferService.cs` (already exists)
- Tests:
  - `/tests/logManager.Tests/Services/UncTransferServiceTests.cs`
  - `/tests/logManager.Tests/Integration/Send-Archive-UNC.Tests.ps1`

### References

- [Source: docs/tech-spec-epic-2.md#Services and Modules - UncTransferService]
- [Source: docs/tech-spec-epic-2.md#AC14: Send-Archive to UNC]
- [Source: docs/tech-spec-epic-2.md#Data Models - TransferResult]
- [Source: docs/epic-stories.md#Story 2.2]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
