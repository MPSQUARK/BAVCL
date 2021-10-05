using ILGPU;
using System;
using System.Linq;
using System.Text;
using DataScience.Core;
using ILGPU.Runtime;
using static DataScience.GPU;
using System.Threading;

namespace DataScience
{
    public abstract class VectorBase<T> : ICacheable where T : unmanaged 
    {
        protected GPU gpu { get; set; }
        public virtual T[] Value { get; set; }


        protected internal int _columns = 1;
        public virtual int Columns { 
            get  { return _columns; }
            set  { _columns = Math.Clamp(value, 1, int.MaxValue);  } 
        }


        protected internal uint _id = 0;
        public uint ID { 
            get {return _id; } 
            set { _id = Math.Clamp(value, 0, uint.MaxValue); } 
        }

        protected internal long _memorySize = 0;
        public long MemorySize { get { return _memorySize; } set { _memorySize = value; } }

        protected VectorBase(GPU gpu, T[] value, int columns = 1, bool Cache = true)
        {
            this.gpu = gpu;
            this.Value = value;
            this.Columns = columns;
            this._memorySize = this.CalculateMemorySize();
            if (Cache)
            {
                this.ID = this.Cache();
            }

        }

        public uint _livecount = 0;
        public uint LiveCount { get {return _livecount; } set {_livecount = value; } }




        #region "Memory Management"

        // Implemented Via ICacheable
        public long CalculateMemorySize()
        {
            return Interop.SizeOf<T>() * this.Value.Length;
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
            // If the vector is not cached - Fail
            if (this._id == 0) { return false; }
            
            // If the vector is live - Fail
            if (this._livecount != 0) { return false; }

            // Else Decache
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

        public uint Cache()
        {
            this._memorySize = this.CalculateMemorySize();

            // Ensure enough space on gpu for cache
            this.gpu.DeCacheLRU(this._memorySize);

            // Allocate data to gpu
            MemoryBuffer Buffer = Allocate();

            // Get a weakreference of buffer
            WeakReference<ICacheable> VectorReference = new WeakReference<ICacheable>(this);

            // Store info about data to LRU
            this._id = this.gpu.Allocate(VectorReference, Buffer, this._memorySize);

            // Get ID 
            return this._id;
        }




        public uint UpdateCache()
        {
            // If the ID does not exist in GPU's Cached Memory
            MemoryBuffer Data;
            if (!gpu.CachedMemory.TryGetValue(this._id, out Data))
            {
                // Try remove this ID from weakReferences
                this.gpu.CachedInfo.TryRemove(this._id, out _);

                // Cache the Data
                return Cache();
            }


            // If the Lengths don't match remove old data and cache again
            if (Data.Length != Value.Length)
            {
                this.gpu.DeCache(this._id);
                return Cache();
            }

            
            // Else if the lengths match update the cache
            
            // Convert Buffer Data to that of this type
            MemoryBuffer<T> data = (MemoryBuffer<T>)Data;

            data.CopyFrom(Value, 0, 0, Value.Length);
            if (this.gpu.CachedMemory.TryUpdate(_id, data, Data))
            {
                return this._id;
            }

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


        #endregion


        public T AccessVal(int row, int col)
        {
            return this.Value[row * this.Columns + col];
        }
        public T[] AccessRow(int row)
        {
            return this.Value[(row * 3)..((row + 1) * 3)];
        }


        // PRINT + CSV
        public void Print()
        {
            Console.WriteLine();
            Console.Write(this.ToString());
            return;
        }

        public string ToCSV()
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool is1D = !(this.Columns == 1);
            for (int i = 0; i < this.Value.Length; i++)
            {
                if ((i % this.Columns == 0) && is1D && i != 0)
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
            if (this.Columns == 1) { return 1; }
            return this.Value.Length / this.Columns;
        }
        public int Length()
        {
            return this.Value.Length;
        }
        public (int, int) Shape()
        {
            return (this.RowCount(), this.Columns);
        }

        public virtual T Max() { return Value.Max(); }
        public virtual T Min() { return Value.Min();}
        public abstract T Mean();
        public abstract T Range();
        public abstract T Sum();
        public bool IsRectangular()
        {
            return (this.Length() % this.Columns == 0);
        }




    }


}
