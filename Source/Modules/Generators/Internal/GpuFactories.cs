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
		var output = new Vector(gpu, length, columns);
		ZeroBuffer(output);
		return output;
	}

	internal static Vector ZerosInPlaceX(Vector vector)
	{
		ZeroBuffer(vector);
		return vector;
	}

	internal static Vector3 Zeros(GPU gpu, int length)
	{
		var output = new Vector3(gpu, length);
		ZeroBuffer(output);
		return output;
	}

	internal static Vector Ones(GPU gpu, int length, int columns = 0)
	{
		var output = new Vector(gpu, length, columns);
		Initialize(output, 1f);
		return output;
	}

	internal static Vector OnesInPlaceX(Vector vector)
	{
		Initialize(vector, 1f);
		return vector;
	}

	internal static Vector Fill(GPU gpu, float value, int length, int columns = 0)
	{
		var output = new Vector(gpu, length, columns);
		Initialize(output, value);
		return output;
	}

	internal static Vector FillInPlaceX(Vector vector, float value)
	{
		Initialize(vector, value);
		return vector;
	}

	internal static Vector3 Fill(GPU gpu, float value, int length) =>
		InitializeVector3(gpu, length, value);

	internal static Vector Arange(GPU gpu, float startval, float endval, float interval, int columns = 0)
	{
        Guard.ArgumentIsFinite(startval);
        Guard.ArgumentIsFinite(endval);

        if (endval < startval && interval > 0f)
			interval = -interval;

		int length = GeneratorsCore.ArangeLength(startval, endval, interval);
		if (length == 1)
			return new Vector(gpu, [startval], columns);

		var output = new Vector(gpu, length, columns);
		if (output.Length == 0)
			return output;

		using (GpuScope.Begin(output))
		{
			gpu.fillSequence(gpu.accelerator.DefaultStream, output.Length, output.GetBuffer().View, startval, interval);
			gpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static Vector Linspace(GPU gpu, float startval, float endval, int steps, int columns = 0)
	{
		Guard.ArgumentIsFinite(startval);
		Guard.ArgumentIsFinite(endval);
		Guard.ArgumentIsNonNegative(steps);
		if (steps == 1)
			return new Vector(gpu, [startval], columns);

		var output = new Vector(gpu, steps, columns);
		if (output.Length == 0)
			return output;

		float step = (endval - startval) / (steps - 1);
		using (GpuScope.Begin(output))
		{
			gpu.fillSequence(gpu.accelerator.DefaultStream, output.Length, output.GetBuffer().View, startval, step);
			gpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static Vector Random(GPU gpu, int length, int? seed, int columns)
	{
		var output = new Vector(gpu, length, columns);
		if (output.Length == 0)
			return output;

		using var rng = CreateRng(gpu, seed);
		using (GpuScope.Begin(output))
		{
			rng.FillUniform(gpu.accelerator.DefaultStream, output.GetBuffer().AsArrayView<float>(0, output.Length));
			gpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static Vector Uniform(GPU gpu, int length, float low, float high, int? seed, int columns)
	{
		Guard.ArgumentIsFinite(low);
		Guard.ArgumentIsFinite(high);
		Guard.ArgumentIsLessThan(low, high);
		var output = new Vector(gpu, length, columns);
		if (output.Length == 0)
			return output;

		using var rng = CreateRng(gpu, seed);
		using (GpuScope.Begin(output))
		{
			var stream = gpu.accelerator.DefaultStream;
			var view = output.GetBuffer().AsArrayView<float>(0, output.Length);
			rng.FillUniform(stream, view);
			gpu.scaleUniform(stream, output.Length, view, low, high, MathF.BitDecrement(high));
			gpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static Vector Normal(GPU gpu, int length, float mean, float stdDev, int? seed, int columns)
	{
		Guard.ArgumentIsFinite(mean);
		Guard.ArgumentIsFinite(stdDev);
		Guard.ArgumentIsNonNegative(stdDev);
		var output = new Vector(gpu, length, columns);
		if (output.Length == 0)
			return output;

		using var rng = CreateRng(gpu, seed);
		using (GpuScope.Begin(output))
		{
			var stream = gpu.accelerator.DefaultStream;
			var view = output.GetBuffer().AsArrayView<float>(0, output.Length);
			rng.FillUniform(stream, view);
			gpu.normalScale(stream, output.Length, view, mean, stdDev);
			gpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static VectorInt ZerosInt(GPU gpu, int length, int columns = 0)
	{
		var output = new VectorInt(gpu, length, columns);
		ZeroBuffer(output);
		return output;
	}

	internal static VectorInt ZerosInPlaceX(VectorInt vector)
	{
		ZeroBuffer(vector);
		return vector;
	}

	internal static VectorInt OnesInt(GPU gpu, int length, int columns = 0)
	{
		var output = new VectorInt(gpu, length, columns);
		Initialize(output, 1);
		return output;
	}

	internal static VectorInt OnesInPlaceX(VectorInt vector)
	{
		Initialize(vector, 1);
		return vector;
	}

	internal static VectorInt Fill(GPU gpu, int value, int length, int columns = 0)
	{
		var output = new VectorInt(gpu, length, columns);
		Initialize(output, value);
		return output;
	}

	internal static VectorInt FillInPlaceX(VectorInt vector, int value)
	{
		Initialize(vector, value);
		return vector;
	}

	internal static VectorInt Uniform(GPU gpu, int length, int low, int high, int? seed, int columns)
	{
		Guard.ArgumentIsLessThan(low, high);
		var output = new VectorInt(gpu, length, columns);
		if (output.Length == 0)
			return output;

		using var rng = CreateRng(gpu, seed);
		using (GpuScope.Begin(output))
		{
			var stream = gpu.accelerator.DefaultStream;
			var view = output.GetBuffer().AsArrayView<int>(0, output.Length);
			rng.FillUniform(stream, view);
			gpu.scaleUniformInt(stream, output.Length, view, low, (long)high - low);
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

	/// <summary>Clears the buffer on the device. Empty vectors have nothing to clear.</summary>
	static void ZeroBuffer<T>(CacheableBase<T> output)
		where T : unmanaged
	{
		if (output.Length == 0)
			return;

		using (GpuScope.Begin(output))
		{
			output.GetBuffer().MemSetToZero(output.Gpu.accelerator.DefaultStream);
			output.Gpu.accelerator.Synchronize();
		}
	}

	/// <summary>Fills the buffer on the device with <paramref name="value"/>. Empty vectors have nothing to fill.</summary>
	static void Initialize<T>(CacheableBase<T> output, T value)
		where T : unmanaged
	{
		if (output.Length == 0)
			return;

		using (GpuScope.Begin(output))
		{
			output.Gpu.accelerator.Initialize(
				output.Gpu.accelerator.DefaultStream,
				output.GetBuffer().View,
				value);
			output.Gpu.accelerator.Synchronize();
		}
	}

	static Vector3 InitializeVector3(GPU gpu, int length, float value)
	{
		var output = new Vector3(gpu, length);
		Initialize(output, value);
		return output;
	}
}
