namespace logManager.Exceptions;

/// <summary>
/// Base exception class for all logManager module exceptions.
/// Provides a common base for targeted exception handling in user scripts.
/// </summary>
public class LogManagerException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogManagerException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public LogManagerException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogManagerException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    public LogManagerException(string message, Exception inner) : base(message, inner)
    {
    }
}
