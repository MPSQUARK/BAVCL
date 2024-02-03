using ILGPU.Runtime;
using System;
using System.Collections.Generic;

namespace BAVCL.MemoryManagement;

public interface IMemoryManager
{
	#region Allocate
	public (uint, MemoryBuffer) Allocate<T>(ICacheable<T> Cacheable, Accelerator accelerator) where T : unmanaged;
	public (uint, MemoryBuffer) Allocate<T>(ICacheable Cacheable, T[] values, Accelerator accelerator) where T : unmanaged;
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
	#endregion

	#region Debug
	/// <summary>
	/// Checks if the item with the given Id is stored in the memory manager.
	/// </summary>
	/// <param name="Id"></param>
	/// <returns></returns>
	public virtual bool IsStored(uint Id) => StoredIDs().Contains(Id);
	/// <summary>
	/// Returns the Ids of all items stored in the memory manager.
	/// </summary>
	/// <returns></returns>
	public HashSet<uint> StoredIDs();
	public virtual string PrintMemoryUsage(bool percentage, string format = "F2")
	{
		string usage;

		if (percentage)
			usage = $"{(((double)MemoryUsed / (double)AvailableMemory) * 100f).ToString(format)}%";
		else
			usage = $"{(MemoryUsed >> 20).ToString(format)}/{(AvailableMemory >> 20).ToString(format)} MB";
		// TODO: Add view in Bytes/KB/GB too

		Console.WriteLine(usage);
		return usage;
	}
	#endregion

	#region Properties
	/// <summary>
	/// The amount of memory available to be used accounting for headroom. 
	/// AvailableMemory <= MaxMemory.
	/// </summary>
	public long AvailableMemory { get; init; }
	public long MemoryUsed { get; }
	protected internal long MemoryFree => AvailableMemory - MemoryUsed;
	#endregion

}
