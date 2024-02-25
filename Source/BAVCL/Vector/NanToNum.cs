using BAVCL.Core.Enums;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vector
{

	public static Vector Nan_to_num(Vector vector, float num) =>
		vector.Copy().Nan_to_num_IP(num);

	public Vector Nan_to_num_IP(float num)
	{
		IncrementLiveCount();

		MemoryBuffer1D<float, Stride1D.Dense> buffer = GetBuffer();

		var kernel = gpu.GetKernel<SIOKernel, float>(KernelType.NanToNum);
		kernel(gpu.accelerator.DefaultStream, Length, buffer.View, num);

		gpu.accelerator.Synchronize();

		DecrementLiveCount();

		return this;
	}

}