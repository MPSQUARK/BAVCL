namespace BAVCL.Core.Exceptions;

public sealed class VesselAlreadyInhabitedException(System.Type vesselType)
	: System.Exception($"Vessel of type {vesselType.Name} is already inhabited (ID != 0) and cannot be possessed.");
