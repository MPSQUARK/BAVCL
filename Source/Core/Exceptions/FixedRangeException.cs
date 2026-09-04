using System;

namespace BAVCL.Core.Exceptions;

/// <summary>Thrown when a value falls outside an inclusive fixed numeric range.</summary>
public sealed class FixedRangeException(string paramName, object? actualValue, object min, object max) 
    : Exception($"Value must be between {min} and {max}.")
{
    public string ParamName { get; } = paramName;

    public object? ActualValue { get; } = actualValue;

    public object Min { get; } = min;

    public object Max { get; } = max;
}
