namespace logManager.Models;

/// <summary>
/// Represents a dated log folder discovered by Find-LogFolders cmdlet.
/// Pipeline-compatible for chaining to downstream functions like Compress-Logs.
/// </summary>
public record LogFolderInfo
{
    /// <summary>
    /// The .NET DirectoryInfo object for the discovered log folder.
    /// </summary>
    public required DirectoryInfo Folder { get; init; }

    /// <summary>
    /// The date parsed from the folder name.
    /// </summary>
    public required DateTime ParsedDate { get; init; }

    /// <summary>
    /// The folder naming pattern that was detected and parsed.
    /// Either "YYYYMMDD" (e.g., 20251008) or "YYYY-MM-DD" (e.g., 2025-10-08).
    /// </summary>
    public required string FolderNamePattern { get; init; }
}
