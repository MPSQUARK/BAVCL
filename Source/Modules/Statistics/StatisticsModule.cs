namespace BAVCL.Modules.Statistics;

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

		public static float Range(Vector vector) => DescriptiveStatistics.Range(vector);

		public static bool All(Vector vector) => DescriptiveStatistics.All(vector);

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

		public static int Range(VectorInt vector) => DescriptiveStatistics.Range(vector);

		public static bool All(VectorInt vector) => DescriptiveStatistics.All(vector);

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

		public float Range() => VectorStatistics.Range(vector);

		public bool All() => VectorStatistics.All(vector);

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

		public int Range() => VectorStatistics.Range(vector);

		public bool All() => VectorStatistics.All(vector);

		public VectorInt ReduceOPX(VectorInt matrix, Operations operation) =>
			VectorStatistics.ReduceOPX(vector, matrix, operation);
	}
}
