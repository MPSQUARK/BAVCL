using System.Collections.Generic;
using System.Linq;

namespace BAVCL.Modules.IO.Internal;

internal static class IntArrayIo
{
	internal static int[] ParseDataElements(IEnumerable<string> elementValues) =>
		elementValues.Select(IntIoParsing.ParseInt).ToArray();
}
