namespace logManager.Models;

/// <summary>
/// Represents a compressed archive created by Compress-Logs cmdlet.
/// Pipeline-compatible for chaining to downstream functions like Send-Archive.
/// Includes compression metrics for observability.
/// </summary>
public record ArchiveInfo
{
    /// <summary>
    /// The .NET FileInfo object for the created archive file.
    /// </summary>
    public required FileInfo ArchiveFile { get; init; }

    /// <summary>
    /// The compression engine used to create the archive.
    /// Either "7-Zip" or "SharpCompress".
    /// </summary>
    public required string CompressionEngine { get; init; }

    /// <summary>
    /// The size of the compressed archive in bytes.
    /// </summary>
    public required long CompressedSize { get; init; }

    /// <summary>
    /// The total size of the source files before compression in bytes.
    /// </summary>
    public required long OriginalSize { get; init; }

    /// <summary>
    /// How long the compression operation took.
    /// Useful for performance monitoring and optimization.
    /// </summary>
    public required TimeSpan CompressionDuration { get; init; }
}
