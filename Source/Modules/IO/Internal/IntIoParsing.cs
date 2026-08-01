using System;
using System.Globalization;

namespace BAVCL.Modules.IO.Internal;

internal static class IntIoParsing
{
	internal static string FormatInt(int value) => value.ToString(CultureInfo.InvariantCulture);

	internal static int ParseInt(string text)
	{
		if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
			throw new FormatException($"'{text}' is not a valid int32.");

		return value;
	}
}
