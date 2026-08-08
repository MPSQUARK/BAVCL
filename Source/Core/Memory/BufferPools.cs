using System.Runtime.CompilerServices;

namespace BAVCL.Core.Memory;

/// <summary>Shared per-<see cref="GPU"/> <see cref="BufferPool"/> instances.</summary>
public static class BufferPools
{
	static readonly ConditionalWeakTable<GPU, BufferPool> _pools = [];

	/// <summary>Returns the buffer pool for <paramref name="gpu"/>, creating it on first use.</summary>
	public static BufferPool For(GPU gpu) =>
		_pools.GetValue(gpu, static g => new BufferPool(g));
}
