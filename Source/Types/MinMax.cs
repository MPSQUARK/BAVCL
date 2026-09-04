namespace BAVCL.Types;

/// <summary>Minimum and maximum of a numeric sample.</summary>
public readonly record struct MinMax<T>(T Min, T Max) where T : unmanaged;
