namespace BAVCL.Core;

/// <summary>
/// Empty domain shell awaiting a pooled buffer from <see cref="Memory.BufferEntity{T}.Possess{TCacheable}"/>.
/// Only <c>CreateVessel</c> factories construct valid vessels.
/// </summary>
public sealed class Vessel<TCacheable> where TCacheable : class
{
	internal TCacheable Target { get; }
	internal GPU Gpu { get; }

	internal Vessel(TCacheable target, GPU gpu)
	{
		Target = target;
		Gpu = gpu;
	}

	/// <summary>True when no LRU GPU buffer is bound to <see cref="Target"/>.</summary>
	internal bool IsVessel => Target is ICacheable { ID: 0 };

	/// <summary>Restores <paramref name="target"/> to empty-vessel invariants after Banish.</summary>
	internal static void RestoreVesselState<TElement>(CacheableBase<TElement> target) where TElement : unmanaged
	{
		target.ID = 0;
		target.Value = [];
		target.Length = 0;
		target.Residence = Residence.Cpu;
	}
}
