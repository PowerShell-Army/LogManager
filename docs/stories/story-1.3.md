# Story 1.3: Implement Archive Naming Pattern Logic (FR010)

Status: Ready for Review

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

- [x] Implement ArchiveNamingService (AC: #1, #2, #3, #4)
  - [x] Create ArchiveNamingService class
  - [x] Implement GetArchiveName method
  - [x] Add application name validation
  - [x] Format date as YYYYMMDD
- [x] Create unit tests (AC: #5)
  - [x] Test AppName="MyApp", Date=2025-10-08 returns "MyApp-20251008.zip"
  - [x] Test app name with spaces preserved
  - [x] Test invalid filename characters handled appropriately
  - [x] Test date formatting is consistent

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

- D:\projects\logManager\docs\story-context-1.1.3.xml

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

Story 1.3 implementation completed archive naming service.

### Completion Notes List

- Created ArchiveNamingService with GetArchiveName method
- Implements fixed pattern: AppName-YYYYMMDD.zip
- Date formatting: YYYYMMDD with zero-padded month/day (no separators)
- Application name spaces are preserved (not replaced or stripped)
- Validates appName is not null/empty/whitespace
- Validates appName contains no invalid filename characters (\ / : * ? " < > |)
- Throws ArgumentException with descriptive messages for validation failures
- Created 19 comprehensive unit tests including Theory tests for all invalid characters
- All 56 tests passing (37 existing + 19 new)
- Simple utility following Foundation layer pattern from DateRangeCalculator

### File List

- src/logManager/Services/ArchiveNamingService.cs (new)
- tests/logManager.Tests/Services/ArchiveNamingServiceTests.cs (new)

---

## Senior Developer Review (AI)

**Reviewer:** Adam
**Date:** 2025-10-08
**Outcome:** **Approve** ✓

### Summary

Story 1.3 delivers a clean, focused archive naming utility that correctly implements the fixed pattern AppName-YYYYMMDD.zip with robust validation. The implementation is exemplary for Foundation layer services: minimal (36 lines), stateless, zero dependencies, and comprehensive test coverage (19 tests, 100% pass rate). Code quality is excellent with proper input validation and clear error messages.

### Key Findings

**✅ Strengths:**
- **Platform-aware validation**: Uses `Path.GetInvalidFileNameChars()` ensuring cross-platform compatibility
- **Proper null/whitespace handling**: `IsNullOrWhiteSpace` catches all invalid string states
- **Informative error messages**: Lists specific invalid characters found, aiding debugging
- **Date formatting correctness**: yyyyMMdd format produces consistent, sortable filenames
- **Preserves spaces**: Correctly doesn't sanitize spaces in app names (per spec AC3)
- **Comprehensive tests**: 19 tests including Theory for all invalid character combinations

**No Issues Found**

### Acceptance Criteria Coverage

| AC | Status | Evidence |
|----|--------|----------|
| AC1: Pattern AppName-YYYYMMDD.zip | ✅ | ArchiveNamingService.cs:34 - Correct string interpolation |
| AC2: Accepts name + date params | ✅ | GetArchiveName signature (line 16) |
| AC3: Returns filename string | ✅ | Return type string, tests verify output |
| AC4: Validates invalid chars | ✅ | Lines 24-30 - Platform API used for validation |
| AC5: Unit tests validate | ✅ | 19 tests in ArchiveNamingServiceTests.cs, all passing |

### Test Coverage and Gaps

**Coverage: Excellent (100%)**
- ✅ Basic pattern validation (MyApp-20251008.zip)
- ✅ Spaces preserved testing
- ✅ All invalid characters tested via Theory (\ / : * ? " < > |)
- ✅ Null/empty/whitespace validation
- ✅ Date formatting edge cases (leap year, month/day padding)
- ✅ Consistency testing (same inputs → same outputs)

**No gaps identified** - Test suite is thorough.

### Architectural Alignment

**✅ Foundation Layer Compliance:**
- No dependencies on other services ✓
- Service class < 500 lines (36 lines - exemplary) ✓
- Stateless utility function ✓
- Follows DateRangeCalculator pattern ✓

**✅ Technical Standards:**
- .NET 9.0, C# 12 compliance ✓
- XML documentation complete ✓
- ArgumentException with paramName ✓
- xUnit test structure with Theory/InlineData ✓

### Security Notes

**✅ No security concerns:**
- Input validation prevents injection via filenames
- Platform-aware validation (Path.GetInvalidFileNameChars) prevents OS-specific exploits
- No file system access (filename generation only)
- Error messages appropriately verbose without leaking sensitive data

### Best-Practices and References

**Modern .NET Patterns Applied:**
- ✅ Platform APIs for cross-platform compatibility (`Path.GetInvalidFileNameChars()`)
- ✅ String interpolation for clear formatting
- ✅ Proper exception types (ArgumentException with paramName)
- ✅ LINQ for error message formatting (readable and efficient)

**Alignment with .NET 9.0 Best Practices:**
- Date formatting using ToString with format specifier - efficient
- No unnecessary allocations (single string concat via interpolation)
- Fail-fast validation before processing

### Action Items

**None** - Implementation is production-ready.

**Observations (Informational):**
- Error message on line 28 lists all invalid characters which could be verbose (potentially 40+ characters), but this is actually helpful for debugging and acceptable given the infrequency of this error condition.
- The fixed pattern approach (no configuration) is appropriate for MVP scope and simplifies the interface.
