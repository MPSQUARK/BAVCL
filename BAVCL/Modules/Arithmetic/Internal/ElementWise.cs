using System;
using BAVCL.Modules.GpuOps;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL.Modules.Arithmetic;

internal static class ElementWiseCore
{
	internal static Vector Abs(Vector vector)
	{
		Vector copy = vector.Copy();
		AbsInPlace(copy);
		return copy;
	}

	internal static void AbsInPlace(Vector vector)
	{
		if (vector.Min() > 0f)
			return;

		using (var scope = vector.CpuScopeAndSync())
		{
			EditableView<float> view = scope.View;
			for (int i = 0; i < vector.Length; i++)
				view[i] = MathF.Abs(view[i]);
		}
	}

	internal static Vector AbsX(Vector vector)
	{
		Vector copy = vector.Copy();
		AbsXInPlace(copy);
		return copy;
	}

	internal static void AbsXInPlace(Vector vector)
	{
		using (GpuScope.Begin(vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense> buffer = vector.GetBuffer();
			vector.Gpu.absKernel(vector.Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View);
			vector.Gpu.accelerator.Synchronize();
		}
	}

	internal static Vector Reciprocal(Vector vector)
	{
		Vector copy = vector.Copy();
		ReciprocalInPlace(copy);
		return copy;
	}

	internal static void ReciprocalInPlace(Vector vector)
	{
		using (GpuScope.Begin(vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense> buffer = vector.GetBuffer();
			vector.Gpu.rcpKernel(vector.Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View);
			vector.Gpu.accelerator.Synchronize();
		}
	}

	internal static Vector Rsqrt(Vector vector)
	{
		Vector copy = vector.Copy();
		RsqrtInPlace(copy);
		return copy;
	}

	internal static void RsqrtInPlace(Vector vector)
	{
		using (var cpu = vector.CpuScopeAndSync())
		{
			EditableView<float> view = cpu.View;
			for (int i = 0; i < vector.Length; i++)
				view[i] = XMath.Rsqrt(view[i]);
		}
	}

	internal static Vector RsqrtX(Vector vector)
	{
		Vector copy = vector.Copy();
		RsqrtXInPlace(copy);
		return copy;
	}

	internal static void RsqrtXInPlace(Vector vector)
	{
		using (GpuScope.Begin(vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense> buffer = vector.GetBuffer();
			vector.Gpu.rsqrtKernel(vector.Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View);
			vector.Gpu.accelerator.Synchronize();
		}
	}

	internal static Vector Diff(Vector vector)
	{
		if (vector.Columns > 1)
			throw new Exception("Diff is for use with 1D Vectors ONLY");

		GPU gpu = vector.Gpu;
		Vector output = new(gpu, vector.Length - 1, vector.Columns);

		using (GpuScope.Begin(output, vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer();

			gpu.diffKernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View);
			gpu.accelerator.Synchronize();
		}

		return output;
	}

	internal static Vector Nan_to_num(Vector vector, float num)
	{
		Vector copy = vector.Copy();
		Nan_to_numInPlace(copy, num);
		return copy;
	}

	internal static void Nan_to_numInPlace(Vector vector, float num)
	{
		using (GpuScope.Begin(vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense> buffer = vector.GetBuffer();
			vector.Gpu.nanToNumKernel(vector.Gpu.accelerator.DefaultStream, vector.Length, buffer.View, num);
			vector.Gpu.accelerator.Synchronize();
		}
	}

	internal static Vector Normalise(Vector vector) =>
		vector.OP(1f / vector.Sum(), Operations.multiply);

	internal static void NormaliseInPlace(Vector vector) =>
		vector.IPOP(1f / vector.Sum(), Operations.multiply);
}
