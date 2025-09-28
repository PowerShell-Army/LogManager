# File Organization Workflow

```mermaid
flowchart TD
    A["Administrator Script Start"] --> B["Get-LogFiles<br/>-DateProperty CreationTime<br/>-Path SourcePath"]

    B --> C["PSCustomObject Array:<br/>FullPath, DateValue, DaysOld"]

    C --> D["Group-LogFilesByDate<br/>Group objects by DateValue"]

    D --> E["For Current Date Group:<br/>Calculate destination folder"]

    E --> F["Create destination folder:<br/>DestinationPath/YYYYMMDD/"]

    F --> G["Move-LogFilesToDateFolders<br/>Move files to date folder"]

    G --> H{"Move Success?"}
    H -->|NO| I["Mark objects as failed<br/>Log error for this date group"]
    H -->|YES| J["Objects updated with:<br/>DestinationFolder, OrganizationStatus"]

    I --> K{"More Date Groups?"}
    J --> K

    K -->|YES| E
    K -->|NO| L["Script Complete"]
```

## Example Object State After Organization

```powershell
# Three example objects after file organization completion:

Object 1:
FullPath              : C:\logs\organized\20250925\app-morning.log
DateValue             : 20250925
DaysOld               : 2
FileSize              : 2.5MB
DestinationFolder     : C:\logs\organized\20250925\
OrganizationStatus    : Moved
Error                 : null

Object 2:
FullPath              : C:\logs\organized\20250924\system-error.log
DateValue             : 20250924
DaysOld               : 3
FileSize              : 850KB
DestinationFolder     : C:\logs\organized\20250924\
OrganizationStatus    : Moved
Error                 : null

Object 3:
FullPath              : C:\logs\source\debug-failed.log
DateValue             : 20250923
DaysOld               : 4
FileSize              : 1.2GB
DestinationFolder     : C:\logs\organized\20250923\
OrganizationStatus    : Failed
Error                 : "Access denied to destination folder"
```