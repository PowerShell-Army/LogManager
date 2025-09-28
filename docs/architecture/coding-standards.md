# Coding Standards

## Core Standards
- **Languages & Runtimes:** C# 10.0, .NET Framework 4.8, PowerShell 5.1+
- **Style & Linting:** Visual Studio default C# formatting, PowerShell Script Analyzer
- **Test Organization:** MSTest for C# unit tests, Pester 5.0+ for PowerShell integration tests

## Naming Conventions
| Element | Convention | Example |
|---------|------------|---------|
| Cmdlets | Verb-Noun pattern | `Get-LogFiles`, `Test-LogFileInS3` |
| C# Classes | PascalCase | `LogItem`, `CompressionService` |
| Properties | PascalCase | `FullPath`, `DateValue` |
| Private Fields | camelCase with underscore | `_s3Client`, `_retryPolicy` |
| Constants | UPPER_CASE | `MAX_RETRY_ATTEMPTS` |

## Critical Rules
- **LogItem Error Property:** Always set Error = null for success, descriptive string for failures
- **Date Format Consistency:** Use YYYYMMDD format throughout system, no exceptions
- **S3 Credential Handling:** Never log or expose AWS credentials in error messages
- **Memory Management:** Dispose of S3 clients and file streams in using statements
- **Pipeline Processing:** Each cmdlet must accept pipeline input and support -WhatIf where applicable
- **Exception Propagation:** Catch and convert exceptions to LogItem.Error, don't let them bubble up
- **Path Handling:** Use Path.Combine() and handle both forward/back slashes for cross-environment compatibility
- **Null Safety:** Check for null parameters and file existence before operations

## PowerShell Specific Guidelines
- **Parameter Validation:** Use [ValidateNotNullOrEmpty] and [ValidateScript] attributes
- **Pipeline Support:** Implement Begin/Process/End methods for efficient pipeline processing
- **Help Documentation:** Include comprehensive .SYNOPSIS, .DESCRIPTION, .PARAMETER, and .EXAMPLE
- **Verbose Output:** Use WriteVerbose for detailed operation tracing
