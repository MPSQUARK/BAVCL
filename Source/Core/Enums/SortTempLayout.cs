namespace BAVCL;

/// <summary>Layout of a rented segmented-sort ping-pong temp buffer.</summary>
internal enum SortTempLayout
{
	/// <summary>Buffer holds only a keys-sized ping-pong region.</summary>
	KeysOnly,

	/// <summary>Buffer holds keys and values ping-pong regions back to back, each sized to the segment length.</summary>
	KeysAndValues,
}
