namespace BAVCL.Core.Memory;

/// <summary>
/// Per-<see cref="GPU"/> pool of reusable GPU buffers bucketed by power-of-two capacity.
/// Use <see cref="Int"/> or <see cref="Float"/> to rent; dispose the entity to return storage.
/// </summary>
public sealed class BufferPool
{
	internal GPU Gpu { get; }

	/// <summary>Pooled <c>int32</c> buffers.</summary>
	public ElementBufferPool<int> Int { get; }

	/// <summary>Pooled <c>float32</c> buffers.</summary>
	public ElementBufferPool<float> Float { get; }

	internal BufferPool(GPU gpu)
	{
		Gpu = gpu;
		Int = new ElementBufferPool<int>(gpu);
		Float = new ElementBufferPool<float>(gpu);
	}

	/// <summary>Drops all pooled slots without freeing LRU buffers (for tests).</summary>
	internal void Clear()
	{
		Int.Clear();
		Float.Clear();
	}
}
