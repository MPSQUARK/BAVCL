namespace BAVCL.Core.Enums;

/// <summary>Binary (1024-based) scale for formatting byte counts (e.g. GPU memory display).</summary>
public enum BinaryByteUnit
{
	/// <summary>Pick B / KB / MB / GB from the magnitude of the value(s) being formatted.</summary>
	Auto,
	Bytes,
	Kilobytes,
	Megabytes,
	Gigabytes,
}
