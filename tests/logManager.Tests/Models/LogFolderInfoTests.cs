using logManager.Models;
using Xunit;

namespace logManager.Tests.Models;

public class LogFolderInfoTests
{
    [Fact]
    public void LogFolderInfo_Creation_SetsAllProperties()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var dirInfo = new DirectoryInfo(tempDir);
        var parsedDate = new DateTime(2025, 10, 8);
        var pattern = "YYYYMMDD";

        // Act
        var logFolderInfo = new LogFolderInfo
        {
            Folder = dirInfo,
            ParsedDate = parsedDate,
            FolderNamePattern = pattern
        };

        // Assert
        Assert.Equal(dirInfo, logFolderInfo.Folder);
        Assert.Equal(parsedDate, logFolderInfo.ParsedDate);
        Assert.Equal(pattern, logFolderInfo.FolderNamePattern);

        // Cleanup
        Directory.Delete(tempDir);
    }

    [Fact]
    public void LogFolderInfo_IsRecord_SupportsValueEquality()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var dirInfo = new DirectoryInfo(tempDir);

        var info1 = new LogFolderInfo
        {
            Folder = dirInfo,
            ParsedDate = new DateTime(2025, 10, 8),
            FolderNamePattern = "YYYY-MM-DD"
        };

        var info2 = new LogFolderInfo
        {
            Folder = dirInfo,
            ParsedDate = new DateTime(2025, 10, 8),
            FolderNamePattern = "YYYY-MM-DD"
        };

        // Act & Assert
        Assert.Equal(info1, info2);

        // Cleanup
        Directory.Delete(tempDir);
    }

    [Theory]
    [InlineData("YYYYMMDD")]
    [InlineData("YYYY-MM-DD")]
    public void LogFolderInfo_SupportsStandardFolderPatterns(string pattern)
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        // Act
        var logFolderInfo = new LogFolderInfo
        {
            Folder = new DirectoryInfo(tempDir),
            ParsedDate = DateTime.Today,
            FolderNamePattern = pattern
        };

        // Assert
        Assert.Equal(pattern, logFolderInfo.FolderNamePattern);

        // Cleanup
        Directory.Delete(tempDir);
    }
}
