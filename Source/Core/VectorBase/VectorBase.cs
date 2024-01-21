using ILGPU;
using ILGPU.Runtime;
using System;
using System.Linq;
using System.Numerics;

namespace BAVCL.Core
{
	public abstract partial class VectorBase<T> : ICacheable<T>, IIO 
		where T : unmanaged, INumber<T>
	{
		protected GPU gpu;

		/// <summary>
		/// The value of the vector. This is the data that is stored on the CPU.
		/// May be different from the value stored on the GPU if not synced.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T[] Value = Array.Empty<T>();

		/// <summary>
		/// The number of columns in the vector. If the vector is 1D, this is 0 or equal to the length of the vector.
		/// Columns of 0 => 1D Vector e.g. [1, 2, 3, 4, 5] (horizontal)
		/// Columns of vector length => 1D Vector e.g. Transpose([1, 2, 3, 4, 5]) (vertical)
		/// </summary>
		/// <value></value>
		public virtual int Columns
		{
			get => _columns;
			set { _columns = value >= 0 ? value : throw new Exception($"Columns must be a positive integer greater than or equal to zero. Recieved {value}"); }
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

		protected VectorBase(GPU gpu, T[] value, int columns = 0, bool Cache = true)
		{
			this.gpu = gpu;
			Columns = columns;
			Value = value;
			Length = value.Length;
			
			if (Cache) this.Cache(value);
		}

		protected VectorBase(GPU gpu, int length, int columns = 0)
		{
			this.gpu = gpu;
			Columns = columns;
			Value = new T[length];
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

		public T GetValue(int row, int col)
		{
			SyncCPU();
			return Value[row * Columns + col];
		}

		public T[] GetValues() => Value;


		// PRINT + CSV
		public virtual void Print() => Console.WriteLine(this.ToString());

		// MATHEMATICAL PROPERTIES 
		public int Rows => Columns == 0 ? 1 : Length / Columns;

		public virtual (int, int) Shape() => (Rows, Columns);

		public virtual T Max() { SyncCPU(); return Value.Max(); }
		public virtual T Min() { SyncCPU(); return Value.Min(); }
		public abstract T Mean();
		public abstract T Range();
		public abstract T Sum();
		public bool IsRectangular() => this.Length % this.Columns == 0;
		public bool Is1D() => this.Columns == 0 || this.Columns == this.Length;
		public bool Is2D() => !Is1D();

	}


}
