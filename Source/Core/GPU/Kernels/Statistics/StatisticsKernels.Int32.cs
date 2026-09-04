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
		dotReduceIntGroupedKernel = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<int>, ArrayView<int>, int>(DotReduceIntGroupedKernel);
		minMaxReduceIntGroupedKernel = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<int>, int>(MinMaxReduceIntGroupedKernel);
		sumReduceIntGroupedKernel = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<int>, int>(SumReduceIntGroupedKernel);
		sumSquaredDiffIntGroupedKernel = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<float>, float, int>(SumSquaredDiffIntGroupedKernel);
		allNonZeroIntGroupedKernel = accelerator.LoadKernel<
			ArrayView<int>, ArrayView<int>, int>(AllNonZeroIntGroupedKernel);
		LoadVarReduceIntGroupedKernel();
	}

	void LoadVarReduceIntGroupedKernel()
	{
        varReduceIntGroupedKernel = SelectVarianceReducePath(accelerator) switch
        {
            VarianceReducePath.SharedScratch => accelerator.LoadKernel<
                                ArrayView<int>, ArrayView<float>, int>(VarReduceIntGroupedSharedKernel),
            VarianceReducePath.WarpTree => accelerator.LoadKernel<
                                ArrayView<int>, ArrayView<float>, int>(VarReduceIntGroupedWarpTreeKernel),
            _ => throw new NotSupportedException("Unsupported variance reduce path."),
        };
    }
}
