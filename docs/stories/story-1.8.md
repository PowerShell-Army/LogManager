# Story 1.8: Implement Archive Existence Query Protocol (FR014)

Status: Draft

## Story

As a system function,
I want consistent archive checking across all destination types,
so that Test-ArchiveExists can work with S3/UNC/local uniformly.

## Acceptance Criteria

1. Provides abstraction for checking archive existence
2. Supports S3 (AWS SDK), UNC paths (.NET), local drives (.NET)
3. Accepts resolved path from FR013 and archive name from FR010
4. Returns boolean: archive exists or not
5. Handles authentication for S3 (IAM credentials)
6. Unit tests with mocked destinations

## Tasks / Subtasks

- [ ] Define IDestinationProvider abstraction (AC: #1, #3, #4)
  - [ ] Create IDestinationProvider interface
  - [ ] Define ArchiveExists method signature
  - [ ] Define DestinationType property
- [ ] Implement S3DestinationProvider (AC: #2, #5)
  - [ ] Create S3DestinationProvider class
  - [ ] Use AWS.Tools.S3 Get-S3Object cmdlet
  - [ ] Parse s3:// URLs to extract bucket and key
  - [ ] Handle IAM credentials (default chain or explicit)
  - [ ] Return true if object exists, false otherwise
- [ ] Implement UncDestinationProvider (AC: #2)
  - [ ] Create UncDestinationProvider class
  - [ ] Use .NET File.Exists for UNC paths
  - [ ] Handle UNC path formatting
- [ ] Implement LocalDestinationProvider (AC: #2)
  - [ ] Create LocalDestinationProvider class
  - [ ] Use .NET File.Exists for local paths
- [ ] Create unit tests (AC: #6)
  - [ ] xUnit: Test S3DestinationProvider with mocked PowerShell invoker
  - [ ] xUnit: Test UncDestinationProvider with mocked IFileSystem
  - [ ] xUnit: Test LocalDestinationProvider with mocked IFileSystem
  - [ ] Test path parsing for all types
  - [ ] Test authentication scenarios for S3

## Dev Notes

- Discovery layer - depends on FR010 (ArchiveNaming), FR013 (PathTokenResolver)
- Required by FR002 (Test-ArchiveExists)
- Multi-destination abstraction layer
- S3 access requires AWS.Tools.S3 PowerShell module
- Uses IFileSystem abstraction for UNC/Local (testability)
- S3 implementation needs IPowerShellInvoker abstraction for testing

### Project Structure Notes

- Interface: `/src/logManager/Abstractions/IDestinationProvider.cs`
- Implementations:
  - `/src/logManager/Infrastructure/S3DestinationProvider.cs`
  - `/src/logManager/Infrastructure/UncDestinationProvider.cs`
  - `/src/logManager/Infrastructure/LocalDestinationProvider.cs`
- PowerShell abstraction: `/src/logManager/Abstractions/IPowerShellInvoker.cs`
- Tests: `/tests/logManager.Tests/Infrastructure/`

### References

- [Source: docs/tech-spec-epic-1.md#Abstractions - IDestinationProvider]
- [Source: docs/tech-spec-epic-1.md#Services - ArchiveExistenceChecker]
- [Source: docs/epic-stories.md#Story 1.8]
- [Source: docs/tech-spec-epic-1.md#Dependencies - AWS S3]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
