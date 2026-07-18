using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;
using System;
using System.Linq;

namespace BAVCL;

public partial class Vector
{
    public static Vector Reverse(Vector vector) =>
        new(vector.Gpu, vector.ToArray().Reverse().ToArray(), vector.Columns);

    public Vector Reverse_IP()
    {
        using (CpuScope(syncOnDispose: true))
        {
            ReadOnlySpan<float> src = GetCpuReadOnlySpan();
            float[] reversed = new float[src.Length];
            for (int i = 0; i < src.Length; i++)
                reversed[i] = src[src.Length - 1 - i];
            Value = reversed;
            Length = Value.Length;
        }

        return this;
    }

    public static Vector ReverseX(Vector vector) =>
        vector.Copy().ReverseX_IP();

    public Vector ReverseX_IP()
    {
        using (GpuScope.Pin(this))
        {
            MemoryBuffer1D<float, Stride1D.Dense> buffer = GetBuffer();
            Gpu.reverseKernel(Gpu.accelerator.DefaultStream, buffer.IntExtent >> 1, buffer.View);
            Gpu.accelerator.Synchronize();
        }

        return this;
    }
}
