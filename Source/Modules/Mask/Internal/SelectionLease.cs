using System;
using BAVCL.Core.Memory;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Modules.Masking;

internal readonly struct SelectionLease(
	BufferEntity<int> flags,
	BufferEntity<int> writePos,
	int elementCount,
	int trueCount) : IDisposable
{
	internal ArrayView1D<int, Stride1D.Dense> Flags => flags.View;
	internal ArrayView1D<int, Stride1D.Dense> WritePos => writePos.View;
	internal ICacheable FlagsPinTarget => flags.PinTarget;
	internal ICacheable WritePosPinTarget => writePos.PinTarget;
	internal int TrueCount => trueCount;
	internal int FalseCount => elementCount - trueCount;

	public void Dispose()
	{
		writePos.Dispose();
		flags.Dispose();
	}
}
