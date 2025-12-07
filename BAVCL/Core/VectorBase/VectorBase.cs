using ILGPU;
using ILGPU.Runtime;
using System;
using System.Linq;

namespace BAVCL.Core
{
	public abstract partial class VectorBase<T> : ICacheable<T>, IIO where T : unmanaged
	{
		protected GPU Gpu;

		public T[] Value = [];

		public virtual int Columns
		{
			get => _columns;
			set { _columns = value > 0 ? value : throw new Exception($"Columns must be a positive integer greater than zero. Recieved {value}"); }
		}

		public int Length
		{
			get => _length;
			set => _length = value;
		}

		public uint ID
		{
			get => _id;
			set => _id = value >= 0 ? value : throw new Exception($"ID CANNOT be less than 0. Recieved: {value}");
		}

		public long MemorySize => (long)Interop.SizeOf<T>() * (long)Value.Length;

		public uint LiveCount
		{
			get => _livecount;
			set => _livecount = value;
		}

		protected internal int _columns = 1;
		protected internal long _memorySize = 0;
		protected volatile internal uint _id = 0;
		protected volatile internal uint _livecount = 0;
		protected volatile internal int _length = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="VectorBase{T}"/> class.
		/// </summary>
		/// <param name="gpu"></param>
		/// <param name="value"></param>
		/// <param name="columns"></param>
		/// <param name="Cache">Preloads the vector onto the GPU at creation time.</param>
		/// <summary>
		protected VectorBase(GPU gpu, T[] value, int columns = 1, bool Cache = true)
		{
			Gpu = gpu;
			Columns = columns;
			Value = value;
			Length = value.Length;

			if (Cache) this.Cache(value);
		}

		protected VectorBase(GPU gpu, int length, int columns = 1)
		{
			this.Gpu = gpu;
			Columns = columns;
			Value = [];
			Length = length;
			CacheEmpty(length);
		}


		public T[] Pull()
		{
			MemoryBuffer1D<T, Stride1D.Dense> buffer = GetBuffer();
			T[] values = new T[buffer.Length];
			buffer.AsArrayView<T>(0, buffer.Length).CopyToCPU(values);
			return values;
		}

		public T[] GetValues() => Value;

		// PRINT + CSV
		public virtual void Print() => Console.WriteLine(this.ToString());

		// MATHEMATICAL PROPERTIES 
		public int RowCount()
		{
			if (Columns == 1) return 1;
			return Length / Columns;
		}

		public virtual (int, int) Shape() => (RowCount(), Columns);

		public virtual T Max() { SyncCPU(); return Value.Max(); }
		public virtual T Min() { SyncCPU(); return Value.Min(); }
		public abstract T Mean();
		public abstract T Range();
		public abstract T Sum();
		public bool IsRectangular() => this.Length % this.Columns == 0;
		public bool Is1D() => this.Columns == 1;

	}


}
