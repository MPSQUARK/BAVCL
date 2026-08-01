using System;
using System.Globalization;

namespace BAVCL.Modules.IO.Internal;

internal static class FloatIoParsing
{
	internal static string FormatFloat(float value) =>
		value switch
		{
			float.NaN => "NaN",
			float.PositiveInfinity => "Infinity",
			float.NegativeInfinity => "-Infinity",
			_ => value.ToString("G9", CultureInfo.InvariantCulture),
		};

	internal static float ParseFloat(string text) =>
		text switch
		{
			"NaN" => float.NaN,
			"Infinity" => float.PositiveInfinity,
			"-Infinity" => float.NegativeInfinity,
			_ => float.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture),
		};
}
