using logManager.Services;
using Xunit;

namespace logManager.Tests.Services;

public class ArchiveNamingServiceTests
{
    private readonly ArchiveNamingService _service = new();

    [Fact]
    public void GetArchiveName_ValidAppNameAndDate_ReturnsExpectedFormat()
    {
        // Arrange
        var appName = "MyApp";
        var date = new DateTime(2025, 10, 8);

        // Act
        var result = _service.GetArchiveName(appName, date);

        // Assert
        Assert.Equal("MyApp-20251008.zip", result);
    }

    [Fact]
    public void GetArchiveName_AppNameWithSpaces_PreservesSpaces()
    {
        // Arrange
        var appName = "My App";
        var date = new DateTime(2025, 10, 8);

        // Act
        var result = _service.GetArchiveName(appName, date);

        // Assert
        Assert.Equal("My App-20251008.zip", result);
    }

    [Fact]
    public void GetArchiveName_DateFormatting_IsYYYYMMDD()
    {
        // Arrange
        var appName = "TestApp";
        var date = new DateTime(2025, 10, 8);

        // Act
        var result = _service.GetArchiveName(appName, date);

        // Assert
        Assert.Contains("20251008", result);
        Assert.DoesNotContain("-10-", result); // No separators in date
        Assert.DoesNotContain("/", result);
    }

    [Fact]
    public void GetArchiveName_Month1Through9_ZeroPadded()
    {
        // Arrange
        var appName = "App";
        var date = new DateTime(2025, 3, 15); // March = 03

        // Act
        var result = _service.GetArchiveName(appName, date);

        // Assert
        Assert.Equal("App-20250315.zip", result);
    }

    [Fact]
    public void GetArchiveName_Day1Through9_ZeroPadded()
    {
        // Arrange
        var appName = "App";
        var date = new DateTime(2025, 10, 5); // Day = 05

        // Act
        var result = _service.GetArchiveName(appName, date);

        // Assert
        Assert.Equal("App-20251005.zip", result);
    }

    [Fact]
    public void GetArchiveName_SameInputs_ReturnsConsistentResults()
    {
        // Arrange
        var appName = "ConsistentApp";
        var date = new DateTime(2025, 10, 8);

        // Act
        var result1 = _service.GetArchiveName(appName, date);
        var result2 = _service.GetArchiveName(appName, date);
        var result3 = _service.GetArchiveName(appName, date);

        // Assert
        Assert.Equal(result1, result2);
        Assert.Equal(result2, result3);
    }

    [Fact]
    public void GetArchiveName_NullAppName_ThrowsArgumentException()
    {
        // Arrange
        var date = DateTime.Today;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _service.GetArchiveName(null!, date));

        Assert.Equal("appName", exception.ParamName);
    }

    [Fact]
    public void GetArchiveName_EmptyAppName_ThrowsArgumentException()
    {
        // Arrange
        var date = DateTime.Today;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _service.GetArchiveName("", date));

        Assert.Equal("appName", exception.ParamName);
    }

    [Fact]
    public void GetArchiveName_WhitespaceAppName_ThrowsArgumentException()
    {
        // Arrange
        var date = DateTime.Today;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _service.GetArchiveName("   ", date));

        Assert.Equal("appName", exception.ParamName);
    }

    [Theory]
    [InlineData("App<Name")]  // < is invalid
    [InlineData("App>Name")]  // > is invalid
    [InlineData("App:Name")]  // : is invalid
    [InlineData("App\"Name")] // " is invalid
    [InlineData("App/Name")]  // / is invalid
    [InlineData("App\\Name")] // \ is invalid
    [InlineData("App|Name")]  // | is invalid
    [InlineData("App?Name")]  // ? is invalid
    [InlineData("App*Name")]  // * is invalid
    public void GetArchiveName_InvalidFilenameCharacters_ThrowsArgumentException(string invalidAppName)
    {
        // Arrange
        var date = DateTime.Today;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            _service.GetArchiveName(invalidAppName, date));

        Assert.Equal("appName", exception.ParamName);
        Assert.Contains("invalid filename characters", exception.Message.ToLower());
    }

    [Fact]
    public void GetArchiveName_VariousDates_FormatsCorrectly()
    {
        // Arrange & Act & Assert
        Assert.Equal("App-20250101.zip", _service.GetArchiveName("App", new DateTime(2025, 1, 1)));
        Assert.Equal("App-20251231.zip", _service.GetArchiveName("App", new DateTime(2025, 12, 31)));
        Assert.Equal("App-20240229.zip", _service.GetArchiveName("App", new DateTime(2024, 2, 29))); // Leap year
    }
}
