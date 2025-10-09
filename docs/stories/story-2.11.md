# Story 2.11: Performance Benchmark with 300K File Dataset

Status: Draft

## Story

As a developer,
I want to benchmark with realistic scale,
so that I can validate NFR001 performance targets.

## Acceptance Criteria

1. Create 300K file test dataset
2. Measure end-to-end archival time
3. Identify performance bottlenecks
4. Validate <5 min scan time for 1M files (extrapolate from 300K)
5. Document performance characteristics

## Tasks / Subtasks

- [ ] Create test dataset (AC: #1)
  - [ ] Create script to generate 300K files
  - [ ] Distribute across 10 dated folders (30K files each)
  - [ ] Use realistic file sizes (varied, 1KB-100KB range)
  - [ ] Save dataset generation script for reproducibility
- [ ] Run end-to-end benchmark (AC: #2, #4)
  - [ ] Pester: Full Invoke-LogArchival workflow with 300K files
  - [ ] Measure discovery time
  - [ ] Measure compression time
  - [ ] Measure transfer time
  - [ ] Measure cleanup time
  - [ ] Measure total end-to-end duration
  - [ ] Extrapolate to 1M files for discovery
  - [ ] Validate <5 minute target for 1M file scan
- [ ] Compression throughput benchmark (AC: #3)
  - [ ] Create 1 GB test folder
  - [ ] Measure 7-Zip compression rate (GB/min)
  - [ ] Measure 7-Zip compression ratio
  - [ ] Measure SharpCompress compression rate (GB/min)
  - [ ] Measure SharpCompress compression ratio
  - [ ] Document comparison
- [ ] Profile bottlenecks (AC: #3)
  - [ ] Use .NET diagnostics to identify slow operations
  - [ ] Document findings
  - [ ] Recommend optimizations if targets not met
- [ ] Document results (AC: #5)
  - [ ] Add performance benchmarks to PERFORMANCE.md
  - [ ] Show all measured metrics
  - [ ] Compare to targets
  - [ ] Document hardware specs used for benchmark

## Dev Notes

- Performance validation
- NFR001 verification
- May reveal optimization opportunities
- 300K file dataset creation may take significant time
- Benchmark should run on representative hardware
- Discovery target: <90 seconds for 300K files
- Extrapolated discovery: <5 minutes for 1M files
- Total workflow target: <10 minutes for 300K files

### Project Structure Notes

- Test dataset: `/tests/logManager.Tests/Performance/TestDataset/` (300K files)
- Dataset generator: `/tests/logManager.Tests/Performance/Generate-TestDataset.ps1`
- Tests:
  - `/tests/logManager.Tests/Performance/300K-File-Benchmark.Tests.ps1`
  - `/tests/logManager.Tests/Performance/Compression-Throughput.Tests.ps1`
- Documentation: `/docs/PERFORMANCE.md`

### References

- [Source: docs/tech-spec-epic-2.md#NFR - Performance]
- [Source: docs/tech-spec-epic-2.md#AC20: Performance Benchmark - 300K Files]
- [Source: docs/tech-spec-epic-2.md#AC21: Performance Benchmark - Compression Throughput]
- [Source: docs/epic-stories.md#Story 2.11]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML/JSON will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

### Completion Notes List

### File List
