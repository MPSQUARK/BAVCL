using System;

namespace BAVCL.Core.Exceptions;

public class KernelNotCompiledException(string kernelName, KernelDomain domain) 
: Exception($"Kernel '{kernelName}' is not compiled. Load KernelDomain.{domain}.")
{
    public string KernelName { get; } = kernelName;

    public KernelDomain? Domain { get; } = domain;
}
