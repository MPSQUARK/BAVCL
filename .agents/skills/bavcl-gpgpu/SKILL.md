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
