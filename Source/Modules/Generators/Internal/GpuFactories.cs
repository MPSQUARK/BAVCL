using System;
using BAVCL.Geometric;
using ILGPU.Algorithms;
using ILGPU.Algorithms.Random;
using ILGPU.Runtime;

namespace BAVCL.Modules.Generators;

internal static class GpuFactoriesCore
{
	internal static Vector Zeros(int length, int columns = 0, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(length, columns, Init.Zeroed, gpu);
	}

	internal static Vector ZerosInPlaceX(Vector vector)
	{
		Guard.ArgumentIsPositive(vector.Length);
		DeviceFill.Zero(vector);
		return vector;
	}

	internal static Vector3 Zeros3(int length, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(length, Init.Zeroed, gpu);
	}

	internal static Vector Ones(int length, int columns = 0, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		var output = new Vector(length, columns, Init.UnInit, gpu);
		DeviceFill.Fill(output, 1f);
		return output;
	}

	internal static Vector OnesInPlaceX(Vector vector)
	{
		Guard.ArgumentIsPositive(vector.Length);
		DeviceFill.Fill(vector, 1f);
		return vector;
	}

	internal static Vector Fill(float value, int length, int columns = 0, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		var output = new Vector(length, columns, Init.UnInit, gpu);
		DeviceFill.Fill(output, value);
		return output;
	}

	internal static Vector FillInPlaceX(Vector vector, float value)
	{
		Guard.ArgumentIsPositive(vector.Length);
		DeviceFill.Fill(vector, value);
		return vector;
	}

	internal static Vector3 Fill3(float value, int length, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		var output = new Vector3(length, Init.UnInit, gpu);
		DeviceFill.Fill(output, value);
		return output;
	}

	internal static Vector Arange(float startval, float endval, float interval, int columns = 0, GPU? gpu = null)
	{
		Guard.ArgumentIsFinite(startval);
		Guard.ArgumentIsFinite(endval);
		Guard.ArgumentIsFinite(interval);
		Guard.ArgumentIsNotZero(interval);
		if (endval < startval && interval > 0) interval = -interval;

		int length = GeneratorsCore.CountSamples(endval - startval, interval);
		Guard.RangeIsNotEmpty(length);
		GPU targetGpu = gpu ?? GPUManager.Default;
		if (length == 1)
			return new Vector([startval], columns, Upload.Now, targetGpu);

		var output = new Vector(length, columns, Init.UnInit, targetGpu);
		using (GpuScope.Begin(output))
		{
			targetGpu.fillSequence(targetGpu.accelerator.DefaultStream, length, output.GetBuffer().View, startval, interval);
			targetGpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static Vector Linspace(float startval, float endval, int steps, int columns = 0, GPU? gpu = null)
	{
		Guard.ArgumentIsFinite(startval);
		Guard.ArgumentIsFinite(endval);
		Guard.ArgumentIsPositive(steps);
		GPU targetGpu = gpu ?? GPUManager.Default;
		if (steps == 1)
			return new Vector([startval], columns, Upload.Now, targetGpu);

		var output = new Vector(steps, columns, Init.UnInit, targetGpu);
		float step = (endval - startval) / (steps - 1);
		using (GpuScope.Begin(output))
		{
			targetGpu.fillSequence(targetGpu.accelerator.DefaultStream, steps, output.GetBuffer().View, startval, step);
			targetGpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static Vector Random(int length, int? seed, int columns, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		GPU targetGpu = gpu ?? GPUManager.Default;
		var output = new Vector(length, columns, Init.UnInit, targetGpu);
		using var rng = CreateRng(targetGpu, seed);
		using (GpuScope.Begin(output))
		{
			rng.FillUniform(targetGpu.accelerator.DefaultStream, output.GetBuffer().AsArrayView<float>(0, length));
			targetGpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static Vector RandomUniform(int length, float low, float high, int? seed, int columns, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		Guard.ArgumentIsFinite(low);
		Guard.ArgumentIsFinite(high);
		Guard.ArgumentIsLessThan(low, high);
		GPU targetGpu = gpu ?? GPUManager.Default;
		var output = new Vector(length, columns, Init.UnInit, targetGpu);
		using var rng = CreateRng(targetGpu, seed);
		using (GpuScope.Begin(output))
		{
			var stream = targetGpu.accelerator.DefaultStream;
			var view = output.GetBuffer().AsArrayView<float>(0, length);
			rng.FillUniform(stream, view);
			targetGpu.scaleUniform(stream, length, view, low, high, MathF.BitDecrement(high));
			targetGpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static Vector RandomNormal(int length, float mean, float stdDev, int? seed, int columns, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		Guard.ArgumentIsFinite(mean);
		Guard.ArgumentIsFinite(stdDev);
		Guard.ArgumentIsNonNegative(stdDev);
		GPU targetGpu = gpu ?? GPUManager.Default;
		var output = new Vector(length, columns, Init.UnInit, targetGpu);
		using var rng = CreateRng(targetGpu, seed);
		using (GpuScope.Begin(output))
		{
			var stream = targetGpu.accelerator.DefaultStream;
			var view = output.GetBuffer().AsArrayView<float>(0, length);
			rng.FillUniform(stream, view);
			targetGpu.normalScale(stream, length, view, mean, stdDev);
			targetGpu.accelerator.Synchronize();
		}
		return output;
	}

	internal static VectorInt ZerosInt(int length, int columns = 0, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(length, columns, Init.Zeroed, gpu);
	}

	internal static VectorInt ZerosInPlaceX(VectorInt vector)
	{
		Guard.ArgumentIsPositive(vector.Length);
		DeviceFill.Zero(vector);
		return vector;
	}

	internal static VectorInt OnesInt(int length, int columns = 0, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		var output = new VectorInt(length, columns, Init.UnInit, gpu);
		DeviceFill.Fill(output, 1);
		return output;
	}

	internal static VectorInt OnesInPlaceX(VectorInt vector)
	{
		Guard.ArgumentIsPositive(vector.Length);
		DeviceFill.Fill(vector, 1);
		return vector;
	}

	internal static VectorInt Fill(int value, int length, int columns = 0, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		var output = new VectorInt(length, columns, Init.UnInit, gpu);
		DeviceFill.Fill(output, value);
		return output;
	}

	internal static VectorInt FillInPlaceX(VectorInt vector, int value)
	{
		Guard.ArgumentIsPositive(vector.Length);
		DeviceFill.Fill(vector, value);
		return vector;
	}

	internal static VectorInt RandomUniform(int length, int low, int high, int? seed, int columns, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		Guard.ArgumentIsLessThan(low, high);
		GPU targetGpu = gpu ?? GPUManager.Default;
		var output = new VectorInt(length, columns, Init.UnInit, targetGpu);
		using var rng = CreateRng(targetGpu, seed);
		using (GpuScope.Begin(output))
		{
			var stream = targetGpu.accelerator.DefaultStream;
			var view = output.GetBuffer().AsArrayView<int>(0, length);
			rng.FillUniform(stream, view);
			targetGpu.scaleUniformInt(stream, length, view, low, (long)high - low);
			targetGpu.accelerator.Synchronize();
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
