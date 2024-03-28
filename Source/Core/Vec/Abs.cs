using System;
using System.Numerics;
using BAVCL.Core.Enums;
using BAVCL.MemoryManagement;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    public Vec<T> AbsXIP()
    {
        // Secure data
        IncrementLiveCount();

        // Get the Memory buffer input/output
        MemoryBuffer1D<T, Stride1D.Dense> buffer = GetBuffer(); // IO

        // RUN
        var kernel = (Action<AcceleratorStream, Index1D, ArrayView<T>>)Gpu.GetKernel<T>(KernelType.Abs);
        kernel(Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View);

        // SYNC
        Gpu.accelerator.Synchronize();

        // Remove Security
        DecrementLiveCount();

        // Output
        return this;
    }
}
