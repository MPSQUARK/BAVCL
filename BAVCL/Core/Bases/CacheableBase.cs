using BAVCL.Core.Enums;
using ILGPU;
using ILGPU.Runtime;
using System;
using System.Threading;

namespace BAVCL.Core;

/// <summary>
/// GPU/CPU coherence and LRU-backed memory for cacheable data types.
/// </summary>
public abstract class CacheableBase<T> : ICacheable<T> where T : unmanaged
{
	internal GPU Gpu;

	internal T[] Value = [];

	ResidenceField _residence;
	Residence _cpuScopeResidenceBeforeOuterEnter;

	internal int _cpuScopeDepth;

	protected volatile internal uint _id;
	protected volatile internal uint _livecount;
	protected volatile internal int _length;

	public Residence Residence
	{
		get => _residence.Value;
		set => _residence.Value = value;
	}

	public int Length
	{
		get => _length;
		set => _length = value;
	}

	public uint ID
	{
		get => _id;
		set => _id = value >= 0 ? value : throw new Exception($"ID CANNOT be less than 0. Recieved: {value}");
	}

	public virtual long MemorySize => (long)Interop.SizeOf<T>() * (long)Length;

	public uint LiveCount
	{
		get => _livecount;
		set => _livecount = value;
	}

	protected CacheableBase(GPU gpu, T[] value, bool cache = true)
	{
		Gpu = gpu;
		Value = value;
		Length = value.Length;

		if (cache)
		{
			Cache(value);
			Residence = Residence.InSync;
			return;
		}

		Residence = Residence.Cpu;
	}

	/// <summary>
	/// Creates GPU-backed storage of specified length.
	/// Warning: may contain uninitialized data — zero or fill before use.
	/// </summary>
	protected CacheableBase(GPU gpu, int length)
	{
		Gpu = gpu;
		Value = [];
		Length = length;
		CacheEmpty(length);
		Residence = Residence.Gpu;
	}

	public T[] Pull()
	{
		MemoryBuffer1D<T, Stride1D.Dense> buffer = GetBuffer();
		T[] values = new T[buffer.Length];
		buffer.AsArrayView<T>(0, buffer.Length).CopyToCPU(values);
		return values;
	}

	public ReadOnlySpan<T> RetrieveReadOnlySpan()
	{
		SyncCPU();
		return GetCpuReadOnlySpan();
	}

	public ReadOnlySpan<T> GetCpuReadOnlySpan() => Value.AsSpan(0, Length);

	public T[] ToArray()
	{
		ReadOnlySpan<T> data = RetrieveReadOnlySpan();
		T[] copy = new T[data.Length];
		data.CopyTo(copy);
		return copy;
	}

	void ICacheable<T>.EditCpu(Action<Memory<T>> edit)
	{
		if (_cpuScopeDepth == 0 || !ResidenceHelper.IsActiveCpu(Residence))
			throw new InvalidOperationException(
				$"{nameof(ICacheable<T>.EditCpu)} requires an open {nameof(CpuScope<T>)}.");

		edit(Value.AsMemory(0, Length));
	}

	public void SyncCPU()
	{
		if (ResidenceHelper.IsInSync(Residence) || ResidenceHelper.IsCpuAuthority(Residence))
			return;

		if (ID != 0)
			Value = Pull();

		Length = Value.Length;
		ResidenceScopeHelper.TransitionOrReconcile(this, Residence, Residence.InSync);
	}

	public void SyncCPU(MemoryBuffer buffer)
	{
		if (ResidenceHelper.IsInSync(Residence) || ResidenceHelper.IsCpuAuthority(Residence))
			return;

		if (Value == null || Value.Length != buffer.Length)
			Value = new T[buffer.Length];

		buffer.AsArrayView<T>(0, buffer.Length).CopyToCPU(Value);
		Length = Value.Length;
		ResidenceScopeHelper.TransitionOrReconcile(this, Residence, Residence.InSync);
	}

	public MemoryBuffer1D<T, Stride1D.Dense> GetBuffer()
	{
		if (ResidenceHelper.IsCpuAuthority(Residence) && !ResidenceHelper.IsActiveCpu(Residence))
			return (MemoryBuffer1D<T, Stride1D.Dense>)UpdateCache();

		return (MemoryBuffer1D<T, Stride1D.Dense>)(Gpu.TryGetBuffer<T>(ID) ?? Cache());
	}

	public MemoryBuffer UpdateCache()
	{
		if (ResidenceHelper.IsInSync(Residence) || ResidenceHelper.IsGpuAuthority(Residence))
			return GetBuffer();

		if (ResidenceHelper.IsActiveCpu(Residence))
			return GetBuffer();

		Length = Value.Length;
		(ID, MemoryBuffer buffer) = Gpu.UpdateBuffer(this);
		ResidenceScopeHelper.TransitionOrReconcile(this, Residence, Residence.InSync);
		return buffer;
	}

	public MemoryBuffer UpdateCache(T[] array)
	{
		Length = array.Length;
		Value = array;
		(ID, MemoryBuffer buffer) = Gpu.UpdateBuffer(this, array);
		ResidenceScopeHelper.TransitionOrReconcile(this, Residence, Residence.InSync);
		return buffer;
	}

	public void DeCache()
	{
		if (ID == 0)
			return;

		if (LiveCount != 0)
			return;

		SyncCPU();
		ID = Gpu.FreeBuffer(ID);
		Residence = Residence.Cpu;
	}

	public void IncrementLiveCount() => Interlocked.Increment(ref _livecount);

	public void DecrementLiveCount() => Interlocked.Decrement(ref _livecount);

	public void EnterCpuScope()
	{
		ResidenceHelper.GuardCrossContext(Residence, enteringCpu: true);

		if (Interlocked.Increment(ref _cpuScopeDepth) != 1)
			return;

		_cpuScopeResidenceBeforeOuterEnter = Residence;

		try
		{
			Residence current = Residence;
			if (!ResidenceHelper.IsActiveCpu(current))
				ResidenceScopeHelper.TransitionOrReconcile(this, current, Residence.ActiveCpu);

			SyncCPU();
		}
		catch
		{
			RollbackCpuScopeEnter();
			throw;
		}
	}

	public void ExitCpuScope(bool syncToGpu)
	{
		if (Interlocked.Decrement(ref _cpuScopeDepth) != 0)
			return;

		CommitCpuView();
		ResidenceScopeHelper.TransitionOrReconcile(this, Residence.ActiveCpu, Residence.Cpu);

		if (syncToGpu)
			UpdateCache();
	}

	public void RollbackCpuScopeEnter()
	{
		if (Interlocked.Decrement(ref _cpuScopeDepth) != 0)
			return;

		if (!ResidenceHelper.IsActiveCpu(Residence))
			return;

		ResidenceScopeHelper.TransitionOrReconcile(
			this,
			Residence.ActiveCpu,
			_cpuScopeResidenceBeforeOuterEnter);
	}

	public void SetResidence(Residence value) => _residence.Value = value;

	public bool TrySetResidence(Residence expected, Residence value) =>
		_residence.TryTransition(expected, value);

	public MemoryBuffer1D<T, Stride1D.Dense> Allocate() =>
		Gpu.accelerator.Allocate1D(Value);

	public MemoryBuffer1D<T, Stride1D.Dense> Allocate(T[] array) =>
		Gpu.accelerator.Allocate1D(array);

	internal void CommitCpuView() => Length = Value.Length;

	internal MemoryBuffer Cache()
	{
		(ID, MemoryBuffer buffer) = Gpu.Allocate(this);
		return buffer;
	}

	internal MemoryBuffer Cache(T[] array)
	{
		(ID, MemoryBuffer buffer) = Gpu.Allocate(this, array);
		return buffer;
	}

	internal MemoryBuffer CacheEmpty(int length)
	{
		(ID, MemoryBuffer buffer) = Gpu.AllocateEmpty<T>(this, length);
		return buffer;
	}
}
