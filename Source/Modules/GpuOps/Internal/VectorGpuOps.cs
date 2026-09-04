using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.GpuOps;

internal static class VectorGpuOps
{
	internal static Vector ScalarOP(Vector vector, float scalar, Operations operation)
	{
		GPU gpu = vector.Gpu;
		Vector output = new(gpu, vector.Length, vector.Columns);

		using (GpuScope.Begin(output, vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer();

			gpu.sOpF(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, scalar, new SpecializedValue<int>((int)operation));
			gpu.accelerator.Synchronize();
		}

		return output;
	}

	internal static Vector ScalarIPOP(Vector vector, float scalar, Operations operation)
	{
		using (GpuScope.Begin(vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense> buffer = vector.GetBuffer();
			vector.Gpu.sOpFIP(vector.Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, scalar, new SpecializedValue<int>((int)operation));
			vector.Gpu.accelerator.Synchronize();
		}

		return vector;
	}

	internal static Vector LogX(Vector vector, float @base)
	{
		Vector copy = vector.Copy();
		LogXIP(copy, @base);
		return copy;
	}

	internal static Vector LogXIP(Vector vector, float @base)
	{
		using (GpuScope.Begin(vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense> buffer = vector.GetBuffer();
			vector.Gpu.logIP(vector.Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, @base);
			vector.Gpu.accelerator.Synchronize();
		}

		return vector;
	}
}
