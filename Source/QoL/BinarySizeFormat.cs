using System;
using BAVCL.Core.Enums;

namespace BAVCL.QoL;

/// <summary>Format byte counts with binary SI-style suffixes (B, KB, MB, GB).</summary>
internal static class BinarySizeFormat
{
	internal static string Format(long bytes, BinaryByteUnit unit, string format)
	{
		BinaryByteUnit resolved = ResolveUnit(bytes, unit);
		double scale = Scale(resolved);
		return $"{(bytes / scale).ToString(format)}{Suffix(resolved)}";
	}

	internal static string FormatPair(long first, long second, BinaryByteUnit unit, string format)
	{
		BinaryByteUnit resolved = ResolveUnit(Math.Max(first, second), unit);
		double scale = Scale(resolved);
		string suffix = Suffix(resolved);
		return $"{(first / scale).ToString(format)}{suffix}/{(second / scale).ToString(format)}{suffix}";
	}

	static BinaryByteUnit ResolveUnit(long magnitude, BinaryByteUnit unit) =>
		unit == BinaryByteUnit.Auto ? PickAutoUnit(magnitude) : unit;

	static BinaryByteUnit PickAutoUnit(long bytes)
	{
		const long kiB = 1024L;
		long miB = kiB * kiB;
		long giB = miB * kiB;

		if (bytes >= giB)
			return BinaryByteUnit.Gigabytes;
		if (bytes >= miB)
			return BinaryByteUnit.Megabytes;
		if (bytes >= kiB)
			return BinaryByteUnit.Kilobytes;
		return BinaryByteUnit.Bytes;
	}

	static double Scale(BinaryByteUnit unit) =>
		unit switch
		{
			BinaryByteUnit.Bytes => 1d,
			BinaryByteUnit.Kilobytes => 1024d,
			BinaryByteUnit.Megabytes => 1024d * 1024d,
			BinaryByteUnit.Gigabytes => 1024d * 1024d * 1024d,
			_ => 1024d * 1024d,
		};

	static string Suffix(BinaryByteUnit unit) =>
		unit switch
		{
			BinaryByteUnit.Bytes => " B",
			BinaryByteUnit.Kilobytes => " KB",
			BinaryByteUnit.Megabytes => " MB",
			BinaryByteUnit.Gigabytes => " GB",
			_ => " MB",
		};
}
