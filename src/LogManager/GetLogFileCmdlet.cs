using System;
using System.IO;
using System.Management.Automation;

namespace LogManager
{
    /// <summary>
    /// Gets log files from a directory with optional filtering by age.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "LogFile")]
    [OutputType(typeof(FileInfo))]
    public class GetLogFileCmdlet : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Root directory path to search for log files.")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; } = string.Empty;

        [Parameter(
            HelpMessage = "Search subdirectories recursively.")]
        public SwitchParameter Recurse { get; set; }

        [Parameter(
            HelpMessage = "Filter files older than this many days.")]
        [ValidateRange(0, int.MaxValue)]
        public int? OlderThan { get; set; }

        [Parameter(
            HelpMessage = "Filter files younger than this many days.")]
        [ValidateRange(0, int.MaxValue)]
        public int? YoungerThan { get; set; }

        [Parameter(
            HelpMessage = "Use file creation date for age comparison instead of last modified date.")]
        public SwitchParameter UseCreationDate { get; set; }

        [Parameter(
            HelpMessage = "File pattern to match (e.g., *.log, *.txt). Defaults to all files.")]
        public string Pattern { get; set; } = "*";

        protected override void ProcessRecord()
        {
            try
            {
                // Resolve the path
                string resolvedPath = GetUnresolvedProviderPathFromPSPath(Path);

                if (!Directory.Exists(resolvedPath))
                {
                    WriteError(new ErrorRecord(
                        new DirectoryNotFoundException($"Directory not found: {resolvedPath}"),
                        "DirectoryNotFound",
                        ErrorCategory.ObjectNotFound,
                        Path));
                    return;
                }

                // Set search option
                SearchOption searchOption = Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                // Get files
                string[] files = Directory.GetFiles(resolvedPath, Pattern, searchOption);

                DateTime now = DateTime.Now;
                int filesFound = 0;

                foreach (string filePath in files)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(filePath);

                        // Determine which date to use
                        DateTime fileDate = UseCreationDate ? fileInfo.CreationTime : fileInfo.LastWriteTime;

                        // Calculate age in days
                        double ageInDays = (now - fileDate).TotalDays;

                        // Apply age filters
                        if (OlderThan.HasValue && ageInDays < OlderThan.Value)
                            continue;

                        if (YoungerThan.HasValue && ageInDays > YoungerThan.Value)
                            continue;

                        WriteObject(fileInfo);
                        filesFound++;
                    }
                    catch (Exception ex)
                    {
                        WriteWarning($"Could not process file {filePath}: {ex.Message}");
                    }
                }

                WriteVerbose($"Found {filesFound} log file(s) in {resolvedPath}");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(
                    ex,
                    "GetLogFileError",
                    ErrorCategory.ReadError,
                    Path));
            }
        }
    }
}
