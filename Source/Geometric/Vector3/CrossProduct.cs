using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;
using System;

namespace BAVCL.Geometric
{
	public partial class Vector3
	{
		public static Vector3 Cross(Vector3 VectorA, Vector3 VectorB)
		{
			if (VectorA.Length != VectorB.Length) { throw new Exception($"Cannot Cross Product two Vector3's together of different lengths. {VectorA.Length} != {VectorB.Length}"); }

			GPU gpu = VectorA.gpu;

			Vector3 Output = new(gpu, VectorA.Length);

			VectorA.IncrementLiveCount();
			VectorB.IncrementLiveCount();
			Output.IncrementLiveCount();

			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = Output.GetBuffer(),        // Output
				buffer2 = VectorA.GetBuffer(),      // Input
				buffer3 = VectorB.GetBuffer();      // Input

			var kernel = gpu.GetKernel<CrossKernel>(Kernels.Cross);
			kernel(gpu.accelerator.DefaultStream, VectorA.Length / 3, buffer.View, buffer2.View, buffer3.View);

			gpu.accelerator.Synchronize();

			VectorA.DecrementLiveCount();
			VectorB.DecrementLiveCount();
			Output.DecrementLiveCount();

			return Output;

		}
	
	
	}
}
