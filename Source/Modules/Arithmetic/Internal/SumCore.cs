using System;
using NumericVectorD = System.Numerics.Vector<double>;
using BAVCL.Core.Helpers;
using BAVCL.Modules.Statistics;

namespace BAVCL.Modules.Arithmetic;

internal static class SumCore
{
	internal static float Sum(float[] array) => CompensatedSumOps.SumNeumaier(array);

	internal static double Sum(double[] array) => SimpleSum(array);

	internal static float Sum(Vector vector) =>
		CompensatedSumOps.SumNeumaier(vector.RetrieveReadOnlySpan());

	internal static float Sum(int[] array)
	{
		int total = 0;
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

	static float Sum(ReadOnlySpan<int> data) => CpuSimdReduce.SumInt(data);

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
}
