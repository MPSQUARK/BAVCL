---
name: bavcl-gpgpu
description: >-
  Use when: writing or reviewing BAVCL ILGPU kernels, GPU dispatch modules,
  mask/vector GPU ops, KernelDomain registration, or any device-side code in
  this repo. Covers: GPU design principles P1-P6, host/device boundary,
  divergent vs uniform branching, BroadcastStrides, GpuScope, KernelModuleLoader.
---

# BAVCL GPGPU

BAVCL is an HPC GPU math library on ILGPU 1.5.3. Device-side code must exploit GPU parallelism and match this repo's patterns — not CPU idioms transplanted onto the accelerator.

## Before host dispatch (not only kernels)

1. Check [Features.md](../../Documentation/Features.md) for an existing `X` / `IP` API.
2. Read [MigrationGuide.md](../../Documentation/MigrationGuide.md#host-api-and-scopes) — Host API and scopes.
3. Then kernel steps below (GPGPUKernelGuide, P1–P6).

### Host API do / don't

| Do | Don't |
|----|-------|
| `RetrieveReadOnlySpan()` for all reads | Open `CpuScope` for read-only |
| `CpuScope` + `scope.View` for in-place edits | Call `SyncCPU()` in module/consumer code |
| `GpuScope.Begin(modified, readOnly…)` when **all** launch buffers already exist | Stack `BeginReadOnly` + `Begin(modified)` for the same launch |
| Nested `GpuScope` only to pin, then allocate, then pin the new buffer (LRU must not evict an unpinned input) | Extra scopes after every buffer is already live |
| Rent scratch from `BufferPools` / cacheable types (LRU) | `accelerator.Allocate1D` (or other device alloc) in algorithm or kernel-host code |
| One `RetrieveReadOnlySpan()` per read loop | `GetAt` / `SetAt` in tight loops |
| Pin via `GpuScope` only | Manual pinning outside `GpuScope` |

## Before coding

1. Read [Documentation/GPGPUKernelGuide.md](../../Documentation/GPGPUKernelGuide.md) — principles (P1–P6) and pre-submit review questions.
2. Read relevant sections of [BAVCLSpecification.md](../../Documentation/BAVCLSpecification.md) §6.6/§7.
3. Read **2–3 existing kernels in the target `KernelDomain`** before writing new ones.
4. Apply global `/code` **Fit the execution niche** — this skill supplies what that means here.

Quick reference: [references/principles.md](./references/principles.md)

## Principles (summary)

| | Principle |
|---|-----------|
| **P1** | Maximize parallelism — map threads to independent work |
| **P2** | Match hardware (element / word / gather as layout dictates) |
| **P3** | Host precomputes O(1) setup; device does the operation |
| **P4** | Avoid **divergent** branching; uniform specialized `switch` is fine |
| **P5** | Kernels simple; dispatch must not dwarf them |
| **P6** | Tests in `BAVCL.Tests`; `Testing Console/` is manual only |

## File placement

| What | Where |
| ---- | ----- |
| Kernel bodies | `BAVCL/Core/GPU/Kernels/<Domain>/` |
| Shared kernel helpers | `BAVCL/Core/GPU/Kernels/Shared/KernelHelpers.cs` |
| Kernel registration | `BAVCL/Core/GPU/KernelModules/KernelModuleLoader.cs` |
| Host dispatch | `BAVCL/Modules/<Domain>/Internal/` |
| Broadcast addressing | `BAVCL/Types/BroadcastStrides.cs` |

## Host CPU/GPU coherence

| Intent | API |
|--------|-----|
| Read | `RetrieveReadOnlySpan()` |
| Edit in-place | `CpuScope` + `scope.View` |
| Structural edit | `CpuScopeAndSync` + `RetrieveReadOnlySpan()` then assign `Value` |

Do not open `CpuScope` for read-only access. Pin with `GpuScope` before kernels.

When a kernel needs temporary device storage, rent a `BufferEntity` from `BufferPools.For(gpu)` (or another LRU cacheable). Do not store `MemoryBuffer` fields on `GPU` or allocate from the accelerator in dispatch code. New layouts that must stay cached get a new cacheable type, the same way pool slots do.

## Pre-submit (must justify any "no")

1. Launch mapping maximizes independent parallel work?
2. Parallel unit fits data layout?
3. O(1) host precompute used where it avoids divergent device logic?
4. Branches uniform (specialization), not data-divergent?
5. Kernel simpler than dispatch?
6. Follows existing pattern in same `KernelDomain`?

## After sessions

When a session discovers a new ILGPU quirk or BAVCL GPU pattern, propose a `/learn` update to `Documentation/GPGPUKernelGuide.md` — not session-only memory.

## Cross-references

- Global: `/code` (niche fit, minimal complexity), `/clarify-requirements` (execution context questions)
- Spec: `Documentation/BAVCLSpecification.md`
