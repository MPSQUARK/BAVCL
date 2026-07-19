using ILGPU;
using ILGPU.Runtime;
using System;
using System.Collections.Generic;

namespace BAVCL.Core;

public abstract partial class VectorBase<T> : ICacheable<T>, IIO where T : unmanaged
{
	protected GPU Gpu;

	internal T[] Value = [];

	ResidenceField _residence;

	public Residence Residence
	{
		get => _residence.Value;
		set => _residence.Value = value;
	}

	internal int _cpuScopeDepth;

	public virtual int Columns
	{
		get => _columns;
		set
		{
			if (value < 0)
				throw new Exception($"Columns must be zero or greater. Recieved {value}");

			_columns = value;
		}
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

	public long MemorySize => (long)Interop.SizeOf<T>() * (long)Length;

	public uint LiveCount
	{
		get => _livecount;
		set => _livecount = value;
	}

	protected internal int _columns = 0;
	protected volatile internal uint _id = 0;
	protected volatile internal uint _livecount = 0;
	protected volatile internal int _length = 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="VectorBase{T}"/> class.
	/// </summary>
	protected VectorBase(GPU gpu, T[] value, int columns = 0, bool Cache = true)
	{
		Gpu = gpu;
		Columns = columns;
		Value = value;
		Length = value.Length;

		if (Cache)
		{
			this.Cache(value);
			Residence = Residence.InSync;
			return;
		}

		Residence = Residence.Cpu;
	}

	/// <summary>
	/// Creates an 'empty' vector of specified length.
	/// Warning: May contain random leftover data. 
	///	Initialize values before use OR use `Zeros` method.  
	/// </summary>
	/// <param name="gpu"></param>
	/// <param name="length"></param>
	/// <param name="columns"></param>
	protected VectorBase(GPU gpu, int length, int columns = 0)
	{
		Gpu = gpu;
		Columns = columns;
		Value = [];
		Length = length;
		CacheEmpty(length);
		Residence = Residence.Gpu;
	}


	public T[] Pull()
	{
		MemoryBuffer1D<T, Stride1D.Dense> buffer = GetBuffer();
		T[] values = new T[buffer.Length];
		buffer.AsArrayView<T>(0, buffer.Length).CopyToCPU(values);
		return values;
	}

	public ReadOnlySpan<T> RetrieveReadOnlySpan()
	{
		SyncCPU();
		return GetCpuReadOnlySpan();
	}

	void ICacheable<T>.EditCpu(Action<Memory<T>> edit)
	{
		if (_cpuScopeDepth == 0 || !ResidenceHelper.IsActiveCpu(Residence))
			throw new InvalidOperationException(
				$"{nameof(ICacheable<T>.EditCpu)} requires an open {nameof(CpuScope<T>)}.");

		edit(Value.AsMemory(0, Length));
	}

	// PRINT + CSV
	public virtual void Print() => Console.WriteLine(this.ToString());

	// MATHEMATICAL PROPERTIES 
	public int RowCount()
	{
		if (Columns == 0)
			return 1;

		if (Columns == 1)
			return Length;

		return Length / Columns;
	}

	public virtual Core.Shape Shape() => Core.Shape.FromStorage(Length, Columns);

	public virtual T Max()
	{
		ReadOnlySpan<T> span = RetrieveReadOnlySpan();
		if (span.Length == 0)
			throw new InvalidOperationException("Cannot compute Max of an empty vector.");

		T max = span[0];
		for (int i = 1; i < span.Length; i++)
		{
			if (Comparer<T>.Default.Compare(span[i], max) > 0)
				max = span[i];
		}

		return max;
	}

	public virtual T Min()
	{
		ReadOnlySpan<T> span = RetrieveReadOnlySpan();
		if (span.Length == 0)
			throw new InvalidOperationException("Cannot compute Min of an empty vector.");

		T min = span[0];
		for (int i = 1; i < span.Length; i++)
		{
			if (Comparer<T>.Default.Compare(span[i], min) < 0)
				min = span[i];
		}

		return min;
	}
	public abstract T Mean();
	public abstract T Range();
	public abstract T Sum();

	internal void CommitCpuView() => Length = Value.Length;

	internal void ValidateIndexForView(int index)
	{
		if (index < 0 || index >= Length)
			throw new IndexOutOfRangeException($"Index {index} is out of range for vector of length {Length}.");
	}

	internal int GetIndexFromCoordinatesForView(int row, int col)
	{
		int index = row * Columns + col;
		ValidateIndexForView(index);
		return index;
	}

	/// <summary>
	/// Returns a read-only view of the current CPU backing store without syncing from GPU.
	/// Call <see cref="RetrieveReadOnlySpan"/> when GPU data may be newer.
	/// </summary>
	public ReadOnlySpan<T> GetCpuReadOnlySpan() => Value.AsSpan(0, Length);

	/// <summary>
	/// Allocates a new heap copy of CPU storage. Prefer <see cref="RetrieveReadOnlySpan"/> for read-only access.
	/// </summary>
	public T[] ToArray()
	{
		ReadOnlySpan<T> data = RetrieveReadOnlySpan();
		T[] copy = new T[data.Length];
		data.CopyTo(copy);
		return copy;
	}

	public bool IsRectangular() => Columns == 0 || Length % Columns == 0;
	public bool Is1D() => Columns == 0;

}
