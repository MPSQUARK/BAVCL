namespace BAVCL.Core.Enums;

/// <summary>Which mask lane compact-gather kernels emit (maps to kernel specialization 0 = true, 1 = complement).</summary>
internal enum CompactGatherLane
{
	True = 0,
	Complement = 1,
}
