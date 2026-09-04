using ILGPU;
using ILGPU.Runtime;
using BAVCL.Modules.GpuOps;

namespace BAVCL.GpuAlgorithms;

internal static class RowReduceAlgorithms
{
	// One thread per row is faster when the inner dimension is narrow.
	const int FusedMinCols = 512;

	internal static Vector Reduce(Vector coeff, Vector matrix, Operations operation)
	{
		if (matrix.Columns < FusedMinCols)
			return VectorVectorOp.RunReduceRowOp(coeff, matrix, operation);

		return ReduceFused(coeff, matrix, operation);
	}

	internal static VectorInt Reduce(VectorInt coeff, VectorInt matrix, Operations operation)
	{
		if (matrix.Columns < FusedMinCols)
			return VectorVectorOp.RunReduceRowOp(coeff, matrix, operation);

		return ReduceFused(coeff, matrix, operation);
	}

	static Vector ReduceFused(Vector coeff, Vector matrix, Operations operation)
	{
		int cols = matrix.Columns;
		int rows = matrix.RowCount();
		GPU gpu = coeff.Gpu;
		var op = new SpecializedValue<int>((int)operation);
		KernelConfig config = (rows, GPU.ComputeRowReduceGroupSize(cols));

		using (GpuScope.BeginReadOnly(coeff, matrix))
		{
			Vector output = new(gpu, rows, 0);
			using (GpuScope.Begin(output))
			{
				gpu.reduceRowFusedCompensated(
					gpu.DefaultStream,
					config,
					coeff.GetBuffer().View,
					matrix.GetBuffer().View,
					output.GetBuffer().View,
					rows,
					cols,
					op);
				gpu.Synchronize();
			}

			return output;
		}
	}

	static VectorInt ReduceFused(VectorInt coeff, VectorInt matrix, Operations operation)
	{
		int cols = matrix.Columns;
		int rows = matrix.RowCount();
		GPU gpu = coeff.Gpu;
		var op = new SpecializedValue<int>((int)operation);
		KernelConfig config = (rows, GPU.ComputeRowReduceGroupSize(cols));

		using (GpuScope.BeginReadOnly(coeff, matrix))
		{
			VectorInt output = new(gpu, rows, 0);
			using (GpuScope.Begin(output))
			{
				gpu.reduceRowFused(
					gpu.DefaultStream,
					config,
					coeff.GetBuffer().View,
					matrix.GetBuffer().View,
					output.GetBuffer().View,
					rows,
					cols,
					op);
				gpu.Synchronize();
			}

			return output;
		}
	}
}
