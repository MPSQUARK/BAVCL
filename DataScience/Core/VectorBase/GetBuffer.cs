using ILGPU.Runtime;

namespace DataScience.Core
{
    public partial class VectorBase<T>
    {
        public MemoryBuffer<T> GetBuffer()
        {
            MemoryBuffer Data;
            if (!this.gpu.CachedMemory.TryGetValue(this._id, out Data))
            {
                this.Cache();
                this.gpu.CachedMemory.TryGetValue(this._id, out Data);
            }
            return (MemoryBuffer<T>)Data;
        }


    }


}
