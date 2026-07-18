using ILGPU.Runtime;
using System;

namespace BAVCL.Core;

public interface ICacheable
{
	Residence Residence { get; set; }
	public uint LiveCount { get; }
	public uint ID { get; set; }
	public long MemorySize { get; }

	public void DeCache();

	public void IncrementLiveCount();

	public void DecrementLiveCount();

	public void SyncCPU();

	public void SyncCPU(MemoryBuffer buffer);
	public MemoryBuffer UpdateCache();
}

public interface ICacheable<T> : ICacheable where T : unmanaged
{
	/// <summary>
	/// Syncs from GPU when needed, then returns a read-only CPU span for upload.
	/// Prefer <see cref="VectorBase{T}.GetCpuReadOnlySpan"/> for user reads without sync.
	/// </summary>
	ReadOnlySpan<T> RetrieveReadOnlySpan();

	MemoryBuffer UpdateCache(T[] array);
}
