using BAVCL.Core;
using ILGPU;
using ILGPU.Runtime;
using System;

namespace BAVCL;

public partial class Vector
{
    /// <summary>
    /// Takes the absolute value of all values in the Vector.
    /// IMPORTANT : Use this method for Vectors of Length less than 100,000
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector Abs(Vector vector) => vector.Copy().Abs_IP();
    
    /// <summary>
    /// Takes the absolute value of all values in this Vector.
    /// IMPORTANT : Use this method for Vectors of Length less than 100,000
    /// </summary>
    public Vector Abs_IP()
    {
        if (Min() > 0f)
            return this;

        using (var scope = this.CpuScopeAndSync())
        {
            EditableView<float> view = scope.View;
            for (int i = 0; i < Length; i++)
                view[i] = MathF.Abs(view[i]);
        }

        return this;
    }
    /// <summary>
    /// Runs on Accelerator. (GPU : Default)
    /// Takes the absolute value of all the values in the Vector.
    /// IMPORTANT : Use this method for Vectors of Length more than 100,000
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector AbsX(Vector vector) => vector.Copy().AbsX_IP();
    
    /// <summary>
    /// Runs on Accelerator. (GPU : Default)
    /// Takes the absolute value of all the values in this Vector.
    /// IMPORTANT : Use this method for Vectors of Length more than 100,000
    /// </summary>
    public Vector AbsX_IP()
    {
        using (GpuScope.Begin(this))
        {
            MemoryBuffer1D<float, Stride1D.Dense> buffer = GetBuffer();
            Gpu.absKernel(Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View);
            Gpu.accelerator.Synchronize();
        }

        return this;
    }
}
