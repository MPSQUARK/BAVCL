namespace BAVCL.Core.Exceptions;

public sealed class VesselGpuMismatchException()
	: System.Exception("Vessel GPU does not match the possessing buffer entity's GPU.");
