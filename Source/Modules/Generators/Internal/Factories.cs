using System;
using static BAVCL.QoL.ArrayFactory;

namespace BAVCL.Modules.Generators;

internal static class FactoriesCore
{
	internal static Vector Zeros(int length, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(new float[length], columns, upload, gpu);
	}

	internal static BAVCL.Geometric.Vector3 Zeros3(int length, Upload upload = Upload.Now, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(new float[length], upload, gpu);
	}

	internal static Vector Ones(int length, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(FilledArray(length, 1f), columns, upload, gpu);
	}

	internal static Vector Fill(float value, int length, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(FilledArray(length, value), columns, upload, gpu);
	}

	internal static BAVCL.Geometric.Vector3 Fill3(float value, int length, Upload upload = Upload.Now, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(FilledArray(length, value), upload, gpu);
	}

	internal static Vector Arange(
		float startval, float endval, float interval, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null)
	{
		float[] values = GeneratorsCore.Arange(startval, endval, interval);
		Guard.RangeIsNotEmpty(values.Length);
		return new(values, columns, upload, gpu);
	}

	internal static Vector Linspace(
		float startval, float endval, int steps, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(steps);
		float[] values = GeneratorsCore.Linspace(startval, endval, steps);
		return new(values, columns, upload, gpu);
	}

	internal static Vector ZerosInPlace(Vector vector, Upload upload = Upload.Now)
	{
		Guard.ArgumentIsPositive(vector.Length);
		using (vector.CpuScope(upload))
			vector.Value = new float[vector.Length];
		return vector;
	}

	internal static Vector OnesInPlace(Vector vector, Upload upload = Upload.Now)
	{
		Guard.ArgumentIsPositive(vector.Length);
		using (vector.CpuScope(upload))
			vector.Value = FilledArray(vector.Length, 1f);
		return vector;
	}

	internal static Vector FillInPlace(Vector vector, float value, Upload upload = Upload.Now)
	{
		Guard.ArgumentIsPositive(vector.Length);
		using (vector.CpuScope(upload))
			vector.Value = FilledArray(vector.Length, value);
		return vector;
	}

	internal static Vector Random(int length, int? seed, int columns, Upload upload = Upload.Now, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(GenerateRandom(length, seed), columns, upload, gpu);
	}

	internal static Vector RandomUniform(int length, float low, float high, int? seed, int columns, Upload upload = Upload.Now, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(GenerateUniform(length, low, high, seed), columns, upload, gpu);
	}

	internal static Vector RandomNormal(int length, float mean, float stdDev, int? seed, int columns, Upload upload = Upload.Now, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(GenerateNormal(length, mean, stdDev, seed), columns, upload, gpu);
	}

	internal static VectorInt ZerosInt(int length, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(new int[length], columns, upload, gpu);
	}

	internal static VectorInt OnesInt(int length, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(FilledArray(length, 1), columns, upload, gpu);
	}

	internal static VectorInt Fill(int value, int length, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(FilledArray(length, value), columns, upload, gpu);
	}

	internal static VectorInt Arange(
		int startval, int endval, int interval, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null)
	{
		int[] values = GeneratorsCore.Arange(startval, endval, interval);
		Guard.RangeIsNotEmpty(values.Length);
		return new(values, columns, upload, gpu);
	}

	internal static VectorInt Linspace(
		int startval, int endval, int steps, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(steps);
		int[] values = GeneratorsCore.Linspace(startval, endval, steps);
		return new(values, columns, upload, gpu);
	}

	internal static VectorInt ZerosInPlace(VectorInt vector, Upload upload = Upload.Now)
	{
		Guard.ArgumentIsPositive(vector.Length);
		using (vector.CpuScope(upload))
			vector.Value = new int[vector.Length];
		return vector;
	}

	internal static VectorInt OnesInPlace(VectorInt vector, Upload upload = Upload.Now)
	{
		Guard.ArgumentIsPositive(vector.Length);
		using (vector.CpuScope(upload))
			vector.Value = FilledArray(vector.Length, 1);
		return vector;
	}

	internal static VectorInt FillInPlace(VectorInt vector, int value, Upload upload = Upload.Now)
	{
		Guard.ArgumentIsPositive(vector.Length);
		using (vector.CpuScope(upload))
			vector.Value = FilledArray(vector.Length, value);
		return vector;
	}

	internal static VectorInt RandomUniform(int length, int low, int high, int? seed, int columns, Upload upload = Upload.Now, GPU? gpu = null)
	{
		Guard.ArgumentIsPositive(length);
		return new(GenerateUniform(length, low, high, seed), columns, upload, gpu);
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
