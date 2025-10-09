# Story 1.3: Implement Archive Naming Pattern Logic (FR010)

Status: Draft

## Story

As a system function,
I want to generate consistent archive filenames,
so that all functions use the same naming convention.

## Acceptance Criteria

1. Generates pattern: `AppName-YYYYMMDD.zip`
2. Accepts application name and date parameters
3. Returns filename string
4. Validates application name (no invalid filename characters)
5. Unit tests validate naming consistency

## Tasks / Subtasks

- [ ] Implement ArchiveNamingService (AC: #1, #2, #3, #4)
  - [ ] Create ArchiveNamingService class
  - [ ] Implement GetArchiveName method
  - [ ] Add application name validation
  - [ ] Format date as YYYYMMDD
- [ ] Create unit tests (AC: #5)
  - [ ] Test AppName="MyApp", Date=2025-10-08 returns "MyApp-20251008.zip"
  - [ ] Test app name with spaces preserved
  - [ ] Test invalid filename characters handled appropriately
  - [ ] Test date formatting is consistent

## Dev Notes

- Foundation layer - no dependencies
- Used by FR002 (Test-ArchiveExists), FR005 (Compress-Logs), FR006 (Send-Archive)
- Simple utility function
- Filename pattern is fixed: AppName-YYYYMMDD.zip
- Date format: YYYYMMDD (no separators)

### Project Structure Notes

- Location: `/src/logManager/Services/ArchiveNamingService.cs`
- Tests: `/tests/logManager.Tests/Services/ArchiveNamingServiceTests.cs`

### References

- [Source: docs/tech-spec-epic-1.md#Services and Modules]
- [Source: docs/tech-spec-epic-1.md#AC3: Archive Naming]
- [Source: docs/epic-stories.md#Story 1.3]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
