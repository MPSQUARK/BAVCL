namespace BAVCL;

/// <summary>
/// Controls when host data is uploaded to the GPU. Used by constructors, assignment,
/// operation results, and CPU scope exit.
/// </summary>
public enum Upload : byte
{
	/// <summary>Upload to the GPU now (<see cref="Core.Residence.InSync"/>).</summary>
	Now = 0,

	/// <summary>Keep data on the CPU until the next GPU operation needs it (<see cref="Core.Residence.Cpu"/>).</summary>
	Defer = 1,
}
