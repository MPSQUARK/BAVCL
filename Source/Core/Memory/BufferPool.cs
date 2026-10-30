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

	/// <summary>Pooled pair storage for <see cref="Complex64Device"/> values.</summary>
	public ElementBufferPool<Complex64Device> Complex64 { get; }

	internal BufferPool(GPU gpu)
	{
		Gpu = gpu;
		Int = new ElementBufferPool<int>(gpu);
		Float = new ElementBufferPool<float>(gpu);
		Complex64 = new ElementBufferPool<Complex64Device>(gpu);
	}

	/// <summary>Drops all pooled slots without freeing LRU buffers (for tests).</summary>
	internal void Clear()
	{
		Int.Clear();
		Float.Clear();
		Complex64.Clear();
	}
}
