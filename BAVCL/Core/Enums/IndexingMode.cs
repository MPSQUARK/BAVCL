namespace BAVCL.Core.Enums;

/// <summary>
/// Bitwise flags for more control over indexing behavior.
/// </summary>
public enum IndexingMode
{
    Normal = 0,
    SyncCPU = 1,
    SyncGPU = 2,
    SyncBoth = 3,
}
