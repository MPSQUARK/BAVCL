using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Core
{
    public partial class VectorBase<T>
    {

        public MemoryBuffer1D<T, Stride1D.Dense> GetBuffer() =>
            (MemoryBuffer1D<T, Stride1D.Dense>)(gpu.TryGetBuffer<T>(ID) ?? Cache()); 


    }

     
}
