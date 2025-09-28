# Technical Assumptions

## Repository Structure: Monorepo
Single repository containing all PowerShell module components and documentation.

## Service Architecture
Monolithic PowerShell binary module (.dll) with granular cmdlets - each cmdlet performs one specific operation while sharing a common custom C# object for data flow.

## Testing Requirements
Unit testing for C# classes and integration testing for PowerShell cmdlet workflows with automated test execution capabilities.

## Additional Technical Assumptions and Requests

• **Language:** C# 10.0 compiled to PowerShell binary module (.dll)
• **Framework:** .NET Framework 4.8 (not .NET Core/.NET 5+) for enterprise Windows compatibility
• **PowerShell Support:** PowerShell 5.1+ (Windows PowerShell)
• **Compression:** 7-Zip auto-detection with .NET ZipFile fallback for reliability
• **Cloud Storage:** AWS.NET APIs for S3 integration
• **Object Design:** Single custom C# class with extensible properties for memory efficiency
• **Error Handling:** Single Error property per object (null = success, string = failure description)
• **Processing Model:** Per-date group processing with fail-fast behavior
• **Date Format:** Consistent YYYYMMDD format throughout system
• **Performance:** Handle 1M+ files within 2-hour maintenance windows
• **Authentication:** Primary authentication via IAM roles at server level, with optional -AccessKey and -SecretKey parameters for credential override when needed. Credentials passed directly to .NET AWS cmdlets without creating AWS profiles.
• **Deployment:** Compatible with Windows Task Scheduler infrastructure
• **CI/CD Pipeline:** GitHub Actions workflow with automated build, test execution (MSTest + Pester), documentation generation, and module packaging
• **Release Management:** Automated release builds with semantic versioning and PowerShell Gallery publishing capability
• **Quality Gates:** Unit test coverage >80%, integration test success, PowerShell Script Analyzer security validation
• **Build Artifacts:** Compiled .dll module, manifest .psd1, generated help files, and example scripts
