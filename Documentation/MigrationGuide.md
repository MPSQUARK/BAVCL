# BAVCL Vector API Migration Guide

**Audience:** Callers upgrading code written against the pre–NumPy-broadcast `Vector` API.  
**Authoritative behavior:** [`BAVCLSpecification.md`](BAVCLSpecification.md) §4.2.5.  
**Status:** July 2026 — applies to the `OP` / `Reduce` / `Matrix` / `Columns` refactor.

---

## Summary of breaking changes

| Area | Before | After |
| ---- | ------ | ----- |
| **`Columns` default** | `1` (flat `[n]` and column `[m,1]` both used `Columns=1`) | `0` = row 1D `(1,N)`; `1` = column `(N,1)`; `N>1` = matrix |
| **`OP` / operators** (`+`, `-`, `*`, `/`, `^`) | Mixed dispatch: equal length → element-wise; some 1D×2D cases → row reduction | **NumPy broadcast only** via `broadcastOpKernel` |
| **Row-wise 1D×2D combine** | Often reached through `OP` when lengths “matched” the old rules | Explicit **`ReduceOP(coeff, matrix, op)`** |
| **Strict matrix element-wise** | `OP` on two 2D buffers | **`MatrixAdd`**, **`MatrixSubtract`**, **`MatrixDivide`**, **`MatrixPow`** |
| **Matrix multiply** | `Cross` (unchanged entry point) | Still **`Cross`**; alias **`MatrixMultiply`** |
| **In-place row reduce** | `ReduceIPOP` (experimental / internal) | **Removed** — use allocating **`ReduceOP`** |
| **`Is1D()`** | Effectively “not a wide matrix” | **`Columns == 0` only** |
| **`RowCount()`** | `Length / Columns` (with `Columns` often `1`) | `0→1`, `1→Length`, else `Length/Columns` |
| **`Flatten()`** | Set `Columns = 1` | Set **`Columns = 0`** |

---

## 1. `Columns` storage model

BAVCL stores row-major data in a flat `Value` array. Logical shape is encoded in **`Length`** and **`Columns`**:

| `Columns` | Logical shape | NumPy equivalent | Example data `[1,2,3,4]` |
| --------- | ------------- | ---------------- | ------------------------ |
| **`0`** (default) | `(1, N)` row vector | `shape (N,)` or `(1, N)` | `new Vector(gpu, data)` or `columns: 0` |
| **`1`** | `(N, 1)` column vector | `shape (N, 1)` | `new Vector(gpu, data, columns: 1)` |
| **`N > 1`** | `(Length/N, N)` matrix | `shape (M, N)` | `new Vector(gpu, data, columns: N)` |

### Migration checklist

1. **Constructors and factories** — omit `columns` for ordinary 1D rows, or pass `0` explicitly.  
   Factories (`Zeros`, `Ones`, `Fill`, `Arange`, `Linspace`) now default to **`Columns = 0`**.

2. **Column vectors** — any vector that must broadcast as `(M, 1)` or participate in `Cross` as a column must use **`columns: 1`**, not the default.

3. **Matrices** — unchanged: pass the column count as `columns` (last axis size).

4. **`Flatten()`** — now sets **`Columns = 0`**. Update any code that assumed `Columns == 1` after flattening.

5. **Assertions** — replace `Columns == 1` meaning “1D” with **`Is1D()`** (`Columns == 0`) or an explicit shape check via **`Shape()`**.

### Before / after

```csharp
// OLD — flat 1D and column both looked like Columns=1
Vector row = new(gpu, new float[] { 1, 2, 3, 4 }, columns: 1);
Vector col = new(gpu, new float[] { 1, 2, 3, 4 }, columns: 1); // ambiguous

// NEW
Vector row = new(gpu, new float[] { 1, 2, 3, 4 });           // Columns=0, shape (1,4)
Vector col = new(gpu, new float[] { 1, 2, 3, 4 }, columns: 1); // shape (4,1)
```

```csharp
// OLD
vector.Flatten(); // Columns became 1

// NEW
vector.Flatten(); // Columns becomes 0
```

### `RowCount()` and `Shape()`

```csharp
// Columns=0, Length=4  →  Shape() (1, 4),  RowCount() 1
// Columns=1, Length=4  →  Shape() (4, 1),  RowCount() 4
// Columns=3, Length=6  →  Shape() (2, 3),  RowCount() 2
```

---

## 2. `OP`, operators, and `IPOP` (broadcast family)

### What changed

`OP(vecA, vecB, op)` and binary operators now **only** perform NumPy-style element-wise broadcast. They no longer fall through to row-reduction when a 1D coefficient vector is paired with a 2D matrix.

- Compatible shapes follow NumPy rules (`1` broadcasts along an axis).
- Incompatible shapes throw **`ShapeMismatchException`**.
- Equal shape + matching storage may use the fast `vectorOpKernel` path internally; behavior is unchanged from the caller’s perspective.

`IPOP` applies the same broadcast rules in-place. If the left operand is **not** already the broadcast output shape, you get **`PerformanceException`** with guidance to swap operands or use allocating `OP`.

### Migration: row reduction used to go through `OP`

**Old mental model (removed):**  
`coeff` length `N`, `matrix` with `N` columns → `OP(coeff, matrix, op)` produced one value per row.

**New:**

```csharp
// Row-wise combine: coeff (1,N) with matrix (M,N) → output column (M,1)
Vector result = Vector.ReduceOP(coeff, matrix, Operations.multiply);
// or
Vector result = coeff.ReduceOP(matrix, Operations.add);
```

Requirements for **`ReduceOP`**:

- `coeff` must be 1D row storage: **`coeff.Is1D()`** (`Columns == 0`).
- `matrix` must be a true 2D matrix: **`matrix.Columns > 1`**.
- **`coeff.Length == matrix.Columns`**.

Output is always a **column vector** with `Columns = 1` and `Length = matrix.RowCount()`.

### Migration: operators on mixed 1D×2D

```csharp
// OLD — might have reduced rows
Vector y = coeff * matrix;

// NEW — broadcast (often throws or does not match old math)
Vector y = coeff * matrix;  // only if NumPy broadcast rules apply

// NEW — same semantics as old row-wise multiply
Vector y = Vector.ReduceOP(coeff, matrix, Operations.multiply);
// or for matmul-style 1D×2D:
Vector y = Vector.Cross(coeff, matrix);
```

`Cross` for `1D × 2D` and `2D × 1D` still uses row dot products internally (`RunReduceRowOp` with `multiply`). **`ReduceOP`** is the explicit API for other reduction operations (`add`, `subtract`, `divide`, `pow`, etc.).

### Migration: strict same-shape matrix math

```csharp
// OLD
Vector sum = Vector.OP(matrixA, matrixB, Operations.add);

// NEW — requires identical (M,N); throws if shapes differ
Vector sum = Vector.MatrixAdd(matrixA, matrixB);
```

Available matrix-calculator methods:

| Method | Semantics |
| ------ | --------- |
| `MatrixAdd` / `MatrixSubtract` / `MatrixDivide` / `MatrixPow` | Same `(M,N)` on both operands |
| `MatrixMultiply` / `Cross` | `(M,K) × (K,N) → (M,N)` for 2D×2D |

All matrix-calculator methods require **`Columns > 1`** on matrix operands.

### Unchanged

- **`OP(vec, scalar, op)`** and scalar operators.
- **Equal-length 1D** (`Columns=0`) element-wise `OP` / operators.
- **`Dot`** — inner product; equal length only.

---

## 3. `ReduceOP` and removed `ReduceIPOP`

### `ReduceOP`

Public API in `BAVCL/Core/Vector/Reduce.cs`. Use whenever you need **one scalar per matrix row** from a shared coefficient vector across columns.

```csharp
Vector coeff = new(gpu, new float[] { 0.5f, 1f, 2f });     // (1,3)
Vector matrix = new(gpu, new float[] { /* M×3 */ }, columns: 3);

Vector perRow = coeff.ReduceOP(matrix, Operations.add);
// Length = M, Columns = 1
```

### `ReduceIPOP` removed

In-place row reduction is **not** supported. Reasons (see spec §4.2.5):

- Coefficient vector length must equal **columns**; output length must equal **rows** — one buffer cannot represent both except in niche square cases.
- The row kernel reads the full coefficient vector while writing row outputs; in-place aliasing is unsafe on the GPU without a snapshot.

**Migration:** always use allocating **`ReduceOP`**.

---

## 4. `Cross` / `MatrixMultiply`

Entry points are unchanged, but behavior depends on the new **`Columns`** rules:

| Operands | Result |
| -------- | ------ |
| 2D `(M,K)` × 2D `(K,N)` | Matrix multiply `(M,N)` |
| 1D `(1,K)` × 2D `(M,K)` | Row dot products → column `(M,1)` |
| 2D `(M,K)` × 1D `(K,)` or column `(K,1)` | Row dot products → column `(M,1)` |

Ensure 1D operands use **`Columns = 0`** and column operands use **`Columns = 1`** so `Shape()` and validation match your intent.

**Not related:** `BAVCL.Geometric.Vector3.Cross` — 3D geometric cross product; unchanged.

---

## 5. Broadcast output storage

When `OP` allocates a broadcast result, storage columns are chosen from logical output shape:

| Output shape | `Columns` stored |
| ------------ | ---------------- |
| `(M, 1)` | `1` |
| `(1, N)` | `0` |
| `(M, N)` | `N` |

Callers should not assume `Columns == 1` for “small” results; use **`Shape()`** or **`Is1D()`**.

---

## 6. Test and helper code

If you mirror NumPy shapes in tests, follow the mapping in `BAVCL.Tests/Helpers/BavclShape.cs`:

```csharp
// NumPy shape → Columns
// [n]           → 0
// [m, 1]        → 1
// [m, n] (n>1)  → n
```

Update any test fixtures that passed `columns: 1` for flat `[n]` arrays.

`GpuTestBase` and similar helpers should default to **`columns: 0`** for generic 1D vectors.

---

## 7. Quick decision tree

```
Two Vector operands, element-wise?
├─ Same NumPy-broadcastable shapes → OP / operators / IPOP
├─ Need one result per matrix row from 1D coeff → ReduceOP
├─ Need matrix multiply or 1D×2D dot rows → Cross / MatrixMultiply
└─ Need strict (M,N) element-wise on two matrices → MatrixAdd / Subtract / Divide / Pow

Is this vector "1D"?
└─ Is1D()  (Columns == 0), not Columns == 1

Is this a column for broadcast?
└─ columns: 1  →  logical (N, 1)
```

---

## 8. Repo search checklist

When migrating a consumer project, search for:

| Pattern | Action |
| ------- | ------ |
| `new Vector(..., 1)` or `columns: 1` on flat data | Decide: row `0` vs column `1` |
| `Columns == 1` as “is 1D” | Use `Is1D()` or `Columns == 0` |
| `OP(` with 1D + 2D operands | Replace with `ReduceOP` or `Cross` as appropriate |
| `ReduceIPOP` | Replace with `ReduceOP` |
| `Flatten()` then `Columns == 1` | Expect `Columns == 0` |
| Omitted `columns` on factories | Now `0`; pass `1` for columns explicitly |

---

## 9. Exceptions you may see after upgrading

| Exception | Typical cause |
| --------- | ------------- |
| `ShapeMismatchException` on `OP` | Operands do not NumPy-broadcast; use `ReduceOP` / `Matrix*` / `Cross` |
| `ShapeMismatchException` on `ReduceOP` | Coeff not `Columns=0`, or matrix not `Columns>1` |
| `LengthMismatchException` on `ReduceOP` / `Cross` | `coeff.Length != matrix.Columns` |
| `PerformanceException` on `IPOP` | Left operand is not the broadcast output shape |

---

## Related documentation

- [`BAVCLSpecification.md`](BAVCLSpecification.md) §4.2.5 — full API tables and kernel notes
- Future: GPU scope / `Residence` migration will be documented separately when that refactor lands
