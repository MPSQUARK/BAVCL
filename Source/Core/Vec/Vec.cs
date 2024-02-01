using System;
using System.Numerics;
using System.Threading;
using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public class Vec<T> : ICacheable<T> 
	where T : unmanaged, INumber<T>
{
	protected GPU Gpu;
	public T[] Values = Array.Empty<T>();
	public virtual uint Columns { get; set; } = 0;
	public int Length { get; private set; }
	public uint ID { get => _id; set => _id = value; }
	public long MemorySize => (long)Interop.SizeOf<T>() * (long)Values.Length;
	public uint LiveCount { get => _livecount; set => _livecount = value; }

	protected volatile uint _id = 0;
	protected volatile uint _livecount = 0;

	public Vec(GPU gpu, T[] values, uint columns = 0, bool cache = true)
	{
		Gpu = gpu;
		Values = values;
		Columns = columns;
		Length = values.Length;
		if (cache) Cache(values);
	}
	
	public Vec(GPU gpu, int length, uint columns = 0)
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
	
	public T[] GetValues() => Values;

	public int Rows =>  Columns == 0 ? 1 : Length / (int)Columns;
	public (int rows, int cols) Shape() => (Rows, (int)Columns);
	public bool IsRectangular() => Length % Columns == 0;
	public bool IsSquare() => Columns == Rows;
	public bool Is1D() => Columns == 0 || Columns == Length;
	public bool Is2D() => !Is1D();
	
	internal MemoryBuffer Cache()
	{
		(ID, MemoryBuffer buffer) = Gpu.Allocate(this);
		return buffer;
	}
	
	internal MemoryBuffer Cache(T[] array)
	{
		(ID, MemoryBuffer buffer) = Gpu.Allocate(this, array);
		return buffer;
	}
	
	internal MemoryBuffer CacheEmpty(int length)
	{
		(ID, MemoryBuffer buffer) = Gpu.AllocateEmpty<T>(this, length);
		return buffer;
	}
	
	public MemoryBuffer1D<T, Stride1D.Dense> GetBuffer() =>
		(MemoryBuffer1D<T, Stride1D.Dense>)(Gpu.TryGetBuffer<T>(ID) ?? Cache());
	
	
	
	public void DeCache()
	{
		// If the vector is not cached - it's rechnically already decached
		if (ID == 0) return;
		
		// If the vector is live - Fail
		if (LiveCount != 0) return;
		
		// Else Decache
		Values = Pull();
		ID = Gpu.GCItem(ID);
	}

	public void DecrementLiveCount() =>
		Interlocked.Decrement(ref _livecount);
	
	public void IncrementLiveCount() =>
		Interlocked.Increment(ref _livecount);

	/*
	* TODO: [OPTIMISATION] If the gpu memory manager can store a flag to track data divergence between CPU/GPU,
	* then this can be used to avoid unnecessary data transfers on this function call
	*/ 	
	public void SyncCPU()
	{
		if (ID != 0) Values = Pull();
		Length = Values.Length;
	}

	public void SyncCPU(MemoryBuffer buffer)
	{
		if (Values.Length != buffer.Length)
			Values = new T[buffer.Length];
		
		buffer.AsArrayView<T>(0, buffer.Length).CopyToCPU(Values);
		Length = Values.Length;
	}
}
