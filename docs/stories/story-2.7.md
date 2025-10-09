# Story 2.7: Optimize File Enumeration for 1M+ File Scale (NFR001)

Status: Draft

## Story

As a developer,
I want efficient file enumeration at massive scale,
so that the module can handle 300K-1M files per day without overwhelming resources.

## Acceptance Criteria

1. Find-LogFiles scans 1M files in <5 minutes (NFR001 target)
2. Uses .NET parallel enumeration where appropriate
3. Efficient filtering to minimize memory usage
4. Performance profiling identifies bottlenecks
5. Benchmark tests with realistic datasets

## Tasks / Subtasks

- [ ] Validate streaming implementation (AC: #3)
  - [ ] Verify Find-LogFiles uses yield return pattern
  - [ ] Verify no entire file list loaded into memory
  - [ ] Review FileDiscoveryService for streaming efficiency
- [ ] Create performance benchmarks (AC: #1, #5)
  - [ ] Create 300K file test dataset
  - [ ] Benchmark Find-LogFiles discovery time
  - [ ] Extrapolate to 1M files
  - [ ] Validate <5 minute target for 1M files
  - [ ] Document performance characteristics
- [ ] Profile and optimize bottlenecks (AC: #2, #4)
  - [ ] Use .NET diagnostics to profile discovery
  - [ ] Identify slow operations
  - [ ] Consider parallel directory processing if needed
  - [ ] Optimize date filtering logic
  - [ ] Measure improvement after optimizations
- [ ] Document performance tuning (AC: #5)
  - [ ] Add performance section to README
  - [ ] Document best practices for large file counts
  - [ ] Show benchmark results

## Dev Notes

- Performance optimization for FR003/FR004
- May require .NET DirectoryInfo.EnumerateFiles optimizations
- Target: <5 min for 1M file scan (NFR001)
- Target: <90 seconds for 300K files
- Streaming memory efficiency is critical
- .NET 8 EnumerateFiles should be efficient by default
- Parallel processing may help for multiple directories

### Project Structure Notes

- Performance tests: `/tests/logManager.Tests/Performance/`
  - FileEnumeration-300K.Tests.ps1
  - FileEnumeration-Extrapolation.Tests.ps1
- Documentation: `/docs/PERFORMANCE.md`

### References

- [Source: docs/tech-spec-epic-2.md#NFR - Performance - NFR001]
- [Source: docs/tech-spec-epic-2.md#AC20: Performance Benchmark - 300K Files]
- [Source: docs/epic-stories.md#Story 2.7]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
