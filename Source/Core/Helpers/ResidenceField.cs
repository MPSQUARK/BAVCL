using System.Threading;

namespace BAVCL.Core;

/// <summary>
/// Thread-safe backing store for <see cref="Residence"/> flag updates.
/// </summary>
internal struct ResidenceField
{
	byte _value;

	public Residence Value
	{
		get => (Residence)Volatile.Read(ref _value);
		set => Volatile.Write(ref _value, (byte)value);
	}

	public bool TryTransition(Residence expected, Residence next)
	{
		byte expectedByte = (byte)expected;
		byte nextByte = (byte)next;
		return Interlocked.CompareExchange(ref _value, nextByte, expectedByte) == expectedByte;
	}
}
