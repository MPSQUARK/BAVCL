using System;

namespace BAVCL.Core;

public static class CpuScope
{
	public static CpuScope<T> Begin<T>(ICacheable<T> cacheable, bool syncToGpu = false) where T : unmanaged =>
		new(cacheable, syncToGpu);
}

/// <summary>
/// CPU coherence scope for <see cref="ICacheable{T}"/>.
/// <see cref="View"/> is available when <see cref="ICacheable{T}.EditCpu"/> supplies non-empty storage.
/// </summary>
public readonly ref struct CpuScope<T> : IDisposable where T : unmanaged
{
	readonly ICacheable<T> _cacheable;
	readonly bool _syncToGpu;
	readonly EditableView<T> _view;
	readonly bool _hasView;

	public bool HasView => _hasView;

	public EditableView<T> View =>
		_hasView
			? _view
			: throw new InvalidOperationException(
				$"CPU scope on {typeof(T).Name} has no editable span.");

	internal CpuScope(ICacheable<T> cacheable, bool syncToGpu)
	{
		_cacheable = cacheable;
		_syncToGpu = syncToGpu;
		cacheable.EnterCpuScope();

		bool hasView = false;
		Memory<T> editable = default;
		cacheable.EditCpu(memory =>
		{
			if (memory.Length == 0)
				return;

			hasView = true;
			editable = memory;
		});

		_hasView = hasView;
		if (hasView)
			_view = new EditableView<T>(editable);
	}

	public void Dispose() => _cacheable.ExitCpuScope(_syncToGpu);
}
