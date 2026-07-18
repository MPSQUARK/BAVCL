using System;
using System.Threading;

namespace BAVCL.Core;

public readonly ref struct CpuScope<T> : IDisposable where T : unmanaged
{
	readonly VectorBase<T> _owner;
	readonly bool _syncToGpuOnDispose;

	/// <summary>
	/// Writable view over the vector's CPU backing store for the duration of this scope.
	/// Assign to a local <see cref="EditableView{T}"/> before using the indexer:
	/// <c>EditableView&lt;T&gt; view = scope.View; view[i] = x;</c>
	/// (indexer writes through the <c>View</c> property directly do not compile.)
	/// </summary>
	public EditableView<T> View { get; }

	internal CpuScope(VectorBase<T> owner, bool syncToGpuOnDispose)
	{
		_owner = owner;
		_syncToGpuOnDispose = syncToGpuOnDispose;

		ResidenceHelper.GuardCrossContext(owner.Residence, enteringCpu: true);
		Interlocked.Increment(ref owner._cpuScopeDepth);

		Residence current = owner.Residence;
		if (!ResidenceHelper.IsActiveCpu(current))
		{
			if (!owner.TrySetResidence(current, Residence.ActiveCpu))
				owner.SetResidence(Residence.ActiveCpu);
		}

		owner.SyncCPU();

		View = new EditableView<T>(owner);
	}

	public void Dispose()
	{
		if (Interlocked.Decrement(ref _owner._cpuScopeDepth) != 0)
			return;

		_owner.CommitCpuView();

		if (!_owner.TrySetResidence(Residence.ActiveCpu, Residence.Cpu))
			_owner.SetResidence(Residence.Cpu);

		if (_syncToGpuOnDispose)
			_owner.UpdateCache();
	}
}
