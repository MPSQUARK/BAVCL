using ILGPU.Runtime;
using System;

namespace BAVCL.Core;

public interface ICacheable
{
	Residence Residence { get; set; }
	uint LiveCount { get; }
	uint ID { get; set; }
	long MemorySize { get; }

	void DeCache();
	internal void IncrementLiveCount();
	internal void DecrementLiveCount();
	void SyncCPU();
	void SyncCPU(MemoryBuffer buffer);
	MemoryBuffer UpdateCache();

	bool TrySetResidence(Residence expected, Residence next);
	void SetResidence(Residence residence);

	void EnterCpuScope();
	void ExitCpuScope(bool syncToGpu);
	void RollbackCpuScopeEnter();
}

public interface ICacheable<T> : ICacheable where T : unmanaged
{
	/// <summary>
	/// Syncs from GPU when needed, then returns a read-only CPU span. Use for all reads.
	/// For writes, use <see cref="CpuScope{T}"/> and <see cref="EditableView{T}"/>.
	/// </summary>
	ReadOnlySpan<T> RetrieveReadOnlySpan();

	/// <summary>
	/// Invokes <paramref name="edit"/> with writable CPU storage during an open <see cref="CpuScope{T}"/>.
	/// Implement explicitly; only <see cref="CpuScope{T}"/> should call this via <see cref="ICacheable{T}"/>.
	/// Pass <see cref="Memory{T}.Empty"/> to <paramref name="edit"/> when no editable surface exists.
	/// </summary>
	void EditCpu(Action<Memory<T>> edit);

	MemoryBuffer UpdateCache(T[] array);
}
