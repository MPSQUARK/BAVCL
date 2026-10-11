using BAVCL.Core.Exceptions;

namespace BAVCL.Core.Helpers;

internal static class GpuValidation
{
	internal static void IsSameGpu(GPU left, GPU right)
	{
		if (ReferenceEquals(left, right))
			return;

		throw new GpuMismatchException();
	}

	internal static void IsSameVesselContainer(GPU vesselGpu, GPU containerGpu)
	{
		if (ReferenceEquals(vesselGpu, containerGpu))
			return;

		throw new VesselGpuMismatchException();
	}
}
