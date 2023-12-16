using ILGPU.Runtime;

namespace BAVCL.Core
{
    public partial class VectorBase<T>
    {
        public MemoryBuffer UpdateCache()
        {
            Length = Value.Length;
            if (ID == 0) return Cache();

            ID = gpu.GCItem(ID);
            return Cache();
        }

        public MemoryBuffer UpdateCache(T[] array)
        {
            Length = array.Length;
            (ID, MemoryBuffer buffer) = gpu.UpdateBuffer(this, array);
            return buffer;
        }


    }


}
