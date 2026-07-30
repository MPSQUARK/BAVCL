using System;

namespace BAVCL.Core;

public readonly struct GpuPinScope : IDisposable
{
	readonly ICacheable[] _modified;
	readonly ICacheable[] _readOnly;

	public GpuPinScope(ICacheable[] modified, ICacheable[] readOnly)
	{
		_modified = modified;
		_readOnly = readOnly;

		int modifiedEntered = 0;
		int readOnlyEntered = 0;

		try
		{
			for (int i = 0; i < _modified.Length; i++)
			{
				EnterModifiedPin(_modified[i]);
				modifiedEntered++;
			}

			for (int i = 0; i < _readOnly.Length; i++)
			{
				EnterReadOnlyPin(_readOnly[i]);
				readOnlyEntered++;
			}
		}
		catch
		{
			RollbackEnteredPins(modifiedEntered, readOnlyEntered);
			throw;
		}
	}

	public void Dispose()
	{
		for (int i = _readOnly.Length - 1; i >= 0; i--)
			ExitReadOnlyPin(_readOnly[i]);

		for (int i = _modified.Length - 1; i >= 0; i--)
			ExitModifiedPin(_modified[i]);
	}

	void RollbackEnteredPins(int modifiedEntered, int readOnlyEntered)
	{
		for (int i = readOnlyEntered - 1; i >= 0; i--)
			ExitReadOnlyPin(_readOnly[i]);

		for (int i = modifiedEntered - 1; i >= 0; i--)
			ExitModifiedPin(_modified[i]);
	}

	static void EnterModifiedPin(ICacheable cacheable)
	{
		ResidenceHelper.GuardCrossContext(cacheable.Residence, enteringCpu: false);

		Residence current = cacheable.Residence;
		if (!ResidenceHelper.IsActiveGpu(current))
			ResidenceScopeHelper.TransitionOrReconcile(cacheable, current, Residence.ActiveGpu);

		cacheable.IncrementLiveCount();
	}

	static void EnterReadOnlyPin(ICacheable cacheable)
	{
		ResidenceHelper.GuardCrossContext(cacheable.Residence, enteringCpu: false);
		cacheable.IncrementLiveCount();
	}

	static void ExitModifiedPin(ICacheable cacheable)
	{
		cacheable.DecrementLiveCount();

		if (cacheable.LiveCount != 0)
			return;

		ResidenceScopeHelper.TransitionOrReconcile(cacheable, Residence.ActiveGpu, Residence.Gpu);
	}

	static void ExitReadOnlyPin(ICacheable cacheable) =>
		cacheable.DecrementLiveCount();
}
