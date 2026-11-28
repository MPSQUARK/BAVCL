namespace BAVCL;

/// <summary>
/// Controls when host-initialized data is uploaded to the GPU.
/// </summary>
public enum Upload : byte
{
	/// <summary>Upload data to the GPU immediately on construction or assignment (<see cref="Core.Residence.InSync"/>).</summary>
	Now = 0,

	/// <summary>Keep data on the CPU until the first GPU operation (<see cref="Core.Residence.Cpu"/>).</summary>
	Defer = 1,
}
