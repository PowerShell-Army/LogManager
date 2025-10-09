using System.Management.Automation;
using logManager.Models;
using logManager.Services;

namespace logManager.Cmdlets;

/// <summary>
/// <para type="synopsis">Calculates a date range from day-based parameters.</para>
/// <para type="description">
/// The Get-DateRange cmdlet calculates date ranges from simple integer day parameters (olderThan/youngerThan),
/// enabling users to define archival windows without dealing with complex date objects.
/// Returns DateRangeResult objects compatible with PowerShell pipeline chaining.
/// </para>
/// </summary>
[Cmdlet(VerbsCommon.Get, "DateRange")]
[OutputType(typeof(DateRangeResult[]))]
public class GetDateRangeCmdlet : PSCmdlet
{
    private readonly DateRangeCalculator _calculator = new();

    /// <summary>
    /// <para type="description">Number of days in the past for the start of the range (oldest date).</para>
    /// </summary>
    [Parameter(Mandatory = true, Position = 0, HelpMessage = "Number of days in the past for the oldest date in the range")]
    [ValidateRange(0, int.MaxValue)]
    public int OlderThan { get; set; }

    /// <summary>
    /// <para type="description">Number of days in the past for the end of the range (newest date). Default is 0 (today).</para>
    /// </summary>
    [Parameter(Mandatory = false, Position = 1, HelpMessage = "Number of days in the past for the newest date in the range (default: 0 = today)")]
    [ValidateRange(0, int.MaxValue)]
    public int YoungerThan { get; set; } = 0;

    /// <summary>
    /// <para type="description">The date criteria type: CreationDate or ModifiedDate. Default is ModifiedDate.</para>
    /// </summary>
    [Parameter(Mandatory = false, HelpMessage = "Date criteria to use: CreationDate or ModifiedDate")]
    public DateCriteriaType DateCriteria { get; set; } = DateCriteriaType.ModifiedDate;

    /// <summary>
    /// Processes the cmdlet request.
    /// </summary>
    protected override void ProcessRecord()
    {
        try
        {
            WriteVerbose($"Calculating date range: olderThan={OlderThan}, youngerThan={YoungerThan}, criteria={DateCriteria}");

            // Calculate the date range
            var results = _calculator.Calculate(OlderThan, YoungerThan, DateCriteria);

            WriteVerbose($"Calculated date range: {results[0].Date:yyyy-MM-dd} to {results[^1].Date:yyyy-MM-dd} ({results.Length} days)");

            // Write each result to the pipeline
            foreach (var result in results)
            {
                WriteObject(result);
            }
        }
        catch (ArgumentException ex)
        {
            var errorRecord = new ErrorRecord(
                ex,
                "DateRangeValidationError",
                ErrorCategory.InvalidArgument,
                null);

            ThrowTerminatingError(errorRecord);
        }
        catch (Exception ex)
        {
            var errorRecord = new ErrorRecord(
                ex,
                "DateRangeCalculationError",
                ErrorCategory.NotSpecified,
                null);

            ThrowTerminatingError(errorRecord);
        }
    }
}
