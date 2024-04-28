using System.Numerics;
using BAVCL.MemoryManagement;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    public MemoryBuffer UpdateCache()
    {
        Length = Values.Length;
        if (ID == 0) return Cache();

        ID = Gpu.GCItem(ID);
        return Cache();
    }

    public MemoryBuffer UpdateCache(T[] array)
    {
        Length = array.Length;
        (ID, MemoryBuffer buffer) = Gpu.UpdateBuffer(this, array);
        return buffer;
    }
}
