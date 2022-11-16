using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Core
{
    public partial class VectorBase<T>
    {
        public MemoryBuffer1D<T, Stride1D.Dense> GetBuffer()
        {
            if (!this.gpu.GPUbuffers.TryGetValue(this._id, out MemoryBuffer Data))
            {
                this.Cache();
                this.gpu.GPUbuffers.TryGetValue(this._id, out Data);
            }
            return (MemoryBuffer1D<T, Stride1D.Dense>)Data;
        }


    }


}
