using System.Numerics;
using BAVCL.MemoryManagement;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
	internal MemoryBuffer Cache()
	{
		(ID, MemoryBuffer buffer) = Gpu.Allocate(this);
		return buffer;
	}

	internal MemoryBuffer Cache(T[] array)
	{
		(ID, MemoryBuffer buffer) = Gpu.Allocate(this, array);
		return buffer;
	}

	internal MemoryBuffer CacheEmpty(int length)
	{
		(ID, MemoryBuffer buffer) = Gpu.AllocateEmpty<T>(this, length);
		return buffer;
	}

}
