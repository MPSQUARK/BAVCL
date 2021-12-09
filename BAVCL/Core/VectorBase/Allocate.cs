using ILGPU.Runtime;

namespace BAVCL.Core
{
    public partial class VectorBase<T>
    {

        public MemoryBuffer<T> Allocate()
        {
            MemoryBuffer<T> buffer = this.gpu.accelerator.Allocate<T>(this.Value.Length);
            buffer.CopyFrom(this.Value, 0, 0, this.Value.Length);
            return buffer;
        }
        public MemoryBuffer<T> Allocate(T[] array)
        {
            MemoryBuffer<T> buffer = this.gpu.accelerator.Allocate<T>(array.Length);
            buffer.CopyFrom(array, 0, 0, array.Length);
            return buffer;
        }


    }


}
