using System;
using System.Collections.Concurrent;
using System.Numerics;
using BAVCL.Core.Helpers;

namespace BAVCL.Core.Memory;

/// <summary>
/// Typed lane of a <see cref="BufferPool"/> — rents and returns <see cref="BufferEntity{T}"/> for one element type.
/// </summary>
public sealed class ElementBufferPool<T> where T : unmanaged
{
	readonly GPU _gpu;
	readonly ConcurrentDictionary<int, ConcurrentBag<BufferSlot<T>>> _buckets = new();

	internal GPU Gpu => _gpu;

	internal ElementBufferPool(GPU gpu) => _gpu = gpu;

	/// <summary>Rents pooled GPU storage of at least <paramref name="length"/> elements.</summary>
	public BufferEntity<T> Rent(int length) =>
		new(this, AcquireSlot(length, capacity => new BufferSlot<T>(_gpu).Also(s => s.EnsureAllocated(capacity))), length);

	internal void Return(BufferSlot<T> slot)
	{
		if (slot.ID == 0)
			throw new InvalidOperationException("Cannot return an empty buffer slot to the pool.");

		_buckets.GetOrAdd(slot.Length, static _ => []).Add(slot);
	}

	internal void Clear() => _buckets.Clear();

	BufferSlot<T> AcquireSlot(int length, Func<int, BufferSlot<T>> factory)
	{
		int capacity = (int)BitOperations.RoundUpToPowerOf2((uint)Math.Max(length, 1));
		ConcurrentBag<BufferSlot<T>> bag = _buckets.GetOrAdd(capacity, static _ => []);

		if (bag.TryTake(out BufferSlot<T>? slot))
			return slot;

		return factory(capacity);
	}
}
