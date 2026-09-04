# BAVCL Migration Guide

This guide covers breaking API changes introduced with the `Shape` struct, CPU/GPU coherence (`Residence` flags), and scope-based pinning.

## Change history

BAVCL is source-referenced (no NuGet yet). Pin your consumer to a **git commit** — not a calendar date or informal version number.

```bash
git checkout <commit>   # e.g. 40b8876
```

Milestones below are **oldest → newest**. When upgrading, read the breaking-change sections for every commit you cross.

| Commit | Summary |
|--------|---------|
| [`9d9b79c`](https://github.com/MPSQUARK/BAVCL/commit/9d9b79c) | `Residence` flags; public `Value` removed; `GetValues` retired from the public read path |
| [`b8527f3`](https://github.com/MPSQUARK/BAVCL/commit/b8527f3) | `Shape` struct; residence model approved |
| [`0a95062`](https://github.com/MPSQUARK/BAVCL/commit/0a95062) | Residence model integrated across cache/sync paths |
| [`1d3b611`](https://github.com/MPSQUARK/BAVCL/commit/1d3b611) | `KernelModuleLoader` — selective kernel domain loading |
| [`0bf7d4f`](https://github.com/MPSQUARK/BAVCL/commit/0bf7d4f) | `KernelWorkloads` bundles |
| [`0539b03`](https://github.com/MPSQUARK/BAVCL/commit/0539b03) | `X` / `IP` suffix rename; GPU-first operator alignment |
| [`3304046`](https://github.com/MPSQUARK/BAVCL/commit/3304046) | `RetrieveReadOnlySpan()` as the read entry point |
| [`57e3bac`](https://github.com/MPSQUARK/BAVCL/commit/57e3bac) | Deprecated read/write APIs removed |
| [`8c5644b`](https://github.com/MPSQUARK/BAVCL/commit/8c5644b) | `GpuScope` pinning conventions |
| [`af8dfac`](https://github.com/MPSQUARK/BAVCL/commit/af8dfac) | Statistics module (`MeanX`, `VarX`, order-stat `*X` foundation) |
| [`40b8876`](https://github.com/MPSQUARK/BAVCL/commit/40b8876) | **HEAD** — current `main` at time of writing |

> **Working tree:** global reduce (`SumX`, `DotX`, `MinX`, `MaxX`, `AllX`, …), numeric guards, and unchecked-arithmetic policy (spec §2.8) are documented in this guide but land **after** `40b8876` — pin to a later commit once merged, or match your local tree.

When upgrading, read sections for your **from → to** commits in order.

## Breaking changes

### Public `Value` field removed

`VectorBase<T>.Value` is now `internal`. Library code and consumers must use the read/write APIs below — do not rely on `GetValues()` (removed from the public surface).

| Need | API |
|------|-----|
| Read (zero-copy, syncs if needed) | `RetrieveReadOnlySpan()` |
| Read single element (syncs if needed) | `GetAt(i)`, `GetAt(row, col)` |
| Read (heap copy, syncs if needed) | `ToArray()` — **always allocates**; prefer `RetrieveReadOnlySpan()` for read-only access |
| Write / resize (CPU edit, no GPU upload) | `using (vector.CpuScope()) { ... }` |
| Write via `EditableView` | `using (var scope = vector.CpuScope()) { scope.View[i] = x; }` when `HasView` |
| Write / resize (CPU edit + GPU upload) | `using (vector.CpuScopeAndSync()) { ... }` |
| Indexer read | `vector[i]` → `GetAt` internally |
| Indexer write | `vector[i] = x` → `SetAt` (opens `CpuScope` internally; not for tight loops) |

### `GetValues()` replaced

`ICacheable<T>.GetValues()` is replaced by `RetrieveReadOnlySpan()` — syncs from GPU when needed (for memory-manager upload and user reads). It cannot be used to mutate CPU storage. For writes, use `CpuScope` and `EditableView<T>`.

### `IndexingMode` removed

Indexer and `GetAt`/`SetAt` overloads no longer take `IndexingMode`. Reads sync automatically; writes open `CpuScope` internally. For batch CPU edits use `CpuScope` explicitly.

### `Shape()` return type

`VectorBase.Shape()` now returns `BAVCL.Core.Shape` instead of `(int, int)`:

```csharp
Shape s = vector.Shape();
if (shapeA.MatchesDimensions(shapeB)) { ... }
```

### Element-wise `*` with mismatched 2D shapes

Previously, `operator *` (and other binary `OP` overloads routed through GpuOps) could combine two vectors **element-wise** whenever they had the **same `Length`**, even when their 2D layouts differed — e.g. a 3×2 vector and a 2×3 vector (both length 6) were multiplied as flat buffers, with the result keeping the left operand's `Columns`.

GpuOps now applies **shape-aware** rules first: operands must either match dimensions exactly or be valid NumPy-style broadcasts. Same length with incompatible 2D shapes (such as `(3,2)` and `(2,3)`) throws `ShapeMismatchException`.

**Matrix multiply is unchanged** — use `Vector.Cross()` / `.Cross()`, not `*`.

**To recover the old flat element-wise behaviour**, treat both operands as 1D by setting `Columns = 0` before the operation, then restore layout on the result if needed:

```csharp
int colsA = vec.Columns;
int colsB = vec2.Columns;

vec.Columns = 0;
vec2.Columns = 0;

Vector result = vec * vec2; // element-wise on the flat buffer

vec.Columns = colsA;
vec2.Columns = colsB;
result.Columns = colsA; // output followed the left operand's layout
```

`Vector.Flatten()` is equivalent to `Columns = 0` when you do not need to preserve the original column count on the inputs.

```csharp
vec.Flatten();
vec2.Flatten();
Vector result = vec * vec2;
result.Columns = 2; // reshape result as needed
```

## Host API and scopes

Authoritative rules for CPU reads, CPU edits, and GPU buffer pinning. **One read path:** `RetrieveReadOnlySpan()` — syncs from GPU when needed, including inside `CpuScope` (no double-sync when already coherent).

### Read model

```csharp
// Read — always RetrieveReadOnlySpan (syncs if GPU newer)
ReadOnlySpan<float> data = vector.RetrieveReadOnlySpan();

// Read inside CpuScope during structural edit — still Retrieve
using (vector.CpuScopeAndSync())
{
    ReadOnlySpan<float> left = vector.RetrieveReadOnlySpan();
    // library internal: Value = [.. left, .. extra]; Length = Value.Length;
}

// Edit in-place
using (var scope = vector.CpuScopeAndSync())
{
    EditableView<float> view = scope.View;
    for (int i = 0; i < view.Length; i++)
        view[i] *= 2f;
}
```

Do **not** open `CpuScope` for read-only access — use `RetrieveReadOnlySpan()` instead.

| Need | Use | Do not use |
|------|-----|------------|
| Read after GPU op | `RetrieveReadOnlySpan()` | `CpuScope` wrapper for read-only |
| Read inside edit session | `RetrieveReadOnlySpan()` | Removed `GetCpuReadOnlySpan()` |
| Edit in-place | `CpuScope` + `scope.View` | `RetrieveReadOnlySpan` for mutation |
| Structural edit (resize) | `CpuScopeAndSync` + `RetrieveReadOnlySpan` + assign backing store | `EditableView` for resize |
| Single element (occasional) | `GetAt` / indexer get | `GetAt` in tight loops |
| Heap copy | `ToArray()` | `Pull()` unless you need detached array semantics |

**Breaking:** `GetRowAsArray(row, noSync)` removed — use `GetRowAsArray(row)` only.

### GetAt / SetAt / indexers — not for tight loops

```csharp
// Bad — may sync per iteration
for (int i = 0; i < n; i++)
    sum += vector.GetAt(i);

// Good — one sync, then span iteration
ReadOnlySpan<float> data = vector.RetrieveReadOnlySpan();
for (int i = 0; i < data.Length; i++)
    sum += data[i];

// Bad — opens CpuScope per iteration
for (int i = 0; i < n; i++)
    vector.SetAt(i, values[i]);

// Good — one scope, batch write
using (var scope = vector.CpuScopeAndSync())
{
    EditableView<float> view = scope.View;
    for (int i = 0; i < n; i++)
        view[i] = values[i];
}
```

`GetAt` / `SetAt` / `vector[i]` remain fine for **occasional** single-element access.

### SyncCPU — callers rarely need it

~99% of consumer code should **never** call `SyncCPU()`. It runs internally via `RetrieveReadOnlySpan()`, `GetAt`, `ToArray()`, `CpuScope`, LRU eviction, and library `*X` / `*IP` methods.

| Caller | Legitimate `SyncCPU`? |
|--------|----------------------|
| Consumer / pipeline code | **No** — use `RetrieveReadOnlySpan()` or a library method |
| `CpuScope` enter (library) | Yes (internal) |
| LRU `GCItem` (library) | Yes (internal) |

**Anti-patterns:**

- `vector.SyncCPU(); span = vector.RetrieveReadOnlySpan()` — redundant
- Opening `CpuScope` only to read after a GPU kernel — use `RetrieveReadOnlySpan()` outside scope
- `GetAt` / `SetAt` / indexers inside `for` loops
- Manual pinning outside `GpuScope` (`LiveCount` is observable for debugging)

### Copy

`Vector.Copy()` uses `ToArray()` (CPU path when coherent) — prefer that over `Pull()` when implementing new copy APIs.

### CPU mutation (`CpuScope`)

Single entry point on `ICacheable<T>`:

```csharp
CpuScope<T> CpuScope.Begin<T>(ICacheable<T> cacheable, bool syncToGpu = false);
```

Extensions: `.CpuScope(syncToGpu: false)` and `.CpuScopeAndSync()` (sugar for `syncToGpu: true`).

CPU scope methods (`EnterCpuScope` / `ExitCpuScope`) live on `ICacheable` (symmetry with `GpuScope`). `ICacheable<T>` exposes `RetrieveReadOnlySpan()` for reads and explicit `EditCpu(Action<Memory<T>>)` for scoped writes (called only by `CpuScope`; guarded by open scope).

```csharp
using (var scope = vector.CpuScopeAndSync())
{
  EditableView<float> view = scope.View;
  view[0] = 1.5f;
}
```

**Important:** assign `scope.View` to a local `EditableView<T>` before writing through the indexer — `scope.View[i] = x` does not compile (CS1612).

Coherence-only hosts (no `.View`): mutate through library APIs inside the scope that replace the backing store.

**Thread safety:** scope depth and residence transitions use `Interlocked`; CPU buffer mutation is caller-synchronized.

### Array resize inside `CpuScope`

When replacing the entire backing array (append, merge, fill) — library pattern:

```csharp
using (vector.CpuScopeAndSync())
{
  ReadOnlySpan<float> left = vector.RetrieveReadOnlySpan();
  // internal: Value = left.ToArray().Concat(right).ToArray(); Length = Value.Length;
}
```

Consumers without `internal` access should build a new `Vector` from `ToArray()` results instead.

## Residence flags (thread-safe)

`ICacheable.Residence` uses `Interlocked.CompareExchange` for guarded transitions (`TrySetResidence`) in scopes and sync paths; volatile reads for observation. `_cpuScopeDepth` uses `Interlocked` operations.

| Flag | Meaning |
|------|---------|
| `Cpu` | CPU data is authoritative |
| `Gpu` | GPU buffer is authoritative |
| `InSync` | Both sides match (`Cpu \| Gpu`) |
| `ActiveCpu` | `CpuScope` open |
| `ActiveGpu` | `GpuScope` pin on a modified vector |

`SyncCPU()` and `UpdateCache()` skip copies when already in the correct state.

## GpuScope

Static class `BAVCL.Core.GpuScope` — use `Begin` / `BeginReadOnly`. Convenience extensions: `icacheable.GpuScope(...)`.

**Prefer** one combined scope when all buffers already exist:

```mermaid
flowchart TD
  subgraph good [Preferred single scope]
    A[All buffers exist] --> B["GpuScope.Begin(output, inputA, inputB)"]
    B --> C[kernel + Synchronize]
  end
  subgraph alloc [Valid nested pattern]
    D[Pin inputs read-only] --> E[Allocate new Vector on GPU]
    E --> F[Pin output in inner scope]
  end
  subgraph bad [Avoid]
    G[BeginReadOnly inputs] --> H[Begin output separately]
    H --> I["Same launch - use combined Begin instead"]
  end
```

```csharp
// Preferred: output + read-only inputs in one scope
using (GpuScope.Begin(output, inputA, inputB))
{
    kernel(output.GetBuffer()...);
    gpu.accelerator.Synchronize();
}

// In-place modified
using (GpuScope.Begin(vector)) { ... }

// Nested: pin inputs before allocation (LRU cannot evict unpinned inputs)
using (GpuScope.BeginReadOnly(inputA, inputB))
{
    Vector output = new(inputA.Gpu, n);
    using (GpuScope.Begin(output)) { kernel(...); }
}
```

**Modified** vectors: `ActiveGpu` + `LiveCount`; on last unpin → `Gpu`  
**Read-only** vectors: `LiveCount` only

`LiveCount` is **observable** for debugging. Pinning is **only** via `GpuScope`.

## Memory manager: FreeBuffer vs GCItem

| API | Sync GPU→CPU? | When |
|-----|---------------|------|
| `FreeBuffer(id)` | Never | Resize, `TransferBuffer`, discard stale buffer |
| `GCItem(id)` | Only if GPU-authoritative | LRU eviction |

## Before / after

### Manual pinning (old)

```csharp
// Manual LiveCount refcount — no longer available on the public API
kernel(...); // unpinned buffers could be evicted under LRU pressure
```

### GpuScope (new)

```csharp
using (GpuScope.Begin(output, vector))
{
    kernel(...);
    gpu.accelerator.Synchronize();
}
```

### Direct Value / SyncCPU / UpdateCache (old)

```csharp
vector.SyncCPU();
vector.Value[i] = x;
vector.UpdateCache();
```

### CpuScope (new)

```csharp
using (var scope = vector.CpuScopeAndSync())
{
    EditableView<float> view = scope.View;
    view[i] = x;
}
```

## Kernel module loading

### Breaking change

`GPUManager.GetGPU()` no longer compiles kernels. You must call `KernelModuleLoader.Load<T>(...)` explicitly after creating a GPU.

`GPUManager.Default` still works — it creates a GPU and loads `KernelWorkloads.Default` (fp32 Arithmetic + Structural).

### Before (monolithic)

```csharp
GPU gpu = GPUManager.GetGPU(); // compiled all kernels at startup
```

### After (modular)

```csharp
// Bare GPU — no kernels
GPU gpu = GPUManager.GetGPU();
KernelModuleLoader.Load<float>(gpu, KernelWorkloads.Default);

// Or use the convenience singleton (same as Default workload)
GPU gpu = GPUManager.Default;

// Per-GPU configuration
var gpuA = GPUManager.GetGPU();
var gpuB = GPUManager.GetGPU();
KernelModuleLoader.Load<float>(gpuA, KernelWorkloads.Default);
KernelModuleLoader.Load<float>(gpuB, KernelWorkloads.Geometry);
```

The generic parameter is the element type of the kernels — `float` today, `double` and others as they are added.

### Workloads

| Bundle | Domains loaded | When to use |
| ------ | -------------- | ----------- |
| `KernelWorkloads.Default` | Arithmetic + Structural | Matrix multiply, element-wise ops, shape ops |
| `KernelWorkloads.Geometry` | Default + Geometry | Vector3 cross, magnitude, distance |

### Custom domain selection

```csharp
KernelModuleLoader.Load<float>(gpu, KernelDomain.Arithmetic, KernelDomain.Structural);
```

`Load` is additive — call it again later to add domains; modules already compiled on that GPU are skipped.

### GpuOps dependency

The `BAVCL.Modules.GpuOps` namespace requires **Arithmetic** (fp32) kernels (broadcast, element-wise, row reduce). Use `KernelWorkloads.Default` or explicitly include `KernelDomain.Arithmetic`.

### Troubleshooting

| Error | Cause | Fix |
| ----- | ----- | --- |
| `KernelNotCompiledException` | Kernel not loaded on this GPU | Load the domain that provides it (see spec §4.5.2) |
| `KernelModuleNotAvailableException` | Requested domain × element type not implemented | Use an implemented combination (see spec §7.5) |
| `ShapeMismatchException` on `vec * vec2` | Same length, different 2D layout | Use `.Cross()` for matmul, or flatten both (`Columns = 0`) for flat element-wise (see above) |

To restore old load-all behaviour: `KernelModuleLoader.LoadAll<float>(gpu)`.

### API naming (suffix, PascalCase, CPU/GPU alignment)

BAVCL now uses a single suffix scheme (pioneered by Sorting):

| Suffix | Meaning | Execution |
|--------|---------|-----------|
| *(none)* | Allocating | **CPU** |
| `IP` | In-place | **CPU** |
| `X` | Allocating | **GPU** |
| `XIP` | In-place | **GPU** |

**Exceptions (unchanged names, GPU-only):** `OP` / `IPOP` on `Vector`, `VectorInt`, and `Mask`; C# operator overloads; explicit `(Vector)` / `(VectorInt)` casts.

#### Suffix / PascalCase renames

| Old | New |
|-----|-----|
| `Abs_IP` | `AbsIP` |
| `AbsX_IP` | `AbsXIP` |
| `ReverseX_IP` | `ReverseXIP` |
| `Transpose_IP` | `TransposeXIP` |
| `Append_IP` | `AppendIP` |
| `Nan_to_num` | `NanToNumX` |
| `Nan_to_num_IP` | `NanToNumXIP` |
| `Aces_approx` | `AcesApprox` |
| `Aces_approx_IP` | `AcesApproxIP` |
| `Vector3.OP_IP` | `Vector3.IPOP` |
| `Log_IP` | `LogXIP` |

(Apply the same `_IP` → `IP` pattern to all other in-place methods.)

#### CPU/GPU alignment renames

GPU paths that previously had no `X` suffix now do:

| Old | New |
|-----|-----|
| `Reciprocal` / `Reciprocal_IP` | `ReciprocalX` / `ReciprocalXIP` |
| `Diff` / `Diff_IP` | `DiffX` / `DiffXIP` |
| `Normalise` / `Normalise_IP` | `NormaliseX` / `NormaliseXIP` |
| `Transpose` / `Transpose_IP` | `TransposeX` / `TransposeXIP` |
| `Cross` (matrix multiply) | `CrossX` |
| `MatrixAdd` … `MatrixMultiply` | `MatrixAddX` … `MatrixMultiplyX` |
| `ReduceOP` | `ReduceOPX` |
| `Mask` / `Filter` / `Partition` | `MaskX` / `FilterX` / `PartitionX` |
| `CompareEquals` / `CompareNotEquals` / `Compare` | `CompareEqualsX` / `CompareNotEqualsX` / `CompareX` |
| `GetColumnAsVector` | `GetColumnAsVectorX` |
| Column `GetSliceAsVector` / `GetSliceAsArray` | `GetSliceAsVectorX` / `GetSliceAsArrayX` |
| `Vector3.Magnitude` / `Distance` / `Dot` / `Cross` / `Normalise` | `*X` variants |
| `VOP` | `VOPX` |
| `Vector3.OP` (allocating) | `Vector3.OPX` |

**Concat:** `Concat` / `ConcatIP` are **row-axis only** (CPU). Column-axis GPU concat uses `ConcatColumnX` / `ConcatColumnXIP`. Passing `ConcatAxis.Column` to `Concat` throws — use the column APIs instead.

### Global reduce (`*X`)

- **`Var()`** and **`Dot()`** are **always CPU** (SIMD). Use explicit `*X` APIs for GPU.
- Load **`KernelWorkloads.Statistics`** before `SumX`, `MinX`, `DotX`, etc. Details: [BAVCLSpecification.md §4](./BAVCLSpecification.md#4-current-functionality-v0), [Features.md](./Features.md#statistics-bavclmodulesstatistics).
- Float `SumX` / `DotX`: always compensated grouped reduce (Neumaier). Int paths: unchecked `int32` accum (see spec §2.8).
- Invalid percentile → `FixedRangeException`. Empty `Mean` → `DivideByZeroException`.

### Numeric guards vs .NET `ArgumentException.ThrowIf*`

.NET 6+ provides argument guards (`ArgumentNullException.ThrowIfNull`, `ArgumentOutOfRangeException.ThrowIfNegative`, `ArgumentException.ThrowIfZero`, etc.). These throw **`ArgumentOutOfRangeException`** or **`ArgumentNullException`** — appropriate when the caller passed an invalid API argument.

BAVCL uses **domain guards** in `Source/Core/Helpers/Guards/` when the failure is a **mathematical precondition**, not a bad parameter:

| Guard | When to use | Exception |
|-------|-------------|-----------|
| `DivideByZeroGuard.ThrowIfZeroLength` | Length is used as a divisor (`Mean`, `MeanX`) | `DivideByZeroException` |
| `EmptySequenceGuard.ThrowIfEmpty` | Operation has no identity on empty input (`Min`, `Max` on int) | `InvalidOperationException` |
| `FixedRangeGuard.ThrowIfOutOfRange` | Value must lie in a fixed inclusive range (percentile ∈ [0, 100]) | `FixedRangeException` |

Do **not** substitute `ArgumentException.ThrowIfZero` for `DivideByZeroGuard` — the exception type communicates intent to callers.

### Numerical accuracy (`float32`)

Target: results within **6 significant decimal places** of the mathematically exact answer where EC accumulation applies. `fp64` is not implemented yet; when it is, expect tighter bounds (typically ≤ 12 dp).

| Operation | CPU / GPU parity | Typical abs error (well-behaved data) | Notes |
|-----------|------------------|---------------------------------------|-------|
| `Sum` / `SumX` (all N) | ✓ | &lt; 10⁻⁵ on 10⁶-element stress (0.1 repeated) | Neumaier; float64 scratch (CPU and GPU) |
| `Dot` / `DotX` | ✓ | Same as sum | Scalar dot uses `scalar * Sum(v)` |
| `Min` / `Max` / `Range` | ✓ | Exact (bit-identical) | Except empty float GPU min → `NaN` |
| `Mean` / `MeanX` | ✓ | Inherited from sum / N | |
| `Var` / `Std` / `*X` | ✓ | &lt; 10⁻⁵ | Single-pass variance (Welford + Chan merge) |
| Order stats (`Percentile`, `Median`, …) | ✓ | &lt; 10⁻⁵ | Sort + linear interpolation |
| `VectorInt` sum / mean | ✓ | Exact until `int32` overflow | Unchecked wrap — see spec §2.8 |

GPU `*X` paths are tested for parity with CPU within the tolerances above. Neumaier EC is always active on float sum/dot (see spec §2.9).

**fp32 precision:** if `Var`/`Sum` look wrong on large-magnitude data, check spec §2.9 *fp32 precision limits* before chasing algorithm bugs.

## Benchmarks

Span/read and memory-manager benchmarks: `BAVCL.Benchmarks` — see `BAVCL.Benchmarks/README.md`.

```bash
dotnet run -c Release --project BAVCL.Benchmarks
```

## See also

- [BAVCLSpecification.md](BAVCLSpecification.md) §3.5, §7 (kernel modules), §2.4, §8.3–8.6
- `BAVCL.Tests` — `VectorSyncCoherenceTests` for regression coverage
