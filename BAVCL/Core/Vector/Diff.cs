using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;
using System;

namespace BAVCL;

public partial class Vector
{
    public static Vector Diff(Vector vector)
    {
        if (vector.Columns > 1)
            throw new Exception("Diff is for use with 1D Vectors ONLY");

        GPU gpu = vector.Gpu;
        Vector output = new(gpu, vector.Length - 1, vector.Columns);

        using (GpuScope.Begin(output, vector))
        {
            MemoryBuffer1D<float, Stride1D.Dense>
                buffer = output.GetBuffer(),
                buffer2 = vector.GetBuffer();

            gpu.diffKernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View);
            gpu.accelerator.Synchronize();
        }

        return output;
    }

    public Vector Diff_IP() => TransferBuffer(Diff(this));
}
