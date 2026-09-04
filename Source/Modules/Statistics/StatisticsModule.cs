namespace BAVCL.Modules.Statistics;
using BAVCL.Types;

public static class VectorStatistics
{
	extension(float[])
	{
		public static float Min(float[] array) => ArrayStatistics.Min(array);

		public static float Min(float[] array, bool includeInfinity) =>
			ArrayStatistics.Min(array, includeInfinity);

		public static float Max(float[] array) => ArrayStatistics.Max(array);

		public static float Max(float[] array, bool ignoreInf) =>
			ArrayStatistics.Max(array, ignoreInf);

		public static float Average(float[] array) => ArrayStatistics.Average(array);
	}

	extension(double[])
	{
		public static double Min(double[] array) => ArrayStatistics.Min(array);

		public static double Min(double[] array, bool includeInfinity) =>
			ArrayStatistics.Min(array, includeInfinity);

		public static double Max(double[] array) => ArrayStatistics.Max(array);

		public static double Max(double[] array, bool ignoreInf) =>
			ArrayStatistics.Max(array, ignoreInf);

		public static double Average(double[] array) => ArrayStatistics.Average(array);
	}

	extension(int[])
	{
		public static int Min(int[] array) => ArrayStatistics.Min(array);

		public static int Max(int[] array) => ArrayStatistics.Max(array);

		public static int Max(int[] array, bool ignoreInf) =>
			ArrayStatistics.Max(array, ignoreInf);

		public static float Average(int[] array) => ArrayStatistics.Average(array);
	}

	extension(uint[])
	{
		public static uint Min(uint[] array) => ArrayStatistics.Min(array);
	}

	extension(long[])
	{
		public static long Min(long[] array) => ArrayStatistics.Min(array);

		public static long Max(long[] array) => ArrayStatistics.Max(array);

		public static long Max(long[] array, bool ignoreInf) =>
			ArrayStatistics.Max(array, ignoreInf);

		public static float Average(long[] array) => ArrayStatistics.Average(array);
	}

	extension(ulong[])
	{
		public static ulong Min(ulong[] array) => ArrayStatistics.Min(array);
	}

	extension(byte[])
	{
		public static byte Min(byte[] array) => ArrayStatistics.Min(array);

		public static byte Max(byte[] array) => ArrayStatistics.Max(array);

		public static byte Max(byte[] array, bool ignoreInf) =>
			ArrayStatistics.Max(array, ignoreInf);
	}

	extension(sbyte[])
	{
		public static sbyte Min(sbyte[] array) => ArrayStatistics.Min(array);
	}

	extension(short[])
	{
		public static short Min(short[] array) => ArrayStatistics.Min(array);
	}

	extension(Vector)
	{
		public static float Mean(Vector vector) => DescriptiveStatistics.Mean(vector);

		public static float Var(Vector vector) => DescriptiveStatistics.Var(vector);

		public static float Std(Vector vector) => DescriptiveStatistics.Std(vector);

		public static float Min(Vector vector) => DescriptiveStatistics.Min(vector);

		public static float Max(Vector vector) => DescriptiveStatistics.Max(vector);

		public static MinMax<float> MinMax(Vector vector) => DescriptiveStatistics.MinMax(vector);

		public static float Range(Vector vector) => DescriptiveStatistics.Range(vector);

		public static bool All(Vector vector) => DescriptiveStatistics.All(vector);

		public static float MeanX(Vector vector) =>
			GpuDescriptiveStatistics.MeanX(vector);

		public static float VarX(Vector vector) =>
			GpuDescriptiveStatistics.VarX(vector);

		public static float StdX(Vector vector) =>
			GpuDescriptiveStatistics.StdX(vector);

		public static float MinX(Vector vector) => GpuDescriptiveStatistics.MinX(vector);

		public static float MaxX(Vector vector) => GpuDescriptiveStatistics.MaxX(vector);

		public static MinMax<float> MinMaxX(Vector vector) => GpuDescriptiveStatistics.MinMaxX(vector);

		public static float RangeX(Vector vector) => GpuDescriptiveStatistics.RangeX(vector);

		public static bool AllX(Vector vector) => GpuDescriptiveStatistics.AllX(vector);

		public static float Percentile(Vector vector, float percentile) =>
			OrderStatistics.Percentile(vector, percentile);

		public static float Median(Vector vector) => OrderStatistics.Median(vector);

		public static float Quartile1(Vector vector) => OrderStatistics.Quartile1(vector);

		public static float Quartile3(Vector vector) => OrderStatistics.Quartile3(vector);

		public static float Iqr(Vector vector) => OrderStatistics.Iqr(vector);

		public static float PercentileX(Vector vector, float percentile) =>
			GpuOrderStatistics.PercentileX(vector, percentile);

		public static float MedianX(Vector vector) => GpuOrderStatistics.MedianX(vector);

		public static float Quartile1X(Vector vector) => GpuOrderStatistics.Quartile1X(vector);

		public static float Quartile3X(Vector vector) => GpuOrderStatistics.Quartile3X(vector);

		public static float IqrX(Vector vector) => GpuOrderStatistics.IqrX(vector);

		public static Vector ReduceOPX(Vector vector, Vector matrix, Operations operation) =>
			ReduceCore.ReduceOPX(vector, matrix, operation);
	}

	extension(BAVCL.Geometric.Vector3)
	{
		public static float Mean(BAVCL.Geometric.Vector3 vector3) =>
			DescriptiveStatistics.Mean(vector3);

		public static float Min(BAVCL.Geometric.Vector3 vector3) =>
			DescriptiveStatistics.Min(vector3);

		public static float Max(BAVCL.Geometric.Vector3 vector3) =>
			DescriptiveStatistics.Max(vector3);

		public static float Range(BAVCL.Geometric.Vector3 vector3) =>
			DescriptiveStatistics.Range(vector3);
	}

	extension(VectorInt)
	{
		public static float Mean(VectorInt vector) => DescriptiveStatistics.Mean(vector);

		public static float Var(VectorInt vector) => DescriptiveStatistics.Var(vector);

		public static float Std(VectorInt vector) => DescriptiveStatistics.Std(vector);

		public static int Min(VectorInt vector) => DescriptiveStatistics.Min(vector);

		public static int Max(VectorInt vector) => DescriptiveStatistics.Max(vector);

		public static MinMax<int> MinMax(VectorInt vector) => DescriptiveStatistics.MinMax(vector);

		public static int Range(VectorInt vector) => DescriptiveStatistics.Range(vector);

		public static bool All(VectorInt vector) => DescriptiveStatistics.All(vector);

		public static float MeanX(VectorInt vector) => GpuDescriptiveStatistics.MeanX(vector);

		public static float VarX(VectorInt vector) => GpuDescriptiveStatistics.VarX(vector);

		public static float StdX(VectorInt vector) => GpuDescriptiveStatistics.StdX(vector);

		public static int MinX(VectorInt vector) => GpuDescriptiveStatistics.MinX(vector);

		public static int MaxX(VectorInt vector) => GpuDescriptiveStatistics.MaxX(vector);

		public static MinMax<int> MinMaxX(VectorInt vector) => GpuDescriptiveStatistics.MinMaxX(vector);

		public static int RangeX(VectorInt vector) => GpuDescriptiveStatistics.RangeX(vector);

		public static bool AllX(VectorInt vector) => GpuDescriptiveStatistics.AllX(vector);

		public static float Percentile(VectorInt vector, float percentile) =>
			OrderStatistics.Percentile(vector, percentile);

		public static float Median(VectorInt vector) => OrderStatistics.Median(vector);

		public static float Quartile1(VectorInt vector) => OrderStatistics.Quartile1(vector);

		public static float Quartile3(VectorInt vector) => OrderStatistics.Quartile3(vector);

		public static float Iqr(VectorInt vector) => OrderStatistics.Iqr(vector);

		public static float PercentileX(VectorInt vector, float percentile) =>
			GpuOrderStatistics.PercentileX(vector, percentile);

		public static float MedianX(VectorInt vector) => GpuOrderStatistics.MedianX(vector);

		public static float Quartile1X(VectorInt vector) => GpuOrderStatistics.Quartile1X(vector);

		public static float Quartile3X(VectorInt vector) => GpuOrderStatistics.Quartile3X(vector);

		public static float IqrX(VectorInt vector) => GpuOrderStatistics.IqrX(vector);

		public static VectorInt ReduceOPX(VectorInt vector, VectorInt matrix, Operations operation) =>
			ReduceCore.ReduceOPX(vector, matrix, operation);
	}
}

public static class StatisticsModule
{
	extension(float[] array)
	{
		public float Min() => VectorStatistics.Min(array);

		public float Min(bool includeInfinity) => VectorStatistics.Min(array, includeInfinity);

		public float Max() => VectorStatistics.Max(array);

		public float Max(bool ignoreInf) => VectorStatistics.Max(array, ignoreInf);

		public float Average() => VectorStatistics.Average(array);
	}

	extension(double[] array)
	{
		public double Min() => VectorStatistics.Min(array);

		public double Min(bool includeInfinity) => VectorStatistics.Min(array, includeInfinity);

		public double Max() => VectorStatistics.Max(array);

		public double Max(bool ignoreInf) => VectorStatistics.Max(array, ignoreInf);

		public double Average() => VectorStatistics.Average(array);
	}

	extension(int[] array)
	{
		public int Min() => VectorStatistics.Min(array);

		public int Max() => VectorStatistics.Max(array);

		public int Max(bool ignoreInf) => VectorStatistics.Max(array, ignoreInf);

		public float Average() => VectorStatistics.Average(array);
	}

	extension(uint[] array)
	{
		public uint Min() => VectorStatistics.Min(array);
	}

	extension(long[] array)
	{
		public long Min() => VectorStatistics.Min(array);

		public long Max() => VectorStatistics.Max(array);

		public long Max(bool ignoreInf) => VectorStatistics.Max(array, ignoreInf);

		public float Average() => VectorStatistics.Average(array);
	}

	extension(ulong[] array)
	{
		public ulong Min() => VectorStatistics.Min(array);
	}

	extension(byte[] array)
	{
		public byte Min() => VectorStatistics.Min(array);

		public byte Max() => VectorStatistics.Max(array);

		public byte Max(bool ignoreInf) => VectorStatistics.Max(array, ignoreInf);
	}

	extension(sbyte[] array)
	{
		public sbyte Min() => VectorStatistics.Min(array);
	}

	extension(short[] array)
	{
		public short Min() => VectorStatistics.Min(array);
	}

	extension(Vector vector)
	{
		public float Mean() => VectorStatistics.Mean(vector);

		public float Var() => VectorStatistics.Var(vector);

		public float Std() => VectorStatistics.Std(vector);

		public float Min() => VectorStatistics.Min(vector);

		public float Max() => VectorStatistics.Max(vector);

		public MinMax<float> MinMax() => VectorStatistics.MinMax(vector);

		public float Range() => VectorStatistics.Range(vector);

		public bool All() => VectorStatistics.All(vector);

		public float MeanX() => VectorStatistics.MeanX(vector);

		public float VarX() => VectorStatistics.VarX(vector);

		public float StdX() => VectorStatistics.StdX(vector);

		public float MinX() => VectorStatistics.MinX(vector);

		public float MaxX() => VectorStatistics.MaxX(vector);

		public MinMax<float> MinMaxX() => VectorStatistics.MinMaxX(vector);

		public float RangeX() => VectorStatistics.RangeX(vector);

		public bool AllX() => VectorStatistics.AllX(vector);

		public float Percentile(float percentile) =>
			VectorStatistics.Percentile(vector, percentile);

		public float Median() => VectorStatistics.Median(vector);

		public float Quartile1() => VectorStatistics.Quartile1(vector);

		public float Quartile3() => VectorStatistics.Quartile3(vector);

		public float Iqr() => VectorStatistics.Iqr(vector);

		public float PercentileX(float percentile) =>
			VectorStatistics.PercentileX(vector, percentile);

		public float MedianX() => VectorStatistics.MedianX(vector);

		public float Quartile1X() => VectorStatistics.Quartile1X(vector);

		public float Quartile3X() => VectorStatistics.Quartile3X(vector);

		public float IqrX() => VectorStatistics.IqrX(vector);

		public Vector ReduceOPX(Vector matrix, Operations operation) =>
			VectorStatistics.ReduceOPX(vector, matrix, operation);
	}

	extension(BAVCL.Geometric.Vector3 vector3)
	{
		public float Mean() => VectorStatistics.Mean(vector3);

		public float Min() => VectorStatistics.Min(vector3);

		public float Max() => VectorStatistics.Max(vector3);

		public float Range() => VectorStatistics.Range(vector3);
	}

	extension(VectorInt vector)
	{
		public float Mean() => VectorStatistics.Mean(vector);

		public float Var() => VectorStatistics.Var(vector);

		public float Std() => VectorStatistics.Std(vector);

		public int Min() => VectorStatistics.Min(vector);

		public int Max() => VectorStatistics.Max(vector);

		public MinMax<int> MinMax() => VectorStatistics.MinMax(vector);

		public int Range() => VectorStatistics.Range(vector);

		public bool All() => VectorStatistics.All(vector);

		public float MeanX() => VectorStatistics.MeanX(vector);

		public float VarX() => VectorStatistics.VarX(vector);

		public float StdX() => VectorStatistics.StdX(vector);

		public int MinX() => VectorStatistics.MinX(vector);

		public int MaxX() => VectorStatistics.MaxX(vector);

		public MinMax<int> MinMaxX() => VectorStatistics.MinMaxX(vector);

		public int RangeX() => VectorStatistics.RangeX(vector);

		public bool AllX() => VectorStatistics.AllX(vector);

		public float Percentile(float percentile) =>
			VectorStatistics.Percentile(vector, percentile);

		public float Median() => VectorStatistics.Median(vector);

		public float Quartile1() => VectorStatistics.Quartile1(vector);

		public float Quartile3() => VectorStatistics.Quartile3(vector);

		public float Iqr() => VectorStatistics.Iqr(vector);

		public float PercentileX(float percentile) =>
			VectorStatistics.PercentileX(vector, percentile);

		public float MedianX() => VectorStatistics.MedianX(vector);

		public float Quartile1X() => VectorStatistics.Quartile1X(vector);

		public float Quartile3X() => VectorStatistics.Quartile3X(vector);

		public float IqrX() => VectorStatistics.IqrX(vector);

		public VectorInt ReduceOPX(VectorInt matrix, Operations operation) =>
			VectorStatistics.ReduceOPX(vector, matrix, operation);
	}
}
