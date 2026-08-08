using System;
using BAVCL.Modules.GpuOps;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using BAVCL.Modules.Statistics;

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
		if (DescriptiveStatistics.Min(vector) > 0f)
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

	internal static Vector ReciprocalX(Vector vector)
	{
		Vector copy = vector.Copy();
		ReciprocalXInPlace(copy);
		return copy;
	}

	internal static void ReciprocalXInPlace(Vector vector)
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
		using (var scope = vector.CpuScopeAndSync())
		{
			EditableView<float> view = scope.View;
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

	internal static Vector DiffX(Vector vector)
	{
		if (vector.Columns > 1)
			throw new Exception("DiffX is for use with 1D Vectors ONLY");

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

	internal static Vector NanToNumX(Vector vector, float num)
	{
		Vector copy = vector.Copy();
		NanToNumXInPlace(copy, num);
		return copy;
	}

	internal static void NanToNumXInPlace(Vector vector, float num)
	{
		using (GpuScope.Begin(vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense> buffer = vector.GetBuffer();
			vector.Gpu.nanToNumKernel(vector.Gpu.accelerator.DefaultStream, vector.Length, buffer.View, num);
			vector.Gpu.accelerator.Synchronize();
		}
	}

	internal static Vector NormaliseX(Vector vector) =>
		vector.OP(1f / vector.Sum(), Operations.multiply);

	internal static void NormaliseXInPlace(Vector vector) =>
		vector.IPOP(1f / vector.Sum(), Operations.multiply);

	internal static VectorInt Abs(VectorInt vector)
	{
		VectorInt copy = vector.Copy();
		AbsInPlace(copy);
		return copy;
	}

	internal static void AbsInPlace(VectorInt vector)
	{
		using (var scope = vector.CpuScopeAndSync())
		{
			EditableView<int> view = scope.View;
			for (int i = 0; i < vector.Length; i++)
				view[i] = view[i] & int.MaxValue;
		}
	}

	internal static VectorInt AbsX(VectorInt vector)
	{
		VectorInt copy = vector.Copy();
		AbsXInPlace(copy);
		return copy;
	}

	internal static void AbsXInPlace(VectorInt vector)
	{
		using (GpuScope.Begin(vector))
		{
			MemoryBuffer1D<int, Stride1D.Dense> buffer = vector.GetBuffer();
			vector.Gpu.absIntKernel(vector.Gpu.DefaultStream, buffer.IntExtent, buffer.View);
			vector.Gpu.Synchronize();
		}
	}

	internal static VectorInt DiffX(VectorInt vector)
	{
		if (vector.Columns > 1)
			throw new Exception("DiffX is for use with 1D Vectors ONLY");

		GPU gpu = vector.Gpu;
		VectorInt output = new(gpu, vector.Length - 1, vector.Columns);

		using (GpuScope.Begin(output, vector))
		{
			MemoryBuffer1D<int, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer();

			gpu.diffIntKernel(gpu.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View);
			gpu.Synchronize();
		}

		return output;
	}
}
