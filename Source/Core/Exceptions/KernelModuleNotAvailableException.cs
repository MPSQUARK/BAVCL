using System;

namespace BAVCL.Core.Exceptions;

public class KernelModuleNotAvailableException(KernelDomain domain, Type elementType) : Exception(
$"No '{domain}' kernel module exists for element type '{elementType.Name}'.")
{ }
