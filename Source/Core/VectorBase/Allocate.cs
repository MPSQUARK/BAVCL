using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Core
{
    public partial class VectorBase<T>
    {

        public MemoryBuffer1D<T, Stride1D.Dense> Allocate() =>
            this.gpu.accelerator.Allocate1D(this.Value);

        public MemoryBuffer1D<T,Stride1D.Dense> Allocate(T[] array) =>
            this.gpu.accelerator.Allocate1D(array);

    }


}
