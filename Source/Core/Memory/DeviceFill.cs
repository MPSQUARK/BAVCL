using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL.Core;

/// <summary>
/// Whole-buffer writes on the device for cacheable storage. Each call synchronises the default stream.
/// The buffer must hold at least one element.
/// </summary>
internal static class DeviceFill
{
	/// <summary>Sets every element of the device buffer to zero.</summary>
	internal static void Zero<T>(CacheableBase<T> output)
		where T : unmanaged
	{
		using (GpuScope.Begin(output))
		{
			output.GetBuffer().MemSetToZero(output.Gpu.accelerator.DefaultStream);
			output.Gpu.accelerator.Synchronize();
		}
	}

	/// <summary>Sets every element of the device buffer to <paramref name="value"/>.</summary>
	internal static void Fill<T>(CacheableBase<T> output, T value)
		where T : unmanaged
	{
		using (GpuScope.Begin(output))
		{
			output.Gpu.accelerator.Initialize(
				output.Gpu.accelerator.DefaultStream,
				output.GetBuffer().View,
				value);
			output.Gpu.accelerator.Synchronize();
		}
	}
}
