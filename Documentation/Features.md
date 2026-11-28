# BAVCL Features

User-facing catalog. **Contracts, policies, and internals:** [BAVCLSpecification.md](BAVCLSpecification.md). **Breaking changes:** [Migrations](Migrations/README.md).

## How features are organized

BAVCL uses **namespace feature gates** — import a module namespace and extension methods appear on `Vector`, `VectorInt`, `Vector3`, `Mask`, and primitive arrays:

```csharp
using BAVCL.Modules.Arithmetic;
using BAVCL.Modules.Structural;
using BAVCL.Modules.Scan;
using BAVCL.Modules.Sorting;

Vector v = new([3f, 1f, 2f]);
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
- **Rolling (0.9.4):** `RollingSum` / `MovingAverage` + `*X` — any window `k`, 1D and per-row 2D (`Columns > 1`), NumPy `convolve(..., mode='same')` edge policy per row; built on `CumSum` + shift-subtract
- `Var` / `VarX` are **population** variance (divide by N). `SampleVar` / `SampleVarX` are unbiased **sample** variance (divide by N−1). `Mean` / `MeanX` are the arithmetic mean (no separate sample vs population formula).
- `SumX` / `DotX` always use compensated grouped GPU reduce (Neumaier), matching CPU `Sum` / `Dot` at all lengths. `RangeX` / `MinMaxX` use a single grouped min/max pass; `AllX` uses a grouped non-zero scan (no full `Mask` materialization)
- `MinMax` / `MinMaxX` return `MinMax<T>`. `Dot(scalar)` / `DotX(scalar)` use `scalar * Sum`
- **Unchecked arithmetic:** all reductions use storage type (`int32`/`float32`); no silent widening — see spec §2.8

### Structural (`BAVCL.Modules.Structural`)

- **GPU factories:** `ZerosX`, `OnesX`, `FillX`, `ArangeX`, `LinspaceX`
- **CPU:** `Append`, `Prepend`, `Merge`, `Reverse`, row-axis `Concat` / `ConcatIP`, row/column slice reads where CPU-backed
- **GPU:** `ReverseX`, `TransposeX`, `GetColumnAsVectorX`, `GetSliceAsVectorX`, `ConcatColumnX` / `ConcatColumnXIP`
- **Output:** `Print`, `ToStr`, `ToCSV`

### Generators (`BAVCL.Modules.Generators`)

- Host-side `Arange` / `Linspace` for `float`, `double` and `int` arrays (no GPU allocation). An arange holds
  `ceil((end - start) / interval)` samples, computed in that type.
- **Random numbers (NumPy-aligned names, `Random` prefix):** `Random` / `RandomX` produce floats in `[0, 1)`;
  `RandomUniform` / `RandomUniformX` produce floats in `[low, high)` and integers in `[low, high)` on `VectorInt`;
  `RandomNormal` / `RandomNormalX` produce Gaussian values with the given mean and standard deviation.
  The GPU variants use ILGPU's `RNG<XorShift128Plus>` `FillUniform`. `RandomX` needs no further
  kernel; `RandomUniformX` maps the samples onto the requested bounds with one scale kernel;
  `RandomNormalX` applies the inverse normal CDF (Acklam approximation, citations.md [12]; about 1.15e-9 relative error
  in double precision, with `float32` rounding on top) in one kernel. The uniform input is
  clamped to `[2^-24, 1 - 2^-24]`, which truncates the tails at roughly ±5.3 standard deviations. The CPU
  `RandomNormal` uses Box-Muller (citations.md [13]). The RNG is created per call, so the same seed reproduces the same
  values on the same device; the GPU stream layout depends on the device's parallel warp count. Random results are always below `high`; a range only a few ulps wide necessarily
  collapses onto `low`.
- **Zero-length requests are rejected:** every factory throws when asked for a length of zero, and `Arange`
  throws when its range produces no samples. Empty vectors are not a valid state to request, so no
  factory returns one.

### GpuOps (`BAVCL.Modules.GpuOps`)

- **`OP` / `IPOP`** — broadcast and scalar element-wise ops (`Operations` enum)
- **`LogX` / `LogXIP`**
- **`VOPX`**, **`OPX` / `IPOP`** on `Vector3`

### Masking (`BAVCL.Modules.Masking`)

- **`OP` / `IPOP`** on `Mask` (bitwise)
- **`MaskX`**, **`FilterX`**, **`PartitionX`** on `Vector` / `VectorInt` (GPU scan compaction in 0.9.3); **`&` / `|` / `/ mask` / `v[mask]`** operators call these accelerated paths (not CPU `Filter`/`Partition`)
- **`Mask`**, **`Filter`**, **`Partition`** on `Vector` / `VectorInt` — explicit CPU `MaskCompactionCpu` (operators use accelerated **`MaskX`** / **`FilterX`** / **`PartitionX`**)
- **`Where`** / **`WhereX`** on `Mask` — true source indices (`np.where(mask)[0]` / `flatnonzero`)
- **`CountTrue`** / **`Any`** / **`All`** + **`*X`** on `Mask`
- **`FirstTrueIndex`** / **`LastTrueIndex`** + **`*X`** on `Mask`
- **`CompareEqualsX`**, **`CompareNotEqualsX`**, **`CompareX`** and comparison operators → `Mask`

### Scan (`BAVCL.Modules.Scan`)

- **`CumSum`**, **`CumSumExclusive`**, **`CumProd`**, **`CumProdExclusive`** + **`IP`** / **`X`** / **`XIP`** on `Vector` / `VectorInt`
- 1D and per-row 2D (`Columns > 1`); int product is unchecked overflow

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
| **`BufferPool`** | Pooled scratch buffers; `BufferEntity<T>.Possess(emptyTarget)` borrows a pooled buffer into any empty domain instance (see spec §5.5.3) |

### Construction, `Upload`, `Init`, and empty shells

Every core data type (`Vector`, `VectorInt`, `VectorComplex64`, `Vector3`, `Mask`) defaults to `GPUManager.Default` when `gpu:` is omitted:

| Pattern | Meaning | Residence |
|---------|---------|-----------|
| `new Vector(values, columns: 0, upload: Upload.Now, gpu: null)` | Construct from host data; `Upload.Now` uploads immediately, `Upload.Defer` keeps on CPU until first GPU op | `InSync` / `Cpu` |
| `new Vector(length, columns: 0, init: Init.UnInit, gpu: null)` | Allocate GPU storage directly; `Init.UnInit` skips clearing, `Init.Zeroed` zeros on device (`Mask` always zeros and has no `Init`) | `Gpu` |
| `new Vector(gpu: null)` | Empty shell (`IsEmpty == true`); populate later via `Assign(array / length / deviceView)` or `entity.Possess(v)` | `None` |

### Scopes and reads

Library `*X` / `*XIP` methods pin GPU buffers internally (`GpuScope`). Callers writing **custom kernels** or new module dispatch must pin explicitly.

| Intent | API |
|--------|-----|
| Read (after GPU or anytime) | `RetrieveReadOnlySpan()` — do not open `CpuScope` for read-only |
| Edit in-place | `CpuScope()` + `scope.View` (`CpuScope(Upload.Defer)` keeps the result on the CPU) |
| Custom kernel | `GpuScope.Begin(output, input…)` then `Synchronize()` |

```csharp
// Read GPU result on CPU
vector.SortAscX();
ReadOnlySpan<float> sorted = vector.RetrieveReadOnlySpan();

// Batch CPU edit
using (var scope = vector.CpuScope())
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

Host API rules (loops, anti-patterns, nested GpuScope): spec [§3.4](BAVCLSpecification.md#34-scope-only-gpu-pinning-implemented) and [§8.6](BAVCLSpecification.md#86-residence-flags-and-coherence-implemented). Breaking steps: [Migrations](Migrations/README.md).

Load kernels before GPU methods:

```csharp
KernelModuleLoader.Load<float>(gpu, KernelWorkloads.Default);
KernelModuleLoader.Load<float>(gpu, KernelWorkloads.Sorting);
```

## Experimental and stubs

See [BAVCLSpecification.md §4.7–4.8](BAVCLSpecification.md#47-experimental-math) for experimental math kernels and placeholder APIs.

## See also

- [BAVCL Specification](BAVCLSpecification.md) — authoritative contracts
- [Migrations](Migrations/README.md) — one file per tagged breaking-change step
- [GPGPU Kernel Guide](GPGPUKernelGuide.md) — kernel authoring
