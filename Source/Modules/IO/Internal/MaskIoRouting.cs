using System;

namespace BAVCL.Modules.IO.Internal;

internal enum MaskWireFormat
{
	Bool,
	Packed,
}

internal static class MaskIoRouting
{
	internal static bool TryResolve(string? dtype, out MaskWireFormat format)
	{
		if (IoSchema.Dtype.Is(dtype, typeof(int)))
		{
			format = MaskWireFormat.Packed;
			return true;
		}

		if (IoSchema.Dtype.Is(dtype, typeof(bool)))
		{
			format = MaskWireFormat.Bool;
			return true;
		}

		format = default;
		return false;
	}

	internal static string UnsupportedDtypeMessage(string? dtype, string formatName) =>
		$"Mask {formatName} dtype '{dtype}' is not supported. Expected '{IoSchema.Dtype.Of<bool>()}' or '{IoSchema.Dtype.Of<int>()}'.";
}
