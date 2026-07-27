using ILGPU.Runtime;

namespace BAVCL.Core;

public partial class VectorBase<T>
{
    public MemoryBuffer UpdateCache()
    {
        Length = Value.Length;
        (ID, MemoryBuffer buffer) = Gpu.UpdateBuffer(this);
        return buffer;
    }

    public MemoryBuffer UpdateCache(T[] array)
    {
        Length = array.Length;
        (ID, MemoryBuffer buffer) = Gpu.UpdateBuffer(this, array);
        return buffer;
    }
}