# Epic 1: Foundation & Core Discovery

**Epic Goal:** Establish foundational project infrastructure, automated CI/CD pipeline, comprehensive documentation, and PowerShell module structure while delivering immediate value through file and folder discovery cmdlets that administrators can use for log analysis and inventory tasks.

## Story 1.1: Project Setup & C# Object Model
As a developer,
I want to create the PowerShell binary module project structure with a custom C# class,
so that I have a foundation for building all cmdlets with shared data objects.

**Acceptance Criteria:**
1. Visual Studio project created for PowerShell binary module (.dll) targeting .NET Framework 4.8
2. Custom C# class LogItem created with properties: FullPath, DateValue, DaysOld, FileSize, Error
3. PowerShell module manifest (.psd1) configured with module metadata and cmdlet exports
4. Basic project structure includes folders for cmdlets, classes, tests, and documentation
5. Solution builds successfully and produces importable PowerShell module
6. XML documentation comments added to all public classes and methods for PowerShell help generation
7. PowerShell help documentation automatically generated via PlatyPS or built-in help system
8. GitHub Actions workflow created for automated build, test, and documentation generation

## Story 1.2: Get-LogFiles Discovery Cmdlet
As a system administrator,
I want to discover log files with date-based filtering,
so that I can inventory and analyze log files by age without manual folder browsing.

**Acceptance Criteria:**
1. Get-LogFiles cmdlet accepts -Path parameter for source directory
2. -DateProperty parameter supports CreationTime and LastWriteTime options (defaults to CreationTime)
3. -ExcludeCurrentDay automatically filters out today's files to avoid production conflicts
4. -CalculateSize optional parameter includes file size calculation (off by default for performance)
5. Returns array of LogItem objects with FullPath, DateValue (YYYYMMDD format), and DaysOld populated
6. Cmdlet handles access denied errors gracefully with Error property populated
7. Memory usage remains efficient for 100K+ file processing

## Story 1.3: Get-LogFolders Discovery Cmdlet
As a system administrator,
I want to discover date-named folders for bulk processing,
so that I can manage pre-organized log folder structures efficiently.

**Acceptance Criteria:**
1. Get-LogFolders cmdlet accepts -Path parameter for source directory
2. Parses folder names in yyyymmdd and yyyy-mm-dd formats to extract DateValue
3. -ExcludeCurrentDay automatically filters out today's folders
4. -CalculateSize optional parameter sums all files within each folder
5. Returns array of LogItem objects with FullPath pointing to folder, DateValue, and DaysOld
6. Handles invalid folder name formats gracefully (skips non-date folders)
7. Supports nested date folder discovery with -Recurse parameter

## Story 1.4: Administrator Documentation & User Guide
As a system administrator,
I want comprehensive documentation and usage examples,
so that I can effectively implement and maintain the LogManager module in my enterprise environment.

**Acceptance Criteria:**
1. Administrator user guide created with step-by-step setup instructions
2. PowerShell usage examples provided for common scenarios (file organization, archival)
3. Windows Task Scheduler integration guide with sample configurations
4. Troubleshooting guide with common error scenarios and solutions
5. Performance tuning guidelines for million-file environments
6. Security best practices documentation for AWS credential management
7. Module installation and update procedures documented
8. Documentation integrated into module help system (Get-Help cmdlet support)
