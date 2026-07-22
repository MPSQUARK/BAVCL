using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.GpuOps;

internal static class VectorVectorOp
{
	internal static Vector VectorVectorOP(Vector vectorA, Vector vectorB, Operations operation)
	{
		GPU gpu = vectorA.Gpu;
		Vector output = new(gpu, vectorA.Length, vectorA.Columns);

		using (GpuScope.Begin(output, vectorA, vectorB))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vectorA.GetBuffer(),
				buffer3 = vectorB.GetBuffer();

			gpu.a_opFKernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer3.View, new SpecializedValue<int>((int)operation));
			gpu.accelerator.Synchronize();
		}

		return output;
	}

	internal static Vector VectorVectorOP_IP(Vector vector, Vector vectorB, Operations operation)
	{
		using (GpuScope.Begin(vector, vectorB))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = vector.GetBuffer(),
				buffer2 = vectorB.GetBuffer();

			vector.Gpu.a_FloatOPKernelIP(vector.Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, new SpecializedValue<int>((int)operation));
			vector.Gpu.accelerator.Synchronize();
		}

		return vector;
	}

	internal static Vector RunReduceRowOp(Vector vector, Vector matrix, Operations operation)
	{
		GPU gpu = vector.Gpu;
		Vector output = new(gpu, matrix.RowCount(), 1);

		using (GpuScope.Begin(output, vector, matrix))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer(),
				buffer3 = matrix.GetBuffer();

			gpu.reduceRowOpKernel(
				gpu.accelerator.DefaultStream,
				matrix.RowCount(),
				buffer.View,
				buffer2.View,
				buffer3.View,
				matrix.Columns,
				new SpecializedValue<int>((int)operation));

			gpu.accelerator.Synchronize();
		}

		return output;
	}
}
