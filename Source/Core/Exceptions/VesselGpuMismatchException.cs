namespace BAVCL.Core.Exceptions;

public sealed class VesselGpuMismatchException()
	: GpuMismatchException("Vessel GPU does not match the possessing buffer entity's GPU.");