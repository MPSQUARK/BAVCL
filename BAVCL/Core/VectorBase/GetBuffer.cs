using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Core;

public partial class VectorBase<T>
{
    /// <summary>
    /// NOTE: This does NOT update the Length property
    /// </summary>
    /// <returns></returns>
    public MemoryBuffer1D<T, Stride1D.Dense> GetBuffer() =>
        (MemoryBuffer1D<T, Stride1D.Dense>)(Gpu.TryGetBuffer<T>(ID) ?? Cache());

}
