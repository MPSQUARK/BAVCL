using System;
using System.Numerics;
using BAVCL.Core.Enums;
using BAVCL.MemoryManagement;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    public static Vec<T> Transpose(Vec<T> vector)
    {
        if (vector.Columns == 1 || vector.Columns >= vector.Length) { throw new Exception("Cannot transpose 1D Vector"); }

        // Get reference to gpu
        GPU gpu = vector.Gpu;

        // Prevent from decache
        vector.IncrementLiveCount();

        // Make the Output Vector
        Vec<T> Output = new(gpu, vector.Length, (uint)vector.Rows);

        // Prevent from decache
        Output.IncrementLiveCount();

        MemoryBuffer1D<T, Stride1D.Dense>
            buffer = Output.GetBuffer(), // Output
            buffer2 = vector.GetBuffer(); // Input

        var kernel = (Action<AcceleratorStream, Index1D, ArrayView<T>, ArrayView<T>, int>)gpu.GetKernel<T>(KernelType.Transpose);
        kernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, (int)vector.Columns);

        gpu.accelerator.Synchronize();

        vector.DecrementLiveCount();
        Output.DecrementLiveCount();

        return Output;
    }
    public Vec<T> Transpose_IP()
    {
        return TransferBuffer(Transpose(this), true);
    }

}
