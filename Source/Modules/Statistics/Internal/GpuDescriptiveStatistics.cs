using System;
using BAVCL.Core.Helpers;
using BAVCL.GpuAlgorithms;
using BAVCL.Types;

namespace BAVCL.Modules.Statistics;

internal static class GpuDescriptiveStatistics
{
	internal static float MeanX(Vector vector)
	{
		Guard.IsNotZero(vector.Length);
		return GlobalReduceAlgorithms.Sum(vector) / vector.Length;
	}

	internal static float MeanX(VectorInt vector)
	{
		Guard.IsNotZero(vector.Length);
		return GlobalReduceAlgorithms.Sum(vector) / vector.Length;
	}

	internal static float VarX(Vector vector) =>
		GlobalReduceAlgorithms.Var(vector);

	internal static float SampleVarX(Vector vector) =>
		GlobalReduceAlgorithms.SampleVar(vector);

	internal static float VarX(VectorInt vector) =>
		GlobalReduceAlgorithms.Var(vector);

	internal static float SampleVarX(VectorInt vector) =>
		GlobalReduceAlgorithms.SampleVar(vector);

	internal static float StdX(Vector vector) => MathF.Sqrt(VarX(vector));

	internal static float SampleStdX(Vector vector) => MathF.Sqrt(SampleVarX(vector));

	internal static float StdX(VectorInt vector) => MathF.Sqrt(VarX(vector));

	internal static float SampleStdX(VectorInt vector) => MathF.Sqrt(SampleVarX(vector));

	internal static float MinX(Vector vector) => GlobalReduceAlgorithms.Min(vector);

	internal static int MinX(VectorInt vector) => GlobalReduceAlgorithms.Min(vector);

	internal static float MaxX(Vector vector) => GlobalReduceAlgorithms.Max(vector);

	internal static int MaxX(VectorInt vector) => GlobalReduceAlgorithms.Max(vector);

	internal static MinMax<float> MinMaxX(Vector vector) => GlobalReduceAlgorithms.MinMaxX(vector);

	internal static MinMax<int> MinMaxX(VectorInt vector) => GlobalReduceAlgorithms.MinMaxX(vector);

	internal static float RangeX(Vector vector) => GlobalReduceAlgorithms.RangeX(vector);

	internal static int RangeX(VectorInt vector) => GlobalReduceAlgorithms.RangeX(vector);

	internal static bool AllX(Vector vector) => GlobalReduceAlgorithms.All(vector);

	internal static bool AllX(VectorInt vector) => GlobalReduceAlgorithms.All(vector);
}
