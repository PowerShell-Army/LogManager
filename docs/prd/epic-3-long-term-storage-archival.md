# Epic 3: Long-Term Storage & Archival

**Epic Goal:** Deliver complete long-term storage workflow enabling automated compression, S3 upload, duplicate detection, and retention-based cleanup for enterprise log archival requirements.

## Story 3.1: Test-LogFileInS3 Duplicate Detection
As a system administrator,
I want to check if log files are already archived in S3,
so that I can avoid redundant compression and upload operations.

**Acceptance Criteria:**
1. Test-LogFileInS3 cmdlet accepts LogItem objects and S3 configuration parameters
2. Supports both IAM role authentication and optional -AccessKey/-SecretKey parameters
3. Resolves S3 path templates with tokens: {SERVER}, {YEAR}, {MONTH}, {DAY}
4. Checks for existing YYYYMMDD.zip files at resolved S3 locations
5. Updates LogItem objects with InS3 status (true/false) and S3Location property
6. Handles S3 authentication failures gracefully with detailed error messages
7. Processes multiple date groups efficiently with batched S3 calls
8. Credentials passed directly to .NET AWS cmdlets without creating AWS profiles

## Story 3.2: Compress-LogFilesByDate with 7-Zip Integration
As a system administrator,
I want to compress log files by date into zip archives,
so that I can reduce storage space before uploading to long-term storage.

**Acceptance Criteria:**
1. Compress-LogFilesByDate cmdlet auto-detects 7-Zip installation across common paths
2. Falls back to .NET ZipFile compression when 7-Zip unavailable
3. Creates date-based zip files named YYYYMMDD.zip for each date group
4. Achieves target 70%+ compression ratios using 7-Zip when available
5. Updates LogItem objects with ZipFileName and CompressedSize properties
6. Implements fail-fast behavior: compression failure stops processing for that date group
7. Cleans up partial/failed zip files automatically
8. Validates zip file integrity before marking compression complete

## Story 3.3: Send-LogFilesToS3 Upload Management
As a system administrator,
I want to upload compressed log archives to S3 storage,
so that I can establish reliable long-term storage with proper path organization.

**Acceptance Criteria:**
1. Send-LogFilesToS3 cmdlet uploads zip files to resolved S3 paths from Test-LogFileInS3
2. Supports both IAM authentication and optional credential parameters
3. Updates LogItem objects with final S3Location and ArchiveStatus properties
4. Implements retry logic for transient S3 upload failures
5. Achieves 99%+ upload success rate under normal network conditions
6. Deletes local zip files after successful S3 upload (always clean up temp files)
7. Provides detailed progress reporting for large file uploads
8. Handles S3 bucket permissions and access errors gracefully

## Story 3.4: Remove-LogFilesAfterArchive Retention Management
As a system administrator,
I want to delete local files based on retention policies after successful archival,
so that I can manage local storage capacity while maintaining S3 backups.

**Acceptance Criteria:**
1. Remove-LogFilesAfterArchive cmdlet implements dual-condition deletion logic
2. Deletes files only when BOTH conditions met: InS3=true AND age > KeepDays parameter
3. Updates LogItem objects with RetentionAction property (Deleted/Retained)
4. Supports both individual files and entire date folders for deletion
5. Provides detailed logging of deletion decisions and actions taken
6. Implements safety checks to prevent accidental data loss
7. Handles file access permission issues gracefully
8. Allows dry-run mode with -WhatIf parameter for validation

## Story 3.5: Complete Long-Term Storage Integration
As a system administrator,
I want to execute the complete archival workflow in a single operation,
so that I can automate enterprise log management through Windows Task Scheduler.

**Acceptance Criteria:**
1. Integration demonstrates full workflow from discovery through cleanup
2. Processes files in date groups with independent failure handling
3. Memory usage remains under 2GB for 1M+ file processing throughout entire workflow
4. Completes within 2-hour maintenance window performance targets
5. Maintains detailed audit trail of all operations in LogItem objects
6. Error reporting enables targeted retry of failed operations
7. Integration testing validates complete workflow with realistic data volumes
8. Documentation includes Task Scheduler configuration examples
