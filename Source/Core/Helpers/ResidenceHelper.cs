using System;

namespace BAVCL.Core;

internal static class ResidenceHelper
{
	public static Residence Authority(Residence residence) => residence & (Residence.Cpu | Residence.Gpu);

	public static bool IsInSync(Residence residence) => Authority(residence) == Residence.InSync;

	public static bool IsCpuAuthority(Residence residence) => Authority(residence) == Residence.Cpu;

	public static bool IsGpuAuthority(Residence residence) => Authority(residence) == Residence.Gpu;

	public static bool IsActiveCpu(Residence residence) =>
		(residence & Residence.ActiveCpu) == Residence.ActiveCpu;

	public static bool IsActiveGpu(Residence residence) =>
		(residence & Residence.ActiveGpu) == Residence.ActiveGpu;

	/// <summary>
	/// True when the GPU buffer can be freed without syncing stale GPU data over CPU (eviction / FreeBuffer).
	/// </summary>
	public static bool CanFreeWithoutSync(Residence residence) =>
		IsCpuAuthority(residence) || IsInSync(residence);

	public static void GuardCrossContext(Residence residence, bool enteringCpu)
	{
		if (enteringCpu && IsActiveGpu(residence))
			throw new InvalidOperationException("Cannot open CpuScope while ActiveGpu scope is open on this object.");

		if (!enteringCpu && IsActiveCpu(residence))
			throw new InvalidOperationException("Cannot pin GpuScope while ActiveCpu scope is open on this object.");
	}
}
