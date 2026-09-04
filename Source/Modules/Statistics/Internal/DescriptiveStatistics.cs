using System;
using BAVCL.Modules.Arithmetic;
using BAVCL.Types;
using BAVCL.Core.Helpers;
using ILGPU.Algorithms;

namespace BAVCL.Modules.Statistics;

internal static class DescriptiveStatistics
{
	internal static float Mean(Vector vector)
	{
		Guard.IsNotZero(vector.Length);
		return vector.Sum() / vector.Length;
	}

	internal static float Mean(BAVCL.Geometric.Vector3 vector3) =>
		vector3.ToArray().Sum() / vector3.Length;

	internal static float Var(Vector vector) =>
		CpuSimdReduce.VarFloat(vector.RetrieveReadOnlySpan());

	internal static float Std(Vector vector) => XMath.Sqrt(Var(vector));

	internal static float Min(Vector vector) =>
		CpuSimdReduce.MinFloat(vector.RetrieveReadOnlySpan());

	internal static float Max(Vector vector) =>
		CpuSimdReduce.MaxFloat(vector.RetrieveReadOnlySpan());

	internal static MinMax<float> MinMax(Vector vector) =>
		CpuSimdReduce.MinMaxFloat(vector.RetrieveReadOnlySpan());

	internal static float Min(BAVCL.Geometric.Vector3 vector3) => ArrayStatistics.Min(vector3.ToArray());

	internal static float Max(BAVCL.Geometric.Vector3 vector3) => ArrayStatistics.Max(vector3.ToArray());

	internal static float Range(Vector vector)
	{
		MinMax<float> bounds = MinMax(vector);
		return bounds.Max - bounds.Min;
	}

	internal static float Range(BAVCL.Geometric.Vector3 vector3) => Max(vector3) - Min(vector3);

	internal static bool All(Vector vector) =>
		CpuSimdReduce.AllFloat(vector.RetrieveReadOnlySpan());

	internal static float Mean(VectorInt vector)
	{
		Guard.IsNotZero(vector.Length);
		return vector.Sum() / vector.Length;
	}

	internal static float Var(VectorInt vector) =>
		CpuSimdReduce.VarInt(vector.RetrieveReadOnlySpan());

	internal static float Std(VectorInt vector) => XMath.Sqrt(Var(vector));

	internal static int Min(VectorInt vector) =>
		CpuSimdReduce.MinInt(vector.RetrieveReadOnlySpan());

	internal static int Max(VectorInt vector) =>
		CpuSimdReduce.MaxInt(vector.RetrieveReadOnlySpan());

	internal static MinMax<int> MinMax(VectorInt vector) =>
		CpuSimdReduce.MinMaxInt(vector.RetrieveReadOnlySpan());

	internal static int Range(VectorInt vector)
	{
		MinMax<int> bounds = MinMax(vector);
		return bounds.Max - bounds.Min;
	}

	internal static bool All(VectorInt vector) =>
		CpuSimdReduce.AllInt(vector.RetrieveReadOnlySpan());
}
