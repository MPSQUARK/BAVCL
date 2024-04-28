using System.Linq;
using System.Numerics;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
	public static Vec<T> Append(Vec<T> vectorA, Vec<T> vectorB)
	{
		vectorA.SyncCPU();
		vectorB.SyncCPU();
		return new Vec<T>(vectorA.Gpu, [.. vectorA.Values, .. vectorB.Values], vectorA.Columns);
	}

	public Vec<T> Append_IP(Vec<T> vector)
	{
		SyncCPU();
		vector.SyncCPU();
		Values = [.. Values, .. vector.Values];
		Length = Values.Length;
		UpdateCache();
		return this;
	}

	public static Vec<T> Prepend(Vec<T> vectorA, Vec<T> vectorB) => Append(vectorB, vectorA);

}
