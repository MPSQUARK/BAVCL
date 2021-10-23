using ILGPU.Runtime;
using System;

namespace DataScience.Core
{
    public partial class VectorBase<T>
    {
        public void Cache()
        {
            this._memorySize = this.CalculateMemorySize();

            // Ensure enough space on gpu for cache
            this.gpu.DeCacheLRU(this._memorySize);

            // Increase Live task count
            this.gpu.AddLiveTask();

            // Allocate data to gpu
            MemoryBuffer Buffer = Allocate();

            // Get a weakreference of buffer
            WeakReference<ICacheable> VectorReference = new WeakReference<ICacheable>(this);

            // Store info about data to LRU
            this._id = this.gpu.Allocate(VectorReference, Buffer, this._memorySize);

            this._length = this.Value.Length;

            // Get ID 
            return;
        }

        public void Cache(T[] array)
        {
            this._memorySize = this.CalculateMemorySize(array);

            // Ensure enough space on gpu for cache
            this.gpu.DeCacheLRU(this._memorySize);

            // Increase Live task count
            this.gpu.AddLiveTask();

            // Allocate data to gpu
            MemoryBuffer Buffer = Allocate(array);

            // Get a weakreference of buffer
            WeakReference<ICacheable> VectorReference = new WeakReference<ICacheable>(this);

            // Store info about data to LRU
            this._id = this.gpu.Allocate(VectorReference, Buffer, this._memorySize);

            // Update Length Property
            this._length = array.Length;

            // Get ID 
            return;
        }



    }


}
