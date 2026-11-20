using System;
using static BAVCL.QoL.ArrayFactory;

namespace BAVCL.Modules.Generators;

internal static class FactoriesCore
{
	internal static Vector Zeros(GPU gpu, int length, int columns = 0)
	{
		Guard.ArgumentIsPositive(length);
		return new(gpu, new float[length], columns);
	}

	internal static BAVCL.Geometric.Vector3 Zeros(GPU gpu, int length)
	{
		Guard.ArgumentIsPositive(length);
		return new(gpu, new float[length], true);
	}

	internal static Vector Ones(GPU gpu, int length, int columns = 0)
	{
		Guard.ArgumentIsPositive(length);
		return new(gpu, FilledArray(length, 1f), columns);
	}

	internal static Vector Fill(GPU gpu, float value, int length, int columns = 0, bool cache = true)
	{
		Guard.ArgumentIsPositive(length);
		return new(gpu, FilledArray(length, value), columns, cache);
	}

	internal static BAVCL.Geometric.Vector3 Fill(GPU gpu, float value, int length)
	{
		Guard.ArgumentIsPositive(length);
		return new(gpu, FilledArray(length, value), true);
	}

	internal static Vector Arange(
		GPU gpu, float startval, float endval, float interval, int columns = 0, bool cache = true)
	{
		float[] values = GeneratorsCore.Arange(startval, endval, interval);
		Guard.RangeIsNotEmpty(values.Length);
		return new(gpu, values, columns, cache);
	}

	internal static Vector Linspace(
		GPU gpu, float startval, float endval, int steps, int columns = 0, bool cache = true)
	{
		Guard.ArgumentIsPositive(steps);
		float[] values = GeneratorsCore.Linspace(startval, endval, steps);
		return new(gpu, values, columns, cache);
	}

	internal static Vector ZerosInPlace(Vector vector, bool sync = true)
	{
		Guard.ArgumentIsPositive(vector.Length);
		using (vector.CpuScope(sync))
			vector.Value = new float[vector.Length];
		return vector;
	}

	internal static Vector OnesInPlace(Vector vector, bool sync = true)
	{
		Guard.ArgumentIsPositive(vector.Length);
		using (vector.CpuScope(sync))
			vector.Value = FilledArray(vector.Length, 1f);
		return vector;
	}

	internal static Vector FillInPlace(Vector vector, float value, bool sync = true)
	{
		Guard.ArgumentIsPositive(vector.Length);
		using (vector.CpuScope(sync))
			vector.Value = FilledArray(vector.Length, value);
		return vector;
	}

	internal static Vector Random(GPU gpu, int length, int? seed, int columns)
	{
		Guard.ArgumentIsPositive(length);
		return new(gpu, GenerateRandom(length, seed), columns);
	}

	internal static Vector RandomUniform(GPU gpu, int length, float low, float high, int? seed, int columns)
	{
		Guard.ArgumentIsPositive(length);
		return new(gpu, GenerateUniform(length, low, high, seed), columns);
	}

	internal static Vector RandomNormal(GPU gpu, int length, float mean, float stdDev, int? seed, int columns)
	{
		Guard.ArgumentIsPositive(length);
		return new(gpu, GenerateNormal(length, mean, stdDev, seed), columns);
	}

	internal static VectorInt ZerosInt(GPU gpu, int length, int columns = 0)
	{
		Guard.ArgumentIsPositive(length);
		return new(gpu, new int[length], columns);
	}

	internal static VectorInt OnesInt(GPU gpu, int length, int columns = 0)
	{
		Guard.ArgumentIsPositive(length);
		return new(gpu, FilledArray(length, 1), columns);
	}

	internal static VectorInt Fill(GPU gpu, int value, int length, int columns = 0, bool cache = true)
	{
		Guard.ArgumentIsPositive(length);
		return new(gpu, FilledArray(length, value), columns, cache);
	}

	internal static VectorInt Arange(
		GPU gpu, int startval, int endval, int interval, int columns = 0, bool cache = true)
	{
		int[] values = GeneratorsCore.Arange(startval, endval, interval);
		Guard.RangeIsNotEmpty(values.Length);
		return new(gpu, values, columns, cache);
	}

	internal static VectorInt Linspace(
		GPU gpu, int startval, int endval, int steps, int columns = 0, bool cache = true)
	{
		Guard.ArgumentIsPositive(steps);
		int[] values = GeneratorsCore.Linspace(startval, endval, steps);
		return new(gpu, values, columns, cache);
	}

	internal static VectorInt ZerosInPlace(VectorInt vector, bool sync = true)
	{
		Guard.ArgumentIsPositive(vector.Length);
		using (vector.CpuScope(sync))
			vector.Value = new int[vector.Length];
		return vector;
	}

	internal static VectorInt OnesInPlace(VectorInt vector, bool sync = true)
	{
		Guard.ArgumentIsPositive(vector.Length);
		using (vector.CpuScope(sync))
			vector.Value = FilledArray(vector.Length, 1);
		return vector;
	}

	internal static VectorInt FillInPlace(VectorInt vector, int value, bool sync = true)
	{
		Guard.ArgumentIsPositive(vector.Length);
		using (vector.CpuScope(sync))
			vector.Value = FilledArray(vector.Length, value);
		return vector;
	}

	internal static VectorInt RandomUniform(GPU gpu, int length, int low, int high, int? seed, int columns)
	{
		Guard.ArgumentIsPositive(length);
		return new(gpu, GenerateUniform(length, low, high, seed), columns);
	}

	static float[] GenerateRandom(int length, int? seed)
	{
		var random = CreateRandom(seed);
		var values = new float[length];
		for (int i = 0; i < values.Length; i++)
			values[i] = random.NextSingle();
		return values;
	}

	static float[] GenerateUniform(int length, float low, float high, int? seed)
	{
		Guard.ArgumentIsFinite(low);
		Guard.ArgumentIsFinite(high);
		Guard.ArgumentIsLessThan(low, high);
		var random = CreateRandom(seed);
		var values = new float[length];
		float belowHigh = MathF.BitDecrement(high);
		for (int i = 0; i < values.Length; i++)
		{
			double sample = random.NextDouble();
			values[i] = MathF.Min((float)(low * (1d - sample) + high * sample), belowHigh);
		}
		return values;
	}

	static int[] GenerateUniform(int length, int low, int high, int? seed)
	{
		Guard.ArgumentIsLessThan(low, high);
		var random = CreateRandom(seed);
		var values = new int[length];
		for (int i = 0; i < values.Length; i++)
			values[i] = (int)random.NextInt64(low, high);
		return values;
	}

	/// <summary>
	/// Box-Muller transform (see citations.md [13]). Each pair of uniform draws yields two normal samples.
	/// For odd lengths the second sample of the final pair is discarded.
	/// </summary>
	static float[] GenerateNormal(int length, float mean, float stdDev, int? seed)
	{
		Guard.ArgumentIsFinite(mean);
		Guard.ArgumentIsFinite(stdDev);
		Guard.ArgumentIsNonNegative(stdDev);
		var random = CreateRandom(seed);
		var values = new float[length];
		for (int i = 0; i < values.Length; i += 2)
		{
			double radius = Math.Sqrt(-2d * Math.Log(1d - random.NextDouble()));
			double angle = 2d * Math.PI * random.NextDouble();
			values[i] = (float)(mean + stdDev * radius * Math.Cos(angle));
			if (i + 1 < values.Length)
				values[i + 1] = (float)(mean + stdDev * radius * Math.Sin(angle));
		}
		return values;
	}

	internal static System.Random CreateRandom(int? seed) =>
		seed is int value ? new System.Random(value) : new System.Random();
}
