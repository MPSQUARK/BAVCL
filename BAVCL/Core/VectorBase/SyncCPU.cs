using ILGPU.Runtime;

namespace BAVCL.Core;

public partial class VectorBase<T>
{
	public void SyncCPU()
	{
		if (ResidenceHelper.IsInSync(Residence) || ResidenceHelper.IsCpuAuthority(Residence))
			return;

		if (ID != 0)
			Value = Pull();

		Length = Value.Length;

		Residence current = Residence;
		if (!TrySetResidence(current, Residence.InSync))
			SetResidence(Residence.InSync);
	}

	public void SyncCPU(MemoryBuffer buffer)
	{
		if (ResidenceHelper.IsInSync(Residence) || ResidenceHelper.IsCpuAuthority(Residence))
			return;

		if (Value == null || Value.Length != buffer.Length)
			Value = new T[buffer.Length];

		buffer.AsArrayView<T>(0, buffer.Length).CopyToCPU(Value);
		Length = Value.Length;

		Residence current = Residence;
		if (!TrySetResidence(current, Residence.InSync))
			SetResidence(Residence.InSync);
	}
}
