using System;
using System.Numerics;
using BAVCL.Core.Enums;
using BAVCL.MemoryManagement;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
	public static Vec<T> Diff(Vec<T> vector)
	{
		if (vector.Columns > 1)
			throw new Exception("Diff is for use with 1D Vectors ONLY");

		GPU gpu = vector.Gpu;

		vector.IncrementLiveCount();

		// Make the Output Vector
		Vec<T> Output = new(gpu, vector.Length - 1, vector.Columns);
		Output.IncrementLiveCount();

		MemoryBuffer1D<T, Stride1D.Dense>
			buffer = Output.GetBuffer(),        // Output
			buffer2 = vector.GetBuffer();       // Input

		var kernel = (Action<AcceleratorStream, Index1D, ArrayView<T>, ArrayView<T>>)gpu.GetKernel<T>(KernelType.Diff);
		kernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View);

		gpu.accelerator.Synchronize();

		vector.DecrementLiveCount();
		Output.DecrementLiveCount();

		return Output;
	}

	public Vec<T> Diff_IP() => TransferBuffer(Diff(this));
}
