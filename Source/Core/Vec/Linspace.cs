using System;
using System.Linq;
using System.Numerics;
using BAVCL.Ext;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{

	/// <summary>
	/// 
	/// </summary>
	/// <param name="startval"></param>
	/// <param name="endval"></param>
	/// <param name="steps"></param>
	/// <returns></returns>
	public static Vec<T> Linspace(GPU gpu, T startval, T endval, int steps, int columns = 1, bool cache = true)
	{
		if (steps <= 1) throw new Exception("Cannot make linspace with less than 1 steps");
		T[] vals = Generators.Linspace(startval, endval, steps).ToArray();
		return new Vec<T>(gpu, vals, columns, cache);
	}

}