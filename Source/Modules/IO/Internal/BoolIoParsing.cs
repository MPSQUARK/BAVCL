using System;
using System.Globalization;

namespace BAVCL.Modules.IO.Internal;

internal static class BoolIoParsing
{
	internal static string FormatCsv(bool value) =>
		Convert.ToByte(value).ToString(CultureInfo.InvariantCulture);

	internal static string FormatXml(bool value) =>
		value.ToString().ToLowerInvariant();

	internal static bool ParseTxtCell(string trimmed)
	{
		if (trimmed is not ("0" or "1"))
			throw new FormatException($"TXT mask cell '{trimmed}' must be 0 or 1.");

		return trimmed[0] == '1';
	}

	internal static bool ParseWireToken(string token) =>
		token switch
		{
			"1" or "true" or "True" => true,
			"0" or "false" or "False" => false,
			_ => throw new FormatException($"Mask data contains invalid bool '{token}'."),
		};
}
