# Story 1.7: Implement Find-LogFolders Function (FR004)

Status: Draft

## Story

As a DevOps engineer,
I want to locate dated folders matching criteria,
so that I can archive folder-based logs as complete units.

## Acceptance Criteria

1. Accepts Path, olderThan, youngerThan parameters
2. Detects folder naming: YYYYMMDD or YYYY-MM-DD formats
3. Returns collection of DirectoryInfo objects with parsed dates
4. Filters folders based on FR001 date range
5. Treats entire folder as archival unit
6. Unit tests validate folder name parsing

## Tasks / Subtasks

- [ ] Implement FindLogFoldersCmdlet (AC: #1, #3, #4)
  - [ ] Create cmdlet with Path and DateRange parameters
  - [ ] Accept DateRange from pipeline (ValueFromPipeline)
  - [ ] Call FolderDiscoveryService for folder enumeration
  - [ ] Return LogFolderInfo array
- [ ] Implement FolderDiscoveryService (AC: #2, #3, #4, #5)
  - [ ] Create service with IFileSystem dependency
  - [ ] Use EnumerateDirectories with yield return
  - [ ] Parse folder names for YYYYMMDD pattern
  - [ ] Parse folder names for YYYY-MM-DD pattern
  - [ ] Match parsed dates against DateRange
  - [ ] Skip folders not matching date patterns
- [ ] Create unit and integration tests (AC: #6)
  - [ ] xUnit: FolderDiscoveryService tests with mocked IFileSystem
  - [ ] Test YYYYMMDD folder parsing (e.g., "20251008")
  - [ ] Test YYYY-MM-DD folder parsing (e.g., "2025-10-08")
  - [ ] Test folders not matching pattern are skipped
  - [ ] Pester: Integration test with real test folders

## Dev Notes

- Discovery layer - depends on FR001 (Get-DateRange)
- Distinct from FR003 for different use cases (folder-based vs file-based logs)
- Uses same IFileSystem abstraction as Find-LogFiles
- Folder name patterns: YYYYMMDD or YYYY-MM-DD only
- Entire folder is treated as a single archival unit

### Project Structure Notes

- Cmdlet: `/src/logManager/Cmdlets/FindLogFoldersCmdlet.cs`
- Service: `/src/logManager/Services/FolderDiscoveryService.cs`
- Model: `/src/logManager/Models/LogFolderInfo.cs`
- Tests:
  - `/tests/logManager.Tests/Services/FolderDiscoveryServiceTests.cs`
  - `/tests/logManager.Tests/Integration/Find-LogFolders.Tests.ps1`

### References

- [Source: docs/tech-spec-epic-1.md#Services and Modules - Find-LogFolders]
- [Source: docs/tech-spec-epic-1.md#AC5: Find-LogFolders Cmdlet]
- [Source: docs/tech-spec-epic-1.md#Data Models - LogFolderInfo]
- [Source: docs/epic-stories.md#Story 1.7]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
