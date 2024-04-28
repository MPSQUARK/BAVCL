using System;
using System.Numerics;
using BAVCL.Core.Enums;
using BAVCL.CustomMath;
using BAVCL.MemoryManagement;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    /// <summary>
    /// IMPORTANT : Use this method for Vectors of Length less than 100,000
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vec<T> Rsqrt(Vec<T> vector) => vector.Copy().Rsqrt_IP();

    /// <summary>
    /// IMPORTANT : Use this method for Vectors of Length less than 100,000
    /// </summary>
    public Vec<T> Rsqrt_IP()
    {
        SyncCPU();

        for (int i = 0; i < this.Length; i++)
            Values[i] = CMath.Rsqrt(Values[i]);

        UpdateCache();

        return this;
    }

    /// <summary>
    /// Runs on Accelerator. (GPU : Default)
    /// IMPORTANT : Use this method for Vectors of Length more than 100,000
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vec<T> RsqrtX(Vec<T> vector) => vector.Copy().Rsqrt_IP();

    /// <summary>
    /// Runs on Accelerator. (GPU : Default)
    /// IMPORTANT : Use this method for Vectors of Length more than 100,000
    /// </summary>
    public Vec<T> RsqrtX_IP()
    {
        // Secure data
        IncrementLiveCount();

        // Get the Memory buffer input/output
        MemoryBuffer1D<T, Stride1D.Dense> buffer = GetBuffer(); // IO

        // RUN
        var kernel = (Action<AcceleratorStream, Index1D, ArrayView<T>>)Gpu.GetKernel<T>(KernelType.Rsqrt);
        kernel(Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View);

        // SYNC
        Gpu.accelerator.Synchronize();

        // Remove Security
        DecrementLiveCount();

        // Output
        return this;
    }

}
