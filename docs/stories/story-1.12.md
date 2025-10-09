# Story 1.12: Write Unit Tests for Foundation and Discovery Layer Functions

Status: Draft

## Story

As a developer,
I want comprehensive unit tests for all Foundation and Discovery functions,
so that I can ensure quality and prevent regressions.

## Acceptance Criteria

1. Unit tests for FR001, FR009, FR010, FR012, FR013
2. Unit tests for FR002, FR003, FR004, FR014
3. Code coverage >80% for these functions
4. Tests validate edge cases and error conditions
5. Tests run in CI/CD pipeline

## Tasks / Subtasks

- [ ] Foundation layer unit tests (AC: #1, #3, #4)
  - [ ] DateRangeCalculator tests (various date ranges, edge cases)
  - [ ] PathTokenResolver tests (all token types, multiple patterns)
  - [ ] ArchiveNamingService tests (various app names and dates)
  - [ ] Exception hierarchy tests (all exception types)
- [ ] Discovery layer unit tests (AC: #2, #3, #4)
  - [ ] FileDiscoveryService tests (mocked IFileSystem)
  - [ ] FolderDiscoveryService tests (folder name parsing)
  - [ ] ArchiveExistenceChecker tests (mocked IDestinationProvider)
  - [ ] All IDestinationProvider implementations (S3, UNC, Local)
- [ ] Setup CI/CD pipeline (AC: #5)
  - [ ] Configure GitHub Actions workflow
  - [ ] Run xUnit tests with code coverage
  - [ ] Run Pester integration tests
  - [ ] Test matrix: Windows + Linux, PowerShell 7.2/7.3/7.4
  - [ ] Fail build if coverage <80%
- [ ] Document testing approach (AC: #5)
  - [ ] Create testing README
  - [ ] Document how to run tests locally
  - [ ] Explain test organization (unit vs integration)

## Dev Notes

- Testing framework: xUnit (C# unit tests), Pester (PowerShell integration tests)
- Mock external dependencies: IFileSystem, IDestinationProvider, ICompressionEngine
- Use [Theory] and [InlineData] for parameterized testing
- Code coverage tool: coverlet (XPlat Code Coverage)
- All tests must be runnable locally and in CI/CD
- Target: 100% coverage for Foundation and Discovery layers

### Project Structure Notes

- xUnit tests: `/tests/logManager.Tests/`
  - Services/
  - Infrastructure/
  - Cmdlets/
- Pester tests: `/tests/logManager.Tests/Integration/`
- CI/CD: `/.github/workflows/test.yml`
- Documentation: `/tests/README.md`

### References

- [Source: docs/tech-spec-epic-1.md#Test Strategy Summary]
- [Source: docs/epic-stories.md#Story 1.12]
- [Source: docs/tech-spec-epic-1.md#Acceptance Criteria sections]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
