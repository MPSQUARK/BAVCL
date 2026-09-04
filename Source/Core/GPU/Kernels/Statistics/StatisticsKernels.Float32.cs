using System;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.ScanReduceOperations;
using ILGPU.Runtime;
using BAVCL.Core.Exceptions;
using static BAVCL.Core.Kernels.KernelSharedMemory;

namespace BAVCL;

public partial class GPU
{
	internal Reduction<float, Stride1D.Dense> ReduceAddFloat
		= (_, _, _) => throw new KernelNotCompiledException(nameof(ReduceAddFloat));

	internal Reduction<float, Stride1D.Dense> ReduceMinFloat
		= (_, _, _) => throw new KernelNotCompiledException(nameof(ReduceMinFloat));

	internal Reduction<float, Stride1D.Dense> ReduceMaxFloat
		= (_, _, _) => throw new KernelNotCompiledException(nameof(ReduceMaxFloat));

	internal void LoadStatisticsFloatKernels()
	{
		ReduceAddFloat = accelerator.CreateReduction<float, Stride1D.Dense, AddFloat>();
		ReduceMinFloat = accelerator.CreateReduction<float, Stride1D.Dense, MinFloat>();
		ReduceMaxFloat = accelerator.CreateReduction<float, Stride1D.Dense, MaxFloat>();
		dotReduceFloatGroupedCompensatedKernel = accelerator.LoadKernel<
			ArrayView<float>, ArrayView<float>, ArrayView<float>, int>(DotReduceFloatGroupedCompensatedKernel);
		minMaxReduceFloatGroupedKernel = accelerator.LoadKernel<
			ArrayView<float>, ArrayView<float>, int>(MinMaxReduceFloatGroupedKernel);
		sumReduceFloatGroupedCompensatedKernel = accelerator.LoadKernel<
			ArrayView<float>, ArrayView<float>, int>(SumReduceFloatGroupedCompensatedKernel);
		allNonZeroFloatGroupedKernel = accelerator.LoadKernel<
			ArrayView<float>, ArrayView<int>, int>(AllNonZeroFloatGroupedKernel);
		LoadVarReduceFloatGroupedKernel();
	}

	void LoadVarReduceFloatGroupedKernel()
	{
        varReduceFloatGroupedKernel = SelectVarianceReducePath(accelerator) switch
        {
            VarianceReducePath.SharedScratch => accelerator.LoadKernel<
                                ArrayView<float>, ArrayView<float>, int>(VarReduceFloatGroupedSharedKernel),
            VarianceReducePath.WarpTree => accelerator.LoadKernel<
                                ArrayView<float>, ArrayView<float>, int>(VarReduceFloatGroupedWarpTreeKernel),
            _ => throw new NotSupportedException("Unsupported variance reduce path."),
        };
    }
}
