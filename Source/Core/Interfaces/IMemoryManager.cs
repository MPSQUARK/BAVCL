using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using BAVCL.Core.Enums;
using BAVCL.Core.Helpers;

namespace BAVCL.Core.Interfaces;

public interface IMemoryManager
{

    #region Allocate
    public (uint, MemoryBuffer) Allocate<T>(ICacheable<T> cacheable, Accelerator accelerator) where T : unmanaged;
    public (uint, MemoryBuffer) Allocate<T>(ICacheable cacheable, T[] values, Accelerator accelerator) where T : unmanaged;
    public (uint, MemoryBuffer) AllocateEmpty<T>(ICacheable cacheable, int length, Accelerator accelerator) where T : unmanaged;
    #endregion

    #region Update
    public (uint, MemoryBuffer) UpdateBuffer<T>(ICacheable<T> cacheable, Accelerator accelerator) where T : unmanaged;
    public (uint, MemoryBuffer) UpdateBuffer<T>(ICacheable cacheable, T[] values, Accelerator accelerator) where T : unmanaged;
    #endregion

    #region Get
    public MemoryBuffer? GetBuffer(uint Id);
    #endregion

    #region Garbage Collection
    public void GC(long memRequired);
    public uint GCItem(uint Id);
    public uint FreeBuffer(uint Id);
    #endregion

    #region Debug
    public HashSet<uint> StoredIDs();
    /// <summary>
    /// Checks whether the item with the given ID is stored in the memory manager.
    /// </summary>
    public virtual bool IsStored(uint id) => StoredIDs().Contains(id);
    public virtual string PrintMemoryUsage(bool percentage, string format = "F2", MemoryUsageUnit unit = MemoryUsageUnit.Auto)
    {
        string usage;

        if (percentage)
            usage = $"{(((double)MemoryUsed / (double)AvailableMemory) * 100f).ToString(format)}%";
        else
            usage = MemoryUsageFormatting.FormatAbsolute(MemoryUsed, AvailableMemory, unit, format);

        Console.WriteLine(usage);
        return usage;
    }
    #endregion

    #region Properties
    /// <summary>
    /// The amount of memory available to be used accounting for headroom. 
    /// AvailableMemory <= MaxMemory.
    /// </summary>
    public long AvailableMemory { get; set; }
    public long MemoryUsed { get; }
    protected internal long MemoryFree => AvailableMemory - MemoryUsed;
    #endregion

}
