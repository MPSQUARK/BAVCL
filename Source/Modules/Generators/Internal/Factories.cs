using System;
using System.Linq;

namespace BAVCL.Modules.Generators;

internal static class FactoriesCore
{
	internal static Vector Zeros(GPU gpu, int length, int columns = 0) =>
		new(gpu, new float[length], columns);

	internal static BAVCL.Geometric.Vector3 Zeros(GPU gpu, int length) =>
		new(gpu, new float[length]);

	internal static Vector Ones(GPU gpu, int length, int columns = 0) =>
		new(gpu, Enumerable.Repeat(1f, length).ToArray(), columns);

	internal static Vector Fill(GPU gpu, float value, int length, int columns = 0, bool cache = true) =>
		new(gpu, Enumerable.Repeat(value, length).ToArray(), columns, cache);

	internal static BAVCL.Geometric.Vector3 Fill(GPU gpu, float value, int length) =>
		new(gpu, Enumerable.Repeat(value, length).ToArray());

	internal static Vector Arange(
		GPU gpu, float startval, float endval, float interval, int columns = 0, bool cache = true) =>
		new(gpu, GeneratorsCore.Arange(startval, endval, interval), columns, cache);

	internal static Vector Linspace(
		GPU gpu, float startval, float endval, int steps, int columns = 0, bool cache = true) =>
		new(gpu, GeneratorsCore.Linspace(startval, endval, steps), columns, cache);

	internal static Vector ZerosInPlace(Vector vector, bool sync = true)
	{
		using (vector.CpuScope(sync))
			vector.Value = new float[vector.Length];
		return vector;
	}

	internal static Vector OnesInPlace(Vector vector, bool sync = true)
	{
		using (vector.CpuScope(sync))
			vector.Value = Enumerable.Repeat(1f, vector.Length).ToArray();
		return vector;
	}

	internal static Vector FillInPlace(Vector vector, float value, bool sync = true)
	{
		using (vector.CpuScope(sync))
			vector.Value = Enumerable.Repeat(value, vector.Length).ToArray();
		return vector;
	}

	internal static Vector UniformRandom(
		GPU gpu, int length, float minimum, float maximum, int seed, int columns = 0) =>
		new(gpu, GenerateUniformRandom(length, minimum, maximum, seed), columns);

	internal static Vector UniformRandom(
		GPU gpu, int length, float minimum, float maximum, int columns = 0) =>
		new(gpu, GenerateUniformRandom(length, minimum, maximum), columns);

	internal static VectorInt ZerosInt(GPU gpu, int length, int columns = 0) =>
		new(gpu, new int[length], columns);

	internal static VectorInt OnesInt(GPU gpu, int length, int columns = 0) =>
		new(gpu, Enumerable.Repeat(1, length).ToArray(), columns);

	internal static VectorInt Fill(GPU gpu, int value, int length, int columns = 0, bool cache = true) =>
		new(gpu, Enumerable.Repeat(value, length).ToArray(), columns, cache);

	internal static VectorInt Arange(
		GPU gpu, int startval, int endval, int interval, int columns = 0, bool cache = true) =>
		new(gpu, GeneratorsCore.Arange(startval, endval, interval), columns, cache);

	internal static VectorInt Linspace(
		GPU gpu, int startval, int endval, int steps, int columns = 0, bool cache = true) =>
		new(gpu, GeneratorsCore.Linspace(startval, endval, steps), columns, cache);

	internal static VectorInt ZerosInPlace(VectorInt vector, bool sync = true)
	{
		using (vector.CpuScope(sync))
			vector.Value = new int[vector.Length];
		return vector;
	}

	internal static VectorInt OnesInPlace(VectorInt vector, bool sync = true)
	{
		using (vector.CpuScope(sync))
			vector.Value = Enumerable.Repeat(1, vector.Length).ToArray();
		return vector;
	}

	internal static VectorInt FillInPlace(VectorInt vector, int value, bool sync = true)
	{
		using (vector.CpuScope(sync))
			vector.Value = Enumerable.Repeat(value, vector.Length).ToArray();
		return vector;
	}

	internal static VectorInt UniformRandom(
		GPU gpu, int length, int minimum, int maximum, int seed, int columns = 0) =>
		new(gpu, GenerateUniformRandom(length, minimum, maximum, seed), columns);

	internal static VectorInt UniformRandom(
		GPU gpu, int length, int minimum, int maximum, int columns = 0) =>
		new(gpu, GenerateUniformRandom(length, minimum, maximum), columns);

	static float[] GenerateUniformRandom(int length, float minimum, float maximum, int seed)
	{
		Guard.ArgumentIsLessThan(minimum, maximum);
		var random = new Random(seed);
		var values = new float[length];
		for (int i = 0; i < values.Length; i++)
		{
			double sample = random.NextDouble();
			values[i] = (float)(minimum * (1d - sample) + maximum * sample);
		}
		return values;
	}

	static float[] GenerateUniformRandom(int length, float minimum, float maximum) =>
		GenerateUniformRandom(length, minimum, maximum, Random.Shared.Next());

	static int[] GenerateUniformRandom(int length, int minimum, int maximum, int seed)
	{
		Guard.ArgumentIsLessThan(minimum, maximum);
		var random = new Random(seed);
		var values = new int[length];
		for (int i = 0; i < values.Length; i++)
			values[i] = (int)random.NextInt64(minimum, maximum);
		return values;
	}

	static int[] GenerateUniformRandom(int length, int minimum, int maximum) =>
		GenerateUniformRandom(length, minimum, maximum, Random.Shared.Next());

}
