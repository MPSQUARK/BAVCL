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
	private static readonly Dictionary<(KernelDomain Domain, Type ElementType), Action<GPU>> Modules = new()
	{
		[(KernelDomain.Arithmetic, typeof(float))] = static gpu => gpu.LoadArithmeticFloat32Kernels(),
		[(KernelDomain.Structural, typeof(float))] = static gpu => gpu.LoadStructuralFloat32Kernels(),
		[(KernelDomain.Geometry, typeof(float))] = static gpu => gpu.LoadGeometryFloat32Kernels(),
		[(KernelDomain.Mask, typeof(float))] = static gpu =>
		{
			gpu.LoadMaskWordKernels();
			gpu.LoadMaskVectorKernels();
		},
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

	/// <summary>Compiles every domain registered for element type <typeparamref name="T"/>.</summary>
	public static void LoadAll<T>(GPU gpu) where T : unmanaged =>
		Load<T>(gpu, [.. Modules.Keys.Where(key => key.ElementType == typeof(T)).Select(key => key.Domain)]);

	private static Action<GPU> Resolve(KernelDomain domain, Type elementType) =>
		Modules.TryGetValue((domain, elementType), out var load)
			? load
			: throw new KernelModuleNotAvailableException(domain, elementType);
}
