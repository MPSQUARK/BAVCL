using System.Collections.Generic;
using System.Numerics;

namespace BAVCL.Modules.Generators;

public static class GeneratorsModule
{
	public static IEnumerable<T> Arange<T>(T startValue, T endValue, T interval) where T : INumber<T> =>
		GeneratorsCore.ArangeEnumerable(startValue, endValue, interval);

	public static IEnumerable<T> Linspace<T>(T startValue, T endValue, int num) where T : INumber<T> =>
		GeneratorsCore.LinspaceEnumerable(startValue, endValue, num);

	public static T[] ArangeArray<T>(T startValue, T endValue, T interval) where T : INumber<T> =>
		GeneratorsCore.Arange(startValue, endValue, interval);

	public static T[] LinspaceArray<T>(T startValue, T endValue, int num) where T : INumber<T> =>
		GeneratorsCore.Linspace(startValue, endValue, num);

	public static float[] Arange(float startValue, float endValue, float interval) =>
		GeneratorsCore.Arange(startValue, endValue, interval);

	public static float[] Linspace(float startValue, float endValue, int num) =>
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
			GeneratorsModule.ArangeArray(startValue, endValue, interval);

		public static int[] Linspace(int startValue, int endValue, int num) =>
			GeneratorsModule.LinspaceArray(startValue, endValue, num);
	}

	extension(double[])
	{
		public static double[] Arange(double startValue, double endValue, double interval) =>
			GeneratorsModule.ArangeArray(startValue, endValue, interval);

		public static double[] Linspace(double startValue, double endValue, int num) =>
			GeneratorsModule.LinspaceArray(startValue, endValue, num);
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

		public static Vector UniformRandom(GPU gpu, int length, float minimum, float maximum, int seed, int columns = 0) =>
			FactoriesCore.UniformRandom(gpu, length, minimum, maximum, seed, columns);

		public static Vector UniformRandom(GPU gpu, int length, float minimum, float maximum, int columns = 0) =>
			FactoriesCore.UniformRandom(gpu, length, minimum, maximum, columns);

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

		public static Vector UniformRandomX(GPU gpu, int length, float minimum, float maximum, int seed, int columns = 0) =>
			GpuFactoriesCore.UniformRandom(gpu, length, minimum, maximum, seed, columns);

		public static Vector UniformRandomX(GPU gpu, int length, float minimum, float maximum, int columns = 0) =>
			GpuFactoriesCore.UniformRandom(gpu, length, minimum, maximum, columns);
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
		public Vector ZerosIP(int length, int columns = 0)
		{
			FactoriesCore.ZerosInPlace(vector, length, columns);
			return vector;
		}

		public Vector ZerosIPX(int length, int columns = 0)
		{
			GpuFactoriesCore.ZerosInPlaceX(vector, length, columns);
			return vector;
		}

		public Vector OnesIP(int length, int columns = 0)
		{
			FactoriesCore.OnesInPlace(vector, length, columns);
			return vector;
		}

		public Vector OnesIPX(int length, int columns = 0)
		{
			GpuFactoriesCore.OnesInPlaceX(vector, length, columns);
			return vector;
		}

		public Vector FillIP(float value, int length, int columns = 0)
		{
			FactoriesCore.FillInPlace(vector, value, length, columns);
			return vector;
		}

		public Vector FillIPX(float value, int length, int columns = 0)
		{
			GpuFactoriesCore.FillInPlaceX(vector, value, length, columns);
			return vector;
		}
	}

	extension(VectorInt vector)
	{
		public VectorInt ZerosIP(int length, int columns = 0)
		{
			FactoriesCore.ZerosInPlace(vector, length, columns);
			return vector;
		}

		public VectorInt ZerosIPX(int length, int columns = 0)
		{
			GpuFactoriesCore.ZerosInPlaceX(vector, length, columns);
			return vector;
		}

		public VectorInt OnesIP(int length, int columns = 0)
		{
			FactoriesCore.OnesInPlace(vector, length, columns);
			return vector;
		}

		public VectorInt OnesIPX(int length, int columns = 0)
		{
			GpuFactoriesCore.OnesInPlaceX(vector, length, columns);
			return vector;
		}

		public VectorInt FillIP(int value, int length, int columns = 0)
		{
			FactoriesCore.FillInPlace(vector, value, length, columns);
			return vector;
		}

		public VectorInt FillIPX(int value, int length, int columns = 0)
		{
			GpuFactoriesCore.FillInPlaceX(vector, value, length, columns);
			return vector;
		}
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

		public static VectorInt UniformRandom(GPU gpu, int length, int minimum, int maximum, int seed, int columns = 0) =>
			FactoriesCore.UniformRandom(gpu, length, minimum, maximum, seed, columns);

		public static VectorInt UniformRandom(GPU gpu, int length, int minimum, int maximum, int columns = 0) =>
			FactoriesCore.UniformRandom(gpu, length, minimum, maximum, columns);

		public static VectorInt ZerosX(GPU gpu, int length, int columns = 0) =>
			GpuFactoriesCore.ZerosInt(gpu, length, columns);

		public static VectorInt OnesX(GPU gpu, int length, int columns = 0) =>
			GpuFactoriesCore.OnesInt(gpu, length, columns);

		public static VectorInt FillX(GPU gpu, int value, int length, int columns = 0) =>
			GpuFactoriesCore.Fill(gpu, value, length, columns);

		public static VectorInt UniformRandomX(GPU gpu, int length, int minimum, int maximum, int seed, int columns = 0) =>
			GpuFactoriesCore.UniformRandom(gpu, length, minimum, maximum, seed, columns);

		public static VectorInt UniformRandomX(GPU gpu, int length, int minimum, int maximum, int columns = 0) =>
			GpuFactoriesCore.UniformRandom(gpu, length, minimum, maximum, columns);
	}
}
