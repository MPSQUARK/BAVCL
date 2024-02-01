using System;
using System.Numerics;

namespace BAVCL.Ext
{
	public static partial class Extensions
	{
		public static T Max<T>(this T[] arr) where T : INumber<T>
		{
			if (arr.Length == 0) throw new Exception("Cannot Be Length 0");
			T max = arr[0];
			for (int i = 1; i < arr.Length; i++)
				if (max < arr[i])
					max = arr[i];
			return max;
		}
	
		public static T Max<T>(this T[] arr, bool includeInfinity=true) where T : INumber<T>
		{
			if (includeInfinity) return arr.Max();

			if (arr.Length == 0) throw new Exception("Cannot Be Length 0");

			T max = arr[0];

			for (int i = 1; i < arr.Length; i++)
			{
				if (T.IsInfinity(arr[i])) continue;
				if (max < arr[i])
					max = arr[i];
			}

			return max;
		}	
	}

}
