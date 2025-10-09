# Story 2.12: Create Module Documentation and Usage Examples

Status: Draft

## Story

As a DevOps engineer,
I want comprehensive documentation,
so that I can understand how to use the module effectively.

## Acceptance Criteria

1. README with quick start guide
2. Function reference documentation
3. Usage examples for common scenarios
4. Pipeline composition examples
5. Troubleshooting guide
6. Performance tuning tips

## Tasks / Subtasks

- [ ] Create README.md (AC: #1)
  - [ ] Project overview and features
  - [ ] Prerequisites (PowerShell 7.2+, .NET 8, AWS.Tools.S3)
  - [ ] Installation instructions
  - [ ] Quick start example
  - [ ] Link to detailed documentation
- [ ] Create USAGE.md (AC: #3, #4)
  - [ ] Common scenario examples
  - [ ] Example: Archive to S3 with compression and cleanup
  - [ ] Example: Archive to UNC share
  - [ ] Example: Archive to local drive
  - [ ] Example: Custom workflow with pipeline chaining
  - [ ] Example: Retry failed archival operation
  - [ ] Show both orchestrator and individual function patterns
- [ ] Create function reference (AC: #2)
  - [ ] Document all cmdlets with parameter descriptions
  - [ ] Get-DateRange
  - [ ] Find-LogFiles, Find-LogFolders
  - [ ] Test-ArchiveExists
  - [ ] Compress-Logs
  - [ ] Send-Archive
  - [ ] Remove-LogSource
  - [ ] Invoke-LogArchival
  - [ ] Show return object schemas
- [ ] Create TROUBLESHOOTING.md (AC: #5)
  - [ ] Common errors and solutions
  - [ ] AWS.Tools.S3 module not installed
  - [ ] 7-Zip not found warnings
  - [ ] S3 permission denied errors
  - [ ] UNC share not reachable errors
  - [ ] Compression failures
  - [ ] Cleanup safety errors
- [ ] Create PERFORMANCE.md (AC: #6)
  - [ ] Performance targets and benchmarks
  - [ ] Best practices for large file counts
  - [ ] Compression engine comparison (7-Zip vs SharpCompress)
  - [ ] Tuning recommendations
  - [ ] Hardware recommendations
- [ ] Add inline XML documentation (AC: #2)
  - [ ] Add XML doc comments to all cmdlets
  - [ ] Document parameters with examples
  - [ ] Enable PowerShell Get-Help cmdlet

## Dev Notes

- User-facing documentation
- Examples should match user journey from PRD
- All examples must be tested and verified to work
- Use real-world scenarios in examples
- Link documentation from README
- XML documentation enables Get-Help cmdlet in PowerShell

### Project Structure Notes

- Documentation:
  - `/README.md`
  - `/docs/USAGE.md`
  - `/docs/FUNCTION-REFERENCE.md`
  - `/docs/TROUBLESHOOTING.md`
  - `/docs/PERFORMANCE.md`
- XML comments in cmdlet source files

### References

- [Source: docs/tech-spec-epic-2.md#AC17, AC22]
- [Source: docs/PRD.md#User Journey]
- [Source: docs/epic-stories.md#Story 2.12]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
