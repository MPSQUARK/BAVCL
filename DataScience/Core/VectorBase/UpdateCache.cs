using ILGPU.Runtime;
using System;

namespace BAVCL.Core
{
    public partial class VectorBase<T>
    {
        public void UpdateCache()
        {
            if (this._id == 0) { return; }

            // If the ID does not exist in GPU's Cached Memory
            MemoryBuffer Data;
            if (!gpu.CachedMemory.TryGetValue(this._id, out Data))
            {
                // Try remove this ID from weakReferences
                this.gpu.CachedInfo.TryRemove(this._id, out _);

                // Cache the Data
                Cache();
                return;
            }


            // If the Lengths don't match remove old data and cache again
            if (Data.Length != Value.Length)
            {
                this._id = this.gpu.DeCache(this._id);
                Cache();
                return;
            }

            // Else if the lengths match update the cache

            // Convert Buffer Data to that of this type
            MemoryBuffer<T> data = (MemoryBuffer<T>)Data;

            // Copy new data to buffer 
            data.CopyFrom(Value, 0, 0, Value.Length);
            if (this.gpu.CachedMemory.TryUpdate(_id, data, Data)) { return; }

            throw new Exception("Unexpected ERROR in UpdateCache");
        }

        public void UpdateCache(T[] array)
        {
            if (this._id == 0) { Cache(array); }

            // If the ID does not exist in GPU's Cached Memory
            MemoryBuffer Data;
            if (!gpu.CachedMemory.TryGetValue(this._id, out Data))
            {
                // Try remove this ID from weakReferences
                this.gpu.CachedInfo.TryRemove(this._id, out _);

                // Cache the Data
                Cache();
                return;
            }


            // If the Lengths don't match remove old data and cache again
            if (Data.Length != array.Length)
            {
                this._id = this.gpu.DeCache(this._id);
                Cache(array);
                return;
            }

            // Else if the lengths match update the cache

            // Convert Buffer Data to that of this type
            MemoryBuffer<T> data = (MemoryBuffer<T>)Data;

            // Copy new data to buffer 
            data.CopyFrom(array, 0, 0, array.Length);
            if (this.gpu.CachedMemory.TryUpdate(_id, data, Data)) { return; }

            throw new Exception("Unexpected ERROR in UpdateCache");
        }


    }


}
