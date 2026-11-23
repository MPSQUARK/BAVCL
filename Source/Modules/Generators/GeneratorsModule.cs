namespace BAVCL.Modules.Generators;

public static class GeneratorsModule
{
	/// <summary>Samples <c>start + i * interval</c>, <c>ceil((end - start) / interval)</c> of them. A range that ends below its start travels by the magnitude of the interval.</summary>
	public static float[] Arange(float startValue, float endValue, float interval) =>
		GeneratorsCore.Arange(startValue, endValue, interval);

	public static double[] Arange(double startValue, double endValue, double interval) =>
		GeneratorsCore.Arange(startValue, endValue, interval);

	public static int[] Arange(int startValue, int endValue, int interval) =>
		GeneratorsCore.Arange(startValue, endValue, interval);

	/// <summary>Evenly spaced samples from start to end inclusive. The endpoints are exact.</summary>
	public static float[] Linspace(float startValue, float endValue, int num) =>
		GeneratorsCore.Linspace(startValue, endValue, num);

	public static double[] Linspace(double startValue, double endValue, int num) =>
		GeneratorsCore.Linspace(startValue, endValue, num);

	public static int[] Linspace(int startValue, int endValue, int num) =>
		GeneratorsCore.Linspace(startValue, endValue, num);
}

public static class GeneratorsModuleExtensions
{
	extension(float[])
	{
		public static float[] Arange(float startValue, float endValue, float interval) =>
			GeneratorsModule.Arange(startValue, endValue, interval);

		public static float[] Linspace(float startValue, float endValue, int num) =>
			GeneratorsModule.Linspace(startValue, endValue, num);
	}

	extension(int[])
	{
		public static int[] Arange(int startValue, int endValue, int interval) =>
			GeneratorsModule.Arange(startValue, endValue, interval);

		public static int[] Linspace(int startValue, int endValue, int num) =>
			GeneratorsModule.Linspace(startValue, endValue, num);
	}

	extension(double[])
	{
		public static double[] Arange(double startValue, double endValue, double interval) =>
			GeneratorsModule.Arange(startValue, endValue, interval);

		public static double[] Linspace(double startValue, double endValue, int num) =>
			GeneratorsModule.Linspace(startValue, endValue, num);
	}

	extension(Vector)
	{
		public static Vector Zeros(GPU gpu, int length, int columns = 0) =>
			FactoriesCore.Zeros(gpu, length, columns);

		public static Vector Ones(GPU gpu, int length, int columns = 0) =>
			FactoriesCore.Ones(gpu, length, columns);

		public static Vector Fill(GPU gpu, float value, int length, int columns = 0, bool cache = true) =>
			FactoriesCore.Fill(gpu, value, length, columns, cache);

		public static Vector Arange(GPU gpu, float startval, float endval, float interval, int columns = 0, bool cache = true) =>
			FactoriesCore.Arange(gpu, startval, endval, interval, columns, cache);

		public static Vector Linspace(GPU gpu, float startval, float endval, int steps, int columns = 0, bool cache = true) =>
			FactoriesCore.Linspace(gpu, startval, endval, steps, columns, cache);

		/// <summary>
		/// Uniform floats in [0, 1). A null seed draws a fresh seed on the host; a fixed seed reproduces the
		/// values. CPU and GPU variants use different generators, so one seed gives different values on each.
		/// </summary>
		public static Vector Random(GPU gpu, int length, int? seed = null, int columns = 0) =>
			FactoriesCore.Random(gpu, length, seed, columns);

		/// <summary>
		/// Uniform floats in [low, high). Throws unless both bounds are finite and low is less than high.
		/// </summary>
		public static Vector RandomUniform(GPU gpu, int length, float low, float high, int? seed = null, int columns = 0) =>
			FactoriesCore.RandomUniform(gpu, length, low, high, seed, columns);

		/// <summary>
		/// Normal values with the given mean and standard deviation. Throws unless mean and stdDev are finite
		/// and stdDev is non-negative.
		/// </summary>
		public static Vector RandomNormal(GPU gpu, int length, float mean = 0f, float stdDev = 1f, int? seed = null, int columns = 0) =>
			FactoriesCore.RandomNormal(gpu, length, mean, stdDev, seed, columns);

		public static Vector ZerosX(GPU gpu, int length, int columns = 0) =>
			GpuFactoriesCore.Zeros(gpu, length, columns);

		public static Vector OnesX(GPU gpu, int length, int columns = 0) =>
			GpuFactoriesCore.Ones(gpu, length, columns);

		public static Vector FillX(GPU gpu, float value, int length, int columns = 0) =>
			GpuFactoriesCore.Fill(gpu, value, length, columns);

		public static Vector ArangeX(GPU gpu, float startval, float endval, float interval, int columns = 0) =>
			GpuFactoriesCore.Arange(gpu, startval, endval, interval, columns);

		public static Vector LinspaceX(GPU gpu, float startval, float endval, int steps, int columns = 0) =>
			GpuFactoriesCore.Linspace(gpu, startval, endval, steps, columns);

		public static Vector RandomX(GPU gpu, int length, int? seed = null, int columns = 0) =>
			GpuFactoriesCore.Random(gpu, length, seed, columns);

		public static Vector RandomUniformX(GPU gpu, int length, float low, float high, int? seed = null, int columns = 0) =>
			GpuFactoriesCore.RandomUniform(gpu, length, low, high, seed, columns);

		public static Vector RandomNormalX(GPU gpu, int length, float mean = 0f, float stdDev = 1f, int? seed = null, int columns = 0) =>
			GpuFactoriesCore.RandomNormal(gpu, length, mean, stdDev, seed, columns);
	}

	extension(BAVCL.Geometric.Vector3)
	{
		public static BAVCL.Geometric.Vector3 Zeros(GPU gpu, int length) =>
			FactoriesCore.Zeros(gpu, length);

		public static BAVCL.Geometric.Vector3 Fill(GPU gpu, float value, int length) =>
			FactoriesCore.Fill(gpu, value, length);

		public static BAVCL.Geometric.Vector3 ZerosX(GPU gpu, int length) =>
			GpuFactoriesCore.Zeros(gpu, length);

		public static BAVCL.Geometric.Vector3 FillX(GPU gpu, float value, int length) =>
			GpuFactoriesCore.Fill(gpu, value, length);
	}

	extension(Vector vector)
	{
		public Vector ZerosIP() =>
			FactoriesCore.ZerosInPlace(vector);

		public Vector ZerosIPX() =>
			GpuFactoriesCore.ZerosInPlaceX(vector);

		public Vector OnesIP() =>
			FactoriesCore.OnesInPlace(vector);

		public Vector OnesIPX() =>
			GpuFactoriesCore.OnesInPlaceX(vector);

		public Vector FillIP(float value) =>
			FactoriesCore.FillInPlace(vector, value);

		public Vector FillIPX(float value) =>
			GpuFactoriesCore.FillInPlaceX(vector, value);
	}

	extension(VectorInt vector)
	{
		public VectorInt ZerosIP() =>
			FactoriesCore.ZerosInPlace(vector);

		public VectorInt ZerosIPX() =>
			GpuFactoriesCore.ZerosInPlaceX(vector);

		public VectorInt OnesIP() =>
			FactoriesCore.OnesInPlace(vector);

		public VectorInt OnesIPX() =>
			GpuFactoriesCore.OnesInPlaceX(vector);

		public VectorInt FillIP(int value) =>
			FactoriesCore.FillInPlace(vector, value);

		public VectorInt FillIPX(int value) =>
			GpuFactoriesCore.FillInPlaceX(vector, value);
	}
}

public static class VectorIntGeneratorsModuleExtensions
{
	extension(VectorInt)
	{
		public static VectorInt Zeros(GPU gpu, int length, int columns = 0) =>
			FactoriesCore.ZerosInt(gpu, length, columns);

		public static VectorInt Ones(GPU gpu, int length, int columns = 0) =>
			FactoriesCore.OnesInt(gpu, length, columns);

		public static VectorInt Fill(GPU gpu, int value, int length, int columns = 0, bool cache = true) =>
			FactoriesCore.Fill(gpu, value, length, columns, cache);

		public static VectorInt Arange(GPU gpu, int startval, int endval, int interval, int columns = 0, bool cache = true) =>
			FactoriesCore.Arange(gpu, startval, endval, interval, columns, cache);

		public static VectorInt Linspace(GPU gpu, int startval, int endval, int steps, int columns = 0, bool cache = true) =>
			FactoriesCore.Linspace(gpu, startval, endval, steps, columns, cache);

		/// <summary>
		/// Uniform integers in [low, high). Throws unless low is less than high.
		/// </summary>
		public static VectorInt RandomUniform(GPU gpu, int length, int low, int high, int? seed = null, int columns = 0) =>
			FactoriesCore.RandomUniform(gpu, length, low, high, seed, columns);

		public static VectorInt ZerosX(GPU gpu, int length, int columns = 0) =>
			GpuFactoriesCore.ZerosInt(gpu, length, columns);

		public static VectorInt OnesX(GPU gpu, int length, int columns = 0) =>
			GpuFactoriesCore.OnesInt(gpu, length, columns);

		public static VectorInt FillX(GPU gpu, int value, int length, int columns = 0) =>
			GpuFactoriesCore.Fill(gpu, value, length, columns);

		public static VectorInt RandomUniformX(GPU gpu, int length, int low, int high, int? seed = null, int columns = 0) =>
			GpuFactoriesCore.RandomUniform(gpu, length, low, high, seed, columns);
	}
}
