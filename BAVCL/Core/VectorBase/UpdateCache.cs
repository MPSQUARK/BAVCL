using ILGPU.Runtime;

namespace BAVCL.Core;

public partial class VectorBase<T>
{
	// TODO: Split into 2 methods, one to Update ONLY, and one to update + return. to skip on the GetBuffer calls
	public MemoryBuffer UpdateCache()
	{
		if (ResidenceHelper.IsInSync(Residence) || ResidenceHelper.IsGpuAuthority(Residence))
			return GetBuffer();

		if (ResidenceHelper.IsActiveCpu(Residence))
			return GetBuffer();

		Length = Value.Length;
		(ID, MemoryBuffer buffer) = Gpu.UpdateBuffer(this);

		Residence current = Residence;
		if (!TrySetResidence(current, Residence.InSync))
			SetResidence(Residence.InSync);
		return buffer;
	}

	public MemoryBuffer UpdateCache(T[] array)
	{
		Length = array.Length;
		Value = array;
		(ID, MemoryBuffer buffer) = Gpu.UpdateBuffer(this, array);

		Residence current = Residence;
		if (!TrySetResidence(current, Residence.InSync))
			SetResidence(Residence.InSync);
		return buffer;
	}
}
