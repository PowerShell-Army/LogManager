# Requirements

## Functional

1. FR1: Get-LogFiles cmdlet discovers files with -DateProperty parameter and -ExcludeCurrentDay functionality
2. FR2: Get-LogFolders cmdlet discovers date-named folders with -ExcludeCurrentDay functionality
3. FR3: Group-LogFilesByDate cmdlet groups objects by DateValue for per-date processing
4. FR4: Move-LogFilesToDateFolders cmdlet moves files to DestinationPath/YYYYMMDD/ folder structure
5. FR5: Test-LogFileInS3 cmdlet checks S3 for existing archives and updates objects with InS3 status
6. FR6: Compress-LogFilesByDate cmdlet auto-detects 7-Zip installation with .NET ZipFile fallback
7. FR7: Send-LogFilesToS3 cmdlet uploads zip files and updates objects with S3Location
8. FR8: Remove-LogFilesAfterArchive cmdlet implements dual-condition deletion (InS3 + retention period)
9. FR9: System uses evolving PSCustomObject with properties: FullPath, DateValue, DaysOld, FileSize, Error
10. FR10: System processes each date group independently with fail-fast behavior per group

## Non Functional

1. NFR1: System must handle 1 million+ files within 2-hour maintenance windows
2. NFR2: Peak memory consumption must remain under 2GB for million-file processing
3. NFR3: System must achieve 99%+ success rate for automated scheduled executions
4. NFR4: Compression must achieve average 70%+ file size reduction through 7-Zip
5. NFR5: S3 upload success rate must be 99%+ for long-term storage transfers
6. NFR6: System must work with .NET Framework 4.8 and PowerShell 5.1+
7. NFR7: Error handling must provide clear failure descriptions with single Error property (null = success, string = failure)
8. NFR8: System must exclude current day log files to avoid production conflicts
