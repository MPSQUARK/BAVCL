using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Core
{
    public partial class VectorBase<T>
    {

        public MemoryBuffer1D<T, Stride1D.Dense> GetBuffer()
        {
            if (gpu.Caches.TryGetValue(ID, out Cache cache))
                return (MemoryBuffer1D<T, Stride1D.Dense>)cache.MemoryBuffer;

            return (MemoryBuffer1D<T, Stride1D.Dense>)Cache();
        }


    }


}
