using System;
using System.Linq;
using System.Numerics;
using BAVCL.Core.Enums;
using BAVCL.MemoryManagement;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    public static Vec<T> Reverse(Vec<T> vector) =>
        new(vector.Gpu, vector.Values.Reverse().ToArray(), vector.Columns);

    public Vec<T> Reverse_IP()
    {
        SyncCPU();
        UpdateCache(Values.Reverse().ToArray());
        return this;
    }
    public static Vec<T> ReverseX(Vec<T> vector) => vector.Copy().ReverseX_IP();

    public Vec<T> ReverseX_IP()
    {
        IncrementLiveCount();

        // Check if the input & output are in Cache
        MemoryBuffer1D<T, Stride1D.Dense> buffer = GetBuffer(); // IO

        var kernel = (Action<AcceleratorStream, Index1D, ArrayView<T>>)Gpu.GetKernel<T>(KernelType.Reverse);
        kernel(Gpu.accelerator.DefaultStream, buffer.IntExtent >> 1, buffer.View);

        Gpu.accelerator.Synchronize();

        DecrementLiveCount();

        return this;
    }
}
