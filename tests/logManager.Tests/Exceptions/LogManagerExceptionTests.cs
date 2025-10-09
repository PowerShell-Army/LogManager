using logManager.Exceptions;
using Xunit;

namespace logManager.Tests.Exceptions;

public class LogManagerExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var exception = new LogManagerException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInner_SetsMessageAndInnerException()
    {
        // Arrange
        var message = "Test error message";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new LogManagerException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void LogManagerException_InheritsFromException()
    {
        // Arrange & Act
        var exception = new LogManagerException("Test");

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }
}
