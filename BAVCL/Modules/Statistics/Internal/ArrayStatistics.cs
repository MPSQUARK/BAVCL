using System;

namespace BAVCL.Modules.Statistics;

internal static class ArrayStatistics
{
	internal static float Min(float[] array)
	{
		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		float min = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (min > array[i])
				min = array[i];
		}

		return min;
	}

	internal static float Min(float[] array, bool includeInfinity)
	{
		if (includeInfinity) return Min(array);

		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		float min = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (float.IsInfinity(array[i])) continue;
			if (min > array[i])
				min = array[i];
		}

		return min;
	}

	internal static double Min(double[] array)
	{
		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		double min = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (min > array[i])
				min = array[i];
		}

		return min;
	}

	internal static double Min(double[] array, bool includeInfinity)
	{
		if (includeInfinity) return Min(array);

		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		double min = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (double.IsInfinity(array[i])) continue;
			if (min > array[i])
				min = array[i];
		}

		return min;
	}

	internal static int Min(int[] array)
	{
		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		int min = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (min > array[i])
				min = array[i];
		}

		return min;
	}

	internal static uint Min(uint[] array)
	{
		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		uint min = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (min > array[i])
				min = array[i];
		}

		return min;
	}

	internal static long Min(long[] array)
	{
		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		long min = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (min > array[i])
				min = array[i];
		}

		return min;
	}

	internal static ulong Min(ulong[] array)
	{
		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		ulong min = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (min > array[i])
				min = array[i];
		}

		return min;
	}

	internal static byte Min(byte[] array)
	{
		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		byte min = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (min > array[i])
				min = array[i];
		}

		return min;
	}

	internal static sbyte Min(sbyte[] array)
	{
		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		sbyte min = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (min > array[i])
				min = array[i];
		}

		return min;
	}

	internal static short Min(short[] array)
	{
		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		short min = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (min > array[i])
				min = array[i];
		}

		return min;
	}

	internal static float Max(float[] array)
	{
		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		float max = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (max < array[i])
				max = array[i];
		}

		return max;
	}

	internal static float Max(float[] array, bool ignoreInf)
	{
		if (!ignoreInf) return Max(array);

		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		float max = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (float.IsInfinity(array[i])) continue;
			if (max < array[i])
				max = array[i];
		}

		return max;
	}

	internal static double Max(double[] array)
	{
		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		double max = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (max < array[i])
				max = array[i];
		}

		return max;
	}

	internal static double Max(double[] array, bool ignoreInf)
	{
		if (!ignoreInf) return Max(array);

		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		double max = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (double.IsInfinity(array[i])) continue;
			if (max < array[i])
				max = array[i];
		}

		return max;
	}

	internal static int Max(int[] array)
	{
		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		int max = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (max < array[i])
				max = array[i];
		}

		return max;
	}

	internal static int Max(int[] array, bool ignoreInf)
	{
		if (!ignoreInf) return Max(array);

		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		int max = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (float.IsInfinity(array[i])) continue;
			if (max < array[i])
				max = array[i];
		}

		return max;
	}

	internal static long Max(long[] array)
	{
		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		long max = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (max < array[i])
				max = array[i];
		}

		return max;
	}

	internal static long Max(long[] array, bool ignoreInf)
	{
		if (!ignoreInf) return Max(array);

		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		long max = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (float.IsInfinity(array[i])) continue;
			if (max < array[i])
				max = array[i];
		}

		return max;
	}

	internal static byte Max(byte[] array)
	{
		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		byte max = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (max < array[i])
				max = array[i];
		}

		return max;
	}

	internal static byte Max(byte[] array, bool ignoreInf)
	{
		if (!ignoreInf) return Max(array);

		if (array.Length == 0) throw new Exception("Cannot Be Length 0");

		byte max = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (float.IsInfinity(array[i])) continue;
			if (max < array[i])
				max = array[i];
		}

		return max;
	}

	internal static float Average(float[] array) => array.Sum() / (float)array.Length;

	internal static double Average(double[] array) => array.Sum() / (double)array.Length;

	internal static float Average(int[] array) => array.Sum() / (float)array.Length;

	internal static float Average(long[] array) => array.Sum() / (float)array.Length;
}
