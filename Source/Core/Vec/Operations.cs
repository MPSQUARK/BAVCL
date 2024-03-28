using System;
using System.Numerics;
using BAVCL.Core.Enums;
using BAVCL.MemoryManagement;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
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

	public Vec<T> IPOP(T scalar, Operations operation) => IPOP(this, scalar, operation);
	public static Vec<T> IPOP(Vec<T> vector, T scalar, Operations operation)
	{
		GPU gpu = vector.Gpu;

		vector.IncrementLiveCount();

		MemoryBuffer1D<T, Stride1D.Dense>
			buffer = vector.GetBuffer();

		var kernel = (Action<AcceleratorStream, Index1D, ArrayView<T>, T, SpecializedValue<int>>)gpu.GetKernel<T>(KernelType.ScalarIPOP);
		kernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, scalar, new SpecializedValue<int>((int)operation));

		gpu.accelerator.Synchronize();

		vector.DecrementLiveCount();

		return vector;
	}

}
