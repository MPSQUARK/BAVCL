using System.Runtime.CompilerServices;

namespace BAVCL.QoL;

/// <summary>Branchless bit and bool conversions for hot paths (CPU and ILGPU kernels).</summary>
public static class Branchless
{
	/// <summary>Maps <c>false</c>→0 and <c>true</c>→1 without branching.</summary>
	public static int BoolToInt(bool value) => Unsafe.As<bool, byte>(ref value);
}
