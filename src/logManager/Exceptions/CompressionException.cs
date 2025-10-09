namespace logManager.Exceptions;

/// <summary>
/// Exception thrown when compression operations fail.
/// This includes failures from both 7-Zip CLI and SharpCompress library.
/// </summary>
public class CompressionException : LogManagerException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompressionException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the compression error.</param>
    public CompressionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompressionException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception (e.g., Process execution error, library error).</param>
    public CompressionException(string message, Exception inner) : base(message, inner)
    {
    }
}
