using ILGPU.Runtime;
using System;
using BAVCL.Core.Interfaces;
using BAVCL.Core;

namespace BAVCL;

public sealed partial class GPU(Accelerator accelerator, IMemoryManager memoryManager) : IDisposable
{
	private readonly IMemoryManager _memoryManager = memoryManager;
	public Accelerator accelerator = accelerator;
	internal AcceleratorStream _defaultStream => accelerator.DefaultStream;
	internal void Synchronize() => accelerator.Synchronize();

	// Wrappers for Memory Manager
	public (uint, MemoryBuffer) Allocate<T>(ICacheable<T> cacheable) where T : unmanaged => _memoryManager.Allocate(cacheable, accelerator);
	public (uint, MemoryBuffer) Allocate<T>(ICacheable cacheable, T[] values) where T : unmanaged => _memoryManager.Allocate(cacheable, values, accelerator);
	public (uint, MemoryBuffer) AllocateEmpty<T>(ICacheable cacheable, int length) where T : unmanaged => _memoryManager.AllocateEmpty<T>(cacheable, length, accelerator);
	public MemoryBuffer? TryGetBuffer<T>(uint Id) where T : unmanaged => _memoryManager.GetBuffer(Id);
	public (uint, MemoryBuffer) UpdateBuffer<T>(ICacheable cacheable, T[] values) where T : unmanaged => _memoryManager.UpdateBuffer(cacheable, values, accelerator);
	public (uint, MemoryBuffer) UpdateBuffer<T>(ICacheable<T> cacheable) where T : unmanaged => _memoryManager.UpdateBuffer(cacheable, accelerator);
	public uint GCItem(uint Id) => _memoryManager.GCItem(Id);
	public string PrintMemoryUsage(bool percentage, string format = "F2") => _memoryManager.PrintMemoryUsage(percentage, format);
	public string GetMemUsage() => _memoryManager.MemoryUsed.ToString();

	public void Dispose() => accelerator.Dispose();
}