using System;
using System.Numerics;
using System.Text;

namespace BAVCL.Core;

public static partial class Extensions
{
	
	public static void Print<T>(this T number) where T : INumber<T>
		=> Console.WriteLine(number.ToString());
	public static void Print<T>(this T[] arr, char separator = ',')
	{
		StringBuilder sb = new();
		for (int i = 0; i < arr.Length - 1; i++)
			sb.Append($"{arr[i]}{separator}");
			
		sb.Append($"{arr[^1]}");
		Console.WriteLine(sb.ToString());
	}
	public static void Print<T>(this T[,] objects, char separator = ',')
	{
		StringBuilder sb = new();
		for (int i = 0; i < objects.GetLength(0); i++)
		{
			int j = 0;
			for (; j < objects.GetLength(1) - 1; j++)
				sb.Append($"{objects[i, j]}{separator}");
			sb.Append($"{objects[i, j]}\n");
		}
		Console.WriteLine(sb.ToString());
	}

	public static void Print(this float value, byte decimalplaces = 2) => 
		Console.WriteLine(value.ToString($"F{decimalplaces}"));

	public static void Print(this float[] arr, byte decimalplaces = 2) => 
		Console.WriteLine(arr.ToStr(decimalplaces));

	public static void Print(this double value, byte decimalplaces = 2) => 
		Console.WriteLine(value.ToString($"F{decimalplaces}"));

}