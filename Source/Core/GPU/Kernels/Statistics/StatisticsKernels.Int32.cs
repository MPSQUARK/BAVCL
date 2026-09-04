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
	internal Reduction<int, Stride1D.Dense> ReduceMinInt32
		= (_, _, _) => throw new KernelNotCompiledException(nameof(ReduceMinInt32));

	internal Reduction<int, Stride1D.Dense> ReduceMaxInt32
		= (_, _, _) => throw new KernelNotCompiledException(nameof(ReduceMaxInt32));

	internal void LoadStatisticsIntKernels()
	{
		ReduceMinInt32 = accelerator.CreateReduction<int, Stride1D.Dense, MinInt32>();
		ReduceMaxInt32 = accelerator.CreateReduction<int, Stride1D.Dense, MaxInt32>();
		dotReduceIntGrouped = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<int>, ArrayView<int>, int>(DotReduceIntGrouped_Kern);
		minMaxReduceIntGrouped = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<int>, int>(MinMaxReduceIntGrouped_Kern);
		sumReduceIntGrouped = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<int>, int>(SumReduceIntGrouped_Kern);
		sumSquaredDiffIntGrouped = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<float>, float, int>(SumSquaredDiffIntGrouped_Kern);
		allNonZeroIntGrouped = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<int>, int>(AllNonZeroIntGrouped_Kern);
		LoadVarReduceIntGroupedKernel();
	}

	void LoadVarReduceIntGroupedKernel()
	{
        varReduceIntGrouped = SelectVarianceReducePath(accelerator) switch
        {
            VarianceReducePath.SharedScratch => accelerator.LoadKernel<
                                ArrayView<int>, ArrayView<float>, int>(VarReduceIntGroupedShared_Kern),
            VarianceReducePath.WarpTree => accelerator.LoadKernel<
                                ArrayView<int>, ArrayView<float>, int>(VarReduceIntGroupedWarpTree_Kern),
            _ => throw new NotSupportedException("Unsupported variance reduce path."),
        };
    }
}
