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
		dotReduceFloatGroupedCompensated = accelerator.LoadKernel<
			ArrayView<float>, ArrayView<float>, ArrayView<float>, int>(DotReduceFloatGroupedCompensated_Kern);
		minMaxReduceFloatGrouped = accelerator.LoadKernel<
			ArrayView<float>, ArrayView<float>, int>(MinMaxReduceFloatGrouped_Kern);
		sumReduceFloatGroupedCompensated = accelerator.LoadKernel<
			ArrayView<float>, ArrayView<float>, int>(SumReduceFloatGroupedCompensated_Kern);
		allNonZeroFloatGrouped = accelerator.LoadKernel<
			ArrayView<float>, ArrayView<int>, int>(AllNonZeroFloatGrouped_Kern);
		LoadVarReduceFloatGroupedKernel();
	}

	void LoadVarReduceFloatGroupedKernel()
	{
        varReduceFloatGrouped = SelectVarianceReducePath(accelerator) switch
        {
            VarianceReducePath.SharedScratch => accelerator.LoadKernel<
                                ArrayView<float>, ArrayView<float>, int>(VarReduceFloatGroupedShared_Kern),
            VarianceReducePath.WarpTree => accelerator.LoadKernel<
                                ArrayView<float>, ArrayView<float>, int>(VarReduceFloatGroupedWarpTree_Kern),
            _ => throw new NotSupportedException("Unsupported variance reduce path."),
        };
    }
}
