# Error Handling Strategy

## General Approach
- **Error Model:** Single Error property per LogItem (null = success, string = failure description)
- **Exception Hierarchy:** Standard .NET exceptions with custom LogManagerException for business logic
- **Error Propagation:** Fail-fast per date group, continue processing other groups

## Logging Standards
- **Library:** Built-in PowerShell Write-Error, Write-Warning, Write-Verbose
- **Format:** Structured messages with correlation context
- **Levels:** Error (processing failures), Warning (non-blocking issues), Verbose (detailed tracing)
- **Required Context:**
  - Correlation ID: Date group identifier (YYYYMMDD)
  - Service Context: Cmdlet name and operation
  - User Context: File/folder path being processed

## Error Handling Patterns

### External API Errors
- **Retry Policy:** Exponential backoff (1s, 2s, 4s, 8s) for S3 transient failures
- **Circuit Breaker:** Not applicable (batch processing, not real-time service)
- **Timeout Configuration:** 30-second timeout per S3 operation, 5-minute timeout per compression
- **Error Translation:** AWS SDK exceptions mapped to user-friendly messages in LogItem.Error

### Business Logic Errors
- **Custom Exceptions:** LogManagerException, CompressionException, S3OperationException
- **User-Facing Errors:** Clear descriptions in LogItem.Error (e.g., "File access denied: C:\Logs\app.log")
- **Error Codes:** Simple text descriptions, no numeric codes (PowerShell convention)

### Data Consistency
- **Transaction Strategy:** Not applicable (stateless operations)
- **Compensation Logic:** Manual cleanup required for partial failures (documented in user guide)
- **Idempotency:** All operations are naturally idempotent (file moves, S3 checks, compression)
