using BAVCL.Geometric;
using BAVCL.Modules.Structural;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.GpuOps;

internal static class Vector3GpuOps
{
	internal static Vector VOPX(Vector3 vector, Operations operation)
	{
		GPU gpu = vector.Gpu;
		Vector output = Vector.Zeros(gpu, vector.RowCount(), 0);

		using (GpuScope.Begin(output, vector))
		{
			Vector3Kernels.LaunchSimdVectorOp(
				gpu,
				output.GetBuffer(),
				vector.GetBuffer(),
				operation);
		}

		return output;
	}

	internal static Vector VOPX(Vector3 left, Vector3 right, Operations operation)
	{
		GPU gpu = left.Gpu;
		Vector output = Vector.Zeros(gpu, left.RowCount(), 0);

		using (GpuScope.Begin(output, left, right))
		{
			Vector3Kernels.LaunchSimdVectorBinaryOp(
				gpu,
				output.GetBuffer(),
				left.GetBuffer(),
				right.GetBuffer(),
				operation);
		}

		return output;
	}

	internal static Vector3 OPX(Vector3 left, Vector3 right, Operations operation)
	{
		GPU gpu = left.Gpu;
		Vector3 output = new(gpu, left.Length);

		using (GpuScope.Begin(output, left, right))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = left.GetBuffer(),
				buffer3 = right.GetBuffer();

			gpu.aOpF(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer3.View, new SpecializedValue<int>((int)operation));
			gpu.accelerator.Synchronize();
		}

		return output;
	}

	internal static Vector3 OPX(Vector3 vector, float scalar, Operations operation)
	{
		GPU gpu = vector.Gpu;
		Vector3 output = new(gpu, vector.Length);

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

	internal static Vector3 IPOP(Vector3 vector, Vector3 other, Operations operation)
	{
		using (GpuScope.Begin(vector, other))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = vector.GetBuffer(),
				buffer2 = other.GetBuffer();

			vector.Gpu.aOpFIP(vector.Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, new SpecializedValue<int>((int)operation));
			vector.Gpu.accelerator.Synchronize();
		}

		return vector;
	}

	internal static Vector3 IPOP(Vector3 vector, float scalar, Operations operation)
	{
		using (GpuScope.Begin(vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense> buffer = vector.GetBuffer();
			vector.Gpu.sOpFIP(vector.Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, scalar, new SpecializedValue<int>((int)operation));
			vector.Gpu.accelerator.Synchronize();
		}

		return vector;
	}
}
