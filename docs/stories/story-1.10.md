# Story 1.10: Implement Compress-Logs Function (FR005)

Status: Draft

## Story

As a DevOps engineer,
I want to compress log files/folders into .zip archives using 7-Zip,
so that I can reduce storage costs and organize archives.

## Acceptance Criteria

1. Accepts input files/folders from FR003/FR004
2. Integrates with 7-Zip (CLI or library)
3. Creates .zip archives on source drive (in-place, NFR002)
4. Uses naming from FR010
5. Accepts optional Compress parameter (compression not always required)
6. Returns archive FileInfo objects
7. Validates NFR002: Never copies to temp location
8. Performance tested with 300K+ file dataset

## Tasks / Subtasks

- [ ] Implement CompressLogsCmdlet (AC: #1, #5, #6)
  - [ ] Create cmdlet with SourcePath, OutputPath, AppName, PreferredEngine parameters
  - [ ] Accept pipeline input: LogFileInfo[], LogFolderInfo[], string[]
  - [ ] Call CompressionService
  - [ ] Return ArchiveInfo array
- [ ] Implement CompressionService (AC: #2, #3, #7)
  - [ ] Create service for engine selection
  - [ ] Detect 7-Zip availability
  - [ ] Select SevenZipCompressor or SharpCompressCompressor
  - [ ] Enforce in-place compression (same drive validation)
  - [ ] Log verbose messages for engine selection
- [ ] Implement ICompressionEngine abstraction (AC: #2)
  - [ ] Define ICompressionEngine interface
  - [ ] IsAvailable property
  - [ ] Compress method returning ArchiveInfo
- [ ] Implement SevenZipCompressor (AC: #2, #3)
  - [ ] Detect 7-Zip in PATH and common install locations
  - [ ] Use Process.Start to invoke 7z CLI
  - [ ] Capture exit code and stderr
  - [ ] Throw CompressionException on failure
- [ ] Implement SharpCompressCompressor (AC: #2, #3)
  - [ ] Use SharpCompress NuGet library
  - [ ] Create .zip archives using library API
  - [ ] In-place compression enforcement
- [ ] Create unit and integration tests (AC: #7, #8)
  - [ ] xUnit: CompressionService engine selection logic
  - [ ] xUnit: SevenZipCompressor with mocked Process.Start
  - [ ] xUnit: SharpCompressCompressor unit tests
  - [ ] Test in-place constraint validation
  - [ ] Pester: Integration test with real 7-Zip
  - [ ] Pester: Fallback to SharpCompress when 7-Zip unavailable
  - [ ] Pester: Performance test with large file set

## Dev Notes

- Processing layer - depends on FR003/FR004, FR010
- In-place compression constraint is critical (NFR002)
- 7-Zip integration research needed
- Hybrid strategy: 7-Zip CLI primary, SharpCompress fallback (ADR-002)
- Must validate source and destination are on same volume
- Performance target deferred to Epic 2 benchmarking
- SharpCompress NuGet: 0.36.0+

### Project Structure Notes

- Cmdlet: `/src/logManager/Cmdlets/CompressLogsCmdlet.cs`
- Service: `/src/logManager/Services/CompressionService.cs`
- Interface: `/src/logManager/Abstractions/ICompressionEngine.cs`
- Implementations:
  - `/src/logManager/Infrastructure/SevenZipCompressor.cs`
  - `/src/logManager/Infrastructure/SharpCompressCompressor.cs`
- Model: `/src/logManager/Models/ArchiveInfo.cs`
- Model: `/src/logManager/Models/CompressionOptions.cs`
- Tests:
  - `/tests/logManager.Tests/Services/CompressionServiceTests.cs`
  - `/tests/logManager.Tests/Infrastructure/SevenZipCompressorTests.cs`
  - `/tests/logManager.Tests/Infrastructure/SharpCompressCompressorTests.cs`
  - `/tests/logManager.Tests/Integration/Compress-Logs.Tests.ps1`

### References

- [Source: docs/tech-spec-epic-1.md#Services and Modules - Compress-Logs]
- [Source: docs/tech-spec-epic-1.md#AC7: Compress-Logs Cmdlet (7-Zip Available)]
- [Source: docs/tech-spec-epic-1.md#AC8: Compress-Logs Cmdlet (7-Zip Unavailable)]
- [Source: docs/tech-spec-epic-1.md#Workflows - Compression Engine Selection Logic]
- [Source: docs/epic-stories.md#Story 1.10]
- [Source: docs/solution-architecture.md#ADR-002: Hybrid Compression Strategy]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
