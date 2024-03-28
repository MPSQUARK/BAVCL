using System.Numerics;
using BAVCL.MemoryManagement;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
	public MemoryBuffer1D<T, Stride1D.Dense> GetBuffer() =>
		(MemoryBuffer1D<T, Stride1D.Dense>)(Gpu.TryGetBuffer<T>(ID) ?? Cache());
}
