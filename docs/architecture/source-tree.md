# Source Tree

```
LogManager/
├── src/                                # Main source code
│   ├── LogManager/                     # PowerShell module project
│   │   ├── Cmdlets/                    # PowerShell cmdlet implementations
│   │   │   ├── Discovery/              # File and folder discovery cmdlets
│   │   │   │   ├── GetLogFilesCmdlet.cs
│   │   │   │   └── GetLogFoldersCmdlet.cs
│   │   │   ├── Organization/           # File organization workflow cmdlets
│   │   │   │   ├── GroupLogFilesByDateCmdlet.cs
│   │   │   │   └── MoveLogFilesToDateFoldersCmdlet.cs
│   │   │   └── Archival/               # Long-term storage workflow cmdlets
│   │   │       ├── TestLogFileInS3Cmdlet.cs
│   │   │       ├── CompressLogFilesByDateCmdlet.cs
│   │   │       ├── SendLogFilesToS3Cmdlet.cs
│   │   │       └── RemoveLogFilesAfterArchiveCmdlet.cs
│   │   ├── Models/                     # Data models and shared objects
│   │   │   ├── LogItem.cs              # Core LogItem class
│   │   │   └── S3Configuration.cs      # S3 configuration helper
│   │   ├── Services/                   # Business logic services
│   │   │   ├── DateParsingService.cs   # Date extraction and validation
│   │   │   ├── CompressionService.cs   # 7-Zip detection and compression
│   │   │   ├── S3Service.cs            # AWS S3 operations wrapper
│   │   │   └── FileSystemService.cs    # File system operations wrapper
│   │   ├── Utilities/                  # Helper utilities
│   │   │   ├── ErrorHandling.cs        # Standardized error handling
│   │   │   ├── PathTemplateResolver.cs # S3 path template processing
│   │   │   └── RetryPolicyHelper.cs    # Exponential backoff retry logic
│   │   ├── LogManager.csproj           # Project file targeting .NET Framework 4.8
│   │   └── LogManager.psd1             # PowerShell module manifest
│   └── LogManager.sln                  # Visual Studio solution file
├── tests/                              # Test projects
│   ├── LogManager.UnitTests/           # C# unit tests
│   │   ├── Cmdlets/                    # Cmdlet unit tests
│   │   │   ├── Discovery/
│   │   │   ├── Organization/
│   │   │   └── Archival/
│   │   ├── Services/                   # Service layer unit tests
│   │   ├── Models/                     # Model unit tests
│   │   └── LogManager.UnitTests.csproj
│   ├── LogManager.IntegrationTests/    # PowerShell integration tests
│   │   ├── WorkflowTests/              # End-to-end workflow tests
│   │   │   ├── FileOrganization.Tests.ps1
│   │   │   └── LongTermStorage.Tests.ps1
│   │   ├── CmdletTests/                # Individual cmdlet integration tests
│   │   └── TestData/                   # Sample test files and folders
│   └── LogManager.PerformanceTests/    # Performance and scale tests
│       ├── MillionFileTest.ps1         # 1M+ file processing validation
│       └── MemoryProfileTests.ps1      # Memory usage validation
├── build/                              # Build and deployment artifacts
│   ├── scripts/                        # Build automation scripts
│   │   ├── Build.ps1                   # Main build script
│   │   ├── Package.ps1                 # Module packaging script
│   │   └── Test.ps1                    # Test execution script
│   └── output/                         # Build output directory
├── docs/                               # Documentation
│   ├── prd.md                          # Product Requirements Document
│   ├── architecture.md                 # This architecture document
│   ├── user-guide.md                   # Administrator usage guide
│   ├── examples/                       # PowerShell usage examples
│   │   ├── file-organization.ps1
│   │   ├── long-term-storage.ps1
│   │   └── task-scheduler-setup.ps1
│   └── api/                            # Generated cmdlet documentation
├── .github/                            # GitHub automation
│   └── workflows/                      # CI/CD pipeline definitions
│       ├── build.yml                   # Build and test automation
│       └── release.yml                 # Release packaging automation
├── .gitignore                          # Git ignore patterns
├── LICENSE                             # License file
├── README.md                           # Project overview and quick start
└── CHANGELOG.md                        # Version history and changes
```
