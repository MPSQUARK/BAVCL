

using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Geometric;


public partial class Vector3
{
	public static Vector VOP(Vector3 vector, Operations operation)
	{
		GPU gpu = vector.Gpu;
		Vector output = Vector.Zeros(gpu, vector.RowCount());

		using (GpuScope.Begin(output, vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer();

			gpu.simdVectorKernel(gpu.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer2.View, 3, new SpecializedValue<int>((int)operation));
			gpu.Synchronize();
		}

		return output;
	}

	public static Vector VOP(Vector3 vectorA, Vector3 vectorB, Operations operation)
	{
		GPU gpu = vectorA.Gpu;
		Vector output = Vector.Zeros(gpu, vectorA.RowCount());

		using (GpuScope.Begin(output, vectorA, vectorB))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vectorA.GetBuffer(),
				buffer3 = vectorB.GetBuffer();

			gpu.simdVectorKernel(gpu.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer3.View, 3, new SpecializedValue<int>((int)operation));
			gpu.Synchronize();
		}

		return output;
	}

	public static Vector3 OP(Vector3 vectorA, Vector3 vectorB, Operations operation)
	{
		GPU gpu = vectorA.Gpu;
		Vector3 output = new(gpu, vectorA.Length);

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

	public Vector3 OP(Vector3 vector, Operations operation)
	{
		GPU gpu = Gpu;
		Vector3 output = new(gpu, vector.Length);

		using (GpuScope.Begin(output, this, vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = GetBuffer(),
				buffer3 = vector.GetBuffer();

			gpu.a_opFKernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer3.View, new SpecializedValue<int>((int)operation));
			gpu.accelerator.Synchronize();
		}

		return output;
	}

	public static Vector3 OP(Vector3 vector, float scalar, Operations operation)
	{
		GPU gpu = vector.Gpu;
		Vector3 output = new(gpu, vector.Length);

		using (GpuScope.Begin(output, vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer();

			gpu.s_opFKernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, scalar, new SpecializedValue<int>((int)operation));
			gpu.accelerator.Synchronize();
		}

		return output;
	}

	public Vector3 OP(float scalar, Operations operation)
	{
		GPU gpu = Gpu;
		Vector3 output = new(gpu, Length);

		using (GpuScope.Begin(output, this))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = GetBuffer();

			gpu.s_opFKernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, scalar, new SpecializedValue<int>((int)operation));
			gpu.accelerator.Synchronize();
		}

		return output;
	}

	public Vector3 OP_IP(Vector3 vector, Operations operation)
	{
		using (GpuScope.Begin(this, vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = GetBuffer(),
				buffer2 = vector.GetBuffer();

			Gpu.a_FloatOPKernelIP(Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, new SpecializedValue<int>((int)operation));
			Gpu.accelerator.Synchronize();
		}

		return this;
	}

	public Vector3 OP_IP(float scalar, Operations operation)
	{
		using (GpuScope.Begin(this))
		{
			MemoryBuffer1D<float, Stride1D.Dense> buffer = GetBuffer();
			Gpu.s_FloatOPKernelIP(Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, scalar, new SpecializedValue<int>((int)operation));
			Gpu.accelerator.Synchronize();
		}

		return this;
	}
}
