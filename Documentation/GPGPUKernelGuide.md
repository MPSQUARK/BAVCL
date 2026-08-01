# BAVCL GPGPU Kernel Guide

Authoritative guide for **device-side work** in BAVCL. This document teaches how to think on GPU in this codebase — not a post-mortem checklist from one incident.

**Related:** [BAVCLSpecification.md](./BAVCLSpecification.md) §6 (Mask API), §6.6 (kernel inventory), §7 (module loading).

**Agent skill:** `.agents/skills/bavcl-gpgpu/` — load before writing or reviewing ILGPU kernels in this repo.

---

## Core principles

These are the authoritative rules. Everything below is elaboration or examples.

### P1 — Maximize parallelism

GPUs win on independent concurrent work. Launch granularity and thread mapping must exploit that strength.

Ask: *can this work run across threads instead of serially inside one thread?*

Reductions, inner loops in matmul, and other algorithms that **require** sequential work **within** a thread are fine. Serial loops over data that **should** be parallel across threads are not.

### P2 — Match hardware strengths

Choose the parallel unit that fits the data layout and operation:

- Element-wise float ops → one thread per element
- Aligned packed masks → one thread per 32-bit word when layouts align
- Scatter/gather when compaction or broadcast breaks regular layout

Use atomics, coalescing, and word-level bitwise ops when they are the natural fit — not as clever tricks.

### P3 — Host prepares, device executes

Push O(1) setup to CPU when it keeps device code simple or removes **divergent** branches: broadcast stride resolution, tail padding masks, output sizing for irregular ops, index tables for gather.

The test is not "how many kernel parameters" — it is *does this belong on the host because it is cheap to precompute and simplifies the device path?* Many `SpecializedValue` parameters are acceptable when genuinely required.

### P4 — Avoid divergent branching; uniform branching is fine

Threads in a warp taking different paths on **data-dependent** conditions hurt performance — prefer branchless selects or host-resolved addressing.

**`switch` on a specialized operation constant** (all threads same branch) is the established BAVCL pattern and is expected. Do not conflate "no if statements" with "no branching."

### P5 — Keep kernels simple; keep dispatch honest

Device code should read as direct work. If the module/dispatch layer is more complex than the kernels it launches, simplify. Reuse existing domain patterns before inventing parallel abstractions.

Reference patterns: `A_FloatOPKernel`, `GpuScope`, `KernelModuleLoader`, post-refactor `MaskBitwiseOps`.

### P6 — Respect repo boundaries

- Automated tests → `BAVCL.Tests` (outside this repo tree at `../BAVCL.Tests/`)
- `Testing Console/` is **manual scratch only** — never add validation harnesses there

---

## Pre-submit review

Answer **yes** to each (or justify **no**) before submitting device-side code:

1. Does launch mapping maximize independent parallel work for this operation?
2. Is the parallel unit aligned with data layout and hardware (element / word / reduction lane)?
3. Is anything precomputable on the host for O(1) cost that would otherwise complicate or diverge device code?
4. Are remaining branches uniform across threads (specialization) rather than data-divergent?
5. Is the kernel simpler than its dispatch layer?
6. Does this follow an existing pattern in the same `KernelDomain`?

No hard caps on loop count, parameter count, or kernel variants.

---

## When loops belong in BAVCL kernels

| Loop purpose | OK when | Example in BAVCL |
| ------------ | ------- | ---------------- |
| Reduction / accumulation within a thread | Algorithm requires serial combine | `AccumulateReduceRow` in `KernelHelpers.cs`, `ReduceRowOpKernel` |
| Inner dimension of matmul | Standard GEMM structure | `MatMulKernel` |
| Serial iteration over parallel output dimension | **Not OK** — wrong parallel unit | Mask v1: loop over bits inside one thread instead of one thread per lane |

---

## Host vs device boundary

**Precompute on host:**

- Broadcast addressing (`BroadcastStrides`)
- Padding tail masks (`lastWord`, `tailMask`)
- Compaction indices when output size depends on mask content
- Anything that would become per-thread shape branching on device

**Keep on device:**

- The actual numeric or bit operation
- Uniform operation dispatch via `SpecializedValue`
- Memory access patterns that must coalesce at runtime

**New domains** should resolve broadcast addressing on the host via `BroadcastStrides` unless there is a documented reason not to.

---

## Branching: divergence vs specialization

| Kind | Verdict | BAVCL pattern |
| ---- | ------- | ------------- |
| Data-dependent per-thread conditions on hot path | Divergent — minimize or replace with branchless ops | Comparisons → lane bit via arithmetic + `Utilities.Select` |
| Shape-dependent per-thread tests (`rows == 1`) | Divergent — resolve on host | `BroadcastStrides` with zero stride |
| `switch ((Operations)op.Value)` | Uniform — all threads same path | `A_FloatOPKernel` |
| `switch ((MaskOperation)op.Value)` | Uniform | `ApplyMaskWordOp` in `KernelHelpers.cs` |

---

## Architecture map

| Concern | Path |
| ------- | ---- |
| Kernel bodies | `BAVCL/Core/GPU/Kernels/` |
| Shared helpers | `BAVCL/Core/GPU/Kernels/Shared/KernelHelpers.cs` |
| Registration | `BAVCL/Core/GPU/KernelModules/KernelModuleLoader.cs` |
| Host dispatch | `BAVCL/Modules/*/Internal/` |
| Broadcast strides | `BAVCL/Types/BroadcastStrides.cs` |
| GPU lifecycle | `GpuScope`, `CacheableBase<T>`, `KernelWorkloads` |

---

## Reference implementations (by principle)

| Principle | Implementation |
| --------- | -------------- |
| **P1** element parallelism | `A_FloatOPKernel`, `VectorCompareMaskKernel` |
| **P2** word-level parallelism | `MaskWordOpKernel` (when layouts align) |
| **P3** host precompute | `BroadcastStrides`; tail `(lastWord, tailMask)` in word kernels |
| **P4** uniform dispatch | Operation `switch` in arithmetic and mask kernels |
| **P5** simple dispatch | `MaskBitwiseOps` (post-refactor) |

---

## Case study: mask v1

One example mapping symptoms to violated principles — not the definition of the rules.

| Symptom | Principle violated | Correct thinking |
| ------- | ------------------ | ---------------- |
| Loop over bits comparing floats | **P1** — failed to maximize parallelism | One thread owns one lane; compare → set bit |
| Five shape `SpecializedValue`s with per-thread branching | **P3 + P4** — O(1) broadcast on host avoids divergent shape tests | `BroadcastStrides`; multiply-add indexing |
| Separate `*IP` kernel stacks | **P5** — over-engineered dispatch | Alias output view with input where safe |
| `MaskValidation.cs` in Testing Console | **P6** | Tests in `BAVCL.Tests` or external scratch harness |
| Delegates in kernel | ILGPU mechanical constraint | Explicit logic (see below) |

---

## ILGPU mechanical constraints

Factual compile/runtime limits — separate from design principles P1–P5:

- No lambda closures or delegates in kernel methods (`Ldftn` compile failure)
- `ILGPU.Util.Utilities.Select` for branchless selection
- `Atomic.Or` / `Atomic.And` for multi-lane writes into one word
- `RetrieveReadOnlySpan()` before printing GPU-backed data — not `GetCpuReadOnlySpan()`

Stack: ILGPU 1.5.3, `GPU` partial class, `LoadAutoGroupedKernel`, `AcceleratorStream`.

---

## Honest host-side exceptions

**Select compaction:** output `Vector` length depends on mask popcount. Indices are collected host-side today; the device runs gather only. This is a **sizing constraint**, not laziness. Upgrade path: device-side scan (see spec §6.6).

---

## Adding a new kernel domain (checklist)

1. Add `KernelDomain` enum member
2. Implement kernels under `BAVCL/Core/GPU/Kernels/<Domain>/`
3. Register in `KernelModuleLoader.cs` and optionally `KernelWorkloads`
4. Host dispatch under `BAVCL/Modules/<Domain>/Internal/`
5. Apply P1–P6 and pre-submit review above
6. Automated tests in `BAVCL.Tests`
