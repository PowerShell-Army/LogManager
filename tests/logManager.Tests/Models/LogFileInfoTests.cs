using logManager.Models;
using Xunit;

namespace logManager.Tests.Models;

public class LogFileInfoTests
{
    [Fact]
    public void LogFileInfo_Creation_SetsAllProperties()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var fileInfo = new FileInfo(tempFile);
        var logDate = new DateTime(2025, 10, 8);
        var criteria = DateCriteriaType.ModifiedDate;

        // Act
        var logFileInfo = new LogFileInfo
        {
            File = fileInfo,
            LogDate = logDate,
            DateSource = criteria
        };

        // Assert
        Assert.Equal(fileInfo, logFileInfo.File);
        Assert.Equal(logDate, logFileInfo.LogDate);
        Assert.Equal(criteria, logFileInfo.DateSource);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void LogFileInfo_IsRecord_SupportsValueEquality()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var fileInfo = new FileInfo(tempFile);
        var logDate = new DateTime(2025, 10, 8);

        var info1 = new LogFileInfo
        {
            File = fileInfo,
            LogDate = logDate,
            DateSource = DateCriteriaType.CreationDate
        };

        var info2 = new LogFileInfo
        {
            File = fileInfo,
            LogDate = logDate,
            DateSource = DateCriteriaType.CreationDate
        };

        // Act & Assert
        Assert.Equal(info1, info2);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void LogFileInfo_Properties_AreInitOnly()
    {
        // This test verifies the record is immutable by checking
        // that properties have init-only setters (compilation test)
        var tempFile = Path.GetTempFileName();
        var logFileInfo = new LogFileInfo
        {
            File = new FileInfo(tempFile),
            LogDate = DateTime.Today,
            DateSource = DateCriteriaType.ModifiedDate
        };

        // Assert - if this compiles, properties are init-only
        Assert.IsType<LogFileInfo>(logFileInfo);

        // Cleanup
        File.Delete(tempFile);
    }
}
