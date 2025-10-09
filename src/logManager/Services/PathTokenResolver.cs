using logManager.Exceptions;
using logManager.Models;

namespace logManager.Services;

/// <summary>
/// Service for resolving path tokens in destination path templates.
/// Foundation layer - no dependencies on other services.
/// </summary>
public class PathTokenResolver
{
    /// <summary>
    /// Resolves tokens in a path template using the provided context.
    /// Supported tokens: {year}, {month}, {day}, {date}, {server}, {app}
    /// </summary>
    /// <param name="template">The path template containing tokens (e.g., "s3://bucket/{year}/{month}/{day}").</param>
    /// <param name="context">Context containing date and optional server/application names.</param>
    /// <returns>The resolved path with all tokens replaced.</returns>
    /// <exception cref="TokenResolutionException">Thrown when required context is missing for a token in the template.</exception>
    /// <exception cref="ArgumentNullException">Thrown when template or context is null.</exception>
    public string ResolveTokens(string template, TokenResolverContext context)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var result = template;

        // Date-based tokens (always available from LogDate)
        result = result.Replace("{year}", context.LogDate.Year.ToString("D4"));
        result = result.Replace("{month}", context.LogDate.Month.ToString("D2"));
        result = result.Replace("{day}", context.LogDate.Day.ToString("D2"));
        result = result.Replace("{date}", context.LogDate.ToString("yyyyMMdd"));

        // Server token (optional - only if template contains it)
        if (result.Contains("{server}"))
        {
            if (string.IsNullOrEmpty(context.ServerName))
            {
                throw new TokenResolutionException(
                    "Template contains {server} token but ServerName was not provided in context. " +
                    "Please provide a ServerName value in TokenResolverContext.");
            }
            result = result.Replace("{server}", context.ServerName);
        }

        // Application token (optional - only if template contains it)
        if (result.Contains("{app}"))
        {
            if (string.IsNullOrEmpty(context.ApplicationName))
            {
                throw new TokenResolutionException(
                    "Template contains {app} token but ApplicationName was not provided in context. " +
                    "Please provide an ApplicationName value in TokenResolverContext.");
            }
            result = result.Replace("{app}", context.ApplicationName);
        }

        return result;
    }
}
