using logManager.Models;
using Xunit;

namespace logManager.Tests.Models;

public class ArchiveInfoTests
{
    [Fact]
    public void ArchiveInfo_Creation_SetsAllProperties()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var archiveFile = new FileInfo(tempFile);
        var engine = "7-Zip";
        var compressedSize = 1024L;
        var originalSize = 4096L;
        var duration = TimeSpan.FromSeconds(5);

        // Act
        var archiveInfo = new ArchiveInfo
        {
            ArchiveFile = archiveFile,
            CompressionEngine = engine,
            CompressedSize = compressedSize,
            OriginalSize = originalSize,
            CompressionDuration = duration
        };

        // Assert
        Assert.Equal(archiveFile, archiveInfo.ArchiveFile);
        Assert.Equal(engine, archiveInfo.CompressionEngine);
        Assert.Equal(compressedSize, archiveInfo.CompressedSize);
        Assert.Equal(originalSize, archiveInfo.OriginalSize);
        Assert.Equal(duration, archiveInfo.CompressionDuration);

        // Cleanup
        File.Delete(tempFile);
    }

    [Theory]
    [InlineData("7-Zip")]
    [InlineData("SharpCompress")]
    public void ArchiveInfo_SupportsStandardCompressionEngines(string engine)
    {
        // Arrange
        var tempFile = Path.GetTempFileName();

        // Act
        var archiveInfo = new ArchiveInfo
        {
            ArchiveFile = new FileInfo(tempFile),
            CompressionEngine = engine,
            CompressedSize = 1000,
            OriginalSize = 2000,
            CompressionDuration = TimeSpan.FromSeconds(1)
        };

        // Assert
        Assert.Equal(engine, archiveInfo.CompressionEngine);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void ArchiveInfo_CompressionMetrics_TracksPerformance()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var originalSize = 10000L;
        var compressedSize = 2500L;
        var duration = TimeSpan.FromMilliseconds(250);

        // Act
        var archiveInfo = new ArchiveInfo
        {
            ArchiveFile = new FileInfo(tempFile),
            CompressionEngine = "SharpCompress",
            CompressedSize = compressedSize,
            OriginalSize = originalSize,
            CompressionDuration = duration
        };

        // Assert - Calculate compression ratio
        var compressionRatio = (double)compressedSize / originalSize;
        Assert.Equal(0.25, compressionRatio); // 25% of original size

        // Assert - Performance metrics available
        Assert.Equal(250, archiveInfo.CompressionDuration.TotalMilliseconds);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void ArchiveInfo_IsRecord_SupportsValueEquality()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var archiveFile = new FileInfo(tempFile);

        var info1 = new ArchiveInfo
        {
            ArchiveFile = archiveFile,
            CompressionEngine = "7-Zip",
            CompressedSize = 1024,
            OriginalSize = 4096,
            CompressionDuration = TimeSpan.FromSeconds(5)
        };

        var info2 = new ArchiveInfo
        {
            ArchiveFile = archiveFile,
            CompressionEngine = "7-Zip",
            CompressedSize = 1024,
            OriginalSize = 4096,
            CompressionDuration = TimeSpan.FromSeconds(5)
        };

        // Act & Assert
        Assert.Equal(info1, info2);

        // Cleanup
        File.Delete(tempFile);
    }
}
