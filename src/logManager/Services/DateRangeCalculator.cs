using logManager.Models;

namespace logManager.Services;

/// <summary>
/// Service for calculating date ranges based on day-based parameters.
/// Foundation layer - no dependencies on other services.
/// </summary>
public class DateRangeCalculator
{
    /// <summary>
    /// Calculates a date range from today going backwards based on olderThan and youngerThan parameters.
    /// </summary>
    /// <param name="olderThan">Number of days in the past for the start of the range (oldest date).</param>
    /// <param name="youngerThan">Number of days in the past for the end of the range (newest date). Default is 0 (today).</param>
    /// <param name="criteria">The date criteria type to associate with the results.</param>
    /// <returns>Array of DateRangeResult objects representing each day in the range (inclusive).</returns>
    /// <exception cref="ArgumentException">Thrown when olderThan is less than youngerThan.</exception>
    public DateRangeResult[] Calculate(int olderThan, int youngerThan, DateCriteriaType criteria)
    {
        // Validate parameters
        if (olderThan < youngerThan)
        {
            throw new ArgumentException(
                $"olderThan ({olderThan}) must be greater than or equal to youngerThan ({youngerThan})",
                nameof(olderThan));
        }

        // Calculate the date range
        var today = DateTime.Today;
        var startDate = today.AddDays(-olderThan);  // Oldest date (furthest in the past)
        var endDate = today.AddDays(-youngerThan);  // Newest date (closest to today)

        // Generate the range (inclusive)
        var dayCount = (endDate - startDate).Days + 1;
        var results = new DateRangeResult[dayCount];

        for (int i = 0; i < dayCount; i++)
        {
            results[i] = new DateRangeResult
            {
                Date = startDate.AddDays(i),
                Criteria = criteria
            };
        }

        return results;
    }
}
