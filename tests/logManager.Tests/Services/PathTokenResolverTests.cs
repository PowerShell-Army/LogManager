using logManager.Exceptions;
using logManager.Models;
using logManager.Services;
using Xunit;

namespace logManager.Tests.Services;

public class PathTokenResolverTests
{
    private readonly PathTokenResolver _resolver = new();

    [Fact]
    public void ResolveTokens_S3PathWithDateTokens_ReturnsResolvedPath()
    {
        // Arrange
        var template = "s3://bucket/{year}/{month}/{day}";
        var context = new TokenResolverContext
        {
            LogDate = new DateTime(2025, 10, 8)
        };

        // Act
        var result = _resolver.ResolveTokens(template, context);

        // Assert
        Assert.Equal("s3://bucket/2025/10/08", result);
    }

    [Fact]
    public void ResolveTokens_UNCPathWithAppAndYear_ReturnsResolvedPath()
    {
        // Arrange
        var template = @"\\server\{app}\{year}";
        var context = new TokenResolverContext
        {
            LogDate = new DateTime(2025, 10, 8),
            ApplicationName = "MyApp"
        };

        // Act
        var result = _resolver.ResolveTokens(template, context);

        // Assert
        Assert.Equal(@"\\server\MyApp\2025", result);
    }

    [Fact]
    public void ResolveTokens_ServerToken_ReplacesCorrectly()
    {
        // Arrange
        var template = @"\\{server}\share\logs";
        var context = new TokenResolverContext
        {
            LogDate = DateTime.Today,
            ServerName = "PROD-SVR01"
        };

        // Act
        var result = _resolver.ResolveTokens(template, context);

        // Assert
        Assert.Equal(@"\\PROD-SVR01\share\logs", result);
    }

    [Fact]
    public void ResolveTokens_AllTokens_ReplacesAll()
    {
        // Arrange
        var template = @"\\{server}\{app}\{year}\{month}\{day}\{date}";
        var context = new TokenResolverContext
        {
            LogDate = new DateTime(2025, 10, 8),
            ServerName = "SERVER01",
            ApplicationName = "TestApp"
        };

        // Act
        var result = _resolver.ResolveTokens(template, context);

        // Assert
        Assert.Equal(@"\\SERVER01\TestApp\2025\10\08\20251008", result);
    }

    [Fact]
    public void ResolveTokens_NoTokens_ReturnsUnchanged()
    {
        // Arrange
        var template = @"\\server\share\logs\archive.zip";
        var context = new TokenResolverContext
        {
            LogDate = DateTime.Today
        };

        // Act
        var result = _resolver.ResolveTokens(template, context);

        // Assert
        Assert.Equal(template, result);
    }

    [Fact]
    public void ResolveTokens_ServerTokenWithoutContext_ThrowsTokenResolutionException()
    {
        // Arrange
        var template = @"\\{server}\share";
        var context = new TokenResolverContext
        {
            LogDate = DateTime.Today
            // ServerName not provided
        };

        // Act & Assert
        var exception = Assert.Throws<TokenResolutionException>(() =>
            _resolver.ResolveTokens(template, context));

        Assert.Contains("{server}", exception.Message);
        Assert.Contains("ServerName", exception.Message);
    }

    [Fact]
    public void ResolveTokens_AppTokenWithoutContext_ThrowsTokenResolutionException()
    {
        // Arrange
        var template = "s3://bucket/{app}/logs";
        var context = new TokenResolverContext
        {
            LogDate = DateTime.Today
            // ApplicationName not provided
        };

        // Act & Assert
        var exception = Assert.Throws<TokenResolutionException>(() =>
            _resolver.ResolveTokens(template, context));

        Assert.Contains("{app}", exception.Message);
        Assert.Contains("ApplicationName", exception.Message);
    }

    [Fact]
    public void ResolveTokens_UsesLogDateNotCurrentDate()
    {
        // Arrange
        var pastDate = new DateTime(2020, 1, 1);
        var template = "{year}-{month}-{day}";
        var context = new TokenResolverContext
        {
            LogDate = pastDate
        };

        // Act
        var result = _resolver.ResolveTokens(template, context);

        // Assert
        Assert.Equal("2020-01-01", result);
        Assert.NotEqual(DateTime.Today.ToString("yyyy-MM-dd"), result);
    }

    [Fact]
    public void ResolveTokens_UNCPathWithYearMonth_ReturnsResolvedPath()
    {
        // Arrange
        var template = @"\\server\share\{year}\{month}";
        var context = new TokenResolverContext
        {
            LogDate = new DateTime(2025, 10, 8)
        };

        // Act
        var result = _resolver.ResolveTokens(template, context);

        // Assert
        Assert.Equal(@"\\server\share\2025\10", result);
    }

    [Fact]
    public void ResolveTokens_LocalPathWithAppAndDate_ReturnsResolvedPath()
    {
        // Arrange
        var template = @"D:\Logs\{app}\{date}";
        var context = new TokenResolverContext
        {
            LogDate = new DateTime(2025, 10, 8),
            ApplicationName = "MyApp"
        };

        // Act
        var result = _resolver.ResolveTokens(template, context);

        // Assert
        Assert.Equal(@"D:\Logs\MyApp\20251008", result);
    }

    [Fact]
    public void ResolveTokens_DateToken_FormatsAsYYYYMMDD()
    {
        // Arrange
        var template = "archive-{date}.zip";
        var context = new TokenResolverContext
        {
            LogDate = new DateTime(2025, 10, 8)
        };

        // Act
        var result = _resolver.ResolveTokens(template, context);

        // Assert
        Assert.Equal("archive-20251008.zip", result);
    }

    [Fact]
    public void ResolveTokens_MonthToken_ZeroPadded()
    {
        // Arrange
        var template = "{month}";
        var context = new TokenResolverContext
        {
            LogDate = new DateTime(2025, 3, 15) // March = 03
        };

        // Act
        var result = _resolver.ResolveTokens(template, context);

        // Assert
        Assert.Equal("03", result);
    }

    [Fact]
    public void ResolveTokens_DayToken_ZeroPadded()
    {
        // Arrange
        var template = "{day}";
        var context = new TokenResolverContext
        {
            LogDate = new DateTime(2025, 10, 5) // Day = 05
        };

        // Act
        var result = _resolver.ResolveTokens(template, context);

        // Assert
        Assert.Equal("05", result);
    }

    [Fact]
    public void ResolveTokens_CaseSensitive_DoesNotMatchIncorrectCase()
    {
        // Arrange
        var template = "{Year}/{Month}/{Day}"; // Wrong case
        var context = new TokenResolverContext
        {
            LogDate = new DateTime(2025, 10, 8)
        };

        // Act
        var result = _resolver.ResolveTokens(template, context);

        // Assert - Should NOT replace tokens with wrong case
        Assert.Equal("{Year}/{Month}/{Day}", result);
    }

    [Fact]
    public void ResolveTokens_NullTemplate_ThrowsArgumentNullException()
    {
        // Arrange
        var context = new TokenResolverContext
        {
            LogDate = DateTime.Today
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _resolver.ResolveTokens(null!, context));
    }

    [Fact]
    public void ResolveTokens_NullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var template = "s3://bucket/logs";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _resolver.ResolveTokens(template, null!));
    }
}
