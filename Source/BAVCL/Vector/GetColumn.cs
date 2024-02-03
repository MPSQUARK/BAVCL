using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL
{
	public partial class Vector
	{

		public static Vector GetColumnAsVector(Vector vector, int column)
		{
			// Get a reference to the GPU
			GPU gpu = vector.gpu;
			
			// Get config data needed
			int[] select = new int[2] { column, vector.Columns };

			// Secure the Input
			vector.IncrementLiveCount();

			// Make Output Vector
			Vector Output = new(gpu, vector.Rows);

			// Secure the Output
			Output.IncrementLiveCount();

			// Get Memory buffer Data
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = Output.GetBuffer(),        // Output
				buffer2 = vector.GetBuffer();       // Input

			// Allocate config Data onto GPU
			MemoryBuffer1D<int, Stride1D.Dense>
				buffer3 = gpu.accelerator.Allocate1D(select);      // Config

			// RUN
			var kernel = gpu.GetKernel<AccessSliceKernel>(Kernels.Access); 
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

		public Vector GetColumnAsVector(int column)
		{
			// Get config data needed
			int[] select = new int[2] { column, Columns };

			// Secure the Input & Output
			IncrementLiveCount();

			// Make Output Vector
			Vector Output = new(gpu, Rows);

			Output.IncrementLiveCount();

			// Get Memory buffer Data
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = Output.GetBuffer(),        // Output
				buffer2 = GetBuffer();              // Input

			// Allocate config Data onto GPU
			MemoryBuffer1D<int, Stride1D.Dense>
				buffer3 = gpu.accelerator.Allocate1D(select);     // Config

			// RUN
			var kernel = gpu.GetKernel<AccessSliceKernel>(Kernels.Access);
			kernel(gpu.accelerator.DefaultStream, Rows, buffer.View, buffer2.View, buffer3.View);

			// SYNC
			gpu.accelerator.Synchronize();

			// Dispose of Config
			buffer3.Dispose();

			// Remove Security
			DecrementLiveCount();
			Output.DecrementLiveCount();

			return Output;
		}

		public static float[] GetColumnAsArray(Vector vector, int column)
		{
			Vector output = vector.GetColumnAsVector(column);
			output.DeCache();
			return output.Value;
		}

		public float[] GetColumnAsArray(int column)
		{
			Vector output = GetColumnAsVector(column);
			output.DeCache();
			return output.Value;
		}

	}


}
