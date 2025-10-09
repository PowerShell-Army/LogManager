using logManager.Exceptions;
using Xunit;

namespace logManager.Tests.Exceptions;

public class CompressionExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        // Arrange
        var message = "Compression failed";

        // Act
        var exception = new CompressionException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInner_SetsMessageAndInnerException()
    {
        // Arrange
        var message = "Compression failed";
        var innerException = new InvalidOperationException("7-Zip process exited with code 2");

        // Act
        var exception = new CompressionException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void CompressionException_InheritsFromLogManagerException()
    {
        // Arrange & Act
        var exception = new CompressionException("Test");

        // Assert
        Assert.IsAssignableFrom<LogManagerException>(exception);
    }

    [Fact]
    public void CompressionException_CanBeCaughtAsLogManagerException()
    {
        // Arrange
        var thrownException = false;
        var caughtAsLogManagerException = false;

        // Act
        try
        {
            throw new CompressionException("Test compression error");
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
