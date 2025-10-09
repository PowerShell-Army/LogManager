# Story 2.5: Implement Remove-LogSource Cleanup Function (FR007)

Status: Draft

## Story

As a DevOps engineer,
I want to optionally delete source files/folders after successful archival,
so that I can free up disk space automatically.

## Acceptance Criteria

1. Accepts DeleteSource boolean parameter
2. Only deletes after FR006 success confirmation
3. Deletes original files/folders that were archived
4. Validates deletion completed successfully
5. Fail-safe: Never deletes without confirmed transfer
6. Unit tests validate safety logic

## Tasks / Subtasks

- [ ] Implement RemoveLogSourceCmdlet (AC: #1, #2, #3, #4, #5)
  - [ ] Create cmdlet with SourcePath and TransferConfirmation parameters
  - [ ] Add optional Force switch
  - [ ] Accept pipeline input: string[], LogFileInfo[], LogFolderInfo[]
  - [ ] Call CleanupService
  - [ ] Return CleanupResult
- [ ] Implement CleanupService (AC: #2, #3, #4, #5)
  - [ ] Create CleanupService for deletion logic
  - [ ] Validate ALL transfers succeeded before ANY deletion
  - [ ] Match source paths to TransferResult confirmations
  - [ ] Extract original source from archive name if needed
  - [ ] If ANY transfer failed or missing → throw CleanupException
  - [ ] Delete files/folders using IFileSystem
  - [ ] Track deleted count and bytes freed
  - [ ] Return CleanupResult with metrics
- [ ] Create unit and integration tests (AC: #6)
  - [ ] xUnit: CleanupService with mocked IFileSystem
  - [ ] Test all transfers successful → deletions proceed
  - [ ] Test ANY transfer failed → NO deletions occur
  - [ ] Test missing confirmation → throw CleanupException
  - [ ] Test partial cleanup scenarios
  - [ ] Pester: Integration test with real files
  - [ ] Test Force switch behavior

## Dev Notes

- Distribution layer - depends on FR006 success
- Safety-critical function - must be conservative (fail-safe)
- All-or-nothing approach: Either delete all sources OR delete none
- Epic 2 orchestrator may allow partial cleanup (only delete successfully transferred)
- CleanupResult includes: FilesDeleted, FoldersDeleted, BytesFreed, DeletedPaths, FailedPaths

### Project Structure Notes

- Cmdlet: `/src/logManager/Cmdlets/RemoveLogSourceCmdlet.cs`
- Service: `/src/logManager/Services/CleanupService.cs`
- Model: `/src/logManager/Models/CleanupResult.cs`
- Exception: `/src/logManager/Exceptions/CleanupException.cs`
- Tests:
  - `/tests/logManager.Tests/Services/CleanupServiceTests.cs`
  - `/tests/logManager.Tests/Integration/Remove-LogSource.Tests.ps1`

### References

- [Source: docs/tech-spec-epic-2.md#Services and Modules - Remove-LogSource, CleanupService]
- [Source: docs/tech-spec-epic-2.md#AC16: Remove-LogSource Safety]
- [Source: docs/tech-spec-epic-2.md#Workflows - Remove-LogSource Safety Logic]
- [Source: docs/epic-stories.md#Story 2.5]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
