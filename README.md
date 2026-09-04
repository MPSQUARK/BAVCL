# BAVCL

> Badass Vector Compute Library — C# GPU-accelerated numerics on ILGPU

**Author:** Marcel Pawelczyk

*In active development.*

## What is BAVCL?

**BAVCL** is a **C# GPU-accelerated numerics library** built on [ILGPU](https://ilgpu.net/) 1.5.3. It provides NumPy-*inspired* vector math with C#-idiomatic APIs — not a direct NumPy port. BAVCL accelerates large array operations on CUDA, OpenCL, or CPU backends while keeping smaller workloads on efficient CPU paths.

Distribution is source-only via project reference (currently).

## Requirements

- .NET 10
- CUDA or OpenCL GPU (optional; CPU fallback via ILGPU)
- ILGPU 1.5.3 (transitive via project reference)

## Quick start

Add a project reference to BAVCL in your `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="path\to\BAVCL\BAVCL.csproj" />
</ItemGroup>
```

Minimal example:

```csharp
using BAVCL;

GPU gpu = GPUManager.Default;

Vector a = new(gpu, [1f, 2f, 3f, 4f]);
Vector b = new(gpu, [10f, 20f, 30f, 40f]);

Vector sum = a + b;
sum.Print();
```

Methods follow a consistent suffix scheme: no suffix = CPU, `X` = GPU, `IP` / `XIP` = in-place (CPU / GPU). See [Features](Documentation/Features.md) for the module catalog and full suffix guide.

## Documentation

- [Features](Documentation/Features.md) — module catalog and API suffix guide
- [BAVCL Specification](Documentation/BAVCLSpecification.md)
- [GPGPU Kernel Guide](Documentation/GPGPUKernelGuide.md)
- [Migration Guide](Documentation/MigrationGuide.md)

## Licence at a glance

BAVCL is source-available and intentionally permissive. The licence permits the vast majority of personal, academic, commercial, research, and software-development uses. A small number of specific restrictions exist to protect the project from direct commercial competition, source-code exploitation for AI training, and clearly defined malicious uses.

| Use | Permitted | Notes |
| --- | --- | --- |
| Personal / hobby / educational | Yes | No attribution required |
| Academic research / publication | Yes | Clear, legible, findable attribution required (e.g. References/Bibliography) |
| Commercial / professional software | Yes | Clear, legible, findable attribution required (e.g. Licences/Info tab) |
| Redistributing BAVCL (source or binary) | Yes | Include licence + attribution where applicable |
| Forking to build a competing .NET GPU math library | No | Includes derivatives and standalone alternatives |
| Password cracking, malicious hacking, fraud-related crypto attacks | No | Authorized security testing with owner permission excluded |

*Examples in the tables below are illustrative and not exhaustive.*

### AI and machine learning

| Activity | Permitted |
| --- | --- |
| Build a neural network using BAVCL | Yes |
| Use BAVCL for model training/inference | Yes |
| Use Claude/Copilot/ChatGPT to write code using BAVCL | Yes |
| Ask an AI to explain how a BAVCL API works | Yes |
| Ask an AI how to use `Vector` or other BAVCL APIs | Yes |
| Train an AI model using BAVCL source as training data | No |
| Fine-tune an AI model on BAVCL source | No |
| Build a training dataset containing BAVCL source or binaries | No |
| Bulk-ingest the repository into a model-training corpus | No |

You can build AI with BAVCL and use AI tools while developing BAVCL software. You cannot use BAVCL's Implementation Materials as training data for a model.

The training restriction exists to prevent use of BAVCL Implementation Materials as training input to exploit the copyrighted work — not because ordinary AI-assisted development or running BAVCL inside ML pipelines is discouraged.

### Unsure whether your use is permitted?

> I have made every effort to make this licence clear and easy to understand. However, if you are still unsure whether your particular use case is permitted, please feel free to open an issue describing what you would like to do.
>
> I will do my best to clarify the intended application of the licence as soon as reasonably possible. My goal is for the restrictions to be transparent and understandable, not for users to have to guess whether they are allowed to use BAVCL.
>
> Please note that any clarification provided in an issue is an explanation of the licence's intent and does not replace or modify the licence itself.

Full terms: [License.txt](License.txt) · Third-party: [THIRD_PARTY_LICENSES.md](THIRD_PARTY_LICENSES.md)

> **Note:** BAVCL is in active development. The licence is subject to change without notice. See [License.txt](License.txt) for the current terms.

## Contributing

BAVCL is in active development. If you have questions, licence clarifications, or bug reports, please open an issue.

Automated tests live in the sibling [BAVCL.Tests](https://github.com/MPSQUARK/BAVCL.Tests) repository — the canonical home for xUnit tests and BenchmarkDotNet benchmarks. The `Testing Console/` project in this solution is for manual smoke tests only.
