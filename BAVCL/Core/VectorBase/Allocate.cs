using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Core
{
    public partial class VectorBase<T>
    {

        public MemoryBuffer1D<T, Stride1D.Dense> Allocate() =>
            this.Gpu.accelerator.Allocate1D(this.Value);

        public MemoryBuffer1D<T, Stride1D.Dense> Allocate(T[] array) =>
            this.Gpu.accelerator.Allocate1D(array);

    }


}
