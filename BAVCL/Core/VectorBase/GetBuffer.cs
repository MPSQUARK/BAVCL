using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Core;

public partial class VectorBase<T>
{
	public MemoryBuffer1D<T, Stride1D.Dense> GetBuffer()
	{
		if (ResidenceHelper.IsCpuAuthority(Residence) && !ResidenceHelper.IsActiveCpu(Residence))
			return (MemoryBuffer1D<T, Stride1D.Dense>)UpdateCache();

		return (MemoryBuffer1D<T, Stride1D.Dense>)(Gpu.TryGetBuffer<T>(ID) ?? Cache());
	}
}
