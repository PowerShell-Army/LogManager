namespace logManager.Services;

/// <summary>
/// Service for generating consistent archive filenames.
/// Foundation layer - no dependencies on other services.
/// </summary>
public class ArchiveNamingService
{
    /// <summary>
    /// Generates an archive filename using the pattern: AppName-YYYYMMDD.zip
    /// </summary>
    /// <param name="appName">The application name.</param>
    /// <param name="date">The date for the archive.</param>
    /// <returns>Archive filename in the format "AppName-YYYYMMDD.zip"</returns>
    /// <exception cref="ArgumentException">Thrown when appName is null, empty, or contains invalid filename characters.</exception>
    public string GetArchiveName(string appName, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(appName))
        {
            throw new ArgumentException("Application name cannot be null or empty.", nameof(appName));
        }

        // Check for invalid filename characters
        var invalidChars = Path.GetInvalidFileNameChars();
        if (appName.IndexOfAny(invalidChars) >= 0)
        {
            throw new ArgumentException(
                $"Application name contains invalid filename characters. Invalid characters: {string.Join(", ", invalidChars.Select(c => $"'{c}'"))}",
                nameof(appName));
        }

        // Format: AppName-YYYYMMDD.zip
        var dateString = date.ToString("yyyyMMdd");
        return $"{appName}-{dateString}.zip";
    }
}
