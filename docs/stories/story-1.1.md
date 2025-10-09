# Story 1.1: Implement Get-DateRange Function (FR001)

Status: Draft

## Story

As a DevOps engineer,
I want to calculate date ranges from simple integer day parameters,
so that I can define archival windows without dealing with complex date objects.

## Acceptance Criteria

1. Function accepts `olderThan` and `youngerThan` integer parameters (days)
2. Returns array of DateTime objects representing each day in range
3. Supports `DateCriteria` parameter: "CreationDate" or "ModifiedDate"
4. Validates parameters (olderThan must be >= youngerThan)
5. Returns PowerShell objects compatible with pipeline chaining
6. Unit tests cover edge cases (same day, 1 day range, 30+ day range)

## Tasks / Subtasks

- [ ] Implement GetDateRangeCmdlet class (AC: #1, #2, #3, #4, #5)
  - [ ] Create cmdlet with OlderThan, YoungerThan, and DateCriteria parameters
  - [ ] Implement parameter validation logic
  - [ ] Create DateRangeResult return object model
  - [ ] Implement date range calculation logic
- [ ] Implement DateRangeCalculator service (AC: #1, #2, #4)
  - [ ] Create service class for date math logic
  - [ ] Implement Calculate method returning DateRangeResult array
  - [ ] Add parameter validation
- [ ] Create unit tests (AC: #6)
  - [ ] Test olderThan=7, youngerThan=0 returns 8 dates
  - [ ] Test olderThan=7, youngerThan=7 returns 1 date
  - [ ] Test olderThan=5, youngerThan=7 throws ArgumentException
  - [ ] Test DateCriteria enum values set correctly
  - [ ] Test edge cases (0 days, 30+ days, negative values)

## Dev Notes

- Foundation layer - no dependencies on other functions
- Must follow <500 line constraint (ADR-004)
- Return objects must include: Date, DateCriteria properties
- Uses modern C# 11/12 record types for return objects
- PowerShell binary module architecture (.NET 8.0, PowerShell 7.2+)

### Project Structure Notes

- Location: `/src/logManager/Cmdlets/GetDateRangeCmdlet.cs`
- Service: `/src/logManager/Services/DateRangeCalculator.cs`
- Models: `/src/logManager/Models/DateRangeResult.cs`
- Tests: `/tests/logManager.Tests/Services/DateRangeCalculatorTests.cs`

### References

- [Source: docs/tech-spec-epic-1.md#Services and Modules]
- [Source: docs/tech-spec-epic-1.md#Data Models and Contracts]
- [Source: docs/tech-spec-epic-1.md#AC1: Get-DateRange Cmdlet]
- [Source: docs/epic-stories.md#Story 1.1]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
