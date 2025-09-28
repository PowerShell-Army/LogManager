# Tech Stack

## Cloud Infrastructure
- **Provider:** AWS (Amazon Web Services)
- **Key Services:** S3 (Simple Storage Service), IAM (Identity and Access Management)
- **Deployment Regions:** Enterprise-configurable via S3 bucket configuration

## Technology Stack Table

| Category | Technology | Version | Purpose | Rationale |
|----------|------------|---------|---------|-----------|
| **Language** | C# | 10.0 | Primary development language | Modern C# features, strong typing, enterprise tooling, .NET Framework 4.8 compatibility |
| **Runtime** | .NET Framework | 4.8 | Target framework | PRD requirement for enterprise Windows compatibility, PowerShell 5.1+ support |
| **PowerShell** | Windows PowerShell | 5.1+ | Module host environment | PRD requirement, enterprise Windows standard |
| **Module Type** | Binary PowerShell Module | .dll | Compiled module format | Performance and type safety for enterprise-scale processing |
| **AWS SDK** | AWS SDK for .NET | 3.7.x | S3 integration | Official AWS .NET library, mature and enterprise-proven |
| **Compression Primary** | 7-Zip | Auto-detect | High-efficiency compression | PRD requirement for 70%+ compression ratios |
| **Compression Fallback** | .NET ZipFile | 4.8 built-in | Reliability fallback | Built-in .NET Framework capability when 7-Zip unavailable |
| **IDE** | Visual Studio | 2022 | Development environment | Full C# 10 support, PowerShell module templates, enterprise debugging |
| **Build Tool** | MSBuild | 17.0+ | Compilation system | Integrated with Visual Studio, .NET Framework build pipeline |
| **Testing Framework** | MSTest | 3.0+ | Unit testing | Microsoft standard, integrated Visual Studio support |
| **PowerShell Testing** | Pester | 5.0+ | PowerShell integration testing | PowerShell community standard for cmdlet testing |
| **Package Manager** | NuGet | 6.0+ | Dependency management | .NET standard for AWS SDK and other dependencies |
| **Version Control** | Git | 2.40+ | Source control | Industry standard, monorepo support |
| **Documentation** | XML Documentation | Built-in | PowerShell help system | Native PowerShell Get-Help integration |
