namespace BAVCL;

/// <summary>
/// Named domain bundles for common setups, so callers do not have to list domains by hand.
/// </summary>
public static class KernelWorkloads
{
	public static KernelDomain[] Default => [KernelDomain.Arithmetic, KernelDomain.Structural];

	public static KernelDomain[] Geometry => [.. Default, KernelDomain.Geometry];
}
