using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using System.Threading;
using BAVCL.Core.Interfaces;

namespace BAVCL;

public sealed partial class GPU(Accelerator accelerator, IMemoryManager memoryManager) : IDisposable
{
	private readonly IMemoryManager _memoryManager = memoryManager;
	private readonly Lock _kernelLoadLock = new();
	private readonly HashSet<(KernelDomain Domain, Type ElementType)> _loadedModules = [];
	public Accelerator accelerator = accelerator;
	internal AcceleratorStream DefaultStream => accelerator.DefaultStream;
	internal void Synchronize() => accelerator.Synchronize();

	/// <summary>
	/// Compiles a kernel module onto this device unless it is already loaded.
	/// Returns whether compilation actually happened.
	/// </summary>
	internal bool LoadModuleOnce(KernelDomain domain, Type elementType, Action<GPU> load)
	{
		var module = (domain, elementType);

		lock (_kernelLoadLock)
		{
			if (_loadedModules.Contains(module))
				return false;

			load(this);
			_loadedModules.Add(module);
			return true;
		}
	}

	// Wrappers for Memory Manager
	public (uint, MemoryBuffer) Allocate<T>(ICacheable<T> cacheable) where T : unmanaged => _memoryManager.Allocate(cacheable, accelerator);
	public (uint, MemoryBuffer) Allocate<T>(ICacheable cacheable, T[] values) where T : unmanaged => _memoryManager.Allocate(cacheable, values, accelerator);
	public (uint, MemoryBuffer) AllocateEmpty<T>(ICacheable cacheable, int length) where T : unmanaged => _memoryManager.AllocateEmpty<T>(cacheable, length, accelerator);
	public MemoryBuffer? TryGetBuffer<T>(uint Id) where T : unmanaged => _memoryManager.GetBuffer(Id);
	public (uint, MemoryBuffer) UpdateBuffer<T>(ICacheable cacheable, T[] values) where T : unmanaged => _memoryManager.UpdateBuffer(cacheable, values, accelerator);
	public (uint, MemoryBuffer) UpdateBuffer<T>(ICacheable<T> cacheable) where T : unmanaged => _memoryManager.UpdateBuffer(cacheable, accelerator);
    /// <summary>
    /// Will remove the buffer from memory and free the ID for reuse.
	/// Will auto-sync GPU data back onto the CPU if GPU data is newer than CPU data.
    /// </summary>
    public uint GCItem(uint Id) => _memoryManager.GCItem(Id);
	/// <summary>
	/// Will remove the buffer from memory and free the ID for reuse. 
	/// Warning: This will NOT sync GPU data back onto the CPU.
	/// </summary>
	public uint FreeBuffer(uint Id) => _memoryManager.FreeBuffer(Id);
	public string PrintMemoryUsage(bool percentage, string format = "F2") => _memoryManager.PrintMemoryUsage(percentage, format);
	public string GetMemUsage() => _memoryManager.MemoryUsed.ToString();

	public void Dispose() => accelerator.Dispose();
}