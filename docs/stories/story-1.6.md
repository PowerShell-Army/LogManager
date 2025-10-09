# Story 1.6: Implement Find-LogFiles Function (FR003)

Status: Draft

## Story

As a DevOps engineer,
I want to locate individual log files matching date criteria,
so that I can archive file-based logs efficiently.

## Acceptance Criteria

1. Accepts Path, olderThan, youngerThan parameters
2. Accepts DateCriteria: "CreationDate" or "ModifiedDate"
3. Supports Recurse parameter for subdirectories
4. Returns collection of FileInfo objects with dates
5. Filters files based on FR001 date range
6. Performance tested with 100K+ files
7. Unit tests validate filtering accuracy

## Tasks / Subtasks

- [ ] Implement FindLogFilesCmdlet (AC: #1, #2, #3, #4, #5)
  - [ ] Create cmdlet with Path, DateRange, DateCriteria, Recurse parameters
  - [ ] Accept DateRange from pipeline (ValueFromPipeline)
  - [ ] Call FileDiscoveryService for file enumeration
  - [ ] Return LogFileInfo array
- [ ] Implement FileDiscoveryService (AC: #4, #5, #6)
  - [ ] Create service with IFileSystem dependency
  - [ ] Use .NET 8 EnumerateFiles with yield return for streaming
  - [ ] Filter by CreationTime or LastWriteTime based on criteria
  - [ ] Match dates against DateRange array
- [ ] Create IFileSystem abstraction (AC: #6, #7)
  - [ ] Define IFileSystem interface
  - [ ] Create FileSystemWrapper implementation
  - [ ] Support EnumerateFiles method
- [ ] Create unit and integration tests (AC: #6, #7)
  - [ ] xUnit: FileDiscoveryService tests with mocked IFileSystem
  - [ ] Pester: Integration test with 100+ real test files
  - [ ] Test CreationDate vs ModifiedDate filtering
  - [ ] Test Recurse vs non-Recurse behavior
  - [ ] Test empty results (no matches)

## Dev Notes

- Discovery layer - depends on FR001 (Get-DateRange)
- Must handle massive file counts efficiently (100K+ files)
- Returns pipeline-compatible objects (FR009)
- Uses .NET 8 streaming enumeration for memory efficiency (NFR001)
- Must not load entire file list into memory

### Project Structure Notes

- Cmdlet: `/src/logManager/Cmdlets/FindLogFilesCmdlet.cs`
- Service: `/src/logManager/Services/FileDiscoveryService.cs`
- Interface: `/src/logManager/Abstractions/IFileSystem.cs`
- Implementation: `/src/logManager/Infrastructure/FileSystemWrapper.cs`
- Model: `/src/logManager/Models/LogFileInfo.cs`
- Tests:
  - `/tests/logManager.Tests/Services/FileDiscoveryServiceTests.cs`
  - `/tests/logManager.Tests/Integration/Find-LogFiles.Tests.ps1`

### References

- [Source: docs/tech-spec-epic-1.md#Services and Modules - Find-LogFiles]
- [Source: docs/tech-spec-epic-1.md#AC4: Find-LogFiles Cmdlet]
- [Source: docs/tech-spec-epic-1.md#Abstractions - IFileSystem]
- [Source: docs/epic-stories.md#Story 1.6]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
