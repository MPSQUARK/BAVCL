using System.Linq;
using System.Numerics;
using BAVCL.MemoryManagement;
using BAVCL.Ext;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
	public static Vec<T> Arange(GPU gpu, T startval, T endval, T interval, int Columns = 1, bool cache = true)
	{
		T[] values = Generators.Arange(startval, endval, interval).ToArray();
		return new Vec<T>(gpu, values, Columns, cache);
	}
}
