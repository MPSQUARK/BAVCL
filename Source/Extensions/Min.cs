﻿using System;
using System.Numerics;

namespace BAVCL.Ext
{
	public static partial class Extensions
	{
		public static T Min<T>(this T[] arr) where T : INumber<T>
		{
			if (arr.Length == 0) throw new Exception("Cannot Be Length 0");
			T min = arr[0];
			for (int i = 1; i < arr.Length; i++)
				if (min > arr[i])
					min = arr[i];
			return min;
		}
		
		public static T Min<T>(this T[] arr, bool includeInfinity=true) where T : INumber<T>
		{
			if (includeInfinity) return arr.Min();

			if (arr.Length == 0) throw new Exception("Cannot Be Length 0");

			T min = arr[0];

			for (int i = 1; i < arr.Length; i++)
			{
				if (T.IsInfinity(arr[i])) continue;
				if (min > arr[i])
					min = arr[i];
			}

			return min;
		}
	}

}
