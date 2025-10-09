namespace logManager.Models;

/// <summary>
/// Represents a log file discovered by Find-LogFiles cmdlet.
/// Pipeline-compatible for chaining to downstream functions like Compress-Logs.
/// </summary>
public record LogFileInfo
{
    /// <summary>
    /// The .NET FileInfo object for the discovered log file.
    /// </summary>
    public required FileInfo File { get; init; }

    /// <summary>
    /// The date extracted from the file (either CreationTime or LastWriteTime).
    /// This is the date that matched the search criteria.
    /// </summary>
    public required DateTime LogDate { get; init; }

    /// <summary>
    /// Indicates which file date property was used for matching (CreationDate or ModifiedDate).
    /// </summary>
    public required DateCriteriaType DateSource { get; init; }
}
