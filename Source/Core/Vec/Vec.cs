using System;
using System.Numerics;
using System.Threading;
using BAVCL.Core.Enums;
using BAVCL.MemoryManagement;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
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

	public void DecrementLiveCount() => Interlocked.Decrement(ref _livecount);
	
	public void IncrementLiveCount() => Interlocked.Increment(ref _livecount);

	/*
	* TODO: [OPTIMISATION] If the gpu memory manager can store a flag to track data divergence between CPU/GPU,
	* then this can be used to avoid unnecessary data transfers on this function call
	*/ 	
	public void SyncCPU()
	{
		if (ID != 0) Values = Pull();
		Length = Values.Length;
	}
	
	public Vec<T> SyncCPUSelf()
	{
		SyncCPU();
		return this;
	}

	public void SyncCPU(MemoryBuffer buffer)
	{
		if (Values.Length != buffer.Length)
			Values = new T[buffer.Length];
		
		buffer.AsArrayView<T>(0, buffer.Length).CopyToCPU(Values);
		Length = Values.Length;
	}
	
	public static Vec<T> OP(Vec<T> vectorA, Vec<T> vectorB, Operations operation)
	{
		if (vectorA.Length != vectorB.Length) throw new ArgumentException("Vectors must be of the same length");
		
		GPU gpu = vectorA.Gpu;
		
		vectorA.IncrementLiveCount();
		vectorB.IncrementLiveCount();
		
		Vec<T> output = new(gpu, vectorA.Length, vectorA.Columns);
		output.IncrementLiveCount();
		
		MemoryBuffer1D<T, Stride1D.Dense> 
			buffer = output.GetBuffer(),
			buffer2 = vectorA.GetBuffer(),
			buffer3 = vectorB.GetBuffer();
		
		var kernel = (Action<AcceleratorStream, Index1D, ArrayView<T>, ArrayView<T>, ArrayView<T>, SpecializedValue<int>>)gpu.GetKernel<T>(KernelType.SeqOP);
		kernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer3.View, new SpecializedValue<int>((int)operation));
		
		gpu.accelerator.Synchronize();
		
		vectorA.DecrementLiveCount();
		vectorB.DecrementLiveCount();
		output.DecrementLiveCount();
			
		return output;
	}
	
	public static Vec<T> OP(Vec<T> vectorA, T scalar, Operations operation)
	{
		GPU gpu = vectorA.Gpu;
		
		vectorA.IncrementLiveCount();
		
		Vec<T> output = new(gpu, vectorA.Length, vectorA.Columns);
		output.IncrementLiveCount();
		
		MemoryBuffer1D<T, Stride1D.Dense> 
			buffer = output.GetBuffer(),
			buffer2 = vectorA.GetBuffer();
		
		var kernel = (Action<AcceleratorStream, Index1D, ArrayView<T>, ArrayView<T>, T, SpecializedValue<int>>)gpu.GetKernel<T>(KernelType.ScalarOP);
		kernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, scalar, new SpecializedValue<int>((int)operation));
		
		gpu.accelerator.Synchronize();
		
		vectorA.DecrementLiveCount();
		output.DecrementLiveCount();
			
		return output;
	}
	
	public Vec<T> AbsXIP()
	{
		// Secure data
		IncrementLiveCount();

		// Get the Memory buffer input/output
		MemoryBuffer1D<T, Stride1D.Dense> buffer = GetBuffer(); // IO

		// RUN
		var kernel = (Action<AcceleratorStream,Index1D, ArrayView<T>>)Gpu.GetKernel<T>(KernelType.Abs);
		kernel(Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View);

		// SYNC
		Gpu.accelerator.Synchronize();

		// Remove Security
		DecrementLiveCount();

		// Output
		return this;
	}
	
	public static Vec<T> operator +(Vec<T> vectorA) => vectorA.AbsXIP();
	public static Vec<T> operator +(Vec<T> vectorA, Vec<T> vectorB) => OP(vectorA, vectorB, Operations.add);
	public static Vec<T> operator +(Vec<T> vectorA, T scalar) => OP(vectorA, scalar, Operations.add);
	public static Vec<T> operator +(T scalar, Vec<T> vectorA) => OP(vectorA, scalar, Operations.add);
		
	public static Vec<T> operator -(Vec<T> vectorA, Vec<T> vectorB) => OP(vectorA, vectorB, Operations.subtract);
	public static Vec<T> operator -(Vec<T> vectorA, T scalar) => OP(vectorA, scalar, Operations.subtract);
	public static Vec<T> operator -(T scalar, Vec<T> vectorA) => OP(vectorA, scalar, Operations.flipSubtract);
		
	public static Vec<T> operator *(Vec<T> vectorA, Vec<T> vectorB) => OP(vectorA, vectorB, Operations.multiply);
	public static Vec<T> operator *(Vec<T> vectorA, T scalar) => OP(vectorA, scalar, Operations.multiply);
	public static Vec<T> operator *(T scalar, Vec<T> vectorA) => OP(vectorA, scalar, Operations.multiply);
		
	public static Vec<T> operator /(Vec<T> vectorA, Vec<T> vectorB) => OP(vectorA, vectorB, Operations.divide);
	public static Vec<T> operator /(Vec<T> vectorA, T scalar) => OP(vectorA, scalar, Operations.divide);
	public static Vec<T> operator /(T scalar, Vec<T> vectorA) => OP(vectorA, scalar, Operations.flipDivide);
		
	public static Vec<T> operator ^(Vec<T> vectorA, Vec<T> vectorB) => OP(vectorA, vectorB, Operations.pow);
	public static Vec<T> operator ^(Vec<T> vectorA, T scalar) => OP(vectorA, scalar, Operations.pow);
	public static Vec<T> operator ^(T scalar, Vec<T> vectorA) => OP(vectorA, scalar, Operations.flipPow);
	
}
