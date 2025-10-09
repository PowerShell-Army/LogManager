# Story 2.6: Implement Invoke-LogArchival Orchestrator Function (FR008)

Status: Draft

## Story

As a DevOps engineer,
I want an all-in-one orchestrator function,
so that I can run complete archival workflows with a single command.

## Acceptance Criteria

1. Chains: FR001 → FR002 → FR003/FR004 → FR005 → FR006 → FR007
2. Accepts all relevant parameters for each step
3. Supports SkipCheck parameter to bypass FR002
4. Supports Compress parameter (optional compression)
5. Supports DeleteSource parameter
6. Returns workflow execution summary
7. Integration tests validate complete workflow
8. Documentation shows usage examples

## Tasks / Subtasks

- [ ] Implement InvokeLogArchivalCmdlet (AC: #1, #2, #3, #4, #5, #6)
  - [ ] Create cmdlet with comprehensive parameter set
  - [ ] Parameters: Path, olderThan, youngerThan, AppName, Destination
  - [ ] Switches: SkipCheck, Compress, DeleteSource, UseFolders
  - [ ] AWS credentials, PreferredEngine parameters
  - [ ] Chain all operations in sequence
  - [ ] Aggregate results into WorkflowResult
  - [ ] Handle partial failures (continue processing remaining dates)
- [ ] Implement workflow orchestration logic (AC: #1)
  - [ ] Step 1: Call Get-DateRange
  - [ ] Step 2: Call Test-ArchiveExists (unless -SkipCheck)
  - [ ] Step 3: Call Find-LogFiles or Find-LogFolders (based on -UseFolders)
  - [ ] Step 4: Call Compress-Logs (if -Compress)
  - [ ] Step 5: Call Send-Archive
  - [ ] Step 6: Call Remove-LogSource (if -DeleteSource and transfers succeeded)
  - [ ] Return WorkflowResult with metrics
- [ ] Implement verbose logging (AC: #8)
  - [ ] Log each workflow step
  - [ ] Report dates processed, skipped, failed
  - [ ] Report compression metrics
  - [ ] Report transfer metrics
  - [ ] Report cleanup metrics
- [ ] Create integration tests (AC: #7)
  - [ ] Pester: Full workflow with S3 destination
  - [ ] Test all switches enabled
  - [ ] Test partial workflow (no compression)
  - [ ] Test -SkipCheck behavior
  - [ ] Test partial failure handling
- [ ] Create documentation (AC: #8)
  - [ ] Add usage examples to USAGE.md
  - [ ] Show common scenarios
  - [ ] Document all parameters and switches

## Dev Notes

- Orchestration layer - depends on all previous functions
- Convenience wrapper for common workflows
- Must support both individual function and orchestrator patterns
- WorkflowResult includes: FilesProcessed, ArchivesCreated, TotalBytesCompressed, TotalBytesTransferred, FilesDeleted, TotalDuration, Errors, Warnings, SkippedDates
- Partial failure handling: Continue processing remaining dates, collect errors

### Project Structure Notes

- Cmdlet: `/src/logManager/Cmdlets/InvokeLogArchivalCmdlet.cs`
- Model: `/src/logManager/Models/WorkflowResult.cs`
- Tests: `/tests/logManager.Tests/Integration/Invoke-LogArchival-EndToEnd.Tests.ps1`
- Documentation: `/docs/USAGE.md` (orchestrator examples section)

### References

- [Source: docs/tech-spec-epic-2.md#Services and Modules - Invoke-LogArchival]
- [Source: docs/tech-spec-epic-2.md#AC17: Invoke-LogArchival End-to-End]
- [Source: docs/tech-spec-epic-2.md#Workflows - End-to-End Orchestration]
- [Source: docs/epic-stories.md#Story 2.6]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
