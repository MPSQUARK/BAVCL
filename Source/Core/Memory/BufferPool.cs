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

	/// <summary>Pooled direct storage for <see cref="System.Numerics.Complex"/> values.</summary>
	public ElementBufferPool<System.Numerics.Complex> Complex { get; }

	/// <summary>Pooled <c>int64</c> buffers.</summary>
	public ElementBufferPool<long> Long { get; }

	/// <summary>Compatibility alias for the pooled complex storage.</summary>
	public ElementBufferPool<System.Numerics.Complex> Complex64 => Complex;

	internal BufferPool()
		: this(GPUManager.Default)
	{
	}

	internal BufferPool(GPU gpu)
	{
		Gpu = gpu;
		Int = new ElementBufferPool<int>(gpu);
		Float = new ElementBufferPool<float>(gpu);
		Complex = new ElementBufferPool<System.Numerics.Complex>(gpu);
		Long = new ElementBufferPool<long>(gpu);
	}

	/// <summary>Drops all pooled slots without freeing LRU buffers (for tests).</summary>
	internal void Clear()
	{
		Int.Clear();
		Float.Clear();
		Complex.Clear();
		Long.Clear();
	}
}
