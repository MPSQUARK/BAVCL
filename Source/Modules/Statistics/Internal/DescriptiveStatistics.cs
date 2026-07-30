using System;
using System.Numerics;
using ILGPU.Algorithms;
using BAVCL.Modules.GpuOps;
using BAVCL.Modules.Arithmetic;

namespace BAVCL.Modules.Statistics;

internal static class DescriptiveStatistics
{
	internal static float Mean(Vector vector) => vector.Sum() / vector.Length;

	internal static float Mean(BAVCL.Geometric.Vector3 vector3) =>
		vector3.ToArray().Sum() / vector3.Length;

	internal static float Var(Vector vector)
	{
		if (vector.Length < 10000)
		{
			int vectorSize = Vector<float>.Count;
			int i = 0;

			ReadOnlySpan<float> data = vector.RetrieveReadOnlySpan();
			float mean = Mean(vector);

			Vector<float> meanvec = new(mean);
			Vector<float> sumVector = Vector<float>.Zero;

			for (; i <= data.Length - vectorSize; i += vectorSize)
			{
				Vector<float> input = new(data.Slice(i, vectorSize));
				Vector<float> difference = input - meanvec;
				sumVector += difference * difference;
			}

			float sum = 0;
			for (int j = 0; j < vectorSize; j++)
				sum += sumVector[j];

			for (; i < data.Length; i++)
				sum += XMath.Pow(data[i] - mean, 2f);

			return sum / vector.Length;
		}

		return vector.OP(Mean(vector), Operations.differenceSquared).Sum() / vector.Length;
	}

	internal static float Std(Vector vector) => XMath.Sqrt(Var(vector));

	internal static float Min(Vector vector)
	{
		ReadOnlySpan<float> data = vector.RetrieveReadOnlySpan();
		if (data.Length == 0) throw new Exception("Cannot Be Length 0");

		float min = data[0];
		for (int i = 1; i < data.Length; i++)
		{
			if (min > data[i])
				min = data[i];
		}

		return min;
	}

	internal static float Max(Vector vector)
	{
		ReadOnlySpan<float> data = vector.RetrieveReadOnlySpan();
		if (data.Length == 0) throw new Exception("Cannot Be Length 0");

		float max = data[0];
		for (int i = 1; i < data.Length; i++)
		{
			if (max < data[i])
				max = data[i];
		}

		return max;
	}

	internal static float Min(BAVCL.Geometric.Vector3 vector3) => ArrayStatistics.Min(vector3.ToArray());

	internal static float Max(BAVCL.Geometric.Vector3 vector3) => ArrayStatistics.Max(vector3.ToArray());

	internal static float Range(Vector vector) => Max(vector) - Min(vector);

	internal static float Range(BAVCL.Geometric.Vector3 vector3) => Max(vector3) - Min(vector3);

	internal static bool All(Vector vector)
	{
		ReadOnlySpan<float> data = vector.RetrieveReadOnlySpan();
		for (int i = 0; i < data.Length; i++)
		{
			if (data[i] == 0f)
				return false;
		}

		return true;
	}
}
