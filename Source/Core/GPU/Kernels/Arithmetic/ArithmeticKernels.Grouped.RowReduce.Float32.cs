using System;
using BAVCL.Core;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Algorithms.ScanReduceOperations;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	internal const int RowReduceGroupSize = 256;
	/// <summary>
	/// Warp size minus one. Must be 2^N - 1.
	/// 31 = 32-wide warp.
	/// </summary>
	internal const int RowReduceWarpSizeMask = 31;

	internal static int ComputeRowReduceGroupSize(int cols)
	{
		if (cols <= 1)
			return 1;

		int capped = Math.Min(cols, RowReduceGroupSize);
		return (capped + RowReduceWarpSizeMask) & ~RowReduceWarpSizeMask;
	}

	internal Action<AcceleratorStream, KernelConfig, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int, SpecializedValue<int>>
		reduceRowFusedCompensated
		= (_, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(reduceRowFusedCompensated));

	static void ReduceRowFusedCompensated_Kern(
		ArrayView<float> coeffs,
		ArrayView<float> matrix,
		ArrayView<float> output,
		int rows,
		int cols,
		SpecializedValue<int> operation)
	{
		int row = Grid.IdxX;
		if (row >= rows)
			return;

		Operations op = (Operations)operation.Value;
		int rowStart = row * cols;
		double sum = 0d;
		double compensation = 0d;
		for (int i = Group.IdxX; i < cols; i += Group.DimX)
		{
			float term = ReduceRowElementFloat(coeffs[i], matrix[rowStart + i], op);
			NeumaierAdd(ref sum, ref compensation, term);
		}

		float partial = (float)(sum + compensation);
		float groupSum = GroupExtensions.AllReduce<float, AddFloat>(partial);
		if (!Group.IsFirstThread)
			return;

		output[row] = ReduceRowUsesDistance(op) ? XMath.Sqrt(groupSum) : groupSum;
	}
}
