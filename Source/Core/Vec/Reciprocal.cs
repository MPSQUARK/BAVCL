using System;
using System.Numerics;
using BAVCL.Core.Enums;
using BAVCL.MemoryManagement;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{

	public static Vec<T> Reciprocal(Vec<T> vector) => vector.Copy().Reciprocal_IP();

	public Vec<T> Reciprocal_IP()
	{
		IncrementLiveCount();

		// Check if the input & output are in Cache
		MemoryBuffer1D<T, Stride1D.Dense> buffer = GetBuffer(); // IO

		var kernel = (Action<AcceleratorStream, Index1D, ArrayView<T>>)Gpu.GetKernel<T>(KernelType.Reciprocal);
		kernel(Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View);

		Gpu.accelerator.Synchronize();

		DecrementLiveCount();

		return this;
	}
}
