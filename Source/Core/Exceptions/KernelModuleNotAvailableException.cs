using System;
using BAVCL.Source.Core.Enums;

namespace BAVCL.Core.Exceptions;

public class KernelModuleNotAvailableException : Exception
{
	public KernelModuleNotAvailableException(KernelDomain domain, Type elementType)
		: base($"No '{domain}' kernel module exists for element type '{elementType.Name}'.")
	{
	}

	internal KernelModuleNotAvailableException(KernelDomainSharedModule domain, Type elementType)
		: base($"No '{domain}' kernel module exists for element type '{elementType.Name}'.")
	{
	}
}
