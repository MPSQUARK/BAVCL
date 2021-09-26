using ILGPU;
using System;
using System.Linq;
using System.Text;
using DataScience.Core;
using ILGPU.Runtime;
using static DataScience.GPU;

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
        public uint Id { 
            get {return _id; } 
            set { _id = Math.Clamp(value, 0, uint.MaxValue); } 
        }

        #region "Memory Management"
           
        // Implemented Via ICacheable
        public long MemorySize()
        {
            return Interop.SizeOf<T>() * this.Value.Length;
        }
        public void Dispose()
        {
            if (this.Id == 0) { return; }
            this.gpu.DeCache(this.Id);
            this.Id = 0;
            return;
        }
        public uint Cache()
        {
            this.Id = this.gpu.Cache(this);
            return this.Id;
        }
        public uint UpdateCache()
        {
            // If there exists no cached data then perform caching 
            GPUData data;
            if (!this.gpu.GData.TryGetValue(this.Id, out data))
            {
                return Cache();
            }

            MemoryBuffer<T> buffer = (MemoryBuffer<T>)data.buffer;

            // If the Lengths don't match remove old data and cache again
            if (buffer.Length != Value.Length)
            {
                this.gpu.DeCache(this.Id);
                return Cache();
            }

            // Else update the cache
            buffer.CopyFrom(Value, 0, 0, Value.Length);
            return this.Id;
        }
        public MemoryBuffer Allocate()
        {
            MemoryBuffer<T> buffer = this.gpu.accelerator.Allocate<T>(this.Value.Length);
            buffer.CopyFrom(this.Value, 0, 0, this.Value.Length);
            return buffer;
        }

        // Generic Methods NOT using ICacheable
        public MemoryBuffer<T> GetBuffer()
        {
            return (MemoryBuffer<T>)this.gpu.GetMemoryBuffer(this);
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
