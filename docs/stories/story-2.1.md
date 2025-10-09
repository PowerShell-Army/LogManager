# Story 2.1: Implement Send-ToS3 Backend Function (FR015)

Status: Draft

## Story

As a backend transfer function,
I want to transfer archives to AWS S3 storage,
so that Send-Archive dispatcher can route S3 destinations to me.

## Acceptance Criteria

1. Accepts archive file path and S3 destination parameters
2. Uses AWS SDK for .NET / AWS.Tools.S3 PowerShell module
3. Supports IAM credentials (default)
4. Supports explicit access key/secret key parameters
5. Uses FR013 for path token resolution
6. Uploads to S3 with resolved key paths
7. Returns success/failure status with details
8. Unit tests with S3 mock

## Tasks / Subtasks

- [ ] Implement S3TransferService (AC: #1, #2, #3, #4, #5, #6, #7)
  - [ ] Create S3TransferService implementing ITransferService
  - [ ] Parse s3:// URLs to extract bucket and key
  - [ ] Use IPowerShellInvoker to call Write-S3Object cmdlet
  - [ ] Support explicit AWS credentials (AccessKey/SecretKey/Region)
  - [ ] Support default credential chain (when credentials not provided)
  - [ ] Return TransferResult with success confirmation
  - [ ] Measure transfer duration and bytes transferred
- [ ] Create IPowerShellInvoker abstraction (AC: #8)
  - [ ] Define IPowerShellInvoker interface
  - [ ] Create PowerShellInvokerWrapper implementation
  - [ ] Support InvokeCommand method for calling PowerShell cmdlets
- [ ] Create unit tests (AC: #8)
  - [ ] xUnit: S3TransferService with mocked IPowerShellInvoker
  - [ ] Test s3://bucket/key parsing
  - [ ] Test Write-S3Object invocation with correct parameters
  - [ ] Test explicit credential passing
  - [ ] Test default credential usage
  - [ ] Test transfer failure handling

## Dev Notes

- Distribution layer backend - depends on FR005 (Compress-Logs), FR013 (PathTokenResolver)
- AWS SDK integration required via AWS.Tools.S3 PowerShell module
- Uses Write-S3Object cmdlet for uploads
- Multipart upload for large files (future enhancement, post-MVP)
- Single responsibility: S3 transfers only
- **Testing Configuration:** Use environment variables: $env:AWS_Access, $env:AWS_Secret, $env:AWS_Region, $env:AWS_Bucket
- **NO OTHER S3 LOCATION CAN BE USED FOR TESTING**

### Project Structure Notes

- Service: `/src/logManager/Services/S3TransferService.cs`
- Interface: `/src/logManager/Abstractions/ITransferService.cs`
- Interface: `/src/logManager/Abstractions/IPowerShellInvoker.cs`
- Implementation: `/src/logManager/Infrastructure/PowerShellInvokerWrapper.cs`
- Model: `/src/logManager/Models/TransferResult.cs`
- Exception: `/src/logManager/Exceptions/TransferException.cs`
- Tests:
  - `/tests/logManager.Tests/Services/S3TransferServiceTests.cs`
  - `/tests/logManager.Tests/Integration/Send-Archive-S3.Tests.ps1`

### References

- [Source: docs/tech-spec-epic-2.md#Services and Modules - S3TransferService]
- [Source: docs/tech-spec-epic-2.md#AC13: Send-Archive to S3]
- [Source: docs/tech-spec-epic-2.md#Data Models - TransferResult]
- [Source: docs/epic-stories.md#Story 2.1]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
