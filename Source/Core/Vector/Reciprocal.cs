using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL
{
	public partial class Vector
	{
		public static Vector Reciprocal(Vector vector) =>
			vector.Copy().Reciprocal_IP();

		public Vector Reciprocal_IP()
		{
			IncrementLiveCount();

			// Check if the input & output are in Cache
			MemoryBuffer1D<float, Stride1D.Dense> buffer = GetBuffer(); // IO

			var kernel = gpu.GetKernel<ReciprocalKernel>(Kernels.Reciprocal);
			kernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View);

			gpu.accelerator.Synchronize();

			DecrementLiveCount();

			return this;
		}

	}
}
