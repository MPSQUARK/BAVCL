using System.Linq;
using System.Numerics;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
	/// <summary>
	/// Gets the UNION of values of vectors A & B. (Removes any duplicates)
	/// Preserves the value of Columns of VectorA.
	/// A∪B != B∪A
	/// </summary>
	/// <param name="vectorA"></param>
	/// <param name="vectorB"></param>
	/// <returns></returns>
	public static Vec<T> Merge(Vec<T> vectorA, Vec<T> vectorB)
	{
		vectorA.SyncCPU();
		vectorB.SyncCPU();
		return new Vec<T>(vectorA.Gpu, vectorA.Values.Union(vectorB.Values).ToArray(), vectorA.Columns);
	}

	/// <summary>
	/// Performs an in-place UNION of values. (Removes any duplicates)
	/// Preserves the value of Columns of this Vector.
	/// </summary>
	/// <param name="vector"></param>
	public Vec<T> Merge_IP(Vec<T> vector)
	{
		SyncCPU();
		vector.SyncCPU();
		UpdateCache(this.Values.Union(vector.Values).ToArray());
		return this;
	}
}
