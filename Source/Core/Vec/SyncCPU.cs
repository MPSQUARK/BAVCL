using System.Numerics;
using BAVCL.MemoryManagement;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    /*
    * TODO: [OPTIMISATION] If the gpu memory manager can store a flag to track data divergence between CPU/GPU,
    * then this can be used to avoid unnecessary data transfers on this function call
    */
    public void SyncCPU()
    {
        if (ID != 0) Values = Pull();
        Length = Values.Length;
    }

    public Vec<T> SyncCPUSelf()
    {
        SyncCPU();
        return this;
    }

    public void SyncCPU(MemoryBuffer buffer)
    {
        if (Values.Length != buffer.Length)
            Values = new T[buffer.Length];

        buffer.AsArrayView<T>(0, buffer.Length).CopyToCPU(Values);
        Length = Values.Length;
    }
}
