# External APIs

## AWS S3 API
- **Purpose:** Long-term storage operations including duplicate detection, upload, and object verification
- **Documentation:** https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/s3.html
- **Base URL(s):** https://s3.amazonaws.com (region-specific endpoints auto-resolved by SDK)
- **Authentication:** Primary: IAM roles at server level; Secondary: -AccessKey/-SecretKey parameters
- **Rate Limits:** S3 request rate limits (3,500 PUT/COPY/POST/DELETE, 5,500 GET/HEAD per prefix per second)

**Key Endpoints Used:**
- `HEAD /{bucket}/{key}` - Check object existence for duplicate detection
- `PUT /{bucket}/{key}` - Upload compressed archive files
- `GET /{bucket}` - List objects for path template validation

**Integration Notes:**
- Credentials passed directly to .NET AWS cmdlets without creating AWS profiles
- S3 path templates support tokens: {SERVER}, {YEAR}, {MONTH}, {DAY}
- Retry logic implements exponential backoff for transient failures
- Regional endpoint optimization for enterprise multi-region deployments
