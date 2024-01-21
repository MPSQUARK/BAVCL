using ILGPU.Runtime;

namespace BAVCL.Core;

public interface ICacheable
{
	public uint LiveCount { get; }
	public uint ID { get; set; }
	public long MemorySize { get; }

	public void DeCache();

	public void IncrementLiveCount();

	public void DecrementLiveCount();

	public void SyncCPU();

	public void SyncCPU(MemoryBuffer buffer);

}

public interface ICacheable<T> : ICacheable where T : unmanaged
{
	public T[] GetValues();
}


