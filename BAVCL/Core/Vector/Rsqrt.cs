using BAVCL.Core;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vector
{
    /// <summary>
    /// Takes the absolute value of all values in the Vector.
    /// IMPORTANT : Use this method for Vectors of Length less than 100,000
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector Rsqrt(Vector vector) => vector.Copy().Rsqrt_IP();

    /// <summary>
    /// Takes the absolute value of all values in this Vector.
    /// IMPORTANT : Use this method for Vectors of Length less than 100,000
    /// </summary>
    public Vector Rsqrt_IP()
    {
        using (var cpu = this.CpuScopeAndSync())
        {
            EditableView<float> view = cpu.View;
            for (int i = 0; i < Length; i++)
                view[i] = XMath.Rsqrt(view[i]);
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
    public static Vector RsqrtX(Vector vector) => vector.Copy().RsqrtX_IP();

    /// <summary>
    /// Runs on Accelerator. (GPU : Default)
    /// Takes the absolute value of all the values in this Vector.
    /// IMPORTANT : Use this method for Vectors of Length more than 100,000
    /// </summary>
    public Vector RsqrtX_IP()
    {
        using (GpuScope.Begin(this))
        {
            MemoryBuffer1D<float, Stride1D.Dense> buffer = GetBuffer();
            Gpu.rsqrtKernel(Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View);
            Gpu.accelerator.Synchronize();
        }

        return this;
    }


}
