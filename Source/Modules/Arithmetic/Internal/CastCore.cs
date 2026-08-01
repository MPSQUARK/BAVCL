using System;
using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.Arithmetic;

/// <summary>
/// Explicit element-wise conversion between <see cref="Vector"/> (float32) and <see cref="VectorInt"/> (int32),
/// matching ordinary C# numeric conversion rules (float truncates toward zero; int widens exactly).
/// </summary>
internal static class CastCore
{
	internal static VectorInt ToVectorInt(Vector vector)
	{
		ReadOnlySpan<float> values = vector.RetrieveReadOnlySpan();
		for (int i = 0; i < values.Length; i++)
		{
			if (!float.IsFinite(values[i]))
			{
				throw new InvalidCastException(
					"Cannot cast Vector to VectorInt: source contains non-finite values (NaN or Infinity). Sanitize the float data before casting.");
			}
		}

		GPU gpu = vector.Gpu;
		VectorInt output = new(gpu, vector.Length, vector.Columns);
		output.Residence = vector.Residence;

		using (GpuScope.Begin(output, vector))
		{
			MemoryBuffer1D<int, Stride1D.Dense> buffer = output.GetBuffer();
			MemoryBuffer1D<float, Stride1D.Dense> buffer2 = vector.GetBuffer();

			gpu.floatToIntKernel(gpu.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View);
			gpu.Synchronize();
		}

		return output;
	}

	internal static Vector ToVector(VectorInt vector)
	{
		GPU gpu = vector.Gpu;
		Vector output = new(gpu, vector.Length, vector.Columns);
		output.Residence = vector.Residence;

		using (GpuScope.Begin(output, vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense> buffer = output.GetBuffer();
			MemoryBuffer1D<int, Stride1D.Dense> buffer2 = vector.GetBuffer();

			gpu.intToFloatKernel(gpu.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View);
			gpu.Synchronize();
		}

		return output;
	}
}
