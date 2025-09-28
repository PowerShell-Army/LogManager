# Epic 2: File Organization System

**Epic Goal:** Deliver complete file organization workflow that allows administrators to sort large, unorganized log directories into manageable date-based folder structures for easier maintenance and processing.

## Story 2.1: Group-LogFilesByDate Cmdlet
As a system administrator,
I want to group discovered log files by their date values,
so that I can process files in organized date-based collections.

**Acceptance Criteria:**
1. Group-LogFilesByDate cmdlet accepts pipeline input of LogItem objects from Get-LogFiles
2. Groups objects by DateValue property into collections (one group per date)
3. Returns grouped collections that can be processed iteratively
4. Maintains original LogItem object properties within each group
5. Handles mixed date ranges efficiently without memory overflow
6. Provides count information for each date group
7. Supports -DateRange parameter to filter specific date ranges

## Story 2.2: Move-LogFilesToDateFolders Cmdlet
As a system administrator,
I want to move files from source locations to date-organized folder structures,
so that I can create manageable folder hierarchies for large log collections.

**Acceptance Criteria:**
1. Move-LogFilesToDateFolders cmdlet accepts -DestinationPath and LogItem objects as input
2. Creates destination folders in format: DestinationPath/YYYYMMDD/
3. Moves files from FullPath to appropriate date folder while preserving filename
4. Updates LogItem objects with new DestinationFolder and OrganizationStatus properties
5. Handles file conflicts with -ConflictResolution parameter (Skip, Overwrite, Rename)
6. Implements fail-fast behavior per date group (failure in one date doesn't affect others)
7. Provides detailed error information in Error property for failed moves
8. Verifies successful moves before marking objects as complete

## Story 2.3: Complete File Organization Integration
As a system administrator,
I want to execute the complete file organization workflow in a single operation,
so that I can efficiently organize large directories without manual intervention.

**Acceptance Criteria:**
1. Integration demonstrates full workflow: Get-LogFiles | Group-LogFilesByDate | Move-LogFilesToDateFolders
2. Memory usage remains under 2GB for 1M+ file processing
3. Processing completes within performance targets for large file sets
4. Error reporting shows which date groups succeeded and which failed
5. Partial completion allows retry of failed date groups only
6. Workflow documentation includes PowerShell examples for common scenarios
7. Integration testing validates end-to-end functionality with various file types and sizes
