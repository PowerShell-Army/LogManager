# Test Strategy and Standards

## Testing Philosophy
- **Approach:** Test-after development with comprehensive coverage for critical paths
- **Coverage Goals:** 80%+ unit test coverage for service layer, 100% cmdlet integration testing
- **Test Pyramid:** Heavy integration testing (PowerShell workflows), moderate unit testing (C# services), minimal E2E (full workflows with real S3)

## Test Types and Organization

### Unit Tests
- **Framework:** MSTest 3.0+ with Moq for mocking
- **File Convention:** `{ClassName}Tests.cs` (e.g., `LogItemTests.cs`, `CompressionServiceTests.cs`)
- **Location:** `tests/LogManager.UnitTests/` mirroring source structure
- **Mocking Library:** Moq 4.0+ for AWS SDK and file system mocking
- **Coverage Requirement:** 80%+ for Services and Models layers

**AI Agent Requirements:**
- Generate tests for all public methods in service classes
- Cover edge cases (null inputs, empty collections, file access denied)
- Follow AAA pattern (Arrange, Act, Assert)
- Mock all external dependencies (AWS SDK, file system operations)

### Integration Tests
- **Scope:** Full PowerShell cmdlet workflows with test data
- **Location:** `tests/LogManager.IntegrationTests/`
- **Test Infrastructure:**
  - **File System:** Temporary test directories with sample log files
  - **AWS S3:** Mocked S3 using LocalStack or AWS SDK mocks for CI/CD
  - **7-Zip:** Test both 7-Zip present and absent scenarios

### End-to-End Tests
- **Framework:** Pester 5.0+ for PowerShell workflow testing
- **Scope:** Complete workflows from file discovery through archival
- **Environment:** Dedicated test environment with controlled S3 bucket
- **Test Data:** Generated test files ranging from small sets (100 files) to large sets (10,000+ files)

## Test Data Management
- **Strategy:** Generated test files with predictable patterns and controlled dates
- **Fixtures:** `tests/LogManager.IntegrationTests/TestData/` with date-based sample files
- **Factories:** PowerShell scripts to generate test log files with various patterns
- **Cleanup:** Automatic cleanup of test files and temporary S3 objects after test completion

## Continuous Testing
- **CI Integration:** GitHub Actions running unit tests on every commit, integration tests on PR
- **Performance Tests:** Weekly automated runs of million-file processing scenarios
- **Security Tests:** PowerShell Script Analyzer for security best practices validation
