using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.GpuOps;

internal static class VectorIntGpuOps
{
	internal static VectorInt ScalarOP(VectorInt vector, int scalar, Operations operation)
	{
		VectorIntOperationValidation.ValidateOperation(operation);
		GPU gpu = vector.Gpu;
		VectorInt output = new(gpu, vector.Length, vector.Columns);

		using (GpuScope.Begin(output, vector))
		{
			MemoryBuffer1D<int, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer();

			gpu.sOpI(
				gpu.DefaultStream,
				buffer.IntExtent,
				buffer.View,
				buffer2.View,
				scalar,
				new SpecializedValue<int>((int)operation));
			gpu.Synchronize();
		}

		return output;
	}

	internal static VectorInt ScalarIPOP(VectorInt vector, int scalar, Operations operation)
	{
		VectorIntOperationValidation.ValidateOperation(operation);
		using (GpuScope.Begin(vector))
		{
			MemoryBuffer1D<int, Stride1D.Dense> buffer = vector.GetBuffer();
			vector.Gpu.sOpIIP(
				vector.Gpu.DefaultStream,
				buffer.IntExtent,
				buffer.View,
				scalar,
				new SpecializedValue<int>((int)operation));
			vector.Gpu.Synchronize();
		}

		return vector;
	}
}
