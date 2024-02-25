using BAVCL.Core.Enums;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Geometric;

public partial class Vector3
{

	public static Vector VOP(Vector3 vector, Operations operation)
	{
		GPU gpu = vector.gpu;

		vector.IncrementLiveCount();

		// Make the Output Vector
		Vector output = Vector.Zeros(gpu, vector.Rows);
		output.IncrementLiveCount();

		MemoryBuffer1D<float, Stride1D.Dense>
			buffer = output.GetBuffer(),        // Output
			buffer2 = vector.GetBuffer();      // Input

		var kernel = gpu.GetKernel<DualVectorOPKernel, float>(KernelType.SIMD);
		kernel(gpu.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer2.View, 3, new SpecializedValue<int>((int)operation));
		gpu.Synchronize();

		vector.DecrementLiveCount();
		output.DecrementLiveCount();

		return output;
	}

	public static Vector VOP(Vector3 vectorA, Vector3 vectorB, Operations operation)
	{
		GPU gpu = vectorA.gpu;

		vectorA.IncrementLiveCount();
		vectorB.IncrementLiveCount();

		// Make the Output Vector
		Vector output = Vector.Zeros(gpu, vectorA.Rows);
		output.IncrementLiveCount();

		MemoryBuffer1D<float, Stride1D.Dense>
			buffer = output.GetBuffer(),        // Output
			buffer2 = vectorA.GetBuffer(),      // Input
			buffer3 = vectorB.GetBuffer();      // Input

		var kernel = gpu.GetKernel<DualVectorOPKernel, float>(KernelType.SIMD);
		kernel(gpu.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer3.View, 3, new SpecializedValue<int>((int)operation));
		gpu.Synchronize();

		vectorA.DecrementLiveCount();
		vectorB.DecrementLiveCount();
		output.DecrementLiveCount();

		return output;
	}

	public static Vector3 OP(Vector3 vectorA, Vector3 vectorB, Operations operation)
	{
		GPU gpu = vectorA.gpu;

		vectorA.IncrementLiveCount();
		vectorB.IncrementLiveCount();

		// Make the Output Vector
		Vector3 Output = new(gpu, vectorA.Length);
		Output.IncrementLiveCount();

		// Check if the input & output are in Cache
		MemoryBuffer1D<float, Stride1D.Dense>
			buffer = Output.GetBuffer(),        // Output
			buffer2 = vectorA.GetBuffer(),      // Input
			buffer3 = vectorB.GetBuffer();      // Input

		// Run the kernel
		var kernel = gpu.GetKernel<A_FloatOPKernel, float>(KernelType.SeqOP);
		kernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer3.View, new SpecializedValue<int>((int)operation));

		// Synchronise the kernel
		gpu.accelerator.Synchronize();

		vectorA.DecrementLiveCount();
		vectorB.DecrementLiveCount();
		Output.DecrementLiveCount();

		// Return the result
		return Output;
	}
	public Vector3 OP(Vector3 vector, Operations operation)
	{
		GPU gpu = this.gpu;

		IncrementLiveCount();
		vector.IncrementLiveCount();

		// Make the Output Vector
		Vector3 Output = new(gpu, vector.Length);
		Output.IncrementLiveCount();

		// Check if the input & output are in Cache
		MemoryBuffer1D<float, Stride1D.Dense>
			buffer = Output.GetBuffer(),        // Output
			buffer2 = GetBuffer(),              // Input
			buffer3 = vector.GetBuffer();       // Input

		// Run the kernel
		var kernel = gpu.GetKernel<A_FloatOPKernel, float>(KernelType.SeqOP);
		kernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer3.View, new SpecializedValue<int>((int)operation));

		// Synchronise the kernel
		gpu.accelerator.Synchronize();

		DecrementLiveCount();
		vector.DecrementLiveCount();
		Output.DecrementLiveCount();

		// Return the result
		return Output;
	}


	public static Vector3 OP(Vector3 vector, float scalar, Operations operation)
	{
		GPU gpu = vector.gpu;

		vector.IncrementLiveCount();

		// Make the Output Vector
		Vector3 Output = new(gpu, vector.Length);

		Output.IncrementLiveCount();

		// Check if the input & output are in Cache
		MemoryBuffer1D<float, Stride1D.Dense>
			buffer = Output.GetBuffer(),        // Output
			buffer2 = vector.GetBuffer();       // Input

		var kernel = gpu.GetKernel<S_FloatOPKernel, float>(KernelType.ScalarOP);
		kernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, scalar, new SpecializedValue<int>((int)operation));

		gpu.accelerator.Synchronize();

		vector.DecrementLiveCount();
		Output.DecrementLiveCount();

		return Output;
	}
	public Vector3 OP(float scalar, Operations operation)
	{
		GPU gpu = this.gpu;

		IncrementLiveCount();

		// Make the Output Vector
		Vector3 Output = new(gpu, Length);

		Output.IncrementLiveCount();

		// Check if the input & output are in Cache
		MemoryBuffer1D<float, Stride1D.Dense>
			buffer = Output.GetBuffer(),        // Output
			buffer2 = this.GetBuffer();         // Input

		var kernel = gpu.GetKernel<S_FloatOPKernel, float>(KernelType.ScalarOP);
		kernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, scalar, new SpecializedValue<int>((int)operation));

		gpu.accelerator.Synchronize();

		DecrementLiveCount();
		Output.DecrementLiveCount();

		return Output;
	}


	public Vector3 OP_IP(Vector3 vector, Operations operation)
	{

		IncrementLiveCount();
		vector.IncrementLiveCount();

		// Check if the input & output are in Cache
		MemoryBuffer1D<float, Stride1D.Dense>
			buffer = GetBuffer(),               // IO
			buffer2 = vector.GetBuffer();       // Input

		// Run the kernel
		var kernel = gpu.GetKernel<A_FloatOPKernelIP, float>(KernelType.SeqIPOP);
		kernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, new SpecializedValue<int>((int)operation));

		// Synchronise the kernel
		gpu.accelerator.Synchronize();

		vector.DecrementLiveCount();
		DecrementLiveCount();

		return this;
	}
	public Vector3 OP_IP(float scalar, Operations operation)
	{
		IncrementLiveCount();

		// Check if the input & output are in Cache
		MemoryBuffer1D<float, Stride1D.Dense> buffer = GetBuffer(); // IO

		var kernel = gpu.GetKernel<S_FloatOPKernelIP, float>(KernelType.ScalarIPOP);
		kernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, scalar, new SpecializedValue<int>((int)operation));

		gpu.accelerator.Synchronize();

		DecrementLiveCount();

		return this;
	}

}