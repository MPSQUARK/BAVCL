using System.Collections.Generic;
using System.Linq;

namespace BAVCL.Modules.IO.Internal;

internal static class FloatArrayIo
{
	internal static float[] ParseDataElements(IEnumerable<string> elementValues) =>
		elementValues.Select(FloatIoParsing.ParseFloat).ToArray();
}
