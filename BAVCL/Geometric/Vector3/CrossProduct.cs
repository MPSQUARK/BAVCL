using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;
using System;

namespace BAVCL.Geometric;

public partial class Vector3
{
    public static Vector3 Cross(Vector3 vectorA, Vector3 vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new Exception($"Cannot Cross Product two Vector3's together of different lengths. {vectorA.Length} != {vectorB.Length}");

        GPU gpu = vectorA.Gpu;
        Vector3 output = new(gpu, vectorA.Length);

        using (GpuScope.Begin(output, vectorA, vectorB))
        {
            MemoryBuffer1D<float, Stride1D.Dense>
                buffer = output.GetBuffer(),
                buffer2 = vectorA.GetBuffer(),
                buffer3 = vectorB.GetBuffer();

            gpu.crossKernel(gpu.accelerator.DefaultStream, vectorA.Length / 3, buffer.View, buffer2.View, buffer3.View);
            gpu.accelerator.Synchronize();
        }

        return output;
    }
}
