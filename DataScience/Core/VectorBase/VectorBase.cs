using ILGPU;
using System;
using System.Linq;
using System.Text;
using DataScience.Core;
using ILGPU.Runtime;
using System.Threading;

namespace DataScience
{
    public abstract class VectorBase<T> : ICacheable, IIO where T : unmanaged 
    {
        protected GPU gpu { get; set; }

        public virtual T[] Value { get; set; }


        protected internal int _columns = 1;
        public virtual int Columns { 
            get  { return _columns; }
            set  { _columns = value > 0 ? value : throw new Exception($"Columns must be a positive integer greater than zero. Recieved {value}"); } 
        }

        protected internal int _length = 0;
        public virtual int Length { get { return _length; } set { _length = value; } }

        protected internal uint _id = 0;
        public uint ID { 
            get {return _id; } 
            set { _id = Math.Clamp(value, 0, uint.MaxValue); } 
        }

        protected internal long _memorySize = 0;
        public long MemorySize { get { return _memorySize; } set { _memorySize = value; } }

        public uint _livecount = 0;
        public uint LiveCount { get { return _livecount; } set { _livecount = value; } }

        protected VectorBase(GPU gpu, T[] value, int columns = 1, bool Cache = true)
        {
            this.gpu = gpu;
            this.Columns = columns;
            if (Cache)
            {
                this.Cache(value);
                return;
            }
            this.Value = value;
            this.Length = value.Length;
            this._memorySize = this.CalculateMemorySize();
        }






        #region "Memory Management"

        // Implemented Via ICacheable
        public long CalculateMemorySize()
        {
            return (long)Interop.SizeOf<T>() * (long)this.Value.Length;
        }
        public long CalculateMemorySize(T[] array)
        {
            return (long)Interop.SizeOf<T>() * (long)array.Length;
        }

        public void IncrementLiveCount()
        {
            Interlocked.Increment(ref _livecount);
        }
        public void DecrementLiveCount()
        {
            Interlocked.Decrement(ref _livecount);
        }


        public bool TryDeCache()
        {
            // If the vector is live - Fail
            if (this._livecount != 0) { return false; }

            // If the vector is not cached - it's rechnically already decached
            if (this._id == 0) { return true; }

            // Else Decache
            this.Value = this.Pull();
            this._id = this.gpu.DeCache(this._id);
            return true;
        }


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
            if (Data.Length != Value.Length)
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




        // Generic Methods NOT using ICacheable
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

        public T[] Pull()
        {
            return GetBuffer().GetAsArray();
        }
        public void SyncCPU()
        {
            if (this._id != 0)
            {
                this.Value = this.Pull();
                return;
            }

            if (this.Value != null)
            {
                return;
            }

            throw new Exception($"No Data Found On GPU. Vector Cache ID = {this._id}");
        }

        #endregion


        public T AccessVal(int row, int col)
        {
            return this.Value[row * this.Columns + col];
        }
        public virtual T[] AccessRow(int row)
        {
            return this.Value[(row * this.Columns)..(++row * this.Columns)];
        }



        // PRINT + CSV
        public void Print()
        {
            Console.Write(this.ToString());
            return;
        }

        public string ToCSV()
        {
            SyncCPU();
            StringBuilder stringBuilder = new StringBuilder();
            bool is1D = (Columns != 1);

            stringBuilder.Append($"{this.Value[0]},");

            for (int i = 1; i < Length; i++)
            {
                if ((i % Columns == 0) && is1D)
                {
                    stringBuilder.AppendLine();
                }
                stringBuilder.Append($"{this.Value[i]},");
            }
            return stringBuilder.ToString();
        }


        // MATHEMATICAL PROPERTIES 
        public int RowCount()
        {
            if (Columns == 1) { return 1; }
            return _length / Columns;
        }

        public (int, int) Shape()
        {
            return (RowCount(), Columns);
        }

        public virtual T Max() { SyncCPU(); return Value.Max(); }
        public virtual T Min() { SyncCPU(); return Value.Min();}
        public abstract T Mean();
        public abstract T Range();
        public abstract T Sum();
        public bool IsRectangular()
        {
            return (this.Length % this.Columns == 0);
        }




    }


}
