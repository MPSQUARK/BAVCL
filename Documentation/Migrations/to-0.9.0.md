# Pre-0.9.0 → 0.9.0

First tagged release. There is no earlier `0.9` tag.

## The backing buffer is no longer public

**Summary.** `Value` is internal, and `GetValues()` is gone. Read with `RetrieveReadOnlySpan()`. Write inside `CpuScope`.

**Why.** Callers were editing one copy while the other device still held the old one.

**Migrate.**

- Replace `GetValues()` and direct `Value` access with `RetrieveReadOnlySpan()`.
- Edit through `CpuScope`. Assign `scope.View` to a local before writing. `scope.View[i] = x` does not compile.
- Do not call `SyncCPU()` yourself.

```csharp
using (var scope = vector.CpuScopeAndSync())
{
    EditableView<float> view = scope.View;
    view[0] = 1.5f;
}
```

## `IndexingMode` is gone

**Summary.** Indexers and `GetAt` / `SetAt` no longer take a mode.

**Why.** Reads sync on their own, and writes already open a scope.

**Migrate.**

- Delete the argument.
- Do not call `SetAt` in a loop. Use one `CpuScope`.

## `Shape()` returns `Shape`

**Summary.** `Shape()` returns `BAVCL.Core.Shape`, not a pair of ints.

**Why.** Callers were unpacking a tuple to ask a question the type can answer.

**Migrate.**

- Use `vector.Shape()` and `MatchesDimensions`.

## Element-wise ops check the layout

**Summary.** `+`, `-`, `*`, `/`, and `^` throw `ShapeMismatchException` when two vectors have the same length but different 2D shapes.

**Why.** A flat multiply was hiding a real shape mismatch.

**Migrate.**

- Matrix multiply is `Cross`, not `*`.
- For the old flat product, set `Columns` to 0 on both sides, multiply, then put the columns back.
