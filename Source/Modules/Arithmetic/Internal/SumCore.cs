using System;
using NumericVectorF = System.Numerics.Vector<float>;
using NumericVectorD = System.Numerics.Vector<double>;

namespace BAVCL.Modules.Arithmetic;

internal static class SumCore
{
	internal static float Sum(float[] array) =>
		array.Length >= 1e4 ? CorrectingSum(array) : SimpleSum(array);

	internal static double Sum(double[] array) => SimpleSum(array);

	internal static float Sum(Vector vector) => SumVector(vector);

	internal static float Sum(int[] array)
	{
		long total = 0;
		for (int i = 0; i < array.Length; i++)
			total += array[i];
		return total;
	}

	internal static float Sum(long[] array)
	{
		long total = 0;
		for (int i = 0; i < array.Length; i++)
			total += array[i];
		return total;
	}

	internal static float Sum(BAVCL.Geometric.Vector3 vector3) => Sum(vector3.ToArray());

	internal static float Sum(VectorInt vector) => Sum(vector.RetrieveReadOnlySpan());

	static float Sum(ReadOnlySpan<int> data)
	{
		long total = 0;
		for (int i = 0; i < data.Length; i++)
			total += data[i];
		return total;
	}

	static float SimpleSum(float[] arr)
	{
		NumericVectorF sumVector = NumericVectorF.Zero;
		int i = 0;
		int vectorSize = NumericVectorF.Count;

		for (; i <= arr.Length - vectorSize; i += vectorSize)
			sumVector += new NumericVectorF(arr, i);

		float result = 0;
		for (; i < arr.Length; i++)
			result += arr[i];
		for (int j = 0; j < vectorSize; j++)
			result += sumVector[j];

		return result;
	}

	static float CorrectingSum(float[] arr)
	{
		NumericVectorF sumVector = NumericVectorF.Zero;
		NumericVectorF c = NumericVectorF.Zero;
		int i = 0;
		int vectorSize = NumericVectorF.Count;

		for (; i <= arr.Length - vectorSize; i += vectorSize)
		{
			NumericVectorF input = new(arr, i);
			NumericVectorF y = input - c;
			NumericVectorF t = sumVector + y;
			c = (t - sumVector) - y;
			sumVector = t;
		}

		float result = 0;
		for (; i < arr.Length; i++)
			result += arr[i];
		for (int j = 0; j < vectorSize; j++)
			result += sumVector[j];

		return result;
	}

	static double SimpleSum(double[] arr)
	{
		NumericVectorD sumVector = NumericVectorD.Zero;
		int i = 0;
		int vectorSize = NumericVectorD.Count;

		for (; i <= arr.Length - vectorSize; i += vectorSize)
			sumVector += new NumericVectorD(arr, i);

		double result = 0;
		for (; i < arr.Length; i++)
			result += arr[i];
		for (int j = 0; j < vectorSize; j++)
			result += sumVector[j];

		return result;
	}

	static float SumVector(Vector vector)
	{
		ReadOnlySpan<float> data = vector.RetrieveReadOnlySpan();
		int vectorSize = NumericVectorF.Count;
		int i = 0;

		NumericVectorF sumVector = NumericVectorF.Zero;

		if (data.Length >= 10_000)
		{
			NumericVectorF c = NumericVectorF.Zero;
			for (; i <= data.Length - vectorSize; i += vectorSize)
			{
				NumericVectorF input = new(data.Slice(i, vectorSize));
				NumericVectorF y = input - c;
				NumericVectorF t = sumVector + y;
				c = (t - sumVector) - y;
				sumVector = t;
			}
		}
		else
		{
			for (; i <= data.Length - vectorSize; i += vectorSize)
			{
				NumericVectorF v = new(data.Slice(i, vectorSize));
				sumVector += v;
			}
		}

		float result = 0;
		for (int j = 0; j < vectorSize; j++)
			result += sumVector[j];

		for (; i < data.Length; i++)
			result += data[i];

		return result;
	}
}
