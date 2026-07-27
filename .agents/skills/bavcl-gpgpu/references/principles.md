# BAVCL GPU Principles (P1–P6)

Full guide: [Documentation/GPGPUKernelGuide.md](../../../Documentation/GPGPUKernelGuide.md)

## P1 — Maximize parallelism

GPUs win on independent concurrent work. Ask: *can this work run across threads instead of serially inside one thread?*

Reductions and matmul inner loops are fine. Serial loops over data that should be parallel across threads are not.

## P2 — Match hardware strengths

Element-wise → one thread per element. Aligned packed masks → one thread per word when possible. Gather/scatter when layout is irregular.

## P3 — Host prepares, device executes

Precompute O(1) setup on CPU when it simplifies device code or removes divergent branches. Many kernel parameters are fine when genuinely required.

## P4 — Divergent vs uniform branching

Data-dependent per-thread branches → bad (divergence). `switch` on specialized operation constant → good (all threads same path).

## P5 — Keep kernels simple

Dispatch must not be more complex than the kernels it launches. Reuse domain patterns before inventing new abstractions.

## P6 — Repo boundaries

Automated tests → `BAVCL.Tests`. `Testing Console/` → manual scratch only.

## Pre-submit questions

1. Launch mapping maximizes parallel work?
2. Parallel unit fits layout?
3. O(1) host precompute where it avoids divergent device logic?
4. Branches uniform, not data-divergent?
5. Kernel simpler than dispatch?
6. Follows existing `KernelDomain` pattern?
