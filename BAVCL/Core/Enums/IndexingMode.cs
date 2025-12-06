namespace BAVCL.Core.Enums;

/// <summary>
/// Bitwise flags for more control over indexing behavior.
/// </summary>
public enum IndexingMode
{
    Normal = 0,
    NoCPUSync = 1,
    NoGPUSync = 2,
    NoSync = 3,
}
