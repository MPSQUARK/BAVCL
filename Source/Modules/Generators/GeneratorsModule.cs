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
}
