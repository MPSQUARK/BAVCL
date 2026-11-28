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
	int _isPossessed;
	int _assignGate;

	protected volatile internal uint _id;
	protected volatile internal uint _livecount;
	protected volatile internal int _length;

	public Residence Residence
	{
		get => _residence.Value;
		set => _residence.Value = value;
	}

	/// <summary>True when this instance has no backing data or GPU buffer yet.</summary>
	public bool IsEmpty => Residence == Residence.None;

	internal bool IsPossessed => Volatile.Read(ref _isPossessed) != 0;

	internal bool TryAcquirePossession() => Interlocked.CompareExchange(ref _isPossessed, 1, 0) == 0;

	internal void ReleasePossession() => Volatile.Write(ref _isPossessed, 0);

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

	/// <summary>Creates an empty shell with no backing data or GPU buffer.</summary>
	protected CacheableBase(GPU? gpu = null)
	{
		Gpu = gpu ?? GPUManager.Default;
		Value = [];
		Length = 0;
		ID = 0;
		Residence = Residence.None;
	}

	/// <summary>Creates storage backed by host <paramref name="values"/>.</summary>
	protected CacheableBase(T[] values, Upload upload = Upload.Now, GPU? gpu = null)
	{
		ArgumentNullException.ThrowIfNull(values);
		Gpu = gpu ?? GPUManager.Default;
		ApplyData(values, upload);
	}

	/// <summary>Creates GPU-backed storage of <paramref name="length"/> elements.</summary>
	protected CacheableBase(int length, Init init = Init.UnInit, GPU? gpu = null)
	{
		Guard.ArgumentIsNonNegative(length);
		Gpu = gpu ?? GPUManager.Default;
		ApplyLength(length, init);
	}

	public T[] Pull()
	{
		Guard.NotEmpty(this);
		MemoryBuffer1D<T, Stride1D.Dense> buffer = GetBuffer();
		T[] values = new T[buffer.Length];
		buffer.AsArrayView<T>(0, buffer.Length).CopyToCPU(values);
		return values;
	}

	public ReadOnlySpan<T> RetrieveReadOnlySpan()
	{
		Guard.NotEmpty(this);
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
				$"{nameof(ICacheable<>.EditCpu)} requires an open {nameof(CpuScope<>)}.");

		edit(Value.AsMemory(0, Length));
	}

	public void SyncCPU()
	{
		if (IsEmpty || ResidenceHelper.IsInSync(Residence) || ResidenceHelper.IsCpuAuthority(Residence))
			return;

		if (ID != 0)
			Value = Pull();

		Length = Value.Length;
		ResidenceScopeHelper.TransitionOrReconcile(this, Residence, Residence.InSync);
	}

	public void SyncCPU(MemoryBuffer buffer)
	{
		if (IsEmpty || ResidenceHelper.IsInSync(Residence) || ResidenceHelper.IsCpuAuthority(Residence))
			return;

		if (Value == null || Value.Length != buffer.Length)
			Value = new T[buffer.Length];

		buffer.AsArrayView<T>(0, buffer.Length).CopyToCPU(Value);
		Length = Value.Length;
		ResidenceScopeHelper.TransitionOrReconcile(this, Residence, Residence.InSync);
	}

	public MemoryBuffer1D<T, Stride1D.Dense> GetBuffer()
	{
		Guard.NotEmpty(this);

		if (ResidenceHelper.IsCpuAuthority(Residence) && !ResidenceHelper.IsActiveCpu(Residence))
			return (MemoryBuffer1D<T, Stride1D.Dense>)UpdateCache();

		return (MemoryBuffer1D<T, Stride1D.Dense>)(Gpu.TryGetBuffer<T>(ID) ?? Cache());
	}

	public MemoryBuffer UpdateCache()
	{
		Guard.NotEmpty(this);

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
		ArgumentNullException.ThrowIfNull(array);
		Length = array.Length;
		Value = array;
		(ID, MemoryBuffer buffer) = Gpu.UpdateBuffer(this, array);
		ResidenceScopeHelper.TransitionOrReconcile(this, Residence, Residence.InSync);
		return buffer;
	}

	public void DeCache()
	{
		if (ID == 0 || LiveCount != 0 || IsPossessed)
			return;

		SyncCPU();
		ID = Gpu.FreeBuffer(ID);
		Residence = Residence.Cpu;
	}

	void ICacheable.IncrementLiveCount() => Interlocked.Increment(ref _livecount);

	void ICacheable.DecrementLiveCount() => Interlocked.Decrement(ref _livecount);

	public void EnterCpuScope()
	{
		Guard.NotEmpty(this);
		ResidenceHelper.GuardCrossContext(Residence, enteringCpu: true);

		if (Interlocked.Increment(ref _cpuScopeDepth) != 1)
			return;

		_cpuScopeResidenceBeforeOuterEnter = Residence;

		try
		{
			Residence current = Residence;
			if (!ResidenceHelper.IsActiveCpu(current))
			{
				// Pull while GPU is still exclusive authority. Transitioning to ActiveCpu first
				// makes IsCpuAuthority true, so SyncCPU would skip the pull and leave stale host storage.
				if (ResidenceHelper.IsGpuAuthority(current))
					SyncCPU();

				ResidenceScopeHelper.TransitionOrReconcile(this, Residence, Residence.ActiveCpu);
			}
		}
		catch
		{
			RollbackCpuScopeEnter();
			throw;
		}
	}

	public void ExitCpuScope(Upload upload)
	{
		if (Interlocked.Decrement(ref _cpuScopeDepth) != 0)
			return;

		CommitCpuView();
		ResidenceScopeHelper.TransitionOrReconcile(this, Residence.ActiveCpu, Residence.Cpu);

		if (upload == Upload.Now)
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

	internal MemoryBuffer CacheEmpty(int length, Init init = Init.UnInit)
	{
		(ID, MemoryBuffer buffer) = Gpu.AllocateEmpty<T>(this, length, init);
		return buffer;
	}

	internal MemoryBuffer Cache(ArrayView1D<T, Stride1D.Dense> deviceView)
	{
		(ID, MemoryBuffer buffer) = Gpu.Allocate(this, deviceView);
		return buffer;
	}

	internal void RestoreEmptyState()
	{
		ID = 0;
		Value = [];
		Length = 0;
		Residence = Residence.None;
		ReleasePossession();
		OnRestoredEmpty();
	}

	protected virtual void OnRestoredEmpty() { }

	/// <summary>
	/// Releases any owned GPU buffer and resets host storage without leaking LRU entries.
	/// </summary>
	protected void ReleaseOwnedStorage()
	{
		Guard.CanReassign(this);
		Gpu.ReleaseBuffer(ID);
		ID = 0;
		Value = [];
		Length = 0;
		Residence = Residence.None;
	}

	protected void AssignDataCore(T[] values, Upload upload)
	{
		ArgumentNullException.ThrowIfNull(values);
		RunExclusiveAssign(() =>
		{
			ReleaseOwnedStorage();
			ApplyData(values, upload);
		});
	}

	protected void AssignLengthCore(int length, Init init)
	{
		Guard.ArgumentIsNonNegative(length);
		RunExclusiveAssign(() =>
		{
			ReleaseOwnedStorage();
			ApplyLength(length, init);
		});
	}

	protected void AssignViewCore(ArrayView1D<T, Stride1D.Dense> deviceView)
	{
		Guard.ArgumentIsPositive(deviceView.IntLength);
		RunExclusiveAssign(() =>
		{
			ReleaseOwnedStorage();
			Value = [];
			Length = deviceView.IntLength;
			Cache(deviceView);
			Residence = Residence.Gpu;
		});
	}

	void RunExclusiveAssign(Action assign)
	{
		if (Interlocked.CompareExchange(ref _assignGate, 1, 0) != 0)
			throw new InvalidOperationException(
				$"Concurrent Assign detected on {GetType().Name}.");

		try
		{
			assign();
		}
		finally
		{
			Volatile.Write(ref _assignGate, 0);
		}
	}

	void ApplyData(T[] values, Upload upload)
	{
		Value = values;
		Length = values.Length;

		if (upload == Upload.Now && values.Length > 0)
		{
			Cache(values);
			Residence = Residence.InSync;
			return;
		}

		Residence = Residence.Cpu;
	}

	void ApplyLength(int length, Init init)
	{
		Value = [];
		Length = length;
		CacheEmpty(length, init);
		Residence = Residence.Gpu;
	}
}
