using System;
using BAVCL.Geometric;
using ILGPU.Algorithms;
using ILGPU.Algorithms.Random;
using ILGPU.Runtime;

namespace BAVCL.Modules.Generators;

internal static class GpuFactoriesCore
{
	internal static Vector Zeros(GPU gpu, int length, int columns = 0)
	{
		Guard.ArgumentIsPositive(length);
		var output = new Vector(gpu, length, columns);
		DeviceFill.Zero(output);
		return output;
	}

	internal static Vector ZerosInPlaceX(Vector vector)
	{
		Guard.ArgumentIsPositive(vector.Length);
		DeviceFill.Zero(vector);
		return vector;
	}

	internal static Vector3 Zeros(GPU gpu, int length)
	{
		Guard.ArgumentIsPositive(length);
		var output = new Vector3(gpu, length);
		DeviceFill.Zero(output);
		return output;
	}

	internal static Vector Ones(GPU gpu, int length, int columns = 0)
	{
		Guard.ArgumentIsPositive(length);
		var output = new Vector(gpu, length, columns);
		DeviceFill.Fill(output, 1f);
		return output;
	}

	internal static Vector OnesInPlaceX(Vector vector)
	{
		Guard.ArgumentIsPositive(vector.Length);
		DeviceFill.Fill(vector, 1f);
		return vector;
	}

	internal static Vector Fill(GPU gpu, float value, int length, int columns = 0)
	{
		Guard.ArgumentIsPositive(length);
		var output = new Vector(gpu, length, columns);
		DeviceFill.Fill(output, value);
		return output;
	}

	internal static Vector FillInPlaceX(Vector vector, float value)
	{
		Guard.ArgumentIsPositive(vector.Length);
		DeviceFill.Fill(vector, value);
		return vector;
	}

	internal static Vector3 Fill(GPU gpu, float value, int length)
	{
		Guard.ArgumentIsPositive(length);
		var output = new Vector3(gpu, length);
		DeviceFill.Fill(output, value);
		return output;
	}

	internal static Vector Arange(GPU gpu, float startval, float endval, float interval, int columns = 0)
	{
		if (endval < startval && interval > 0f)
			interval = -interval;

		int length = GeneratorsCore.ArangeLength(startval, endval, interval);
		Guard.RangeIsNotEmpty(length);
		if (length == 1)
			return new Vector(gpu, [startval], columns);

		var output = new Vector(gpu, length, columns);
		using (GpuScope.Begin(output))
		{
			gpu.fillSequence(gpu.accelerator.DefaultStream, length, output.GetBuffer().View, startval, interval);
			gpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static Vector Linspace(GPU gpu, float startval, float endval, int steps, int columns = 0)
	{
		Guard.ArgumentIsFinite(startval);
		Guard.ArgumentIsFinite(endval);
		Guard.ArgumentIsPositive(steps);
		if (steps == 1)
			return new Vector(gpu, [startval], columns);

		var output = new Vector(gpu, steps, columns);
		float step = (endval - startval) / (steps - 1);
		using (GpuScope.Begin(output))
		{
			gpu.fillSequence(gpu.accelerator.DefaultStream, steps, output.GetBuffer().View, startval, step);
			gpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static Vector Random(GPU gpu, int length, int? seed, int columns)
	{
		Guard.ArgumentIsPositive(length);
		var output = new Vector(gpu, length, columns);
		using var rng = CreateRng(gpu, seed);
		using (GpuScope.Begin(output))
		{
			rng.FillUniform(gpu.accelerator.DefaultStream, output.GetBuffer().AsArrayView<float>(0, length));
			gpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static Vector RandomUniform(GPU gpu, int length, float low, float high, int? seed, int columns)
	{
		Guard.ArgumentIsPositive(length);
		Guard.ArgumentIsFinite(low);
		Guard.ArgumentIsFinite(high);
		Guard.ArgumentIsLessThan(low, high);
		var output = new Vector(gpu, length, columns);
		using var rng = CreateRng(gpu, seed);
		using (GpuScope.Begin(output))
		{
			var stream = gpu.accelerator.DefaultStream;
			var view = output.GetBuffer().AsArrayView<float>(0, length);
			rng.FillUniform(stream, view);
			gpu.scaleUniform(stream, length, view, low, high, MathF.BitDecrement(high));
			gpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static Vector RandomNormal(GPU gpu, int length, float mean, float stdDev, int? seed, int columns)
	{
		Guard.ArgumentIsPositive(length);
		Guard.ArgumentIsFinite(mean);
		Guard.ArgumentIsFinite(stdDev);
		Guard.ArgumentIsNonNegative(stdDev);
		var output = new Vector(gpu, length, columns);
		using var rng = CreateRng(gpu, seed);
		using (GpuScope.Begin(output))
		{
			var stream = gpu.accelerator.DefaultStream;
			var view = output.GetBuffer().AsArrayView<float>(0, length);
			rng.FillUniform(stream, view);
			gpu.normalScale(stream, length, view, mean, stdDev);
			gpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static VectorInt ZerosInt(GPU gpu, int length, int columns = 0)
	{
		Guard.ArgumentIsPositive(length);
		var output = new VectorInt(gpu, length, columns);
		DeviceFill.Zero(output);
		return output;
	}

	internal static VectorInt ZerosInPlaceX(VectorInt vector)
	{
		Guard.ArgumentIsPositive(vector.Length);
		DeviceFill.Zero(vector);
		return vector;
	}

	internal static VectorInt OnesInt(GPU gpu, int length, int columns = 0)
	{
		Guard.ArgumentIsPositive(length);
		var output = new VectorInt(gpu, length, columns);
		DeviceFill.Fill(output, 1);
		return output;
	}

	internal static VectorInt OnesInPlaceX(VectorInt vector)
	{
		Guard.ArgumentIsPositive(vector.Length);
		DeviceFill.Fill(vector, 1);
		return vector;
	}

	internal static VectorInt Fill(GPU gpu, int value, int length, int columns = 0)
	{
		Guard.ArgumentIsPositive(length);
		var output = new VectorInt(gpu, length, columns);
		DeviceFill.Fill(output, value);
		return output;
	}

	internal static VectorInt FillInPlaceX(VectorInt vector, int value)
	{
		Guard.ArgumentIsPositive(vector.Length);
		DeviceFill.Fill(vector, value);
		return vector;
	}

	internal static VectorInt RandomUniform(GPU gpu, int length, int low, int high, int? seed, int columns)
	{
		Guard.ArgumentIsPositive(length);
		Guard.ArgumentIsLessThan(low, high);
		var output = new VectorInt(gpu, length, columns);
		using var rng = CreateRng(gpu, seed);
		using (GpuScope.Begin(output))
		{
			var stream = gpu.accelerator.DefaultStream;
			var view = output.GetBuffer().AsArrayView<int>(0, length);
			rng.FillUniform(stream, view);
			gpu.scaleUniformInt(stream, length, view, low, (long)high - low);
			gpu.accelerator.Synchronize();
		}
		return output;
	}

	/// <summary>
	/// Creates the ILGPU RNG for one factory call. ILGPU's XorShift128Plus is Vigna's xorshift128+
	/// (see citations.md [15]), itself a scrambled form of Marsaglia's xorshift (citations.md [14]).
	/// Callers keep it alive until the stream has synchronised and then dispose it.
	/// </summary>
	static RNG<XorShift128Plus> CreateRng(GPU gpu, int? seed) =>
		gpu.accelerator.CreateRNG<XorShift128Plus>(FactoriesCore.CreateRandom(seed));

}
