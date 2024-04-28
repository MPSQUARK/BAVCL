using System;
using System.Numerics;
using System.Threading;
using BAVCL.Core.Enums;
using BAVCL.MemoryManagement;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
	protected volatile uint _id = 0;
	protected volatile uint _livecount = 0;
	protected int _columns = 0;

	protected GPU Gpu;

	public T[] Values = Array.Empty<T>();
	public virtual int Columns
	{
		get => _columns;
		set => _columns = int.IsPositive(value) ? value : throw new Exception("Columns must be positive.");
	}
	public int Length { get; private set; }
	public uint ID { get => _id; set => _id = value; }
	public long MemorySize => (long)Interop.SizeOf<T>() * (long)Length;
	public uint LiveCount { get => _livecount; set => _livecount = value; }
	public int Rows => Columns == 0 ? 1 : Length / Columns;


	public T[] GetValues() => Values;
	public (int rows, int cols) Shape() => (Rows, Columns);
	public bool IsRectangular() => Length % Columns == 0;
	public bool IsSquare() => Columns == Rows;
	public bool Is1D() => Columns == 0 || Columns == Length;
	public bool Is2D() => !Is1D();


	public Vec(GPU gpu, T[] values, int columns = 0, bool cache = true)
	{
		Gpu = gpu;
		Values = values;
		Columns = columns;
		Length = values.Length;
		if (cache) Cache(values);
	}

	public Vec(GPU gpu, int length, int columns = 0)
	{
		Gpu = gpu;
		Values = new T[length];
		Columns = columns;
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
		return Values[row * Columns + col];
	}

	public void DecrementLiveCount() => Interlocked.Decrement(ref _livecount);
	public void IncrementLiveCount() => Interlocked.Increment(ref _livecount);

}
