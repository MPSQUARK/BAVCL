# BAVCL Features

User-facing catalog of what BAVCL provides today. For full contracts and internals, see [BAVCLSpecification.md](BAVCLSpecification.md).

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

- **CPU:** `Abs`, `Rsqrt`, `Sum`, `Dot` (scalar result via broadcast + CPU sum)
- **GPU:** `AbsX`, `ReciprocalX`, `DiffX`, `NanToNumX`, `NormaliseX`, `RsqrtX`
- **Matrix:** `CrossX` (matrix multiply), `MatrixAddX`, `MatrixSubtractX`, `MatrixDivideX`, `MatrixPowX`, `MatrixMultiplyX`
- In-place variants use `IP` (CPU) or `XIP` (GPU) suffixes

### Statistics (`BAVCL.Modules.Statistics`)

- **CPU:** `Sum`, `Mean`, `Var`, `Std`, `Min`, `Max`, `Range` on vectors and primitive arrays
- **GPU:** `ReduceOPX` — row reduction against a coefficient vector

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
| **`GpuScope` / `CpuScope`** | RAII buffer pinning and CPU edit scopes |
| **LRU cache** | Automatic GPU buffer caching via `IMemoryManager` |
| **`BufferPool`** | Pooled scratch buffers; **entity / vessel / possess / banish** API for domain temps (see spec §4.5.3) |

Load kernels before GPU methods:

```csharp
KernelModuleLoader.Load<float>(gpu, KernelWorkloads.Default);
KernelModuleLoader.Load<float>(gpu, KernelWorkloads.Sorting);
```

## Experimental and stubs

See [BAVCLSpecification.md §4.7–4.8](BAVCLSpecification.md#47-experimental-math) for experimental math kernels and placeholder APIs.

## See also

- [BAVCL Specification](BAVCLSpecification.md) — authoritative contracts
- [Migration Guide](MigrationGuide.md) — breaking API changes including naming
- [GPGPU Kernel Guide](GPGPUKernelGuide.md) — kernel authoring
