namespace logManager.Models;

/// <summary>
/// Represents a single date in a calculated date range.
/// Returned by Get-DateRange cmdlet and used throughout the pipeline.
/// </summary>
public record DateRangeResult
{
    /// <summary>
    /// The date in the range.
    /// </summary>
    public required DateTime Date { get; init; }

    /// <summary>
    /// The date criteria type (CreationDate or ModifiedDate) associated with this range.
    /// </summary>
    public required DateCriteriaType Criteria { get; init; }
}
