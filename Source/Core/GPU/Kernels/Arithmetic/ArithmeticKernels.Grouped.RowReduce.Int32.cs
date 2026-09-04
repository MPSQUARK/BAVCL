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
	internal Action<AcceleratorStream, KernelConfig, ArrayView<int>, ArrayView<int>, ArrayView<int>, int, int, SpecializedValue<int>>
		reduceRowFused
		= (_, _, _, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(reduceRowFused));

	static void ReduceRowFused_Kern(
		ArrayView<int> coeffs,
		ArrayView<int> matrix,
		ArrayView<int> output,
		int rows,
		int cols,
		SpecializedValue<int> operation)
	{
		int row = Grid.IdxX;
		if (row >= rows)
			return;

		Operations op = (Operations)operation.Value;
		int rowStart = row * cols;
		int partial = 0;
		for (int i = Group.IdxX; i < cols; i += Group.DimX)
			partial += ReduceRowElementInt(coeffs[i], matrix[rowStart + i], op);

		int groupSum = GroupExtensions.AllReduce<int, AddInt32>(partial);
		if (Group.IsFirstThread)
			output[row] = groupSum;
	}
}
