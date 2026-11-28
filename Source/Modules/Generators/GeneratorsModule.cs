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
		public static Vector Zeros(int length, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.Zeros(length, columns, upload, gpu);

		public static Vector Ones(int length, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.Ones(length, columns, upload, gpu);

		public static Vector Fill(float value, int length, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.Fill(value, length, columns, upload, gpu);

		public static Vector Arange(float startval, float endval, float interval, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.Arange(startval, endval, interval, columns, upload, gpu);

		public static Vector Linspace(float startval, float endval, int steps, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.Linspace(startval, endval, steps, columns, upload, gpu);

		/// <summary>
		/// Uniform floats in [0, 1). A null seed draws a fresh seed on the host; a fixed seed reproduces the
		/// values. CPU and GPU variants use different generators, so one seed gives different values on each.
		/// </summary>
		public static Vector Random(int length, int? seed = null, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.Random(length, seed, columns, upload, gpu);

		/// <summary>
		/// Uniform floats in [low, high). Throws unless both bounds are finite and low is less than high.
		/// </summary>
		public static Vector RandomUniform(int length, float low, float high, int? seed = null, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.RandomUniform(length, low, high, seed, columns, upload, gpu);

		/// <summary>
		/// Normal values with the given mean and standard deviation. Throws unless mean and stdDev are finite
		/// and stdDev is non-negative.
		/// </summary>
		public static Vector RandomNormal(int length, float mean = 0f, float stdDev = 1f, int? seed = null, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.RandomNormal(length, mean, stdDev, seed, columns, upload, gpu);

		public static Vector ZerosX(int length, int columns = 0, GPU? gpu = null) =>
			GpuFactoriesCore.Zeros(length, columns, gpu);

		public static Vector OnesX(int length, int columns = 0, GPU? gpu = null) =>
			GpuFactoriesCore.Ones(length, columns, gpu);

		public static Vector FillX(float value, int length, int columns = 0, GPU? gpu = null) =>
			GpuFactoriesCore.Fill(value, length, columns, gpu);

		public static Vector ArangeX(float startval, float endval, float interval, int columns = 0, GPU? gpu = null) =>
			GpuFactoriesCore.Arange(startval, endval, interval, columns, gpu);

		public static Vector LinspaceX(float startval, float endval, int steps, int columns = 0, GPU? gpu = null) =>
			GpuFactoriesCore.Linspace(startval, endval, steps, columns, gpu);

		public static Vector RandomX(int length, int? seed = null, int columns = 0, GPU? gpu = null) =>
			GpuFactoriesCore.Random(length, seed, columns, gpu);

		public static Vector RandomUniformX(int length, float low, float high, int? seed = null, int columns = 0, GPU? gpu = null) =>
			GpuFactoriesCore.RandomUniform(length, low, high, seed, columns, gpu);

		public static Vector RandomNormalX(int length, float mean = 0f, float stdDev = 1f, int? seed = null, int columns = 0, GPU? gpu = null) =>
			GpuFactoriesCore.RandomNormal(length, mean, stdDev, seed, columns, gpu);
	}

	extension(BAVCL.Geometric.Vector3)
	{
		public static BAVCL.Geometric.Vector3 Zeros(int length, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.Zeros3(length, upload, gpu);

		public static BAVCL.Geometric.Vector3 Fill(float value, int length, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.Fill3(value, length, upload, gpu);

		public static BAVCL.Geometric.Vector3 ZerosX(int length, GPU? gpu = null) =>
			GpuFactoriesCore.Zeros3(length, gpu);

		public static BAVCL.Geometric.Vector3 FillX(float value, int length, GPU? gpu = null) =>
			GpuFactoriesCore.Fill3(value, length, gpu);
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
		public static VectorInt Zeros(int length, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.ZerosInt(length, columns, upload, gpu);

		public static VectorInt Ones(int length, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.OnesInt(length, columns, upload, gpu);

		public static VectorInt Fill(int value, int length, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.Fill(value, length, columns, upload, gpu);

		public static VectorInt Arange(int startval, int endval, int interval, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.Arange(startval, endval, interval, columns, upload, gpu);

		public static VectorInt Linspace(int startval, int endval, int steps, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.Linspace(startval, endval, steps, columns, upload, gpu);

		/// <summary>
		/// Uniform integers in [low, high). Throws unless low is less than high.
		/// </summary>
		public static VectorInt RandomUniform(int length, int low, int high, int? seed = null, int columns = 0, Upload upload = Upload.Now, GPU? gpu = null) =>
			FactoriesCore.RandomUniform(length, low, high, seed, columns, upload, gpu);

		public static VectorInt ZerosX(int length, int columns = 0, GPU? gpu = null) =>
			GpuFactoriesCore.ZerosInt(length, columns, gpu);

		public static VectorInt OnesX(int length, int columns = 0, GPU? gpu = null) =>
			GpuFactoriesCore.OnesInt(length, columns, gpu);

		public static VectorInt FillX(int value, int length, int columns = 0, GPU? gpu = null) =>
			GpuFactoriesCore.Fill(value, length, columns, gpu);

		public static VectorInt RandomUniformX(int length, int low, int high, int? seed = null, int columns = 0, GPU? gpu = null) =>
			GpuFactoriesCore.RandomUniform(length, low, high, seed, columns, gpu);
	}
}
