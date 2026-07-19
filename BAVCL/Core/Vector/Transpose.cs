using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;
using System;

namespace BAVCL;

public partial class Vector
{
    public static Vector Transpose(Vector vector)
    {
        if (vector.Is1D() || vector.Columns >= vector.Length) { throw new Exception("Cannot transpose 1D Vector"); }

        Vector output = new(vector.Gpu, vector.Length, vector.RowCount());

        using (GpuScope.Begin(output, vector))
        {
            MemoryBuffer1D<float, Stride1D.Dense>
                buffer = output.GetBuffer(),
                buffer2 = vector.GetBuffer();

            vector.Gpu.transposekernel(vector.Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, vector.Columns);
            vector.Gpu.accelerator.Synchronize();
        }

        return output;
    }

    public Vector Transpose_IP() => TransferBuffer(Transpose(this), true);
}
