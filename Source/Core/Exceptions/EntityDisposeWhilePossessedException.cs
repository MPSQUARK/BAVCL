namespace BAVCL.Core.Exceptions;

public sealed class EntityDisposeWhilePossessedException()
	: System.Exception("Cannot dispose a buffer entity while it still possesses a vessel. Banish the soul first.");
