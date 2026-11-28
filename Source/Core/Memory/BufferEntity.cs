using System;
using System.Threading;
using ILGPU;
using ILGPU.Runtime;
using BAVCL.Core.Exceptions;

namespace BAVCL.Core.Memory;

/// <summary>
/// A rented pooled GPU buffer (entity). Dispose to return storage to the <see cref="BufferPool"/>.
/// Use <see cref="Possess{TCacheable}"/> with an empty domain instance for domain APIs.
/// </summary>
public sealed class BufferEntity<T> : IDisposable where T : unmanaged
{
	readonly ElementBufferPool<T> _pool;
	readonly BufferSlot<T> _slot;
	readonly int _length;
	int _disposed;
	int _possessed;

	internal BufferSlot<T> Slot => _slot;

	/// <summary>Pin target for <see cref="GpuScope"/> on the kernel path.</summary>
	public ICacheable PinTarget => _slot;

	public GPU Gpu => _pool.Gpu;

	/// <summary>Logical rent length (≤ pooled capacity).</summary>
	public int Length => _length;

	/// <summary>Pooled GPU capacity in <typeparamref name="T"/> elements.</summary>
	public int Capacity => _slot.Length;

	public uint BufferId => _slot.ID;

	/// <summary>Kernel subview over the rented length.</summary>
	public ArrayView1D<T, Stride1D.Dense> View => _slot.GetBuffer().View.SubView(0, _length);

	/// <summary>Uploads the rented range from host memory.</summary>
	public void CopyFromCPU(T[] values) => _slot.GetBuffer().AsArrayView<T>(0, _length).CopyFromCPU(values);

	/// <summary>Downloads the rented range into host memory.</summary>
	public void CopyToCPU(T[] values) => _slot.GetBuffer().AsArrayView<T>(0, _length).CopyToCPU(values);

	/// <summary>CPU read view over <see cref="Length"/> after a pinned kernel launch.</summary>
	internal ReadOnlySpan<T> ReadRentedSpan() => _slot.RetrieveReadOnlySpan()[.._length];

	/// <summary>First rented element after a single-lane reduction or scalar kernel output.</summary>
	internal T ReadRentedFirst() => ReadRentedSpan()[0];

	internal BufferEntity(ElementBufferPool<T> pool, BufferSlot<T> slot, int length)
	{
		_pool = pool;
		_slot = slot;
		_length = length;
	}

	/// <summary>
	/// Inhabits an empty <paramref name="target"/> with this entity's pooled buffer. Dispose the returned scope to restore it to empty.
	/// </summary>
	public PossessionScope<TCacheable> Possess<TCacheable>(TCacheable target)
		where TCacheable : CacheableBase<T> =>
		new(this, target);

	internal void BeginPossession<TCacheable>(TCacheable target) where TCacheable : CacheableBase<T>
	{
		ArgumentNullException.ThrowIfNull(target);
		ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

		if (Interlocked.CompareExchange(ref _possessed, 1, 0) != 0)
			throw new EntityAlreadyPossessedException();

		bool targetClaimed = false;
		try
		{
			Guard.IsEmptyForPossession(target);
			Guard.MatchingGpu(Gpu, target.Gpu);
			Guard.AcquirePossession(target);
			targetClaimed = true;
			BufferIdTransfer.Possess(_slot, target);
		}
		catch
		{
			if (targetClaimed)
				target.ReleasePossession();

			Volatile.Write(ref _possessed, 0);
			throw;
		}
	}

	internal void Banish<TCacheable>(TCacheable target) where TCacheable : CacheableBase<T>
	{
		if (Interlocked.Exchange(ref _possessed, 0) == 0)
			return;

		BufferIdTransfer.Banish(target, _slot);
	}

	public void Dispose()
	{
		if (Volatile.Read(ref _disposed) != 0)
			return;

		if (Volatile.Read(ref _possessed) != 0)
			throw new EntityDisposeWhilePossessedException();

		if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
			return;

		_pool.Return(_slot);
	}

	/// <summary>RAII possession scope — dispose to return the pooled buffer and restore the target to empty.</summary>
	public struct PossessionScope<TCacheable> : IDisposable
		where TCacheable : CacheableBase<T>
	{
		readonly BufferEntity<T> _entity;
		readonly TCacheable _target;
		int _disposed;

		/// <summary>The inhabited domain instance while possessed.</summary>
		public readonly TCacheable Soul => _target;

		internal PossessionScope(BufferEntity<T> entity, TCacheable target)
		{
			_entity = entity;
			_target = target;
			_entity.BeginPossession(target);
		}

		public void Dispose()
		{
			if (Interlocked.Exchange(ref _disposed, 1) != 0)
				return;

			_entity.Banish(_target);
		}
	}
}

public static class BufferEntityExtensions
{
	/// <summary>Inhabits an empty <see cref="Types.Mask"/> with this entity's pooled word buffer.</summary>
	public static BufferEntity<int>.PossessionScope<Types.Mask> Possess(
		this BufferEntity<int> entity,
		Types.Mask target,
		int elementCount,
		int columns = 0)
	{
		ArgumentNullException.ThrowIfNull(entity);
		ArgumentNullException.ThrowIfNull(target);
		Guard.IsEmptyForPossession(target);

		target.SetPossessedLayout(elementCount, columns);
		try
		{
			return entity.Possess(target);
		}
		catch
		{
			target.RestoreEmptyState();
			throw;
		}
	}
}
