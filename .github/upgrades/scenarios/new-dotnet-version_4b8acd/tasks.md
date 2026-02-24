# SistemaVenta .NET 10.0 Upgrade Tasks

## Overview

This document lists the executable tasks to perform an all-at-once upgrade of the SistemaVenta solution to .NET 10.0: prerequisites verification, a single atomic framework+package upgrade across all projects, followed by test execution and fixes. Tasks follow the plan's atomic approach and are designed to be LLM-executable.

**Progress**: 0/3 tasks complete (0%) ![0%](https://progress-bar.xyz/0)

---

## Tasks

### [▶] TASK-001: Verify prerequisites
**References**: Plan §Implementation Timeline Phase 0

- [✓] (1) Verify required .NET SDK/runtime version is installed per Plan §Prerequisites (check sdk version and `dotnet --list-sdks`)
- [▶] (2) Runtime/sdk version meets minimum requirements for target (.NET 10.0) (**Verify**)
- [ ] (3) Check for presence and compatibility of `global.json`, `Directory.Build.props`, `Directory.Packages.props` and other shared MSBuild files as specified in Plan §Detailed Execution Steps
- [ ] (4) Configuration files compatible with target version (**Verify**)
- [ ] (5) Verify required CLI/tools (e.g., `dotnet` workload components) are available per Plan §Prerequisites (**Verify**)

### [ ] TASK-002: Atomic framework and package upgrade with compilation fixes
**References**: Plan §Implementation Timeline Phase 1, Plan §Package Update Reference, Plan §Breaking Changes Catalog, Plan §Detailed Execution Steps

- [ ] (1) Update TargetFramework/TargetFrameworks in all project files listed in Plan §Detailed Execution Steps (update per Plan §Implementation Timeline Phase 1)
- [ ] (2) Update all package references across projects to target versions per Plan §Package Update Reference
- [ ] (3) Restore dependencies for the solution (e.g., `dotnet restore`) and ensure all packages restore successfully (**Verify**)
- [ ] (4) Build the entire solution and fix all compilation errors caused by framework and package upgrades following guidance in Plan §Breaking Changes Catalog
- [ ] (5) Rebuild solution to verify fixes; solution builds with 0 errors (**Verify**)
- [ ] (6) Commit changes with message: "TASK-002: Atomic framework and dependency upgrade"

### [ ] TASK-003: Run full test suite and validate upgrade
**References**: Plan §Implementation Timeline Phase 2, Plan §Testing & Validation Strategy, Plan §Detailed Execution Steps (Step 5 Execute Tests)

- [ ] (1) Run all test projects listed in Plan §Detailed Execution Steps (execute full automated test suites)
- [ ] (2) Fix any test failures (reference Plan §Breaking Changes Catalog for common issues)
- [ ] (3) Re-run tests after fixes
- [ ] (4) All tests pass with 0 failures (**Verify**)
- [ ] (5) Commit test fixes with message: "TASK-003: Complete testing and validation"

---