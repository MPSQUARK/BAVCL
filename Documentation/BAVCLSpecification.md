# BAVCL Specification

Authoritative reference for BAVCL behavior and design intent. Documents **current implementation** where verified, plus **roadmap** items explicitly marked as future. When code and spec diverge, reconcile manually — update the spec, change the code, or record an open gap in [Section 19](#19-code-vs-vision-gaps). Neither side wins automatically.

**Version:** Discovery draft — July 2026
**Author:** Marcel Pawelczyk
**Status:** PROGRAM STILL IN DEVELOPMENT

Document Disclaimer: This document is intended for AI coding tools for reasoning and project alignment. While the contents may be beneficial for human use, the content may be verbose.

---

## Table of Contents

1. [Identity and Scope](#1-identity-and-scope)
2. [Design Principles](#2-design-principles)
3. [Architecture Overview](#3-architecture-overview)
4. [Current Functionality (v0)](#4-current-functionality-v0)
5. [Type System Roadmap](#5-type-system-roadmap)
6. [Mask Specification](#6-mask-specification)
7. [Kernel Module System](#7-kernel-module-system)
8. [GPU and Memory](#8-gpu-and-memory)
9. [Code Organization Roadmap](#9-code-organization-roadmap)
10. [CPU Execution Path](#10-cpu-execution-path)
11. [Geometry Module](#11-geometry-module)
12. [Astrophysics Module](#12-astrophysics-module)
13. [IO Module](#13-io-module)
14. [Plotting](#14-plotting)
15. [Experimental Math](#15-experimental-math)
16. [Testing and Quality](#16-testing-and-quality)
17. [Platform Support](#17-platform-support)
18. [Deferred / Nice-to-Have](#18-deferred--nice-to-have)
19. [Code vs Vision Gaps](#19-code-vs-vision-gaps)
20. [Related Projects](#20-related-projects)

---

## 1. Identity and Scope

### 1.1 What BAVCL Is

**BAVCL** is a **C# GPU-accelerated numerics library** built on [ILGPU](https://ilgpu.net/) 1.5.3. It provides NumPy-_inspired_ vector math with C#-idiomatic APIs — not a direct NumPy port.

| Attribute        | Value                                                  |
| ---------------- | ------------------------------------------------------ |
| Language         | C# (.NET 10)                                           |
| GPU runtime      | ILGPU 1.5.3 + ILGPU.Algorithms                         |
| Distribution     | Source-only project reference                          |
| License          | Source-available custom licence — see [License.txt](../License.txt) and [THIRD_PARTY_LICENSES.md](../THIRD_PARTY_LICENSES.md) |
| Primary consumer | **FALCON** (astrophysics application)                  |

### 1.2 Purpose

General-purpose GPU data-science / numerics for C# projects.

BAVCL accelerates large array operations on CUDA, OpenCL, or CPU backends while keeping smaller workloads on efficient CPU paths (SIMD). Actual backend implementation is dependant on ILGPU's implementation so this statement may not be correct.

### 1.3 Audience

- FALCON project (My Master's project)
- Personal, educational, academic, commercial, and research use per [License.txt](../License.txt) (attribution required for published research and commercial/professional use)

### 1.4 Solution Structure

```
BAVCL.sln
├── BAVCL/                  # Core library (net10.0)
└── Testing Console/        # Demo app + BenchmarkDotNet harness

BAVCL.Tests/                # Sibling repo — canonical test home
└── (separate solution at C:\Users\marce\Repos\BAVCL.Tests)
```

### 1.5 Out of Scope (v1) - Currently

- Matrix linear algebra (deferred)
- Table / dataframe structures (deferred)
- Public NuGet packaging

---

## 2. Design Principles

### 2.1 C#-Idiomatic API

Inspired by NumPy semantics but shaped for C# conventions: explicit types, properties, operator overloads, extension methods, and `IDisposable` patterns.

### 2.2 Specialized Types

- **Specialized types** where they add value: `Vector` (fp32), `Vector3`, `Mask`, future `VectorInt`, etc.
- **`VectorBase<T>`** provides shared GPU/cache infrastructure; there is **no** public generic `Vector<T>` fallback — add a new specialized type when a new element type is needed.

### 2.3 GPU-First / CPU-Explicit Pattern

Most high-throughput paths (operators, `OP`/`IPOP`, broadcast, mask ops) use **GPU kernels** directly. CPU paths exist where explicitly documented (e.g. `Abs()`, `Sum()`, small-array statistics).

| Pattern | Execution | When to use |
| -------- | --------- | ----------- |
| Default operators / `OP` | GPU (ILGPU) | Element-wise and broadcast math on `Vector` |
| Explicit CPU methods | CPU (SIMD where implemented) | `Abs()`, `Sum()`, `Mean()`, debugging, small arrays |
| `X` suffix | GPU kernel | Named GPU entry points (`AbsX`, `RsqrtX`, `ReverseX`, …) |

Examples: `Abs()` (CPU) vs `AbsX()` (GPU); `Sum()` (CPU) vs GPU `OP` for large element-wise work.

### 2.4 Scope-Only GPU Pinning (IMPLEMENTED)

`LiveCount` is managed by `GpuScope.Begin()`. Individual operations use scopes internally; callers wrapping custom kernels should use:

```csharp
using (GpuScope.Begin(modified: output, readOnly: inputA, inputB))
{
    kernel(...);
    gpu.accelerator.Synchronize();
}
```

CPU edits: `CpuScope.Begin<T>(ICacheable<T>, bool syncToGpu)` or `.CpuScope()` / `.CpuScopeAndSync()`. Scope on `ICacheable`; `GetReadOnlySpan` / explicit `EditCpu` on `ICacheable<T>`. See [MigrationGuide.md](MigrationGuide.md).

### 2.5 Pluggable Memory Management

`IMemoryManager` interface allows alternative strategies. Default: LRU auto-cache with configurable memory cap (80% of device memory).

### 2.6 Extension-Method Organization (IMPLEMENTED)

Data types stay thin (`Vector.cs`, `Vector3.cs` — constructors, operators, copy/equals, conversions, indexers). Operations live in `Modules/` as **namespace feature gates**: import only the modules you need and the corresponding extensions appear on `Vector`, `Vector3`, and primitive arrays.

| Namespace | Public API file | Internal helpers |
|-----------|-----------------|------------------|
| `BAVCL.Modules.Arithmetic` | `ArithmeticModule.cs` (`VectorArithmetic`, `VectorArithmeticExtensions`) | `Internal/SumCore.cs`, `Cross.cs`, `ElementWise.cs`, … |
| `BAVCL.Modules.Statistics` | `StatisticsModule.cs` (`VectorStatistics`, `VectorStatisticsExtensions`) | `Internal/DescriptiveStatistics.cs`, `ArrayStatistics.cs`, … |
| `BAVCL.Modules.Structural` | `StructuralModule.cs` (`VectorStructural`, `VectorStructuralExtensions`) | `Internal/Factories.cs`, `ShapeOps.cs`, `Formatting.cs` |
| `BAVCL.Modules.Generators` | `GeneratorsModule.cs` | `Internal/GeneratorsCore.cs` |
| `BAVCL.Modules.Geometric` | `GeometricModule.cs` (`Vector3Geometric`, `Vector3GeometricExtensions`) | `Internal/Vector3Geometry.cs` |
| `BAVCL.Modules.GpuOps` | `GpuOpsModule.cs` | `Internal/Broadcast.cs`, `VectorGpuOps.cs`, `Vector3GpuOps.cs`, `VectorVectorOp.cs`, … |
| `BAVCL.Modules.Masking` | `MaskModule.cs` (`MaskModuleExtensions`) | `Internal/MaskBitwiseOps.cs`, `MaskVectorOps.cs` |
| `BAVCL.Modules.IO` | `IO.cs`, formatters (`JsonFormatter`, `CsvFormatter`, `XmlFormatter`, `TxtFormatter`) | `Internal/IoSchema.cs`, `StructuredCsv.cs`, … |

Primitive-array `Print`, `Sum`, `Average`, `Min`, and `Max` live in **Structural** and **Statistics** modules (the former `Extensions/` folder was merged into `Modules/` — see §9.2).

Each module exposes **one API-catalog file** with C# 14 extension blocks. Static and instance members are split across paired public classes when required (CS0111), e.g. `VectorArithmetic` (static) + `VectorArithmeticExtensions` (instance + `*_IP`). Implementation lives in `Internal/` as `internal static` types — consumers never import or reference them.

```csharp
using BAVCL;                          // core Vector only

using BAVCL.Modules.Arithmetic;       // + Vector.Sum(v), v.Cross(b), …
using BAVCL.Modules.Statistics;       // + v.Mean(), arr.Min(), …

Vector.Sum(vec);   // NumPy-style static extension
vec.Sum();         // ndarray-style instance extension
```

`VectorBase<T>.Gpu` is `internal` to enable module access without public exposure.

### 2.7 Source-Only Distribution

Consumers reference the BAVCL project directly. No NuGet packaging planned YET.

---

## 3. Architecture Overview

### 3.1 High-Level Component Diagram

```mermaid
flowchart TD
    subgraph consumers [Consumers]
        FALCON[FALCON]
        TC[Testing Console]
        Other[Other Projects]
    end

    subgraph bavcl [BAVCL Library]
        GM[GPUManager]
        GPU[GPU Instance]
        KM[Kernel Modules]
        MM[IMemoryManager / LRU]
        GS[GPUScope]
        subgraph types [Vector Types]
            CB[CacheableBase T]
            VB[VectorBase T]
            V[Vector fp32]
            V3[Vector3]
            VT[Vertex CPU]
        end
        subgraph ops [Operations Extensions]
            OP[Abs Normalise Sum etc]
        end
    end

    consumers --> GM
    GM --> GPU
    GPU --> KM
    GPU --> MM
    consumers --> GS
    GS --> types
    types --> VB
    VB --> CB
    V --> VB
    V3 --> VB
    ops --> types
    ops --> GPU
```

### 3.2 Type Hierarchy

```mermaid
classDiagram
    class ICacheable {
        +uint ID
        +uint LiveCount
        +long MemorySize
        +DeCache()
        +SyncCPU()
    }
    class CacheableBase~T~ {
        +T[] Value
        +int Length
        +virtual MemorySize
        +GetBuffer()
        +SyncCPU()
        +Pull()
    }
    class VectorBase~T~ {
        +int Columns
        +Shape()
        +indexers
        +ToCSV()
    }
    class Vector {
        fp32 specialized
    }
    class Vector3 {
        3-component GPU
        Columns = 3 fixed
    }
    class Mask {
        packed bits implemented
    }

    ICacheable <|.. CacheableBase
    CacheableBase <|-- VectorBase
    CacheableBase <|.. Mask
    VectorBase <|-- Vector
    VectorBase <|-- Vector3
```

**`CacheableBase<T>`** is the abstract base for **any GPU-cacheable data** (`Source/Core/Bases/CacheableBase.cs`). It defines:

- GPU caching lifecycle (`Cache`, `DeCache`, `SyncCPU`, `GetBuffer`, `UpdateCache`)
- Coherence state (`ID`, `LiveCount`, `Residence`, `Length`)
- Host-side backing store (`Value[]`)
- `virtual MemorySize` — default `sizeof(T) × Length`; overridable for packed types (e.g. `Mask`)
- Span/read APIs (`Pull`, `RetrieveReadOnlySpan`, `ToArray`, `CpuScope` host)

**`VectorBase<T>`** extends `CacheableBase<T>` with vector shape and indexing:

- Shape parameters (`Columns`, `RowCount`, `Shape()`)
- Indexers and coordinate access (`GetAt`, `SetAt`)
- `IIO` surface (`Print`, `ToCSV` forwarder to Structural module)

Operations (`Sum`, `Mean`, `Min`, `Max`, etc.) live in **`Modules/`** — not on the type hierarchy.

It is **not** a "CPU mirror" — `CacheableBase` owns memory; `VectorBase` owns vector semantics shared by all vector kinds.

### 3.3 GPU Lifecycle

```mermaid
sequenceDiagram
    participant User
    participant GPUScope
    participant Vector
    participant LRU
    participant GPU

    User->>Vector: new Vector(gpu, data)
    Vector->>LRU: Cache() allocate buffer
    LRU-->>Vector: ID assigned

    User->>GPUScope: Pin(vecA, vecB)
    GPUScope->>Vector: Increment LiveCount

    User->>Vector: AbsX() via extension
    Vector->>GPU: Launch kernel
    GPU-->>Vector: Result on device

    User->>GPUScope: Dispose
    GPUScope->>Vector: Decrement LiveCount

    Note over LRU: If memory full and LiveCount==0
    LRU->>Vector: SyncCPU on eviction
    LRU->>LRU: Dispose buffer
```

### 3.4 GPUScope Flow

```mermaid
flowchart LR
    subgraph pin [GPUScope.Pin]
        A[Increment LiveCount per ICacheable]
        B[Return IDisposable scope]
        C[On Dispose decrement all]
    end
    subgraph run [GPUScope.Run]
        D[Pin internally]
        E[Execute callback]
        F[Dispose even on exception]
    end
    A --> B --> C
    D --> E --> F
```

| API                                 | Use case                           |
| ----------------------------------- | ---------------------------------- |
| `GPUScope.Pin(params ICacheable[])` | Multi-statement GPU blocks         |
| `GPUScope.Run(cacheables, Func<T>)` | Single expressions; exception-safe |
| `GPUScope.Run(cacheables, Action)`  | Void GPU blocks                    |

Nested scopes are supported via `Interlocked` refcount on `LiveCount`.

### 3.5 Kernel Module Registration (IMPLEMENTED)

```mermaid
flowchart TD
    Consumer[Consumer]
    GPUManager[GPUManager]
    Workloads[KernelWorkloads]
    Loader[KernelModuleLoader]
    GPU0[GPU instance 0]
    GPU1[GPU instance 1]

    Consumer --> GPUManager
    GPUManager -->|"GetGPU(): device + memory only"| GPU0
    GPUManager --> GPU1
    Consumer --> Config
    Consumer --> Loader
    Loader -->|"Load(gpu, config)"| GPU0
    Loader -->|"Load(gpu, config)"| GPU1
```

GPU creation and kernel loading are **separate steps**. Each `GPU` instance is configured independently via `KernelModuleLoader.Load<T>(gpu, domains)`. There is no implicit Core domain — if no modules are loaded, no kernels compile.

`GPUManager.Default` convenience: creates a GPU and loads `KernelWorkloads.Default` (fp32 Arithmetic + Structural).

### 3.6 Multi-GPU Data Flow (Target - WIP)

```mermaid
flowchart LR
    GM[GPUManager.Default]
    GPU0[GPU 0 Primary]
    GPU1[GPU 1 Secondary]
    VecA[Vector on GPU0]
    VecB[Vector on GPU1]
    Transfer[Cross-device transfer]
    Op[GPU Operation]

    GM --> GPU0
    GM --> GPU1
    VecA --> Transfer
    VecB --> Transfer
    Transfer --> Op
```

---

## 4. Current Functionality (v0)

This section documents **what exists in code today**. See [Section 19](#19-code-vs-vision-gaps) for open gaps and reconciliation status.

### 4.1 Entry Points

| Entry                       | Location                            | Role                                        |
| --------------------------- | ----------------------------------- | ------------------------------------------- |
| `GPUManager.Default`        | `Source/Core/GPU/Services/GPUManager.cs` | Singleton lazy GPU + Default workload     |
| `GPUManager.GetGPU()`       | same                                | Create bare GPU (no kernels) with memory cap |
| `GPUManager.GetGPU<TMem>()` | same                                | GPU with custom `IMemoryManager`          |
| `KernelModuleLoader.Load<T>()` | `Source/Core/GPU/KernelModules/`  | Compile selected domains for element type `T` onto a GPU |
| `KernelModuleLoader.LoadAll<T>()` | same                          | Compile every registered domain for `T`   |
| `KernelWorkloads.Default` / `.Geometry` | same                    | Named domain bundles (`Default` = Arithmetic + Structural + Mask) |

### 4.2 Vector (float32)

**Type:** `BAVCL.Vector` — `sealed partial class : VectorBase<float>`
**Type file:** `Source/Types/Vector.cs` (constructors, operators, mask/compare surface, copy/equals, conversions, indexers)
**Operations:** extension modules — Arithmetic, Statistics, Structural, Generators, GpuOps, Masking (see §2.6)

#### 4.2.1 Construction and Properties

| Method / Property                             | Description                                |
| --------------------------------------------- | ------------------------------------------ |
| `Vector(GPU, float[], columns=0, cache=true)` | Construct from array; `columns=0` row 1D (default), `1` column, `N>1` matrix |
| `Vector(GPU, int length, columns=0)`          | Uninitialized length (may contain garbage) |
| `Copy(cache=true)`                            | Deep copy                                  |
| `Equals(Vector)`                              | Element-wise equality after CPU sync       |
| `Shape()`                                     | `Shape` struct (`Rows`, `Cols`)            |
| `Flatten()`                                   | Set `Columns = 0` (1D row storage)         |
| `ToVector3()`                                 | Convert when length % 3 == 0               |

#### 4.2.2 Statistical Properties (CPU)

Implemented in `BAVCL.Modules.Statistics` (`StatisticsModule`, `VectorStatistics`).

| Method           | Implementation                                             | Notes    |
| ---------------- | ---------------------------------------------------------- | -------- |
| `Sum()`          | CPU SIMD (`System.Numerics.Vector<float>`); Kahan for ≥10⁴ | Override |
| `Mean()`         | `Sum() / Length`                                           |          |
| `Var()`          | SIMD for small; GPU `differenceSquared` for large          |          |
| `Std()`          | `Sqrt(Var())`                                              |          |
| `Min()`, `Max()` | CPU via `RetrieveReadOnlySpan()` after sync when needed    |          |
| `Range()`        | `Max - Min`                                                |          |

#### 4.2.3 Factory Methods

Implemented in `BAVCL.Modules.Structural` (`VectorStructural`) and `BAVCL.Modules.Generators` (`GeneratorsModule`).

| Method                                       | Description          |
| -------------------------------------------- | -------------------- |
| `Zeros(gpu, length, columns)`                | Zero-filled vector   |
| `Ones(gpu, length, columns)`                 | Ones-filled          |
| `Fill(gpu, value, length, columns)`          | Constant fill        |
| `Arange(gpu, start, end, interval, columns)` | Range with step      |
| `Linspace(gpu, start, end, steps, columns)`  | Evenly spaced values |
| `GeneratorsModule.Arange(...)` / `Linspace(...)` static | Return `float[]` only |

#### 4.2.4 Element-wise and Unary Operations

| Operation  | CPU               | GPU (`X` / module)        | In-place (`_IP`) |
| ---------- | ----------------- | ------------------------- | ---------------- |
| Abs        | `Abs`, `Abs_IP`   | `AbsX`, `AbsX_IP`         | yes              |
| Reciprocal | —                 | GPU kernel                | `Reciprocal_IP`  |
| Rsqrt      | CPU path          | `RsqrtX`, `RsqrtX_IP`     | yes              |
| Reverse    | `Reverse()` (CPU) | `ReverseX`, `ReverseX_IP` | yes              |
| Diff       | —                 | allocating GPU kernel     | `Diff_IP` (reuses buffer) |
| NanToNum   | —                 | `nanToNumKernel`          | `Nan_to_num_IP`  |
| Normalise  | —                 | GPU `OP` multiply (`GpuOpsModule`) | `Normalise_IP` |
| Log        | —                 | `LogKernel` (`GpuOpsModule`) | `Log_IP`      |

#### 4.2.5 Binary Operations and Operators

**Migration:** See [`MigrationGuide.md`](MigrationGuide.md) for breaking changes to `Columns`, `OP`/`IPOP`, `ReduceOP`, and `Matrix*`.

Binary `+`, `-`, `*`, `/`, `^` operator overloads use **NumPy-style element-wise broadcast** via `OP()` / `IPOP()`. Three separate API families exist for different semantics:

| Family | Methods | Semantics |
| ------ | ------- | --------- |
| **Broadcast (default)** | `OP`, `IPOP`, operators | NumPy element-wise broadcast via `broadcastOpKernel` / `broadcastOpKernelIP` |
| **Matrix calculator** | `MatrixAdd`, `MatrixSubtract`, `MatrixDivide`, `MatrixPow`, `MatrixMultiply`, `Cross` | Strict 2D rules: same shape for add/sub/div/pow; inner-dimension match for multiply |
| **Row reduction** | `ReduceOP` | `reduceRowOpKernel` — one output per matrix row (not broadcast, not matmul) |

| Method                        | Description                              |
| ----------------------------- | ---------------------------------------- |
| `OP(vecA, vecB, Operations)`  | NumPy broadcast element-wise             |
| `OP(vec, scalar, Operations)` | Vector-scalar                            |
| `IPOP(vecB, Operations)`      | In-place broadcast when left shape equals output shape |
| `IPOP(scalar, Operations)`    | In-place vector-scalar                   |
| `MatrixAdd` / `MatrixSubtract` / `MatrixDivide` / `MatrixPow` | Identical `(M,N)` matrices, element-wise |
| `MatrixMultiply` / `Cross`    | Matrix multiply `(M,K) × (K,N)` — `Cross` is the primary name; `MatrixMultiply` is an alias |
| `ReduceOP(vector, matrix, op)` | 1D row coefficient (`Columns=0`), length == matrix columns; allocates output length == matrix rows |
| `Dot(vecA, vecB)`             | Scalar inner product (equal length only) |

**`Operations` enum** (`Source/Core/Enums/Operations.cs`): `multiply`, `add`, `subtract`, `divide`, `pow`, `flipDivide`, `flipSubtract`, `flipPow`, `differenceSquared`, `distance`, `magnitude`.

**Storage (`Columns`):**

| `Columns` | Logical shape | Example |
| --------- | ------------- | ------- |
| `0` (default) | `(1, N)` row | `[1,2,3,4]` |
| `1` | `(N, 1)` column | `[[1],[2],[3],[4]]` |
| `N > 1` | `(Length/N, N)` matrix | `[[1,2,3],[4,5,6]]` |

`RowCount()` and `Shape()` derive from `Columns` and `Length` as above. `Is1D()` is true only when `Columns == 0`.

**No in-place row reduce:** `ReduceIPOP` is intentionally omitted. Row reduction reads a full coefficient vector (`Length == matrix.Columns`) and writes one scalar per row (`Length == matrix.RowCount()`). A single buffer cannot satisfy both layouts except on square matrices, and even then the row-wise kernel reads every coefficient element on each thread while writing row outputs into the same buffer — unsafe GPU aliasing without a coefficient snapshot. A column-wise per-thread scheme would avoid aliasing but would not implement shared-coefficient row reduction and would harm row-major coalescing. Use allocating `ReduceOP` instead.

**Broadcasting:** Operand shapes are resolved host-side into `BroadcastStrides` (row stride, column stride) where a length-one axis gets stride `0`. `broadcastOpKernel` / `broadcastOpKernelIP` map each output element to operand indices via multiply-add with no shape tests on device; only the operation stays specialized. Incompatible shapes throw `ShapeMismatchException`. `IPOP` throws `PerformanceException` (prefix: *This operation will lead to degraded performance:*) when the left operand would need resizing.

**Note:** `Vector3.Cross` is a separate optimised 3D geometric kernel — not related to `Vector.Cross` (matrix multiply).

**Note:** Unary `+` operator currently calls `AbsX` (likely unintentional — see §19 #21).

#### 4.2.5a Mask and comparison operators (on `Vector`)

Defined on `Source/Types/Vector.cs`; GPU kernels in `BAVCL.Modules.Masking`. See [§6.5](#65-vector-comparisons--mask-gpu) for full semantics.

| Surface | Examples |
| ------- | -------- |
| Compare → `Mask` | `CompareEquals`, `CompareNotEquals`, `Compare`, `>`, `<`, `>=`, `<=` |
| Filter / select | `vector & mask`, `vector.Filter(mask, fill)`, `vector << mask`, `vector[mask]` |

#### 4.2.6 Structural Operations

| Method                                   | Description                                 |
| ---------------------------------------- | ------------------------------------------- |
| `Transpose` / `Transpose_IP`             | GPU transpose kernel                        |
| `Dot(vecA, vecB)` / `Dot(scalar)`        | Dot product                                 |
| `Concat(vecA, vecB, axis, warp)`         | Concatenate along axis                      |
| `Append` / `Prepend`                     | Vector append                               |
| `Merge`                                  | Merge vectors                               |
| `GetSliceAsVector` / `GetSliceAsArray`   | Slice by row or column                      |
| `GetColumnAsVector` / `GetColumnAsArray` | Column extraction                           |
| `GetRowAsVector` / `GetRowAsArray`       | Row extraction                              |
| `TransferBuffer`                         | Copy GPU buffer reference to another vector |
| `All()`                                  | True if no zero values                      |

#### 4.2.7 Output

| Method                          | Description      |
| ------------------------------- | ---------------- |
| `Print(decimalplaces, syncCPU)` | Console output   |
| `ToStr(decimalplaces, syncCPU)` | Formatted string |
| `ToCSV()` (via `VectorBase`)    | CSV string       |

### 4.3 Vector3 (GPU 3D)

**Type:** `BAVCL.Geometric.Vector3` — `sealed partial class : VectorBase<float>`, `Columns` fixed at 3
**Type file:** `Source/Modules/Geometric/Types/Vector3.cs`
**Operations:** Geometric, GpuOps, Structural, Statistics, Arithmetic modules (see §2.6)

| Category     | Methods                                                                            |
| ------------ | ---------------------------------------------------------------------------------- |
| Construction | `Vector3(gpu, float[])`, `Vector3(gpu, int length)` — length must be multiple of 3 |
| Conversion   | `ToVector()`, `ToVector(columns)` — uses `Pull()` today (buffer reuse TBD — §19 #22) |
| Factory      | `VectorStructural.Zeros(gpu, length)`, `Fill(gpu, value, length)`                  |
| Operators    | `+`, `-`, `*`, `/`, `^` with Vector3 and float (GPU via `GpuOpsModule`)            |
| Geometry     | `Cross(vecA, vecB)`, `Dot(vecA, vecB)`, `Magnitude()`, `Magnitude(vecA, vecB)`, `Distance(vec)` — `LengthMismatchException` on size mismatch |
| Per-row ops  | `VOP(vec, op)`, `VOP(vecA, vecB, op)` → returns `Vector` of per-row results        |
| Indexing     | `this[int row, Coord]`, `GetAt`, `SetAt` (reads sync via `GetReadOnlySpan`; writes use `CpuScope`) |
| Concat       | With `Vector3`, `Vertex`, arrays, lists                                            |
| Access       | `AccessRow(vec, row)`                                                              |
| Copy         | `Copy()`                                                                           |
| Stats        | `Mean()`, `Min()`, `Max()`, `Range()` (Statistics); `Sum()` (Arithmetic) — **flat-buffer semantics** (treats storage as `float[]`, not per-Vector3 element); rework TBD (§19 #15) |

### 4.4 Vertex (CPU 3D)

**Type:** `BAVCL.Geometric.Vertex` — `struct`, CPU-only, complementary to `Vector3`
**File:** `Source/Modules/Geometric/Types/Vertex.cs`

| Category            | Methods                                                              |
| ------------------- | -------------------------------------------------------------------- |
| Construction        | `(x,y,z)`, `(float[])`, `(double x,y,z)`                             |
| Presets             | `UP`, `DOWN`, `FORWARD`, `BACKWARD`, `LEFT`, `RIGHT`                 |
| Operators           | `+`, `-`, `*`, `/` with Vertex and float                             |
| Math                | `Magnitude`, `Distance`, `Dot`, `Cross`, `UnitVector`, `Fract`       |
| Rendering           | `Reflect`, `Refract`, `NormalReflectance`, `Aces_approx`, `Reinhard` |
| Component setters   | `SetX`, `SetY`, `SetZ`                                               |
| Implicit conversion | From`Vector3` (pulls full array)                                     |

### 4.5 GPU Infrastructure

#### 4.5.1 GPU Class

`BAVCL.GPU` — wraps `Accelerator` + `IMemoryManager`. Provides allocation, buffer lookup, GC, kernel delegates, `Dispose()`.

#### 4.5.2 Compiled Kernels

Kernels are loaded selectively via `KernelModuleLoader` (see §7). Each domain file under `Source/Core/GPU/Kernels/` holds its own `partial GPU` delegate fields, load method, and kernel bodies.

| Kernel                                    | Domain      | Purpose                                         |
| ----------------------------------------- | ----------- | ----------------------------------------------- |
| `appendKernel`                            | Structural  | Append rows                                     |
| `getSliceKernel`                          | Structural  | Slice extraction                                |
| `reverseKernel`                           | Structural  | Reverse in-place                                |
| `transposekernel`                         | Structural  | Matrix transpose                                |
| `nanToNumKernel`                          | Arithmetic  | Replace NaN/Inf                                 |
| `a_opFKernel` / `s_opFKernel`             | Arithmetic  | Binary ops (array/scalar)                       |
| `a_FloatOPKernelIP` / `s_FloatOPKernelIP` | Arithmetic  | In-place binary ops                             |
| `broadcastOpKernel` / `broadcastOpKernelIP` | Arithmetic  | NumPy-style element-wise broadcast              |
| `reduceRowOpKernel`                       | Arithmetic  | Row-wise vector-matrix reduction (`ReduceOP`)   |
| `matmulKernel`                            | Arithmetic  | Matrix multiply (`Cross` / `MatrixMultiply`)    |
| `diffKernel`                              | Arithmetic  | Adjacent difference                             |
| `absKernel`                               | Arithmetic  | Absolute value                                  |
| `rcpKernel`                               | Arithmetic  | Reciprocal                                      |
| `rsqrtKernel`                             | Arithmetic  | Reciprocal sqrt                                 |
| `LogKernel`                               | Arithmetic  | Log with configurable base                      |
| `crossKernel`                             | Geometry    | 3D cross product                                |
| `normaliseKernel`                         | Geometry    | Per-row normalisation                           |
| `simdVectorKernel`                        | Geometry    | Per-row 3-wide ops (Vector3 magnitude/distance) |
| Mask kernels (8)                          | Mask        | See [§6.6](#66-kernel-strategy)                 |

**GpuOps module dependency:** `Modules/GpuOps` requires **Arithmetic** (fp32) kernels loaded (broadcast, element-wise, row reduce).

**Mask module dependency:** `Modules/Masking` requires **Mask** domain kernels (`KernelWorkloads.Default` includes Mask).

**Never loaded:** `TestSQRTKernel`, `TestMYSQRTKernel` in `Kernels/Experimental/` — no module provides them, so they throw `KernelNotCompiledException` if called.

#### 4.5.3 LRU Memory Manager

`BAVCL.Core.LRU` (in `Source/Core/Memory/`) implements `IMemoryManager`:

- `ConcurrentDictionary<uint, GpuCacheEntry>` tracks GPU buffers
- LRU queue for eviction order
- `AvailableMemory` = device memory × cap (default 80%)
- `MemoryUsed` tracked via `sizeof(T) × length` estimates
- On eviction when `LiveCount == 0`: sync to CPU via `SyncCPU(buffer)`, dispose buffer
- `GC(memRequired)` evicts until space available
- **`GetBuffer`**: lock-free dictionary read
- **`UpdateBuffer`**: `GetReadOnlySpan()` / CPU sync **outside** `lock(this)`; dictionary lookup, `CopyFromCPU`, and allocation **inside** the lock

**TODOs in code:** dirty-flag to skip unnecessary CPU sync; only sync if data changed.

### 4.6 IO

`BAVCL.Modules.IO.IO` — typed persistence with formatter strategies. Every on-disk file is a
**collection** (one canonical shape per format, used for both a single document and many):

| Format | Collection wire shape |
| ------ | ---------------------- |
| CSV | `schemaVersion,<n>` line, then one shared item header row + one metadata/data row per item (items omit `schemaVersion`) |
| JSON | `{"schemaVersion":<n>,"items":[{...},...]}` (items omit `schemaVersion`) |
| XML | `<root schemaVersion="<n>">` wrapper with typed child elements per item (`<vector>`, `<vector3>`, `<mask>` — element name encodes type; children omit `schemaVersion` and `type`) |
| TXT | Pipe `ToStr` segments joined by a line containing exactly `---` (single item → no delimiter) |

Mask CSV rows are homogeneous per file: bool **or** packed for every row, fixed by the first item
written to that `FileSession`.

| API | Notes |
| --- | ----- |
| `IO.Serialize<T, TFormatter>(value, fileName, directory?, overwrite?, flags?)` | One-shot: write a finalized collection file containing exactly `value` |
| `IO.Deserialize<T, TFormatter>(gpu, fileName, directory?)` | Read the file; throws unless it contains exactly one document |
| `IO.DeserializeAll<T, TFormatter>(gpu, fileName, directory?)` | Read every document in the file as `IReadOnlyList<T>` |
| `CreateWriter<T, TFormatter>` / `CreateReader<T, TFormatter>` | Returns a `FileSession<T, TFormatter>` |

`FileSession<T, TFormatter>` (`IDisposable`):

| Method | Behavior |
| --- | ----- |
| `string Serialize(value, flags?)` | Formatter fragment only; no disk I/O |
| `void Write(value, flags?)` | Replace the file with a finalized collection containing exactly one item |
| `void Append(value, flags?)` | Open the collection (if needed) and add another item |
| `void Flush()` | Finalize an in-progress collection (closes the JSON `]` / XML `</root>`); idempotent |
| `void Dispose()` | Calls `Flush()` |
| `T Deserialize(gpu)` | Exactly one document; throws (pointing to `DeserializeAll`) otherwise |
| `IReadOnlyList<T> DeserializeAll(gpu)` | Every document in the file |
| `void WriteRaw(content)` / `string ReadRaw()` | Raw text access; `WriteRaw` throws while a collection is open |

Formatters implement two per-type interfaces, both singletons via `ISingleton<TFormatter>` with
`Default` + `Extension`:

- `IFormatter<T>` — serializes/deserializes a single item's bare fragment (unchanged shape from
  before multi-document support: a JSON object, an XML element (type from node name), a CSV header+row, or a TXT
  pipe grid).
- `ICollectionFormatter<T>` — combines fragments into a collection file (`OpenCollection` /
  `AppendItem` / `CloseCollection`) and splits a collection file back into `IReadOnlyList<T>`
  (`DeserializeAll`).

JSON payloads are minimal (data + columns; Mask packed adds `count`). Optional `type`, `dtype`,
`schemaVersion` on collection roots (and on per-item `IFormatter` fragments). No forced
`saved_data/` subdirectory.

**Per-format metadata rules:**

| Field | JSON collection items | CSV collection items | XML collection items | `IFormatter` fragments |
| ----- | --------------------- | -------------------- | -------------------- | ---------------------- |
| `schemaVersion` | Omitted (on root) | Omitted (on root line) | Omitted (on `<root>`) | Present (JSON/CSV/XML) |
| `type` | Present (`type` property) | Present (`type` column) | **Omitted** — element name (`<vector>`, `<vector3>`, `<mask>`) encodes type | JSON/CSV: present; XML: omitted (node name) |

JSON and CSV lack a typed node name, so collection items retain `type` for self-description.
XML omits it deliberately to avoid repeating information already expressed by the element tag.

**Fragment `schemaVersion` (XML):** when the attribute is absent on an `IFormatter` fragment,
deserialization assumes `CurrentSchemaVersion` rather than failing. Serialized round-trips always
include the attribute; the default exists for hand-crafted or legacy fragments only.

Out of scope for now: mixing different BAVCL types (e.g. `Vector` + `Mask`) in one file, a
forward-only streaming reader, and streamed/chunked write for very large datasets — all are
potential future roadmap items (see §13.4).

**Read/write matrix (current):**

| Type | JSON | CSV | XML | TXT |
| ---- | ---- | --- | --- | --- |
| `Vector` | R+W | R+W | R+W | R+W |
| `Vector3` | R+W | R+W | R+W | R+W |
| `Mask` (bool) | R+W | R+W | R+W | R+W |
| `Mask` (packed) | R+W | R+W | R+W | W packed; **R bool grid only** (packed TXT read not yet implemented — §19 #32) |

Formatters: `JsonFormatter`, `CsvFormatter`, `XmlFormatter`, `TxtFormatter` (singleton `Default` + `Extension`).

### 4.7 Experimental Math

`Source/Experimental/TestCls.cs` (~820 lines) — fast approximations for sqrt, cbrt, log2. Referenced from kernel experiments. Multiple commented-out alternate implementations. Not production-tested.

### 4.8 Stubs

| Type              | State                                                                    |
| ----------------- | ------------------------------------------------------------------------ |
| `Matrix`          | `Source/Types/Matrix.cs` — constructor throws `NotImplementedException`; only `MatrixLength()` works |
| `Table`           | `Source/Types/Table.cs` — empty class                                    |
| `Mask`            | **Implemented** — `Source/Types/Mask.cs`; helpers in `Source/Core/Helpers/MaskBitOps.cs`; GPU module in `Source/Modules/Mask/` (namespace `BAVCL.Modules.Masking`) |

Plotting prototype removed from repo; see [§14 Plotting](#14-plotting) (roadmap only).

### 4.9 Utility

`Source/Core/Helpers/Util.cs` (`BAVCL.Core.Util`) — `IsClose()` for float comparison with NaN/Inf handling.

---

## 5. Type System Roadmap

### 5.1 Priority Order

Broadening beyond fp32 is a priority. **`Mask` is implemented** (see Section 6); resize and CPU aggregates remain roadmap.

1. **float64 / double** — `VectorDouble` or dedicated double type
2. **int32** — `VectorInt`
3. **int64**
4. **uint**
5. **Complex** — ideally `System.Numerics.Complex` as unmanaged struct; may require custom `ComplexFloat` if constraints block it

### 5.2 Specialized Types

```
USE specialized type per element kind (Vector, Vector3, Mask, future VectorInt, …)
VectorBase<T> provides shared infrastructure — no public Vector<T> generic fallback
```

### 5.3 Matrix and Table

Nice-to-have, **not** near-term. Vector 2D layout (`Columns > 1`) covers current matrix-like needs.

---

## 6. Mask Specification

### 6.1 Storage

**Packed bits** for GPU efficiency — multiple boolean flags per machine word. Values are strictly 0 or 1.

Exact layout (bits per word, alignment) to be **benchmark-driven** on target GPUs.

### 6.2 Filter (apply mask)

Masked-out elements receive a **configurable fill value** (default `0`):

```csharp
var filtered = vector & mask;              // fill = default(float) = 0f
var filtered = vector.Filter(mask, fill);  // explicit fill (e.g. NaN)
```

Requires `using BAVCL.Modules.Masking` for `Filter` extension.

### 6.3 Select (compact)

Returns a **1D** `Vector` (`Columns = 0`) containing only elements where the mask is `true` (NumPy-style):

```csharp
var data = vector << mask;     // primary operator
var data = vector[mask];       // indexer
var data = vector.Select(mask); // LINQ-like name
```

### 6.4 Mask×Mask bitwise (GPU)

Namespace `BAVCL.Modules.Masking` (folder `Source/Modules/Mask/`) for `OP` / `IPOP` extensions.

| Operation | Syntax |
| --------- | ------ |
| And | `maskA & maskB` |
| Or | `maskA \| maskB` |
| Xor | `maskA ^ maskB` |
| Not | `~mask` / `!mask` |
| Set all | `+mask` / `SetAll()` |
| Clear all | `-mask` / `ClearAll()` |
| In-place | `&=`, `\|=`, `^=` |
| Nand / Nor / Xnor | methods + composed `~(a&b)` etc. |

Broadcast rules match `Vector` shape broadcast.

`MaskOperation` carries **binary lane operations only** (`And`, `Or`, `Xor`, `Nand`, `Nor`, `Xnor`). Complement, set-all and clear-all are dispatched as an operation against a constant word (`x ^ -1`, `x | -1`, `x & 0`), so they need no enum member and no dedicated kernel.

### 6.5 Vector comparisons → Mask (GPU)

| Operation | Syntax |
| --------- | ------ |
| Greater / Less / GEq / LEq | `vectorA > vectorB`, `<`, `>=`, `<=` |
| Scalar compare | `vector > 0.5f`, etc. |
| Element-wise == | `vector.CompareEquals(other)` / `CompareEquals(scalar)` |
| Element-wise != | `vector.CompareNotEquals(other)` / `CompareNotEquals(scalar)` |
| Unified | `vector.Compare(other, VectorComparison.GreaterOrEqual)` |

`bool Equals(Vector)` remains **aggregate** structural equality (unchanged).

NaN: `CompareEquals` treats NaN == NaN as `true`; ordered comparisons yield `false` when NaN is involved.

`KernelWorkloads.Default` includes `KernelDomain.Mask`. GPU execution via `KernelDomain.Mask` kernels (`SpecializedValue<int>` dispatch).

### 6.6 Kernel strategy

Authoring rules and design principles: **[GPGPUKernelGuide.md](./GPGPUKernelGuide.md)** (P1–P6). This section is the kernel inventory for `KernelDomain.Mask`.

`KernelDomain.Mask` compiles eight kernels. Launch mapping maximizes parallelism (P1); shape setup is host-resolved where possible (P3); the only device `switch` is over the `SpecializedValue<int>` operation, which folds to a constant at specialization time so every thread follows one path (P4). Reduction-style loops inside a thread remain valid where the algorithm requires them — see the guide.

| Kernel | Parallelism | Notes |
| ------ | ----------- | ----- |
| `maskWordOpKernel` | one thread per packed word | 32 lanes resolved by one bitwise instruction; aliasing output with left gives the in-place form |
| `maskWordConstOpKernel` | one thread per packed word | complement / set-all / clear-all against a constant word |
| `maskLaneOpKernel` | one thread per lane | broadcast breaks word alignment, so lanes are addressed individually and merged with `Atomic.Or` |
| `maskLaneOpKernelIP` | one thread per lane | in-place broadcast; each thread owns one lane, so atomic clear-then-set cannot race a neighbour |
| `vectorCompareMaskKernel` | one thread per element | compare, then `Atomic.Or` the lane into zeroed mask storage |
| `vectorScalarCompareMaskKernel` | one thread per element | no shape parameters |
| `vectorMaskFilterKernel` | one thread per element | branchless `Utilities.Select` between the source value and the fill |
| `vectorGatherKernel` | one thread per output element | `output[i] = input[indices[i]]` |

**Broadcast addressing.** Operand shapes are resolved host-side into a `BroadcastStrides` pair (row stride, column stride) where a length-one axis gets stride `0`. Operand indexing is then a multiply-add with no shape tests; only the operation stays specialized. Vector broadcast and mask lane kernels share this pattern.

**Padding lanes.** Word kernels take a precomputed `(lastWord, tailMask)` pair and clear padding with a `Utilities.Select`. Lane kernels need no padding handling: mask storage is allocated zeroed and only logical lanes are launched.

**Compaction.** `Select` must size its output `Vector` before launching, and that length depends on mask contents, so surviving source indices are collected host-side from the packed words; the gather itself stays on the device. A device-side scan would remove the host pass and is the natural upgrade once mask aggregates land (§6.8).

### 6.7 Resize (planned future)

**Current implementation:** logical size is fixed after construction. `ElementCount` is stored in a `readonly` field set only in constructors; inherited `CacheableBase.Length` is the packed **storage word count** (not boolean element count).

**Planned:** resize support analogous to array resize — logical length is not fixed forever, but changes only through explicit resize/replace operations (not silent mutation of `Length` on the base type).

**API (TBD during implementation):**

- `Resize(int newElementCount)` — grow/shrink with default fill (`false`) for new slots
- and/or `ReplaceFrom(ReadOnlySpan<bool>)` / `ReplaceFrom(bool[])` — full replace with new logical content

**Coupled state:** a resize must update all of the following in one coordinated operation (same structural-edit rules as `Vector` length changes):

| Field | Meaning |
| ----- | ------- |
| `Value` | new `int[]` packed word buffer |
| `CacheableBase._length` | storage word count (GPU buffer / span length) |
| `_elementCount` | logical boolean element count |

Logical element count **cannot** be derived from word count alone (e.g. 97 and 100 booleans both use 4 words), so `_elementCount` must remain an explicit stored field.

**Threading (when resize is implemented):**

- Remove `readonly` from `_elementCount`.
- Mark `_elementCount` as `volatile int`, mirroring `CacheableBase._length` — cross-thread **visibility** for post-construction updates, not full atomicity with `Value` or bit reads.
- Centralize writes in the resize/replace API; do not scatter `_elementCount` updates across call sites.
- Prefer `CpuScope` (or equivalent) for resize, consistent with other structural mutations; document that unsynchronized concurrent resize + `GetBit` / indexer reads are not supported.
- `Residence` cross-thread safety is already handled by `ResidenceField` (`Volatile.Read`/`Write` + `Interlocked.CompareExchange`); no change required there for resize.

**Reference:** `Vector` structural ops already resize by replacing `Value` and setting `Length = Value.Length` (e.g. `Modules/Structural/Internal/Factories.cs`, `ShapeOps.cs`).

### 6.8 Mask aggregates (roadmap — CPU SIMD)

Planned CPU-side helpers (not GPU kernels in v1):

- `mask.CountTrue()` — popcount over logical bits (padding excluded)
- `mask.Any()` — any set bit
- `mask.All()` — all logical bits set

Implementation target: `System.Numerics.Vector<int>` / packed word scan via `RetrieveReadOnlySpan`.

---

## 7. Kernel Module System

**Agent / implementer reference:** see [GPGPUKernelGuide.md](./GPGPUKernelGuide.md) for GPU design principles, host/device boundaries, and patterns. Project skill: `.agents/skills/bavcl-gpgpu/`.

### 7.1 Module Dimensions

Two axes of modularity:

| Axis         | Examples                                                    |
| ------------ | ----------------------------------------------------------- |
| **Domain**   | `KernelDomain` enum: Arithmetic, Structural, Geometry, Statistics, LinearAlgebra, Astrophysics, Mask |
| **Element type** | The CLR type itself (`typeof(T)` from `Load<T>`): `float`, `double`, `int`, … |

There is no parallel datatype enum — the generic parameter is the key. No implicit Core domain: load nothing and nothing compiles.

### 7.2 Registration

- Loaded per `GPU` instance via `KernelModuleLoader.Load<T>(gpu, domains)`
- `Load` is **additive**: already-loaded `(domain, type)` pairs are skipped
- Only requested kernels are compiled — reduces startup latency and JIT memory vs monolithic load-all
- Unimplemented `(domain, type)` throws `KernelModuleNotAvailableException` at load time

### 7.3 API

```csharp
// 1. Create GPU(s) — device + memory only, no kernel compilation
var gpuA = GPUManager.GetGPU();
var gpuB = GPUManager.GetGPU(memoryCap: 0.5f);

// 2. Configure each GPU independently
KernelModuleLoader.Load<float>(gpuA, KernelWorkloads.Default);
KernelModuleLoader.Load<float>(gpuB, KernelWorkloads.Geometry);

// Explicit domains
KernelModuleLoader.Load<float>(gpuA, KernelDomain.Arithmetic, KernelDomain.Structural);

// Everything registered for a type
KernelModuleLoader.LoadAll<float>(gpuA);

// Convenience singleton (GetGPU + Default workload)
GPU gpu = GPUManager.Default;
```

### 7.4 Workloads

`KernelWorkloads` exposes named `KernelDomain[]` bundles that feed straight into `Load<T>`.

| Bundle                     | Domains                           | Purpose                                             |
| -------------------------- | --------------------------------- | --------------------------------------------------- |
| `KernelWorkloads.Default`  | Arithmetic, Structural, **Mask**  | Standard numerics + mask kernels                    |
| `KernelWorkloads.Geometry` | Default + Geometry                | Vector3 GPU ops (cross, magnitude/distance)         |

For full parity with the old load-all behaviour, use `LoadAll<T>`.

### 7.5 Implemented fp32 Modules

| Domain        | Kernels                                                                 | Status   |
| ------------- | ----------------------------------------------------------------------- | -------- |
| Structural    | append, getSlice, reverse, transpose                                    | Implemented |
| Arithmetic    | abs, rcp, rsqrt, diff, nanToNum, Log, matmul, a/s op, broadcast, reduceRow | Implemented |
| Geometry      | cross, normalise, simdVector                                            | Implemented |
| Mask          | maskWordOp*, maskLaneOp*, vectorCompare*, vectorMaskFilter, vectorGather | Implemented |
| Statistics    | —                                                                       | Not yet  |
| LinearAlgebra | — (`matmul` in Arithmetic for now)                                    | Not yet  |
| Astrophysics  | —                                                                       | Not yet  |

### 7.6 Extensibility

Adding a module (e.g. fp64 arithmetic, or a new `KernelDomain.Mask`) is two steps:

1. Add a kernel file under `Core/GPU/Kernels/{Domain}/` holding that module's delegate fields, its `Load{Domain}{Type}Kernels()` method, and the kernel bodies.
2. Add one entry to the `Modules` dictionary in `KernelModuleLoader`:

```csharp
[(KernelDomain.Arithmetic, typeof(double))] = static gpu => gpu.LoadArithmeticFloat64Kernels(),
```

Callers then use `Load<double>(gpu, KernelDomain.Arithmetic)` with no API change.

File layout:

```
Source/Core/GPU/
  KernelModules/
    KernelDomain.cs         # domain enum
    KernelWorkloads.cs      # named domain bundles
    KernelModuleLoader.cs   # registry dictionary + Load<T> / LoadAll<T>
  Kernels/
    Arithmetic/ArithmeticKernels.Float32.cs
    Structural/StructuralKernels.Float32.cs
    Geometry/GeometryKernels.Float32.cs
    Mask/MaskKernels.Int32.cs, MaskKernels.Float32.cs
    Experimental/ExperimentalKernels.cs       # never-loaded test stubs
    Shared/KernelHelpers.cs
```

Per-device load state (which `(domain, type)` pairs are compiled) lives on `GPU` itself.

---

## 8. GPU and Memory

### 8.1 Device Selection

Preference order: **CUDA > OpenCL > CPU**. Tie-break by `MaxConstantMemory`. `forceCPU` flag available.

### 8.2 Memory Cap

Default: 80% of device memory (`memoryCap = 0.8f`). Configurable per `GetGPU()` call.

### 8.3 Pluggable Memory Manager

`IMemoryManager` contract (`BAVCL/Core/Interfaces/IMemoryManager.cs`):

- `Allocate`, `AllocateEmpty`, `UpdateBuffer`
- `GetBuffer`, `GC`, `GCItem` (evict with conditional sync), `FreeBuffer` (discard without sync)
- `AvailableMemory`, `MemoryUsed`, `PrintMemoryUsage`

Alternative implementations can replace LRU without changing vector types.

### 8.4 GpuScope (IMPLEMENTED)

`BAVCL/Core/Memory/Scopes/GpuScope.cs` — `GpuScope.Begin(modified, readOnly)` on `ICacheable`. Modified vectors get `ActiveGpu`; read-only vectors bump `LiveCount` only. Nested pins supported for late output allocation. Residence transitions use `ResidenceScopeHelper` (CAS with re-read/reconcile; no force writes).

### 8.5 Actual GPU Memory Tracking (Exploration) - Planned future

**Goal:** Query or track real GPU memory consumption instead of relying solely on `sizeof(T) × length` estimates.

Benefits:

- More accurate `MemoryUsed` reporting
- Faster allocation decisions (less estimation overhead in `GC()`)
- Better eviction timing

`IMemoryManager.PrintMemoryUsage` TODO: Bytes/KB/GB display formats.

Feasibility depends on ILGPU and device APIs — document as investigation item.

### 8.6 Residence Flags and Coherence (IMPLEMENTED)

`ICacheable.Residence` tracks CPU/GPU authority and active scopes (`Cpu`, `Gpu`, `InSync`, `ActiveCpu`, `ActiveGpu`). Updates use `ResidenceField.TryTransition` (`Interlocked.CompareExchange` on the underlying byte) for compare-and-swap transitions; `_cpuScopeDepth` uses `Interlocked`. `SyncCPU()` / `UpdateCache()` no-op when data is already in the target state. LRU eviction via `GCItem` syncs only when GPU-authoritative; resize uses `FreeBuffer` without sync.

**Read API:**

| Method | Behavior |
|--------|----------|
| `GetCpuReadOnlySpan()` | Zero-copy view of current CPU buffer; **no GPU sync** |
| `GetReadOnlySpan()` | `SyncCPU()` then `GetCpuReadOnlySpan()` — use for reads and LRU upload |
| `GetAt` / indexers (get) | `GetReadOnlySpan()[index]` |
| `ToArray()` | **Always allocates** a heap copy; syncs when needed |
| `ICacheable<T>.GetReadOnlySpan()` | Same as above; used by memory manager for upload |

**Write API:** `CpuScope` + `EditableView<T>` or `SetAt` (opens `CpuScope` internally; not for tight loops). `IndexingMode` removed — use scoping instead.

**Shape caching (future):** Cache a `Shape` field on `VectorBase`, invalidated when `Length` or `Columns` change. Marginal benefit today (`Shape()` is cheap); revisit if called in hot loops.

### 8.7 Multi-GPU (Target)

- `GPUManager.Default` for primary device
- API to enumerate and create additional `GPU` instances
- Cross-device data transfer when operands reside on different GPUs
- Automatic workload distribution: **deferred**

---

## 9. Code Organization Roadmap

### 9.1 Previous State (pre-refactor)

Operations were spread across **25+ partial class files** per type (`Core/Vector/*.cs`, `Geometric/Vector3/*.cs`).

### 9.2 Current Layout (IMPLEMENTED)

```
BAVCL/
  Core/
    Bases/
      CacheableBase.cs       # ALL memory: ICacheable<T>, coherence, LRU, virtual MemorySize
      VectorBase.cs          # Columns, shape, IIO forwarders, indexers, validation
    Types/
      Vector.cs              # Slim: ctors, operators, Copy, Equals, ToVector3
      Matrix.cs              # Stub
      Table.cs               # Stub
    GPU/ Memory/ Interfaces/ …
  Modules/
    Arithmetic/
      ArithmeticModule.cs
      Internal/                       # SumCore, Cross, ElementWise, DotProduct, MatrixOps
    Statistics/
      StatisticsModule.cs
      Internal/                       # DescriptiveStatistics, ArrayStatistics, Reduce
    Structural/
      StructuralModule.cs
      Internal/                       # Factories, ShapeOps, Formatting (incl. ToCsv)
    Geometric/
      GeometricModule.cs
      Types/
        Vector3.cs                    # Single file: ctors, Copy, Coord indexers, operators
        Vertex.cs                     # CPU 3-vector struct
        Coord.cs                      # BAVCL.Geometric.Enums
      Internal/                       # Vector3Geometry
    GpuOps/
      GpuOpsModule.cs
      Internal/                       # Broadcast, VectorGpuOps, Vector3GpuOps, VectorVectorOp, Vector3Kernels
```

The former `BAVCL/Extensions/` folder has been merged into `Modules/`. Global usings in `GlobalUsings.cs` import Arithmetic, Structural, and Statistics for in-library convenience. External consumers opt in per module via `using BAVCL.Modules.*`.

### 9.3 Completed Migration Steps

1. Changed `VectorBase<T>.Gpu` from `protected` to `internal`
2. Extracted all operations to `Modules/` extension methods
3. Collapsed `Vector` and `Vector3` partial classes into slim type definitions
4. Removed abstract `Sum()`/`Mean()`/`Range()` from `VectorBase<T>`
5. Consolidated per-operation public classes into one API-catalog file per module with `Internal/` implementation helpers
6. Extracted `CacheableBase<T>` from `VectorBase<T>` — memory in `Core/Bases/CacheableBase.cs`; `VectorBase` retains shape/indexing only; `Min`/`Max`/`ToCSV` implementation in Modules
7. Consolidated type folders: `Core/Bases/` (CacheableBase + VectorBase), `Core/Types/` (Vector, Matrix, Table), `Modules/Geometric/Types/` (Vector3, Vertex, Coord); merged Vector3 partials into one file

### 9.4 .NET 11 Discriminated Unions

Evaluate C# DUs (expected .NET 11) to reduce per-type operation boilerplate. Until then, multi-type methods live in shared operation files.

---

## 10. CPU Execution Path

### 10.1 Contract

| Pattern                                 | Execution                    | Scope required   |
| --------------------------------------- | ---------------------------- | ---------------- |
| `Abs()`, `Sum()`, `Mean()`              | CPU (SIMD where implemented) | No               |
| `AbsX()`, `ReverseX()`, operator `OP()` | GPU kernel                   | Yes (`GPUScope`) |

### 10.2 SIMD Usage (v0)

`Vector.Sum()` uses `System.Numerics.Vector<float>` with Kahan summation for arrays ≥ 10⁴ elements. `Var()` uses SIMD for smaller arrays.

### 10.3 Operator Overloads (Target)

Default operators should use CPU paths. GPU variants via `X` suffix or explicit `OPX` methods. **Current code diverges:** all operators route through GPU kernels.

---

## 11. Geometry Module

### 11.1 Vector3 — Permanent Specialized Type

GPU-backed 3D vectors. Length always multiple of 3. `Columns` locked to 3.

Long-term: stays as permanent specialized type (not replaced by generic `Vector<T>`).

**Stats rework:** `Mean()`, `Range()`, `Sum()` on Vector3 need rework or removal — not meaningful as flat-array statistics.

### 11.2 Vertex — Complementary CPU Type

CPU-side `struct` for rendering and ray-tracing helpers: reflect, refract, tone mapping. Works alongside `Vector3`; not deprecated.

### 11.3 Vector3 ↔ Vector Conversion

Target: reuse GPU buffer ID on conversion instead of `Pull()` + reallocate. TODO in `Vector3.cs`.

---

## 12. Astrophysics Module

### 12.1 Scope

Driven by **FALCON** requirements. Primary use case: **age-from-redshift integrals** using astrophysical models (reverse-engineered from Astropy).

### 12.2 Relationship to FALCON

FALCON is the **primary consumer** of BAVCL. Astrophysics operations in BAVCL exist to support FALCON's computational needs.

### 12.3 Detailed Formulas

Specific integral definitions, cosmological parameters, and model variants belong in a FALCON specification or Astrophysics subsection — to be expanded when FALCON integration begins.

### 12.4 FITS I/O

Deferred until core IO formats are polished. FITS is a later priority for astrophysics data interchange.

---

## 13. IO Module

### 13.1 v1 Priority — Polish Existing + Structured Formats

| Format | Status | Notes |
| ------ | ------ | ----- |
| CSV | **R+W** — `Vector`, `Vector3`, `Mask` (multi-document collections) | `CsvFormatter` |
| TXT | **R+W** — `Vector`, `Vector3`, `Mask` (bool); packed mask **write** only | `TxtFormatter`; packed TXT read gap — §19 #32 |
| JSON | **R+W** — `Vector`, `Vector3`, `Mask` | `JsonFormatter` |
| XML | **R+W** — `Vector`, `Vector3`, `Mask` | `XmlFormatter` |
| YAML | Not implemented | Human-readable config + data |

Formatters: `JsonFormatter`, `CsvFormatter`, `XmlFormatter`, `TxtFormatter`. Later placeholders: FITS, NPY, HDF5.

### 13.2 Later Formats

- FITS (astrophysics)
- NPY (NumPy binary)
- HDF5 (large scientific datasets)

### 13.3 Design Goals

IO persists computed results via generic `FileSession<T, TFormatter>` writers/readers. **JSON and XML** are structured interchange formats for `Vector`, `Vector3`, and `Mask` (packed default + bool interop). `IIO` remains a display/CSV helper contract, not the persistence surface.

### 13.4 Roadmap

- **Streamed/chunked write for very large datasets** — avoid building the full serialized
  string/XML tree/JSON document in memory; write collection envelopes and per-item fragments
  incrementally to disk (complements today's `Append`/`Flush` session model).
- Mixed BAVCL types in one file (e.g. `Vector` + `Vector3` + `Mask`).
- Forward-only streaming reader (`ReadNext`).

---

## 14. Plotting

### 14.1 Current State

No in-repo plotting prototype. Prior `Plotter.cs` (Windows / `System.Drawing`) was removed.

### 14.2 Target

Cross-platform plotting for debugging and visualization. **Low priority.** Replace `System.Drawing` with a portable library when undertaken.

---

## 15. Experimental Math

### 15.1 Status

Stays in `BAVCL/Experimental/`. Opt-in — consumers import explicitly.

### 15.2 Target State

- Trim to **one best implementation** per function (sqrt, cbrt, log)
- Remove hundreds of lines of commented alternatives
- xUnit tests for accuracy bounds
- Not promoted to production kernels unless explicitly chosen

---

## 16. Testing and Quality

### 16.1 Test Repository

**Canonical home:** `C:\Users\marce\Repos\BAVCL.Tests` (sibling repo, separate solution).

| Aspect           | Current                          | Target                             |
| ---------------- | -------------------------------- | ---------------------------------- |
| Framework        | xUnit                            | xUnit                              |
| Benchmarks       | BenchmarkDotNet in same project  | Keep BenchmarkDotNet               |
| Target framework | net10.0 (library); tests may lag | **net10.0** aligned everywhere     |
| Project type     | Test + benchmark in sibling repo | Separate test + benchmark concerns |
| Coverage         | Extensive GPU/CPU suites (Vector, Mask, IO, …) | CI gate on `dotnet test` |

### 16.2 Test Strategy

1. **xUnit correctness** — every operation validated against CPU reference or known values
2. **GPU vs CPU parity** — `AbsX()` results match `Abs()` within tolerance
3. **BenchmarkDotNet regression** — performance tracked over time
4. **Experimental math** — accuracy bounds tested

### 16.3 Test Suites (BAVCL.Tests)

Includes Vector broadcast, matrix ops, Mask, IO, GpuScope, and related suites. See sibling repo for current file list.

### 16.4 CI (Target)

Current: CodeQL security scan only. Target: build + `dotnet test` + benchmark regression gate.

### 16.5 Library Repo Tests Folder

No test project in the library solution — all authoritative tests live in **BAVCL.Tests** (sibling repo).

---

## 17. Platform Support

### 17.1 Targets

Fully cross-platform via ILGPU:

| Backend     | Priority             |
| ----------- | -------------------- |
| NVIDIA CUDA | Highest performance  |
| OpenCL      | Broad GPU support    |
| CPU         | Fallback / debugging |

ILGPU supports GPUs roughly GTX 750 era and newer. BAVCL should match ILGPU's device support.

### 17.2 .NET Version

- **Now:** .NET 10
- **Evaluate:** .NET 11 discriminated unions for operation boilerplate reduction

### 17.3 Known Config Issues

VS Code `launch.json` references `net6.0` but projects target `net10.0` — stale config to fix separately.

---

## 18. Deferred / Nice-to-Have

| Item                        | Notes                             |
| --------------------------- | --------------------------------- |
| Matrix type                 | Full linear algebra — deferred    |
| Table / dataframe           | Columnar named data — deferred    |
| Multi-GPU auto-distribution | Manual device selection first     |
| Public NuGet                | Not planned                       |
| README                      | To be created after spec approval |

---

## 19. Code vs Vision Gaps

Living reconciliation log. Rows are **open** until manually closed. When code and spec diverge, pick one: update spec, change code, or document intentional gap.

| #   | Area                 | Status | Current code / gap | Target / notes | Key files |
| --- | -------------------- | ------ | ------------------ | -------------- | --------- |
| 1   | Type breadth         | **Open** | fp32 `Vector`, `Vector3`, **`Mask`** implemented | fp64, int32/64, uint, Complex | `Source/Types/` |
| 3   | GPU-first API        | **Aligned** | Operators / `OP` use GPU kernels; explicit CPU methods where documented | Documented in §2.3 | `Source/Types/Vector.cs`, modules |
| 6   | Kernel loading       | **Aligned** | Selective `KernelModuleLoader.Load<T>` | Per-domain modules | `Source/Core/GPU/KernelModules/` |
| 7   | Multi-GPU            | **Open** | `GPUManager.Default` only | Create/enumerate GPUs; cross-device transfer | `GPUManager.cs` |
| 8   | Mask                 | **Open** | GPU ops + type implemented; resize not yet | §6.7 resize; §6.8 CPU aggregates | `Source/Types/Mask.cs`, `Source/Modules/Mask/` |
| 9   | Matrix/Table         | **Open** | Stubs throw or empty | Deferred | `Source/Types/Matrix.cs`, `Table.cs` |
| 10  | Astrophysics         | **Open** | Not in library repo | FALCON integrals (§12) | — |
| 11  | IO formats           | **Open** | CSV/TXT/JSON/XML for Vector/Vector3/Mask | YAML; FITS/NPY/HDF5 later | `Source/Modules/IO/` |
| 12  | Plotting             | **Aligned** | Removed from repo | Cross-platform plotting roadmap (§14) | — |
| 13  | Experimental         | **Open** | Bloated, untested | One impl each, xUnit tested | `Source/Experimental/TestCls.cs` |
| 14  | Tests                | **Open** | BAVCL.Tests extensive xUnit suites | net10.0 alignment; CI gate | `BAVCL.Tests/` |
| 15  | Vector3 stats        | **Open** | Mean/Min/Max/Range/Sum on flat buffer | Rework for 3D semantics or remove | `Vector3` + Statistics/Arithmetic modules |
| 18  | Memory accounting    | **Open** | `sizeof(T) × length` estimate | Explore actual GPU memory tracking | `LRU.cs` |
| 21  | Unary `+` operator   | **Open** | Calls `AbsX` | Identity or documented intent | `Source/Types/Vector.cs` |
| 22  | Vector3 buffer reuse | **Open** | `Pull()` on conversion | Pass buffer ID between types | `Vector3.cs` |
| 23  | Vertex + Vector3     | **Open** | Implicit conversion pulls GPU data | Optimise GPU path | `Vertex.cs` |
| 25  | .NET version         | **Open** | Library net10.0; tests may lag | Align all to net10.0 | `.csproj` |
| 31  | Mask resize          | **Open** | `ElementCount` fixed (`readonly`) | Resize/replace API (§6.7) | `Source/Types/Mask.cs` |
| 32  | TXT packed mask read | **Open** | `TxtFormatter` writes packed; reads bool grid only | Packed TXT deserialize (parity with JSON/CSV/XML) | `Source/Modules/IO/TxtFormatter.cs` |

**Closed / aligned (removed from active tracking):** LiveCount + GpuScope (#4, #5, #26); memory sync Residence (#17); code organization / Modules migration (#19); print extensions merged into modules (#24); Vector3 error messages (#16); kernel modules + Mask in Default (#27); RsqrtX (#28); Shape type at `Source/Types/Shape.cs` (#29); shape caching on VectorBase not needed post-`BroadcastStrides` (#30). Generic `Vector<T>` fallback not planned (former #2).

---

## 20. Related Projects

### 20.1 FALCON

Primary consumer. Astrophysics calculations including age-from-redshift integrals (Astropy-equivalent models reverse-engineered in FALCON). BAVCL provides the GPU numerics foundation.

### 20.2 BAVCL.Tests

Companion test repository at `C:\Users\marce\Repos\BAVCL.Tests`. Source-only sibling referencing BAVCL project. Canonical location for xUnit tests and BenchmarkDotNet benchmarks.

### 20.3 Testing Console

`Testing Console/` in the BAVCL solution — manual smoke tests and benchmark entry point. Not a replacement for BAVCL.Tests.

---

## Appendix A: Configuration Defaults

| Setting                 | Default                       | Location                               |
| ----------------------- | ----------------------------- | -------------------------------------- |
| Memory cap              | 0.8 (80% of device)           | `GPUManager.GetGPU()`                  |
| Accelerator preference  | CUDA > OpenCL > CPU           | `GPUManager._acceleratorPrefOrder`     |
| Auto-cache on construct | `true`                        | `VectorBase` constructor `Cache` param |
| IO output path          | `{directory}/{name}.{ext}` (cwd default) | `IO.CreateWriter<T, TFormatter>()`     |
| Kernels loaded          | `KernelWorkloads.Default` on `GPUManager.Default`; otherwise explicit | `KernelModuleLoader`, `GPUManager` |

## Appendix B: Key Interfaces

| Interface        | Purpose                                             |
| ---------------- | --------------------------------------------------- |
| `ICacheable`     | GPU cache contract: ID, LiveCount, Residence (volatile), DeCache, SyncCPU |
| `ICacheable<T>`  | Typed cache contract: `GetReadOnlySpan()`, `UpdateCache(T[])` |
| `IMemoryManager` | Pluggable GPU memory strategy                       |
| `IIO`            | CSV/string export contract                          |

## Appendix C: Open Design Items

| Item                                   | Status                                |
| -------------------------------------- | ------------------------------------- |
| Packed-bit mask word layout            | Benchmark-driven                      |
| `System.Numerics.Complex` as unmanaged | May need custom struct                |
| FALCON astrophysics formulas           | Scope defined; details in FALCON spec |
| Actual GPU memory API feasibility      | Investigation                         |
| .NET 11 DU impact on Operations/       | Evaluate when available               |

---

_End of specification. Reconcile against code via §19; update after intentional changes._
