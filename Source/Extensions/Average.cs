using System;
using System.Numerics;

namespace BAVCL.Ext;

public static partial class Extensions
{
	public static double Average<T>(this T[] arr) where T : IFloatingPoint<T> => 
		Convert.ToDouble(arr.Sum()) / arr.Length;

	public static float Average(this float[] arr) => arr.Sum() / arr.Length;
	public static double Average(this double[] arr) => arr.Sum() / arr.Length;
}
