namespace BAVCL.Core.Memory;

/// <summary>Internal pool custody object holding an LRU buffer ID between rentals.</summary>
internal sealed class BufferSlot<T> : CacheableBase<T> where T : unmanaged
{
	internal BufferSlot(GPU gpu) : base(gpu, [], cache: false) { }

	internal void EnsureAllocated(int capacity)
	{
		if (ID != 0)
			return;

		Length = capacity;
		CacheEmpty(capacity);
		Residence = Residence.Gpu;
	}
}
