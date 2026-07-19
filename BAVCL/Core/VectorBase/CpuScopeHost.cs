using System.Threading;

namespace BAVCL.Core;

// TODO: This along with other memory scope things should be refactored into a seperate abstract class in the future

/// <summary>
/// CPU scope for <see cref="VectorBase{T}"/>. Nested scopes on the same instance are supported.
/// Depth counting and residence transitions are thread-safe; CPU buffer mutation is caller-synchronized.
/// </summary>
public abstract partial class VectorBase<T> where T : unmanaged
{
	Residence _cpuScopeResidenceBeforeOuterEnter;

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
}
