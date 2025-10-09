namespace logManager.Models;

/// <summary>
/// Specifies which file date property to use for filtering.
/// </summary>
public enum DateCriteriaType
{
    /// <summary>
    /// Use the file or folder creation date.
    /// </summary>
    CreationDate,

    /// <summary>
    /// Use the file or folder last modified date.
    /// </summary>
    ModifiedDate
}
