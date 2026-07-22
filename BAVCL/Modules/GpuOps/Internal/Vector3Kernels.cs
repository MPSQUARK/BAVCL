using BAVCL.Core;
using BAVCL.Geometric;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.GpuOps;

internal static class Vector3Kernels
{
	internal static void LaunchSimdVectorOp(
		GPU gpu,
		MemoryBuffer1D<float, Stride1D.Dense> output,
		MemoryBuffer1D<float, Stride1D.Dense> input,
		Operations operation)
	{
		gpu.simdVectorKernel(
			gpu.DefaultStream,
			output.IntExtent,
			output.View,
			input.View,
			input.View,
			3,
			new SpecializedValue<int>((int)operation));
		gpu.Synchronize();
	}

	internal static void LaunchSimdVectorBinaryOp(
		GPU gpu,
		MemoryBuffer1D<float, Stride1D.Dense> output,
		MemoryBuffer1D<float, Stride1D.Dense> left,
		MemoryBuffer1D<float, Stride1D.Dense> right,
		Operations operation)
	{
		gpu.simdVectorKernel(
			gpu.DefaultStream,
			output.IntExtent,
			output.View,
			left.View,
			right.View,
			3,
			new SpecializedValue<int>((int)operation));
		gpu.Synchronize();
	}
}
