using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Core
{
    public partial class VectorBase<T>
    {
        public void SyncCPU()
        {
            if (ID != 0) Value = Pull();
        }

        public void SyncCPU(MemoryBuffer buffer)
        {
            Value = ((MemoryBuffer1D<T, Stride1D.Dense>)buffer).GetAsArray1D();
        }


    }


}
