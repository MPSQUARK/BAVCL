# BAVCL Features

User-facing catalog. **Contracts, policies, and internals:** [BAVCLSpecification.md](BAVCLSpecification.md). **Breaking changes:** [MigrationGuide.md](MigrationGuide.md).

## How features are organized

BAVCL uses **namespace feature gates** — import a module namespace and extension methods appear on `Vector`, `VectorInt`, `Vector3`, `Mask`, and primitive arrays:

```csharp
using BAVCL.Modules.Arithmetic;
using BAVCL.Modules.Structural;
using BAVCL.Modules.Sorting;

Vector v = new(gpu, [3f, 1f, 2f]);
v.SortAscIP();
v.Print();
```

Core types (`Vector`, `GPU`, scopes) live in `BAVCL`. Operations live in `BAVCL.Modules.*`.

## API suffix guide

| Suffix | Meaning | Execution |
|--------|---------|-----------|
| *(none)* | Allocating — returns a new value | **CPU** |
| `IP` | In-place — mutates the receiver | **CPU** |
| `X` | Allocating | **GPU** |
| `XIP` | In-place | **GPU** |

Order qualifiers for sorting: `Asc` / `Desc` (e.g. `SortAscXIP()`).

### Documented exceptions

These names are **GPU-only** by design — no `X` suffix:

| API | Role |
|-----|------|
| **`OP` / `IPOP`** | NumPy-style element-wise broadcast on `Vector`, `VectorInt`, and bitwise ops on `Mask` |
| **C# operators** | `+`, `-`, `*`, `/`, `^`, `>`, `<`, `&`, `\|`, … route through GPU kernels (operators cannot take an `X` suffix) |
| **Explicit casts** | `(VectorInt)vector` / `(Vector)vectorInt` use GPU cast kernels |

`Vector3` uses separate **`OPX` / `IPOP`** (not the broadcast `OP` family) and **`VOPX`** for per-vertex vector results.

## Core types

| Type | Role |
|------|------|
| **`Vector`** | Primary fp32 array / matrix (`Columns` encodes 1D vs 2D layout) |
| **`VectorInt`** | int32 arrays; bitwise ops, sorting, mask integration |
| **`Vector3`** | Packed 3D vertices on GPU (length multiple of 3) |
| **`Mask`** | Packed bit masks for comparisons, filtering, partitioning |
| **`Vertex`** | CPU-side 3D point with rendering helpers (tone mapping, etc.) |

## Modules

### Arithmetic (`BAVCL.Modules.Arithmetic`)

- **CPU:** `Abs`, `Rsqrt`, `Sum`, `Dot` (SIMD; scalar result)
- **GPU:** `SumX`, `DotX`, `AbsX`, `ReciprocalX`, `DiffX`, `NanToNumX`, `NormaliseX`, `RsqrtX`
- **Matrix:** `CrossX` (matrix multiply), `MatrixAddX`, `MatrixSubtractX`, `MatrixDivideX`, `MatrixPowX`, `MatrixMultiplyX`
- In-place variants use `IP` (CPU) or `XIP` (GPU) suffixes

### Statistics (`BAVCL.Modules.Statistics`)

- **CPU:** `Sum` (via Arithmetic), `Mean`, `Var` / `SampleVar`, `Std` / `SampleStd`, `Min`, `Max`, `Range`, `All`, `Percentile`, `Median`, `Quartile1`, `Quartile3`, `Iqr` on vectors and primitive arrays
- **GPU:** `SumX`, `MinX`, `MaxX`, `MeanX`, `VarX` / `SampleVarX`, `StdX` / `SampleStdX`, `AllX`, `RangeX`, `PercentileX`, `MedianX`, `Quartile1X`, `Quartile3X`, `IqrX` on `Vector` / `VectorInt`; `ReduceOPX` — row reduction against a coefficient vector
- `Var` / `VarX` are **population** variance (divide by N). `SampleVar` / `SampleVarX` are unbiased **sample** variance (divide by N−1). `Mean` / `MeanX` are the arithmetic mean (no separate sample vs population formula).
- `SumX` / `DotX` always use compensated grouped GPU reduce (Neumaier), matching CPU `Sum` / `Dot` at all lengths. `RangeX` / `MinMaxX` use a single grouped min/max pass; `AllX` uses a grouped non-zero scan (no full `Mask` materialization)
- `MinMax` / `MinMaxX` return `MinMax<T>`. `Dot(scalar)` / `DotX(scalar)` use `scalar * Sum`
- **Unchecked arithmetic:** all reductions use storage type (`int32`/`float32`); no silent widening — see spec §2.8

### Structural (`BAVCL.Modules.Structural`)

- **Factories:** `Zeros`, `Ones`, `Fill`, `Arange`, `Linspace`
- **CPU:** `Append`, `Prepend`, `Merge`, `Reverse`, row-axis `Concat` / `ConcatIP`, row/column slice reads where CPU-backed
- **GPU:** `ReverseX`, `TransposeX`, `GetColumnAsVectorX`, `GetSliceAsVectorX`, `ConcatColumnX` / `ConcatColumnXIP`
- **Output:** `Print`, `ToStr`, `ToCSV`

### Generators (`BAVCL.Modules.Generators`)

- Host-side `Arange` / `Linspace` returning `float[]` (no GPU allocation)

### GpuOps (`BAVCL.Modules.GpuOps`)

- **`OP` / `IPOP`** — broadcast and scalar element-wise ops (`Operations` enum)
- **`LogX` / `LogXIP`**
- **`VOPX`**, **`OPX` / `IPOP`** on `Vector3`

### Masking (`BAVCL.Modules.Masking`)

- **`OP` / `IPOP`** on `Mask` (bitwise)
- **`MaskX`**, **`FilterX`**, **`PartitionX`** on `Vector` / `VectorInt`
- **`CompareEqualsX`**, **`CompareNotEqualsX`**, **`CompareX`** and comparison operators → `Mask`

### Sorting (`BAVCL.Modules.Sorting`)

Per `Vector` and `VectorInt`:

| | CPU allocating | CPU in-place | GPU allocating | GPU in-place |
|--|----------------|--------------|----------------|--------------|
| Sort | `SortAsc` / `SortDesc` | `SortAscIP` / `SortDescIP` | `SortAscX` / `SortDescX` | `SortAscXIP` / `SortDescXIP` |
| Argsort | `ArgsortAsc` / `ArgsortDesc` | `ArgsortAscIP(indices)` | `ArgsortAscX` / `ArgsortDescX` | `ArgsortAscXIP(indices)` |

- **1D:** global sort; **2D:** per-row sort when `Columns > 1`
- Requires **Sorting** kernel domain loaded on the target `GPU`

### Geometric (`BAVCL.Modules.Geometric`)

- **`Vector3`:** `MagnitudeX`, `DistanceX`, `DotX`, `CrossX`, `NormaliseX`, `AccessRow`, `Concat` / `ConcatIP`, `ConcatColumnX`
- **`Vertex`:** CPU helpers — `FractIP`, `CrossIP`, `UnitVectorIP`, `AcesApprox`, `Reinhard`, etc.

### IO (`BAVCL.Modules.IO`)

- **Formats:** JSON, CSV, XML, TXT
- **`FileSession`** for read/write workflows
- Mask serialization (packed / bool layouts)

## GPU platform

| Component | Role |
|-----------|------|
| **`GPUManager`** | Default GPU singleton, device selection, memory cap |
| **`KernelModuleLoader`** | Compile kernel domains (`Default`, `Geometry`, `Sorting`, …) per element type |
| **LRU cache** | Automatic GPU buffer caching via `IMemoryManager` |
| **`BufferPool`** | Pooled scratch buffers; **entity / vessel / possess / banish** API for domain temps (see spec §4.5.3) |

### Scopes and reads

Library `*X` / `*XIP` methods pin GPU buffers internally (`GpuScope`). Callers writing **custom kernels** or new module dispatch must pin explicitly.

| Intent | API |
|--------|-----|
| Read (after GPU or anytime) | `RetrieveReadOnlySpan()` — do not open `CpuScope` for read-only |
| Edit in-place | `CpuScope` / `CpuScopeAndSync` + `scope.View` |
| Custom kernel | `GpuScope.Begin(output, input…)` then `Synchronize()` |

```csharp
// Read GPU result on CPU
vector.SortAscX();
ReadOnlySpan<float> sorted = vector.RetrieveReadOnlySpan();

// Batch CPU edit
using (var scope = vector.CpuScopeAndSync())
{
    EditableView<float> view = scope.View;
    view[0] = 1f;
}

// Custom kernel — prefer combined pin
using (GpuScope.Begin(output, input))
{
    kernel(...);
    gpu.accelerator.Synchronize();
}
```

**Module pointers:**

- **Statistics `MedianX`** — `SortAscX()` then `RetrieveReadOnlySpan()` (load `KernelWorkloads.Statistics`)
- **Sorting `SortAscXIP`** — pins internally; requires `KernelWorkloads.Sorting`
- **GpuOps `OPX`** — pins operands internally; requires `KernelWorkloads.Default`

Host API rules (loops, anti-patterns, nested GpuScope): [Migration Guide](MigrationGuide.md#host-api-and-scopes).

Load kernels before GPU methods:

```csharp
KernelModuleLoader.Load<float>(gpu, KernelWorkloads.Default);
KernelModuleLoader.Load<float>(gpu, KernelWorkloads.Sorting);
```

## Experimental and stubs

See [BAVCLSpecification.md §4.7–4.8](BAVCLSpecification.md#47-experimental-math) for experimental math kernels and placeholder APIs.

## See also

- [BAVCL Specification](BAVCLSpecification.md) — authoritative contracts
- [Migration Guide](MigrationGuide.md) — breaking API changes, **host API and scopes**
- [GPGPU Kernel Guide](GPGPUKernelGuide.md) — kernel authoring
