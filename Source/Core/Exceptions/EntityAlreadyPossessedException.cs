namespace BAVCL.Core.Exceptions;

public sealed class EntityAlreadyPossessedException()
	: System.Exception("Buffer entity already possesses a vessel. Banish the current soul before possessing another.");
