namespace BAVCL.Core.Memory;

/// <summary>
/// Moves LRU buffer IDs between a pool <see cref="BufferSlot{T}"/> and a domain vessel.
/// Orchestrated only by <see cref="BufferEntity{T}"/> — not part of the general cacheable API.
/// </summary>
internal static class BufferIdTransfer
{
	internal static void Possess<T>(BufferSlot<T> slot, CacheableBase<T> soul) where T : unmanaged
	{
		Residence transferred = slot.Residence;
		soul.ID = slot.ID;
		soul.Length = slot.Length;
		if (soul.Value.Length < soul.Length)
			soul.Value = new T[soul.Length];
		else
			soul.Value = slot.Value;
		soul.SetResidence(transferred);

		Vessel<CacheableBase<T>>.RestoreVesselState(slot);
	}

	internal static void Banish<T>(CacheableBase<T> soul, BufferSlot<T> slot) where T : unmanaged
	{
		if (soul.ID == 0)
			return;

		Residence transferred = soul.Residence;
		slot.ID = soul.ID;
		slot.Length = soul.Length;
		if (slot.Value.Length < slot.Length)
			slot.Value = new T[slot.Length];
		else
			slot.Value = soul.Value;
		slot.SetResidence(transferred);

		Vessel<CacheableBase<T>>.RestoreVesselState(soul);
	}
}
