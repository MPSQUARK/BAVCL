using System;

namespace BAVCL.Core.Exceptions;

/// <summary>
/// Thrown when a call would hurt performance by misusing the library API
/// (e.g. in-place broadcast that would resize the left operand).
/// </summary>
public class PerformanceException : Exception
{
	public const string Prefix = "This operation will lead to degraded performance: ";

	public PerformanceException(string action)
		: base(Prefix + action) { }

	public PerformanceException()
		: this("Swap operand order OR use allocating overload 'OP'.") { }
}
