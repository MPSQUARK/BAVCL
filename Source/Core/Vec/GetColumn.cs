using System;
using System.Numerics;
using BAVCL.Core.Enums;
using BAVCL.MemoryManagement;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{

	public static Vec<T> GetColumnAsVector(Vec<T> vector, int column)
	{
		// Get a reference to the GPU
		GPU gpu = vector.Gpu;

		// Get config data needed
		int[] select = [column, vector.Columns];

		// Secure the Input
		vector.IncrementLiveCount();

		// Make Output Vector
		Vec<T> Output = new(gpu, vector.Rows);

		// Secure the Output
		Output.IncrementLiveCount();

		// Get Memory buffer Data
		MemoryBuffer1D<T, Stride1D.Dense>
			buffer = Output.GetBuffer(),        // Output
			buffer2 = vector.GetBuffer();       // Input

		// Allocate config Data onto GPU
		MemoryBuffer1D<int, Stride1D.Dense>
			buffer3 = gpu.accelerator.Allocate1D(select);      // Config

		// RUN
		var kernel = (Action<AcceleratorStream, Index1D, ArrayView<T>, ArrayView<T>, ArrayView<int>>)gpu.GetKernel<T>(KernelType.Access);
		kernel(gpu.accelerator.DefaultStream, vector.Rows, buffer.View, buffer2.View, buffer3.View);

		// SYNC
		gpu.accelerator.Synchronize();

		// Dispose of Config
		buffer3.Dispose();

		// Remove Security
		Output.DecrementLiveCount();
		vector.DecrementLiveCount();

		return Output;
	}

	public Vec<T> GetColumnAsVector(int column)
	{
		// Get config data needed
		int[] select = [column, Columns];

		// Secure the Input & Output
		IncrementLiveCount();

		// Make Output Vector
		Vec<T> Output = new(Gpu, Rows);

		Output.IncrementLiveCount();

		// Get Memory buffer Data
		MemoryBuffer1D<T, Stride1D.Dense>
			buffer = Output.GetBuffer(),        // Output
			buffer2 = GetBuffer();              // Input

		// Allocate config Data onto GPU
		MemoryBuffer1D<int, Stride1D.Dense>
			buffer3 = Gpu.accelerator.Allocate1D(select);     // Config

		// RUN
		var kernel = (Action<AcceleratorStream, Index1D, ArrayView<T>, ArrayView<T>, ArrayView<int>>)Gpu.GetKernel<T>(KernelType.Access);
		kernel(Gpu.accelerator.DefaultStream, Rows, buffer.View, buffer2.View, buffer3.View);

		// SYNC
		Gpu.accelerator.Synchronize();

		// Dispose of Config
		buffer3.Dispose();

		// Remove Security
		DecrementLiveCount();
		Output.DecrementLiveCount();

		return Output;
	}

	public static T[] GetColumnAsArray(Vec<T> vector, int column) => vector.GetColumnAsArray(column);

	public T[] GetColumnAsArray(int column)
	{
		Vec<T> output = GetColumnAsVector(column);
		output.DeCache();
		return output.Values;
	}

}
