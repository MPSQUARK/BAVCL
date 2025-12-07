

using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Geometric
{

	public partial class Vector3
	{

		public static Vector VOP(Vector3 vector, Operations operation)
		{
			GPU gpu = vector.Gpu;

			vector.IncrementLiveCount();

			// Make the Output Vector
			Vector output = Vector.Zeros(gpu, vector.RowCount());
			output.IncrementLiveCount();

			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),        // Output
				buffer2 = vector.GetBuffer();      // Input

			gpu.simdVectorKernel(gpu.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer2.View, 3, new SpecializedValue<int>((int)operation));
			gpu.Synchronize();

			vector.DecrementLiveCount();
			output.DecrementLiveCount();

			return output;
		}

		public static Vector VOP(Vector3 vectorA, Vector3 vectorB, Operations operation)
		{
			GPU gpu = vectorA.Gpu;

			vectorA.IncrementLiveCount();
			vectorB.IncrementLiveCount();

			// Make the Output Vector
			Vector output = Vector.Zeros(gpu, vectorA.RowCount());
			output.IncrementLiveCount();

			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),        // Output
				buffer2 = vectorA.GetBuffer(),      // Input
				buffer3 = vectorB.GetBuffer();      // Input

			gpu.simdVectorKernel(gpu.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer3.View, 3, new SpecializedValue<int>((int)operation));
			gpu.Synchronize();

			vectorA.DecrementLiveCount();
			vectorB.DecrementLiveCount();
			output.DecrementLiveCount();

			return output;
		}

		public static Vector3 OP(Vector3 vectorA, Vector3 vectorB, Operations operation)
		{
			GPU gpu = vectorA.Gpu;

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
			gpu.a_opFKernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer3.View, new SpecializedValue<int>((int)operation));

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
			GPU gpu = this.Gpu;

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
			gpu.a_opFKernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer3.View, new SpecializedValue<int>((int)operation));

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
			GPU gpu = vector.Gpu;

			vector.IncrementLiveCount();

			// Make the Output Vector
			Vector3 Output = new(gpu, vector.Length);

			Output.IncrementLiveCount();

			// Check if the input & output are in Cache
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = Output.GetBuffer(),        // Output
				buffer2 = vector.GetBuffer();       // Input

			gpu.s_opFKernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, scalar, new SpecializedValue<int>((int)operation));

			gpu.accelerator.Synchronize();

			vector.DecrementLiveCount();
			Output.DecrementLiveCount();

			return Output;
		}
		public Vector3 OP(float scalar, Operations operation)
		{
			GPU gpu = this.Gpu;

			IncrementLiveCount();

			// Make the Output Vector
			Vector3 Output = new(gpu, Length);

			Output.IncrementLiveCount();

			// Check if the input & output are in Cache
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = Output.GetBuffer(),        // Output
				buffer2 = this.GetBuffer();         // Input

			gpu.s_opFKernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, scalar, new SpecializedValue<int>((int)operation));

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
			Gpu.a_FloatOPKernelIP(Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, new SpecializedValue<int>((int)operation));

			// Synchronise the kernel
			Gpu.accelerator.Synchronize();

			vector.DecrementLiveCount();
			DecrementLiveCount();

			return this;
		}
		public Vector3 OP_IP(float scalar, Operations operation)
		{
			IncrementLiveCount();

			// Check if the input & output are in Cache
			MemoryBuffer1D<float, Stride1D.Dense> buffer = GetBuffer(); // IO

			Gpu.s_FloatOPKernelIP(Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, scalar, new SpecializedValue<int>((int)operation));

			Gpu.accelerator.Synchronize();

			DecrementLiveCount();

			return this;
		}





	}


}
