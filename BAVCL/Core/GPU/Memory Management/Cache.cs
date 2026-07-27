using BAVCL.Core;
using ILGPU.Runtime;
using System;


namespace BAVCL;

public struct Cache(MemoryBuffer memoryBuffer, WeakReference<ICacheable> cachedObjRef)
{
    public MemoryBuffer MemoryBuffer = memoryBuffer;
    public WeakReference<ICacheable> CachedObjRef = cachedObjRef;
}
