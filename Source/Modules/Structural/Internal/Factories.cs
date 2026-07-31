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
}
