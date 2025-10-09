# Story 2.9: Test Idempotent Execution and Resumability (NFR003)

Status: Draft

## Story

As a DevOps engineer,
I want to validate safe retry behavior,
so that I can confidently re-run failed archival operations.

## Acceptance Criteria

1. Integration tests simulate partial failures
2. Re-running after failure skips already-archived dates
3. No duplicate archives created
4. Data integrity maintained across retries
5. Documentation shows retry scenarios

## Tasks / Subtasks

- [ ] Create idempotent execution integration tests (AC: #1, #2, #3, #4)
  - [ ] Pester: Simulate first run archives 7 dates, 5 succeed, 2 fail
  - [ ] Mock S3 to fail specific dates
  - [ ] Run Invoke-LogArchival workflow
  - [ ] Verify 5 archives created successfully
  - [ ] Re-run same Invoke-LogArchival command
  - [ ] Verify Test-ArchiveExists filters out 5 successful dates
  - [ ] Verify only 2 failed dates are re-attempted
  - [ ] Verify no duplicate archives created
  - [ ] Verify data integrity (archives match expected content)
- [ ] Test all-archived scenario (AC: #2)
  - [ ] Pester: All dates already archived
  - [ ] Run Invoke-LogArchival
  - [ ] Verify WorkflowResult shows ArchivesCreated=0
  - [ ] Verify SkippedDates contains all dates
- [ ] Update documentation (AC: #5)
  - [ ] Add retry scenarios to USAGE.md
  - [ ] Show example: First run partial success
  - [ ] Show example: Retry completes remaining work
  - [ ] Explain check-first workflow benefits

## Dev Notes

- Validation of NFR003
- Integration testing for FR011
- End-to-end idempotent workflow testing
- Epic 1 Story 1.11 validated check-first at component level
- This story validates at orchestrator level
- Partial failure handling in Invoke-LogArchival critical

### Project Structure Notes

- Tests: `/tests/logManager.Tests/Integration/Idempotent-Execution.Tests.ps1`
- Documentation: `/docs/USAGE.md` (retry scenarios section)

### References

- [Source: docs/tech-spec-epic-2.md#NFR - Reliability/Availability - NFR003]
- [Source: docs/tech-spec-epic-2.md#AC18: Invoke-LogArchival Idempotent Execution]
- [Source: docs/epic-stories.md#Story 2.9]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
