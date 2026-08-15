# Agent context — BAVCL

BAVCL is a C# GPU-accelerated numerics library on ILGPU. Authoritative spec: [Documentation/BAVCLSpecification.md](Documentation/BAVCLSpecification.md).

## Read this first

| When | Read |
|------|------|
| Picking an API / suffix (`X`, `IP`, modules) | [Documentation/Features.md](Documentation/Features.md) |
| `GpuScope`, `CpuScope`, reads, `Copy`, LRU pinning | [Documentation/MigrationGuide.md](Documentation/MigrationGuide.md) — [Host API and scopes](Documentation/MigrationGuide.md#host-api-and-scopes) |
| ILGPU kernels or module GPU dispatch | [Documentation/GPGPUKernelGuide.md](Documentation/GPGPUKernelGuide.md) + `.agents/skills/bavcl-gpgpu/SKILL.md` |
| Full contracts | [Documentation/BAVCLSpecification.md](Documentation/BAVCLSpecification.md) |

## Host API quick rules

- **Read:** `RetrieveReadOnlySpan()` always — outside scope, inside `CpuScope`, after GPU ops
- **Do not** open `CpuScope` for read-only; **do not** call `SyncCPU()` in consumer code
- **GpuScope:** one `GpuScope.Begin(modified, readOnly…)` when buffers exist; nested scopes only for allocate-after-pin (see `SortAlgorithms.ArgsortIntX`)
- **Edit in-place:** `CpuScope` + `scope.View` (`EditableView`)
- **Structural edit:** `CpuScopeAndSync` + `RetrieveReadOnlySpan()` for inputs, then assign backing store (library-internal `Value` / `Length`)
- **Loops:** one `RetrieveReadOnlySpan()` for read loops; one `CpuScope` + `View` for write loops — not `GetAt` / `SetAt`
- **Pinning:** only via `GpuScope` — `LiveCount` is observable for debugging
- **Discovery:** check Features.md before inventing a code path

| Intent | API |
|--------|-----|
| Read (anywhere) | `RetrieveReadOnlySpan()` |
| Edit in-place | `CpuScope` / `CpuScopeAndSync` + `scope.View` |
| Edit structural | `CpuScopeAndSync` + `RetrieveReadOnlySpan()` then assign backing store |

## Device-side code (kernels, GPU dispatch, ILGPU)

Before writing or reviewing any device-side work:

1. Load the **`bavcl-gpgpu`** project skill (`.agents/skills/bavcl-gpgpu/SKILL.md`)
2. Read [Documentation/GPGPUKernelGuide.md](Documentation/GPGPUKernelGuide.md) — principles P1–P6 and pre-submit review

## General code quality

Use global `/code` and `/clarify-requirements` skills for domain-agnostic quality and alignment.

## Testing boundaries

- Automated tests: `BAVCL.Tests` (sibling repo)
- `Testing Console/`: manual scratch only — do not add validation harnesses there
- **IO tests** are tagged `[Trait("Category", "IO")]` and excluded from default `dotnet test` (SSD wear). Run them with:
  `dotnet test -p:IncludeIOTests=true --filter Category=IO`
