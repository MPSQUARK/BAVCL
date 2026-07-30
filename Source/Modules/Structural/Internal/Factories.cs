using System;
using System.Linq;
using ILGPU.Algorithms;

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
		float[] values = Arange(startval, endval, interval);
		return new Vector(gpu, values, columns, cache);
	}

	internal static float[] Arange(float startval, float endval, float interval)
	{
		int steps = (int)((endval - startval) / interval);
		if (endval < startval && interval > 0) { steps = XMath.Abs(steps); interval = -interval; }
		if (endval % interval != 0) { steps++; }

		float[] values = new float[steps];

		for (int i = 0; i < steps; i++)
			values[i] = startval + (i * interval);

		return values;
	}

	internal static Vector Linspace(GPU gpu, float startval, float endval, int steps, int columns = 0, bool cache = true)
	{
		float[] arr = Linspace(startval, endval, steps);
		return new Vector(gpu, arr, columns, cache);
	}

	internal static float[] Linspace(float startval, float endval, int steps)
	{
		if (steps <= 1) throw new Exception("Cannot make linspace with less than 1 steps");
		float interval = (endval - startval) / (steps - 1);

		float[] arr = new float[steps];

		for (int i = 0; i < steps; i++)
			arr[i] = startval + (i * interval);

		return arr;
	}
}
