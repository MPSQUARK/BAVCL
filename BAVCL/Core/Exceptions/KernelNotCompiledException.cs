using System;

namespace BAVCL.Core.Exceptions;

public class KernelNotCompiledException(string kernelName) : Exception(
$"Kernel: '{kernelName}' not compiled. Ensure that it has been compiled before use.")
{ }
