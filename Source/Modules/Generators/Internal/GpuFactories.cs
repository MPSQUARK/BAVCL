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
		if (endval < startval && interval > 0f)
			interval = -interval;

		int length = GeneratorsCore.ArangeLength(startval, endval, interval);
		var output = new Vector(gpu, length, columns);
		using (GpuScope.Begin(output))
		{
			var buffer = output.GetBuffer();
			if (buffer.IntExtent > 0)
			{
				gpu.fillSequence(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, startval, interval);
				gpu.accelerator.Synchronize();
			}
		}
		return output;
	}

	internal static Vector Linspace(GPU gpu, float startval, float endval, int steps, int columns = 0)
	{
		Guard.ArgumentIsNonNegative(steps);
		if (steps == 1)
		{
			var single = new Vector(gpu, 1, columns);
			using (GpuScope.Begin(single))
				single.GetBuffer().AsArrayView<float>(0, 1).CopyFromCPU([startval]);
			return single;
		}

		var output = new Vector(gpu, steps, columns);
		using (GpuScope.Begin(output))
		{
			var buffer = output.GetBuffer();
			if (buffer.IntExtent > 0)
			{
				float step = (endval - startval) / (steps - 1);
				gpu.fillSequence(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, startval, step);
				gpu.accelerator.Synchronize();
			}
		}
		return output;
	}

	internal static Vector UniformRandom(
		GPU gpu, int length, float minimum, float maximum, int seed, int columns = 0)
	{
		Guard.ArgumentIsLessThan(minimum, maximum);
		var output = new Vector(gpu, length, columns);
		using (GpuScope.Begin(output))
		{
			var buffer = output.GetBuffer();
			if (buffer.IntExtent > 0)
			{
				using var rng = new RNG<XorShift128Plus>(gpu.accelerator, new Random(seed));
				gpu.fillUniform(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View,
					rng.GetView(rng.MaxNumParallelWarps), minimum, maximum);
				gpu.accelerator.Synchronize();
			}
		}
		return output;
	}

	internal static Vector UniformRandom(
		GPU gpu, int length, float minimum, float maximum, int columns = 0) =>
		UniformRandom(gpu, length, minimum, maximum, Random.Shared.Next(), columns);

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

	internal static VectorInt UniformRandom(
		GPU gpu, int length, int minimum, int maximum, int seed, int columns = 0)
	{
		Guard.ArgumentIsLessThan(minimum, maximum);
		var output = new VectorInt(gpu, length, columns);
		using (GpuScope.Begin(output))
		{
			var buffer = output.GetBuffer();
			if (buffer.IntExtent > 0)
			{
				using var rng = new RNG<XorShift128Plus>(gpu.accelerator, new Random(seed));
				gpu.fillUniformInt(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View,
					rng.GetView(rng.MaxNumParallelWarps), minimum, (long)maximum - minimum);
				gpu.accelerator.Synchronize();
			}
		}
		return output;
	}

	internal static VectorInt UniformRandom(
		GPU gpu, int length, int minimum, int maximum, int columns = 0) =>
		UniformRandom(gpu, length, minimum, maximum, Random.Shared.Next(), columns);

	static void ZeroBuffer<T>(CacheableBase<T> output)
		where T : unmanaged
	{
		using (GpuScope.Begin(output))
		{
			var buffer = output.GetBuffer();
			if (buffer.IntExtent > 0)
			{
				buffer.MemSetToZero(output.Gpu.accelerator.DefaultStream);
				output.Gpu.accelerator.Synchronize();
			}
		}
	}

	static void Initialize<T>(CacheableBase<T> output, T value)
		where T : unmanaged
	{
		using (GpuScope.Begin(output))
		{
			var buffer = output.GetBuffer();
			if (buffer.IntExtent > 0)
			{
				output.Gpu.accelerator.Initialize(
					output.Gpu.accelerator.DefaultStream,
					buffer.View,
					value);
				output.Gpu.accelerator.Synchronize();
			}
		}
	}

	static Vector3 InitializeVector3(GPU gpu, int length, float value)
	{
		var output = new Vector3(gpu, length);
		Initialize(output, value);
		return output;
	}
}
