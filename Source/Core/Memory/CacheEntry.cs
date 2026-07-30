using ILGPU.Runtime;
using System;

namespace BAVCL.Core;

internal sealed class CacheEntry(MemoryBuffer memoryBuffer, WeakReference<ICacheable> cachedObjRef)
{
	public MemoryBuffer MemoryBuffer => memoryBuffer;
	public WeakReference<ICacheable> CachedObjRef => cachedObjRef;
}
