# Agent context — BAVCL

BAVCL is a C# GPU-accelerated numerics library on ILGPU. Authoritative spec: [Documentation/BAVCLSpecification.md](Documentation/BAVCLSpecification.md).

## Device-side code (kernels, GPU dispatch, ILGPU)

Before writing or reviewing any device-side work:

1. Load the **`bavcl-gpgpu`** project skill (`.agents/skills/bavcl-gpgpu/SKILL.md`)
2. Read [Documentation/GPGPUKernelGuide.md](Documentation/GPGPUKernelGuide.md) — principles P1–P6 and pre-submit review

## General code quality

Use global `/code` and `/clarify-requirements` skills for domain-agnostic quality and alignment.

## Testing boundaries

- Automated tests: `BAVCL.Tests` (sibling repo)
- `Testing Console/`: manual scratch only — do not add validation harnesses there
