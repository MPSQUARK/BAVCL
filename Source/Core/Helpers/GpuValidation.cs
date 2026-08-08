using BAVCL.Core.Exceptions;

namespace BAVCL.Core.Helpers;

internal static class GpuValidation
{
	internal static void RequireSame(GPU expected, GPU actual)
	{
		if (ReferenceEquals(expected, actual))
			return;

		throw new VesselGpuMismatchException();
	}
}
