# BAVCL Migration Guide

This guide covers breaking API changes introduced with the `Shape` struct, CPU/GPU coherence (`Residence` flags), and scope-based pinning.

## Breaking changes

### Public `Value` field removed

`VectorBase<T>.Value` is now `internal`. Library code and consumers must use the read/write APIs below — do not rely on `GetValues()` (removed from the public surface).

| Need | API |
|------|-----|
| Read (zero-copy, no GPU sync) | `GetCpuReadOnlySpan()` — call `SyncCPU()` first when GPU may be newer |
| Read (zero-copy, syncs if needed) | `GetReadOnlySpan()` |
| Read single element (syncs if needed) | `GetAt(i)`, `GetAt(row, col)` |
| Read (heap copy, syncs if needed) | `ToArray()` — **always allocates**; prefer `GetReadOnlySpan()` for read-only access |
| Write / resize (CPU edit, no GPU upload) | `using (vector.CpuScope()) { ... }` |
| Write via `EditableView` | `using (var scope = vector.CpuScope()) { scope.View[i] = x; }` when `HasView` |
| Write / resize (CPU edit + GPU upload) | `using (vector.CpuScopeAndSync()) { ... }` |
| Indexer read | `vector[i]` → `GetAt` internally |
| Indexer write | `vector[i] = x` → `SetAt` (opens `CpuScope` internally; not for tight loops) |

### `GetValues()` replaced

`ICacheable<T>.GetValues()` is replaced by `GetReadOnlySpan()` — syncs from GPU when needed (for memory-manager upload and user reads). It cannot be used to mutate CPU storage. User reads without sync use `GetCpuReadOnlySpan()` on `VectorBase<T>` after an explicit `SyncCPU()` when required.

### `IndexingMode` removed

Indexer and `GetAt`/`SetAt` overloads no longer take `IndexingMode`. Reads sync automatically; writes open `CpuScope` internally. For batch CPU edits use `CpuScope` explicitly.

### `Shape()` return type

`VectorBase.Shape()` now returns `BAVCL.Core.Shape` instead of `(int, int)`:

```csharp
Shape s = vector.Shape();
if (shapeA.MatchesDimensions(shapeB)) { ... }
```

## Correct usage patterns

### Read-only (no scope)

```csharp
ReadOnlySpan<float> data = vector.GetReadOnlySpan(); // sync + zero-copy
float x = vector.GetAt(0); // syncs automatically
float[] copy = vector.ToArray(); // syncs + heap allocation

// Peek at CPU buffer without GPU pull (caller must ensure freshness):
vector.SyncCPU();
ReadOnlySpan<float> peek = vector.GetCpuReadOnlySpan();
```

### CPU mutation (`CpuScope`)

Single entry point on `ICacheable<T>`:

```csharp
CpuScope<T> CpuScope.Begin<T>(ICacheable<T> cacheable, bool syncToGpu = false);
```

Extensions: `.CpuScope(syncToGpu: false)` and `.CpuScopeAndSync()` (sugar for `syncToGpu: true`).

CPU scope methods (`EnterCpuScope` / `ExitCpuScope`) live on `ICacheable` (symmetry with `GpuScope`). `ICacheable<T>` exposes `GetReadOnlySpan()` for reads and explicit `EditCpu(Action<Span<T>>)` for scoped writes (called only by `CpuScope`; guarded by open scope).

```csharp
using (var scope = vector.CpuScopeAndSync())
{
  EditableView<float> view = scope.View;
  view[0] = 1.5f;
}
```

**Important:** assign `scope.View` to a local `EditableView<T>` before writing through the indexer — `scope.View[i] = x` does not compile (CS1612).

Coherence-only (no `.View`): mutate through your own APIs inside the scope:

```csharp
using (vector.CpuScopeAndSync()) { /* resize / replace backing store */ }
```

**Thread safety:** scope depth and residence transitions use `Interlocked`; CPU buffer mutation is caller-synchronized.

**Indexer writes:** `SetAt` / `vector[i] = x` open a scope per call — convenience only, **not for tight loops**. Batch edits should use explicit `CpuScope` + `View` or span.

### Array resize inside `CpuScope`

When replacing the entire backing array (append, merge, fill):

```csharp
using (var scope = vector.CpuScopeAndSync())
{
  Value = left.ToArray().Concat(right.ToArray()).ToArray(); // internal API in library
  Length = Value.Length;
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

```csharp
// Output + two read-only inputs
using (GpuScope.Begin(output, inputA, inputB))
{
    kernel(output.GetBuffer()...);
    gpu.accelerator.Synchronize();
}

// In-place modified
using (GpuScope.Begin(vector)) { ... }

// Nested: pin inputs, allocate output, pin output
using (GpuScope.BeginReadOnly(inputA, inputB))
{
    Vector output = new(inputA.Gpu, n);
    using (GpuScope.Begin(output)) { kernel(...); }
}
```

**Modified** vectors: `ActiveGpu` + `LiveCount`; on last unpin → `Gpu`  
**Read-only** vectors: `LiveCount` only

Do **not** call `IncrementLiveCount` / `DecrementLiveCount` manually.

## Memory manager: FreeBuffer vs GCItem

| API | Sync GPU→CPU? | When |
|-----|---------------|------|
| `FreeBuffer(id)` | Never | Resize, `TransferBuffer`, discard stale buffer |
| `GCItem(id)` | Only if GPU-authoritative | LRU eviction |

## Before / after

### Manual LiveCount (old)

```csharp
vector.IncrementLiveCount();
output.IncrementLiveCount();
kernel(...);
vector.DecrementLiveCount();
output.DecrementLiveCount();
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

To restore old load-all behaviour: `KernelModuleLoader.LoadAll<float>(gpu)`.

## Benchmarks

Span/read and memory-manager benchmarks: `BAVCL.Benchmarks` — see `BAVCL.Benchmarks/README.md`.

```bash
dotnet run -c Release --project BAVCL.Benchmarks
```

## See also

- [BAVCLSpecification.md](BAVCLSpecification.md) §3.5, §7 (kernel modules), §2.4, §8.3–8.6
- `BAVCL.Tests` — `VectorSyncCoherenceTests` for regression coverage
