using System.Runtime.CompilerServices;

namespace BAVCL.QoL;

/// <summary>Branchless conversions for host CPU paths. Kernel equivalent: <see cref="GPU.BoolToInt"/>.</summary>
public static class Branchless
{
	/// <summary>
	/// Maps <c>false</c>→0 and <c>true</c>→1 without branching (host CPU only; uses <c>Unsafe.As</c>).
	/// Use <see cref="GPU.BoolToInt"/> inside kernels.
	/// </summary>
	public static int BoolToInt(bool value) => Unsafe.As<bool, byte>(ref value);
}
