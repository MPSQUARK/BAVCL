using ILGPU;
using ILGPU.Runtime;
using System;
using BAVCL.Core;

namespace BAVCL;

public partial class Vector
{
    /// <summary>
    /// Concatinates VectorB onto the end of VectorA.
    /// Preserves the value of Columns of VectorA.
    /// </summary>
    /// <param name="vectorA"></param>
    /// <param name="vectorB"></param>
    /// <returns></returns>
    public static Vector Concat(Vector vectorA, Vector vectorB, char axis = 'r', bool warp = false)
    {
        return vectorA.Copy().Concat_IP(vectorB, axis, warp);
    }
    public Vector Concat_IP(Vector vector, char axis = 'r', bool warp = false)
    {
        if (axis == 'r')
        {
            this.Append_IP(vector);
            return this;
        }

        // IF Concat in COLUMN mode

        // IF 2D
        if (Columns > 1 && vector.Columns > 1)
        {
            if ((RowCount() != vector.RowCount()) && (RowCount() != vector.Columns))
            {
                throw new Exception(
                    $"Vectors CANNOT be appended. " +
                    $"This Vector has the shape ({this.RowCount()},{this.Columns}). " +
                    $"The 2D Vector being appended has the shape ({vector.RowCount()},{vector.Columns})");
            }

            if (RowCount() == vector.Columns)
            {
                if (!warp)
                {
                    vector.Transpose_IP();
                }

                if (warp && (vector.Length % RowCount() == 0))
                {
                    vector.Columns = vector.Length / RowCount();
                }

            }

        }
        // IF 1D
        if (vector.Is1D())
        {

            if (vector.Length % RowCount() != 0)
            {
                throw new Exception($"Vectors CANNOT be appended. " +
                    $"This array has shape ({RowCount()},{Columns}), 1D vector being appended has {vector.Length} Length");
            }

            vector.Columns = vector.Length / RowCount();

        }

        Vector Output = new(Gpu, vector.Length + Length);

        using (GpuScope.Pin(Output, this, vector))
        {
            MemoryBuffer1D<float, Stride1D.Dense>
                buffer = Output.GetBuffer(),
                buffer2 = GetBuffer(),
                buffer3 = vector.GetBuffer();

            Gpu.appendKernel(Gpu.accelerator.DefaultStream, RowCount(), buffer.View, buffer2.View, buffer3.View, Columns, vector.Columns);
            Gpu.accelerator.Synchronize();
        }

        this.Columns += vector.Columns;

        return TransferBuffer(Output);
    }

}
