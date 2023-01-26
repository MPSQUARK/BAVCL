using ILGPU;
using ILGPU.Runtime;
using System;

namespace BAVCL.Core
{
    public partial class VectorBase<T>
    {
        public MemoryBuffer UpdateCache()
        {
            if (ID == 0) return Cache();

            ID = gpu.GCItem(ID);
            return Cache();
        }

        public MemoryBuffer UpdateCache(T[] array)
        {
            (ID, MemoryBuffer buffer) = gpu.UpdateBuffer(this, array);
            return buffer;
        }


    }


}
