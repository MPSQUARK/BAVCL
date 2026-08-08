using System;
using BAVCL.Core.Helpers;
using ILGPU;
using ILGPU.Runtime;
using BAVCL.Core.Exceptions;

namespace BAVCL.Core.Memory;

/// <summary>
/// A rented pooled GPU buffer (entity). Dispose to return storage to the <see cref="BufferPool"/>.
/// Use <see cref="Possess{TCacheable}"/> with a <see cref="Vessel{TCacheable}"/> for domain APIs.
/// </summary>
public sealed class BufferEntity<T> : IDisposable where T : unmanaged
{
	readonly ElementBufferPool<T> _pool;
	readonly BufferSlot<T> _slot;
	readonly int _length;
	bool _disposed;
	bool _possessed;

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

	internal BufferEntity(ElementBufferPool<T> pool, BufferSlot<T> slot, int length)
	{
		_pool = pool;
		_slot = slot;
		_length = length;
	}

	/// <summary>
	/// Inhabits <paramref name="vessel"/> with this entity's buffer. Dispose the returned scope to Banish.
	/// </summary>
	public PossessionScope<TCacheable> Possess<TCacheable>(Vessel<TCacheable> vessel)
		where TCacheable : CacheableBase<T> =>
		new(this, vessel);

	internal void BeginPossession<TCacheable>(Vessel<TCacheable> vessel) where TCacheable : CacheableBase<T>
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		if (_possessed)
			throw new EntityAlreadyPossessedException();

		if (!vessel.IsVessel)
			throw new VesselAlreadyInhabitedException(typeof(TCacheable));

		GpuValidation.RequireSame(Gpu, vessel.Gpu);
		BufferIdTransfer.Possess(_slot, vessel.Target);
		_possessed = true;
	}

	internal void Banish<TCacheable>(Vessel<TCacheable> vessel) where TCacheable : CacheableBase<T>
	{
		if (!_possessed)
			return;

		BufferIdTransfer.Banish(vessel.Target, _slot);
		_possessed = false;
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		if (_possessed)
			throw new EntityDisposeWhilePossessedException();

		_pool.Return(_slot);
		_disposed = true;
	}

	/// <summary>RAII possession scope — dispose to Banish the soul.</summary>
	public struct PossessionScope<TCacheable> : IDisposable
		where TCacheable : CacheableBase<T>
	{
		readonly BufferEntity<T> _entity;
		readonly Vessel<TCacheable> _vessel;
		bool _disposed;

		/// <summary>The inhabited domain instance while possessed.</summary>
		public readonly TCacheable Soul => _vessel.Target;

		internal PossessionScope(BufferEntity<T> entity, Vessel<TCacheable> vessel)
		{
			_entity = entity;
			_vessel = vessel;
			_entity.BeginPossession(vessel);
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_entity.Banish(_vessel);
		}
	}
}
