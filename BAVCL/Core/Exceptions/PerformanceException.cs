using System;

namespace BAVCL.Core.Exceptions;

public class PerformanceException(string message) : Exception(message);
