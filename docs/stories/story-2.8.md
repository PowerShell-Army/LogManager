# Story 2.8: Validate In-Place Compression Constraint Enforcement (NFR002)

Status: Draft

## Story

As a developer,
I want to ensure compression never uses temp locations,
so that the module doesn't double disk I/O or storage requirements.

## Acceptance Criteria

1. Compress-Logs verified to compress on source drive only
2. Code review confirms no temp file creation
3. Integration tests monitor file I/O locations
4. Documentation explicitly states constraint

## Tasks / Subtasks

- [ ] Code review for temp file usage (AC: #2)
  - [ ] Review SevenZipCompressor implementation
  - [ ] Review SharpCompressCompressor implementation
  - [ ] Verify no Path.GetTempPath() usage
  - [ ] Verify working directory is source location
- [ ] Create validation tests (AC: #1, #3)
  - [ ] Pester: Test compression creates archive on same drive as source
  - [ ] Test cross-drive compression throws CompressionException
  - [ ] Monitor file I/O during compression (no temp files created)
- [ ] Update documentation (AC: #4)
  - [ ] Document NFR002 constraint in README
  - [ ] Explain why in-place compression is required
  - [ ] Show that compression works on source drive only

## Dev Notes

- Validation of NFR002 constraint
- Critical for handling massive file counts
- Both 7-Zip and SharpCompress must enforce constraint
- Working directory for 7-Zip Process.Start must be source directory
- SharpCompress output path must be on source drive
- Already implemented in Epic 1 Story 1.10, this story validates it

### Project Structure Notes

- Tests: `/tests/logManager.Tests/Integration/InPlace-Compression-Validation.Tests.ps1`
- Documentation: `/docs/README.md` (constraints section)

### References

- [Source: docs/tech-spec-epic-2.md#NFR - Performance - NFR002]
- [Source: docs/tech-spec-epic-1.md#AC7: Compress-Logs Cmdlet]
- [Source: docs/epic-stories.md#Story 2.8]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
