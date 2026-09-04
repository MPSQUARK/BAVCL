using System;
using System.Numerics;
using BAVCL.Core.Helpers;
using BAVCL.Core.Helpers.Numerics;

namespace BAVCL.Modules.Statistics;

internal static class CpuSimdReduce
{
	internal static float MinFloat(ReadOnlySpan<float> data)
	{
		Guard.IsNotEmpty(data.Length);

		int vectorSize = Vector<float>.Count;
		int i = 0;
		Vector<float> minVec = new(float.PositiveInfinity);

		for (; i <= data.Length - vectorSize; i += vectorSize)
		{
			Vector<float> input = new(data.Slice(i, vectorSize));
			minVec = System.Numerics.Vector.Min(minVec, input);
		}

		float min = HorizontalMin(minVec);
		for (; i < data.Length; i++)
			min = MathF.Min(min, data[i]);

		return min;
	}

	internal static float MaxFloat(ReadOnlySpan<float> data)
	{
		Guard.IsNotEmpty(data.Length);

		int vectorSize = Vector<float>.Count;
		int i = 0;
		Vector<float> maxVec = new(float.NegativeInfinity);

		for (; i <= data.Length - vectorSize; i += vectorSize)
		{
			Vector<float> input = new(data.Slice(i, vectorSize));
			maxVec = System.Numerics.Vector.Max(maxVec, input);
		}

		float max = HorizontalMax(maxVec);
		for (; i < data.Length; i++)
			max = MathF.Max(max, data[i]);

		return max;
	}

	internal static float SumFloat(ReadOnlySpan<float> data) =>
		CompensatedSumOps.SumNeumaier(data);

	internal static float DotFloat(ReadOnlySpan<float> left, ReadOnlySpan<float> right) =>
		CompensatedSumOps.DotNeumaier(left, right);

	internal static float DotFloat(ReadOnlySpan<float> vector, float scalar) =>
		scalar * SumFloat(vector);

	internal static float VarFloat(ReadOnlySpan<float> data) =>
		VarianceAccumOps.PopulationVariance(data);

	internal static float SampleVarFloat(ReadOnlySpan<float> data) =>
		VarianceAccumOps.SampleVariance(data);

	internal static bool AllFloat(ReadOnlySpan<float> data)
	{
		int vectorSize = Vector<float>.Count;
		int i = 0;
		Vector<float> zero = Vector<float>.Zero;

		for (; i <= data.Length - vectorSize; i += vectorSize)
		{
			Vector<float> input = new(data.Slice(i, vectorSize));
			if (System.Numerics.Vector.EqualsAny(input, zero))
				return false;
		}

		for (; i < data.Length; i++)
		{
			if (data[i] == 0f)
				return false;
		}

		return true;
	}

	internal static Types.MinMax<float> MinMaxFloat(ReadOnlySpan<float> data)
	{
		Guard.IsNotEmpty(data.Length);

		int vectorSize = Vector<float>.Count;
		int i = 0;
		Vector<float> minVec = new(float.PositiveInfinity);
		Vector<float> maxVec = new(float.NegativeInfinity);

		for (; i <= data.Length - vectorSize; i += vectorSize)
		{
			Vector<float> input = new(data.Slice(i, vectorSize));
			minVec = System.Numerics.Vector.Min(minVec, input);
			maxVec = System.Numerics.Vector.Max(maxVec, input);
		}

		float min = HorizontalMin(minVec);
		float max = HorizontalMax(maxVec);
		for (; i < data.Length; i++)
		{
			min = MathF.Min(min, data[i]);
			max = MathF.Max(max, data[i]);
		}

		return new Types.MinMax<float>(min, max);
	}

	internal static Types.MinMax<int> MinMaxInt(ReadOnlySpan<int> data)
	{
		Guard.IsNotEmpty(data.Length);

		int vectorSize = Vector<int>.Count;
		int i = 0;
		Vector<int> minVec = new(int.MaxValue);
		Vector<int> maxVec = new(int.MinValue);

		for (; i <= data.Length - vectorSize; i += vectorSize)
		{
			Vector<int> input = new(data.Slice(i, vectorSize));
			minVec = System.Numerics.Vector.Min(minVec, input);
			maxVec = System.Numerics.Vector.Max(maxVec, input);
		}

		int min = HorizontalMin(minVec);
		int max = HorizontalMax(maxVec);
		for (; i < data.Length; i++)
		{
			min = Math.Min(min, data[i]);
			max = Math.Max(max, data[i]);
		}

		return new Types.MinMax<int>(min, max);
	}

	internal static int MinInt(ReadOnlySpan<int> data)
	{
		Guard.IsNotEmpty(data.Length);

		int vectorSize = Vector<int>.Count;
		int i = 0;
		Vector<int> minVec = new(int.MaxValue);

		for (; i <= data.Length - vectorSize; i += vectorSize)
		{
			Vector<int> input = new(data.Slice(i, vectorSize));
			minVec = System.Numerics.Vector.Min(minVec, input);
		}

		int min = HorizontalMin(minVec);
		for (; i < data.Length; i++)
			min = Math.Min(min, data[i]);

		return min;
	}

	internal static int MaxInt(ReadOnlySpan<int> data)
	{
		Guard.IsNotEmpty(data.Length);

		int vectorSize = Vector<int>.Count;
		int i = 0;
		Vector<int> maxVec = new(int.MinValue);

		for (; i <= data.Length - vectorSize; i += vectorSize)
		{
			Vector<int> input = new(data.Slice(i, vectorSize));
			maxVec = System.Numerics.Vector.Max(maxVec, input);
		}

		int max = HorizontalMax(maxVec);
		for (; i < data.Length; i++)
			max = Math.Max(max, data[i]);

		return max;
	}

	internal static float SumInt(ReadOnlySpan<int> data)
	{
		int total = 0;
		int vectorSize = Vector<int>.Count;
		int i = 0;

		for (; i <= data.Length - vectorSize; i += vectorSize)
		{
			Vector<int> input = new(data.Slice(i, vectorSize));
			for (int lane = 0; lane < vectorSize; lane++)
				total += input[lane];
		}

		for (; i < data.Length; i++)
			total += data[i];

		return total;
	}

	internal static float VarInt(ReadOnlySpan<int> data) =>
		VarianceAccumOps.PopulationVariance(data);

	internal static float SampleVarInt(ReadOnlySpan<int> data) =>
		VarianceAccumOps.SampleVariance(data);

	internal static float DotInt(ReadOnlySpan<int> left, ReadOnlySpan<int> right)
	{
		int total = 0;
		int vectorSize = Vector<int>.Count;
		int i = 0;

		for (; i <= left.Length - vectorSize; i += vectorSize)
		{
			Vector<int> a = new(left.Slice(i, vectorSize));
			Vector<int> b = new(right.Slice(i, vectorSize));
			for (int lane = 0; lane < vectorSize; lane++)
				total += a[lane] * b[lane];
		}

		for (; i < left.Length; i++)
			total += left[i] * right[i];

		return total;
	}

	internal static float DotInt(ReadOnlySpan<int> vector, int scalar) =>
		scalar * SumInt(vector);

	internal static bool AllInt(ReadOnlySpan<int> data)
	{
		int vectorSize = Vector<int>.Count;
		int i = 0;
		Vector<int> zero = Vector<int>.Zero;

		for (; i <= data.Length - vectorSize; i += vectorSize)
		{
			Vector<int> input = new(data.Slice(i, vectorSize));
			if (System.Numerics.Vector.EqualsAny(input, zero))
				return false;
		}

		for (; i < data.Length; i++)
		{
			if (data[i] == 0)
				return false;
		}

		return true;
	}

	internal static float[] RowReduceFloatCompensated(
		ReadOnlySpan<float> coeffs,
		ReadOnlySpan<float> matrix,
		int rows,
		int cols,
		Operations operation)
	{
		float[] output = new float[rows];
		for (int row = 0; row < rows; row++)
		{
			int rowStart = row * cols;
			Float64NeumaierState acc = default;
			for (int i = 0; i < cols; i++)
			{
				float term = ReduceRowOps.ElementFloat(coeffs[i], matrix[rowStart + i], operation);
				acc.Add(term);
			}

			float total = acc.ToSingle();
			output[row] = ReduceRowOps.UsesDistance(operation) ? MathF.Sqrt(total) : total;
		}

		return output;
	}

	static float HorizontalMin(Vector<float> vector)
	{
		float result = vector[0];
		for (int lane = 1; lane < Vector<float>.Count; lane++)
			result = MathF.Min(result, vector[lane]);
		return result;
	}

	static float HorizontalMax(Vector<float> vector)
	{
		float result = vector[0];
		for (int lane = 1; lane < Vector<float>.Count; lane++)
			result = MathF.Max(result, vector[lane]);
		return result;
	}

	static int HorizontalMin(Vector<int> vector)
	{
		int result = vector[0];
		for (int lane = 1; lane < Vector<int>.Count; lane++)
			result = Math.Min(result, vector[lane]);
		return result;
	}

	static int HorizontalMax(Vector<int> vector)
	{
		int result = vector[0];
		for (int lane = 1; lane < Vector<int>.Count; lane++)
			result = Math.Max(result, vector[lane]);
		return result;
	}
}
