using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BAVCL.Core.Exceptions;

namespace BAVCL;

/// <summary>
/// Compiles kernel modules onto a <see cref="GPU"/>. Each device is configured independently.
/// </summary>
public static class KernelModuleLoader
{
	private static readonly Dictionary<(int Domain, Type ElementType), Action<GPU>> Modules = new()
	{
		[((int)KernelDomainSharedModule.MaskCore, typeof(float))] = static gpu => gpu.LoadMaskCoreKernels(),
        [((int)KernelDomainSharedModule.MaskCore, typeof(int))] = static gpu => gpu.LoadMaskCoreKernels(),
        [((int)KernelDomain.Arithmetic, typeof(float))] = static gpu => gpu.LoadArithmeticFloat32Kernels(),
		[((int)KernelDomain.Structural, typeof(float))] = static gpu => gpu.LoadStructuralFloat32Kernels(),
		[((int)KernelDomain.Geometry, typeof(float))] = static gpu => gpu.LoadGeometryFloat32Kernels(),
		[((int)KernelDomain.Mask, typeof(float))] = static gpu =>
		{
			gpu.LoadModuleOnce(KernelDomainSharedModule.MaskCore, typeof(float), Resolve(KernelDomainSharedModule.MaskCore, typeof(float)));
			gpu.LoadMaskVectorKernels();
		},
		[((int)KernelDomain.Arithmetic, typeof(int))] = static gpu => gpu.LoadArithmeticInt32Kernels(),
		[((int)KernelDomain.Structural, typeof(int))] = static gpu => gpu.LoadStructuralInt32Kernels(),
		[((int)KernelDomain.Mask, typeof(int))] = static gpu =>
		{
			gpu.LoadModuleOnce(KernelDomainSharedModule.MaskCore, typeof(int), Resolve(KernelDomainSharedModule.MaskCore, typeof(int)));
			gpu.LoadMaskVectorIntKernels();
		},
		[((int)KernelDomain.Sorting, typeof(float))] = static gpu => gpu.LoadSortFloatKernels(),
		[((int)KernelDomain.Sorting, typeof(int))] = static gpu => gpu.LoadSortSegmentedRadixKernels(),
		[((int)KernelDomain.Statistics, typeof(float))] = static gpu => gpu.LoadStatisticsFloatKernels(),
		[((int)KernelDomain.Statistics, typeof(int))] = static gpu => gpu.LoadStatisticsIntKernels(),
	};

	/// <summary>
	/// Compiles the given domains for element type <typeparamref name="T"/>. Modules already
	/// loaded on this device are skipped, so repeated calls are additive.
	/// </summary>
	public static void Load<T>(GPU gpu, params KernelDomain[] domains) where T : unmanaged
	{
		ArgumentNullException.ThrowIfNull(gpu);

		var timer = Stopwatch.StartNew();
		var compiled = 0;

		foreach (var domain in domains)
			if (gpu.LoadModuleOnce(domain, typeof(T), Resolve(domain, typeof(T))))
				compiled++;

		timer.Stop();

		if (compiled == 0)
			return;

		Console.WriteLine($"Compiled {compiled} kernel module(s) for {typeof(T).Name} in {timer.Elapsed.TotalMilliseconds:F2} ms");
	}

	internal static void Load<T>(GPU gpu, params KernelDomainSharedModule[] domains) where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(gpu);

        var timer = Stopwatch.StartNew();
        var compiled = 0;

        foreach (var domain in domains)
            if (gpu.LoadModuleOnce(domain, typeof(T), Resolve(domain, typeof(T))))
                compiled++;

        timer.Stop();

        if (compiled == 0)
            return;

        Console.WriteLine($"Compiled {compiled} kernel [shared] module(s) for {typeof(T).Name} in {timer.Elapsed.TotalMilliseconds:F2} ms");
    }


    /// <summary>Compiles every domain registered for element type <typeparamref name="T"/>.</summary>
	public static void LoadAll<T>(GPU gpu) where T : unmanaged =>
		Load<T>(gpu, [.. Modules.Keys
			.Where(key => key.ElementType == typeof(T) && Enum.IsDefined(typeof(KernelDomain), key.Domain))
			.Select(key => (KernelDomain)key.Domain)]);

	private static Action<GPU> Resolve(KernelDomainSharedModule domain, Type elementType) =>
		Modules.TryGetValue(((int)domain, elementType), out Action<GPU>? load)
			? load
			: throw new KernelModuleNotAvailableException(domain, elementType);

	private static Action<GPU> Resolve(KernelDomain domain, Type elementType) =>
		Modules.TryGetValue(((int)domain, elementType), out Action<GPU>? load)
			? load
			: throw new KernelModuleNotAvailableException(domain, elementType);
}
