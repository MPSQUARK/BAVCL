using System;
using System.Linq;

namespace DataScience.Core
{
    public abstract partial class VectorBase<T> : ICacheable, IIO where T : unmanaged 
    {
        protected GPU gpu 
        { 
            get; 
            set; 
        }
        public virtual T[] Value 
        {
            get;
            set;
        }
        public virtual int Columns
        {
            get { return _columns; }
            set { _columns = value > 0 ? value : throw new Exception($"Columns must be a positive integer greater than zero. Recieved {value}"); }
        }
        public virtual int Length 
        { 
            get { return _length; } 
            set { _length = value; } 
        }
        public uint ID
        {
            get { return _id; }
            set { _id = Math.Clamp(value, 0, uint.MaxValue); }
        }
        public long MemorySize 
        { 
            get { return _memorySize; } 
            set { _memorySize = value; } 
        }
        public uint LiveCount 
        { 
            get { return _livecount; } 
            set { _livecount = value; } 
        }

        protected internal int _columns = 1;
        protected internal int _length = 0;
        protected internal uint _id = 0;
        protected internal long _memorySize = 0;
        protected internal uint _livecount = 0;

        

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



        public T[] Pull()
        {
            return GetBuffer().GetAsArray();
        }

        public T GetValue(int row, int col)
        {
            SyncCPU();
            return this.Value[row * this.Columns + col];
        }

        // PRINT + CSV
        public void Print()
        {
            Console.Write(this.ToString());
            return;
        }

        // MATHEMATICAL PROPERTIES 
        public int RowCount()
        {
            if (Columns == 1) { return 1; }
            return _length / Columns;
        }

        public virtual (int, int) Shape()
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
