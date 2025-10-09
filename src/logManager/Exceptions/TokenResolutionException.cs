namespace logManager.Exceptions;

/// <summary>
/// Exception thrown when path token resolution fails.
/// This occurs when required context values are missing or when tokens cannot be resolved.
/// </summary>
public class TokenResolutionException : LogManagerException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenResolutionException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the token resolution error.</param>
    public TokenResolutionException(string message) : base(message)
    {
    }
}
