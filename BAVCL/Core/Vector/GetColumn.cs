using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vector
{
    public static Vector GetColumnAsVector(Vector vector, int column)
    {
        int[] select = [column, vector.Columns];
        Vector output = new(vector.Gpu, vector.RowCount());

        using (GpuScope.Pin(output, vector))
        {
            MemoryBuffer1D<float, Stride1D.Dense>
                buffer = output.GetBuffer(),
                buffer2 = vector.GetBuffer();

            MemoryBuffer1D<int, Stride1D.Dense> buffer3 = vector.Gpu.accelerator.Allocate1D(select);

            vector.Gpu.getSliceKernel(vector.Gpu.accelerator.DefaultStream, vector.RowCount(), buffer.View, buffer2.View, buffer3.View);
            vector.Gpu.accelerator.Synchronize();
            buffer3.Dispose();
        }

        return output;
    }

    public Vector GetColumnAsVector(int column)
    {
        int[] select = [column, Columns];
        Vector output = new(Gpu, RowCount());

        using (GpuScope.Pin(output, this))
        {
            MemoryBuffer1D<float, Stride1D.Dense>
                buffer = output.GetBuffer(),
                buffer2 = GetBuffer();

            MemoryBuffer1D<int, Stride1D.Dense> buffer3 = Gpu.accelerator.Allocate1D(select);

            Gpu.getSliceKernel(Gpu.accelerator.DefaultStream, RowCount(), buffer.View, buffer2.View, buffer3.View);
            Gpu.accelerator.Synchronize();
            buffer3.Dispose();
        }

        return output;
    }

    public static float[] GetColumnAsArray(Vector vector, int column) =>
        vector.GetColumnAsVector(column).ToArray();

    public float[] GetColumnAsArray(int column) =>
        GetColumnAsVector(column).ToArray();
}
