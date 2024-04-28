using System;
using System.Numerics;
using BAVCL.Core.Enums;
using BAVCL.MemoryManagement;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{

	public static Vec<T> Nan_to_num(Vec<T> vector, T num) => vector.Copy().Nan_to_num_IP(num);

	public Vec<T> Nan_to_num_IP(T num)
	{
		IncrementLiveCount();

		MemoryBuffer1D<T, Stride1D.Dense> buffer = GetBuffer();

		var kernel = (Action<AcceleratorStream, Index1D, ArrayView<T>, T>)Gpu.GetKernel<T>(KernelType.NanToNum);
		kernel(Gpu.accelerator.DefaultStream, Length, buffer.View, num);

		Gpu.accelerator.Synchronize();

		DecrementLiveCount();

		return this;
	}

}
