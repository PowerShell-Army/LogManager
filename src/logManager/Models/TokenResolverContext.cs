namespace logManager.Models;

/// <summary>
/// Context information required for resolving path tokens.
/// Contains the date being archived and optional server/application names.
/// </summary>
public record TokenResolverContext
{
    /// <summary>
    /// The date of the log being archived (NOT the current date).
    /// Used to resolve {year}, {month}, {day}, and {date} tokens.
    /// </summary>
    public required DateTime LogDate { get; init; }

    /// <summary>
    /// Server name for resolving {server} token.
    /// Optional - only required if template contains {server} token.
    /// </summary>
    public string? ServerName { get; init; }

    /// <summary>
    /// Application name for resolving {app} token.
    /// Optional - only required if template contains {app} token.
    /// </summary>
    public string? ApplicationName { get; init; }
}
