using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vector
{
    public static Vector Reciprocal(Vector vector) =>
        vector.Copy().Reciprocal_IP();

    public Vector Reciprocal_IP()
    {
        using (GpuScope.Pin(this))
        {
            MemoryBuffer1D<float, Stride1D.Dense> buffer = GetBuffer();
            Gpu.rcpKernel(Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View);
            Gpu.accelerator.Synchronize();
        }

        return this;
    }
}
