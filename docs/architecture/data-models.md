# Data Models

## LogItem

**Purpose:** Central data structure that evolves through the processing pipeline, carrying file/folder information and operation status across all cmdlets.

**Key Attributes:**
- FullPath: string - Complete file or folder path
- DateValue: string - Extracted date in YYYYMMDD format for grouping operations
- DaysOld: int - Age calculation from DateValue to current date
- FileSize: long? - File size in bytes (nullable, populated when -CalculateSize used)
- Error: string? - Error description (null = success, string = failure description)
- InS3: bool? - S3 archive status (nullable until checked)
- S3Location: string? - Final S3 path after upload
- ZipFileName: string? - Local zip file name after compression
- CompressedSize: long? - Zip file size after compression
- DestinationFolder: string? - Target folder after organization
- OrganizationStatus: string? - File move operation result
- ArchiveStatus: string? - S3 upload operation result
- RetentionAction: string? - Cleanup operation result

**Relationships:**
- Aggregated into date-based collections by Group-LogFilesByDate
- Shared across all cmdlets in both File Organization and Long-Term Storage workflows
- Memory-efficient evolution pattern adds properties as needed without object recreation

**C# Class Definition:**
```csharp
public class LogItem
{
    public string FullPath { get; set; }
    public string DateValue { get; set; }
    public int DaysOld { get; set; }
    public long? FileSize { get; set; }
    public string Error { get; set; }
    public bool? InS3 { get; set; }
    public string S3Location { get; set; }
    public string ZipFileName { get; set; }
    public long? CompressedSize { get; set; }
    public string DestinationFolder { get; set; }
    public string OrganizationStatus { get; set; }
    public string ArchiveStatus { get; set; }
    public string RetentionAction { get; set; }
}
```
