namespace BAVCL.Core.Exceptions;

public class GpuMismatchException(string message = "Operands must reside on the same GPU instance.") : System.Exception(message);
