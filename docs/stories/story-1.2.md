# Story 1.2: Implement Path Token Resolution Engine (FR013)

Status: Draft

## Story

As a system function,
I want to resolve path tokens independently of transfer operations,
so that both Test-ArchiveExists and Send-Archive can use consistent destination paths.

## Acceptance Criteria

1. Resolves tokens: `{year}`, `{month}`, `{day}`, `{date}`, `{server}`, `{app}`
2. Uses log date being archived, NOT current date
3. Accepts template string and date/context parameters
4. Returns fully resolved path string
5. Handles UNC paths (\\\\server\\share), S3 paths (s3://bucket/key), local paths (D:\\path)
6. Unit tests validate token replacement accuracy

## Tasks / Subtasks

- [ ] Implement PathTokenResolver service (AC: #1, #2, #3, #4, #5)
  - [ ] Create PathTokenResolver class with ResolveTokens method
  - [ ] Implement token replacement logic for all supported tokens
  - [ ] Create TokenResolverContext record for input parameters
  - [ ] Handle all path types (UNC, S3, local)
- [ ] Add error handling (AC: #6)
  - [ ] Create TokenResolutionException for invalid tokens
  - [ ] Validate template and context inputs
- [ ] Create unit tests (AC: #6)
  - [ ] Test S3 path with {year}/{month}/{day} tokens
  - [ ] Test UNC path with {app}/{year} tokens
  - [ ] Test {server} token replacement
  - [ ] Test template with no tokens returns unchanged
  - [ ] Test missing required context throws exception

## Dev Notes

- Foundation layer - no dependencies
- Critical for FR002 (Test-ArchiveExists) and FR006 (Send-Archive)
- Must be reusable utility function
- Uses log date being archived, NOT current date
- Token format: case-sensitive, curly braces required

### Project Structure Notes

- Location: `/src/logManager/Services/PathTokenResolver.cs`
- Models: `/src/logManager/Models/TokenResolverContext.cs`
- Exceptions: `/src/logManager/Exceptions/TokenResolutionException.cs`
- Tests: `/tests/logManager.Tests/Services/PathTokenResolverTests.cs`

### References

- [Source: docs/tech-spec-epic-1.md#Services and Modules]
- [Source: docs/tech-spec-epic-1.md#Data Models and Contracts - TokenResolverContext]
- [Source: docs/tech-spec-epic-1.md#AC2: Path Token Resolution]
- [Source: docs/epic-stories.md#Story 1.2]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
