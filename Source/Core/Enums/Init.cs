namespace BAVCL;

/// <summary>
/// Controls whether newly allocated GPU storage is zero-filled or left uninitialized.
/// </summary>
public enum Init : byte
{
	/// <summary>Allocate GPU storage without clearing it (fastest; contents are undefined until written).</summary>
	UnInit = 0,

	/// <summary>Allocate GPU storage and zero-fill every element on the device.</summary>
	Zeroed = 1,
}
