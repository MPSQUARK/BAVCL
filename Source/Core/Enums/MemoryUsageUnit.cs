namespace BAVCL.Core.Enums;

/// <summary>Display unit for <see cref="BAVCL.Core.Interfaces.IMemoryManager.PrintMemoryUsage"/> absolute mode.</summary>
public enum MemoryUsageUnit
{
	/// <summary>Pick B / KB / MB / GB from the larger of used vs available.</summary>
	Auto,
	Bytes,
	Kilobytes,
	Megabytes,
	Gigabytes,
}
