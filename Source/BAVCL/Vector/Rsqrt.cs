
using BAVCL.Core.Enums;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vector
{
	/// <summary>
	/// Takes the absolute value of all values in the Vector.
	/// IMPORTANT : Use this method for Vectors of Length less than 100,000
	/// </summary>
	/// <param name="vector"></param>
	/// <returns></returns>
	public static Vector Rsqrt(Vector vector) => vector.Copy().Rsqrt_IP();

	/// <summary>
	/// Takes the absolute value of all values in this Vector.
	/// IMPORTANT : Use this method for Vectors of Length less than 100,000
	/// </summary>
	public Vector Rsqrt_IP()
	{
		SyncCPU();

		if (Min() > 0f) { return this; }

		for (int i = 0; i < this.Length; i++)
		{
			Value[i] = XMath.Rsqrt(Value[i]);
		}

		UpdateCache();

		return this;
	}

	/// <summary>
	/// Runs on Accelerator. (GPU : Default)
	/// Takes the absolute value of all the values in the Vector.
	/// IMPORTANT : Use this method for Vectors of Length more than 100,000
	/// </summary>
	/// <param name="vector"></param>
	/// <returns></returns>
	public static Vector RsqrtX(Vector vector) => vector.Copy().Rsqrt_IP();

	/// <summary>
	/// Runs on Accelerator. (GPU : Default)
	/// Takes the absolute value of all the values in this Vector.
	/// IMPORTANT : Use this method for Vectors of Length more than 100,000
	/// </summary>
	public Vector RsqrtX_IP()
	{
		// Secure data
		IncrementLiveCount();

		// Get the Memory buffer input/output
		MemoryBuffer1D<float, Stride1D.Dense> buffer = GetBuffer(); // IO

		// RUN
		var kernel = gpu.GetKernel<IOKernel>(Kernels.Rsqrt);
		kernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View);

		// SYNC
		gpu.accelerator.Synchronize();

		// Remove Security
		DecrementLiveCount();

		// Output
		return this;
	}
}