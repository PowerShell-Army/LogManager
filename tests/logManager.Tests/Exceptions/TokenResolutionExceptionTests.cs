using logManager.Exceptions;
using Xunit;

namespace logManager.Tests.Exceptions;

public class TokenResolutionExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        // Arrange
        var message = "Token resolution failed: missing {server} context";

        // Act
        var exception = new TokenResolutionException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void TokenResolutionException_InheritsFromLogManagerException()
    {
        // Arrange & Act
        var exception = new TokenResolutionException("Test");

        // Assert
        Assert.IsAssignableFrom<LogManagerException>(exception);
    }

    [Fact]
    public void TokenResolutionException_CanBeCaughtAsLogManagerException()
    {
        // Arrange
        var thrownException = false;
        var caughtAsLogManagerException = false;

        // Act
        try
        {
            throw new TokenResolutionException("Missing token context");
        }
        catch (LogManagerException)
        {
            thrownException = true;
            caughtAsLogManagerException = true;
        }

        // Assert
        Assert.True(thrownException);
        Assert.True(caughtAsLogManagerException);
    }
}
