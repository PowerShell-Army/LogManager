# Story 2.10: Write Integration Tests for Complete Archival Workflows

Status: Draft

## Story

As a developer,
I want end-to-end integration tests,
so that I can validate complete workflows work correctly.

## Acceptance Criteria

1. Integration tests for full orchestrator workflow
2. Tests cover S3, UNC, local destinations
3. Tests validate check-first workflow
4. Tests validate compression and transfer
5. Tests validate optional cleanup
6. Tests run in CI/CD pipeline

## Tasks / Subtasks

- [ ] Create S3 integration tests (AC: #1, #2, #3, #4, #5)
  - [ ] Pester: Full Invoke-LogArchival with S3 destination
  - [ ] Use $env:AWS_Access, $env:AWS_Secret, $env:AWS_Bucket
  - [ ] Test with -Compress and -DeleteSource switches
  - [ ] Verify archives uploaded to S3
  - [ ] Verify source files deleted
  - [ ] Test check-first workflow (re-run skips uploaded)
- [ ] Create UNC integration tests (AC: #1, #2, #3, #4, #5)
  - [ ] Pester: Full Invoke-LogArchival with UNC destination
  - [ ] Use $env:UNC_Drive
  - [ ] Test with -Compress and -DeleteSource switches
  - [ ] Verify archives copied to UNC share
  - [ ] Verify source files deleted
- [ ] Create local integration tests (AC: #1, #2, #3, #4, #5)
  - [ ] Pester: Full Invoke-LogArchival with local destination
  - [ ] Use $env:Local_Drive
  - [ ] Test with -Compress and -DeleteSource switches
  - [ ] Verify archives copied to local destination
  - [ ] Verify source files deleted
- [ ] Create partial workflow tests (AC: #5)
  - [ ] Test without -Compress (no compression step)
  - [ ] Test without -DeleteSource (no cleanup step)
  - [ ] Test with -SkipCheck (no archive existence check)
- [ ] Setup CI/CD pipeline (AC: #6)
  - [ ] Update GitHub Actions workflow
  - [ ] Run integration tests on every PR
  - [ ] Use environment variables for test infrastructure
  - [ ] Run on Windows and Linux

## Dev Notes

- End-to-end testing
- Use environment variables for real LTS destination testing
- **Testing Configuration (MANDATORY):**
  - AWS S3: $env:AWS_Access, $env:AWS_Secret, $env:AWS_Region, $env:AWS_Bucket
  - Local: $env:Local_Drive = 'D:\TEST_DATA'
  - UNC: $env:UNC_Drive = '\\10.0.10.10\storage\TEST_DATA'
- **NO OTHER LOCATIONS CAN BE USED FOR INTEGRATION TESTING**
- Mock external dependencies for unit tests only

### Project Structure Notes

- Tests:
  - `/tests/logManager.Tests/Integration/Invoke-LogArchival-S3-EndToEnd.Tests.ps1`
  - `/tests/logManager.Tests/Integration/Invoke-LogArchival-UNC-EndToEnd.Tests.ps1`
  - `/tests/logManager.Tests/Integration/Invoke-LogArchival-Local-EndToEnd.Tests.ps1`
  - `/tests/logManager.Tests/Integration/Partial-Workflow.Tests.ps1`
- CI/CD: `/.github/workflows/integration-tests.yml`

### References

- [Source: docs/tech-spec-epic-2.md#Test Strategy Summary]
- [Source: docs/tech-spec-epic-2.md#AC17: Invoke-LogArchival End-to-End]
- [Source: docs/epic-stories.md#Story 2.10]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
