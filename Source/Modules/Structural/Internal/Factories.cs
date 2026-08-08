using System.Linq;
using BAVCL.Modules.Generators;

namespace BAVCL.Modules.Structural;

internal static class FactoriesCore
{
	internal static Vector Zeros(GPU gpu, int length, int columns = 0) =>
		new(gpu, new float[length], columns);

	internal static void ZerosInPlace(Vector vector, int length, int columns = 0)
	{
		using (vector.CpuScopeAndSync())
		{
			vector.Value = new float[length];
			vector.Length = vector.Value.Length;
			vector.Columns = columns;
		}
	}

	internal static BAVCL.Geometric.Vector3 Zeros(GPU gpu, int length) =>
		new(gpu, new float[length]);

	internal static Vector Ones(GPU gpu, int length, int columns = 0) =>
		new(gpu, Enumerable.Repeat(1f, length).ToArray(), columns);

	internal static void OnesInPlace(Vector vector, int length, int columns = 0)
	{
		using (vector.CpuScopeAndSync())
		{
			vector.Value = Enumerable.Repeat(1f, length).ToArray();
			vector.Length = vector.Value.Length;
			vector.Columns = columns;
		}
	}

	internal static Vector Fill(GPU gpu, float value, int length, int columns = 0, bool cache = true) =>
		new(gpu, Enumerable.Repeat(value, length).ToArray(), columns, cache);

	internal static void FillInPlace(Vector vector, float value, int length, int columns = 0)
	{
		using (vector.CpuScopeAndSync())
		{
			vector.Value = Enumerable.Repeat(value, length).ToArray();
			vector.Length = vector.Value.Length;
			vector.Columns = columns;
		}
	}

	internal static BAVCL.Geometric.Vector3 Fill(GPU gpu, float value, int length) =>
		new(gpu, Enumerable.Repeat(value, length).ToArray());

	internal static Vector Arange(GPU gpu, float startval, float endval, float interval, int columns = 0, bool cache = true)
	{
		float[] values = GeneratorsCore.Arange(startval, endval, interval);
		return new Vector(gpu, values, columns, cache);	
	}

	internal static Vector Linspace(GPU gpu, float startval, float endval, int steps, int columns = 0, bool cache = true)
	{
		float[] arr = GeneratorsCore.Linspace(startval, endval, steps);
		return new Vector(gpu, arr, columns, cache);
	}

	internal static VectorInt ZerosInt(GPU gpu, int length, int columns = 0) =>
		new(gpu, new int[length], columns);

	internal static void ZerosInPlace(VectorInt vector, int length, int columns = 0)
	{
		using (vector.CpuScopeAndSync())
		{
			vector.Value = new int[length];
			vector.Length = vector.Value.Length;
			vector.Columns = columns;
		}
	}

	internal static VectorInt OnesInt(GPU gpu, int length, int columns = 0) =>
		new(gpu, Enumerable.Repeat(1, length).ToArray(), columns);

	internal static void OnesInPlace(VectorInt vector, int length, int columns = 0)
	{
		using (vector.CpuScopeAndSync())
		{
			vector.Value = Enumerable.Repeat(1, length).ToArray();
			vector.Length = vector.Value.Length;
			vector.Columns = columns;
		}
	}

	internal static VectorInt Fill(GPU gpu, int value, int length, int columns = 0, bool cache = true) =>
		new(gpu, Enumerable.Repeat(value, length).ToArray(), columns, cache);

	internal static void FillInPlace(VectorInt vector, int value, int length, int columns = 0)
	{
		using (vector.CpuScopeAndSync())
		{
			vector.Value = Enumerable.Repeat(value, length).ToArray();
			vector.Length = vector.Value.Length;
			vector.Columns = columns;
		}
	}

	internal static VectorInt Arange(GPU gpu, int startval, int endval, int interval, int columns = 0, bool cache = true)
	{
		int[] values = GeneratorsCore.Arange(startval, endval, interval);
		return new VectorInt(gpu, values, columns, cache);
	}

	internal static VectorInt Linspace(GPU gpu, int startval, int endval, int steps, int columns = 0, bool cache = true)
	{
		int[] arr = GeneratorsCore.Linspace(startval, endval, steps);
		return new VectorInt(gpu, arr, columns, cache);
	}
}
