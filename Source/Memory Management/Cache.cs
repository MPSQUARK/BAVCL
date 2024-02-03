using ILGPU.Runtime;
using System;

namespace BAVCL.MemoryManagement;

public struct Cache
{
	public MemoryBuffer MemoryBuffer;
	public WeakReference<ICacheable> CachedObjRef;

	public Cache(MemoryBuffer memoryBuffer, WeakReference<ICacheable> cachedObjRef)
	{
		MemoryBuffer = memoryBuffer;
		CachedObjRef = cachedObjRef;
	}
}