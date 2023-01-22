using BAVCL.Core;
using ILGPU.Runtime;
using System;


namespace BAVCL
{
    public struct Cache
    {
        public MemoryBuffer MemoryBuffer;
        public WeakReference<ICacheable> CachedObjRef;
        public uint LiveCount;

        public Cache(MemoryBuffer memoryBuffer, WeakReference<ICacheable> cachedObjRef)
        {
            MemoryBuffer = memoryBuffer;
            CachedObjRef = cachedObjRef;
            LiveCount = 0;
        }
    }
}
