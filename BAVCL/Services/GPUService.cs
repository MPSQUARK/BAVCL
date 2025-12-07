using System;
using System.Collections.Generic;
using BAVCL.Core;
using BAVCL.Core.Interfaces;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Services;

public static class GPUManager
{
    private static readonly object _lock = new();
    private static readonly Lazy<Context> _context = new(
        () => Context.Create(builder => builder.Default().EnableAlgorithms())
    );
    private static GPU? _defaultGPU = null;

    private static Dictionary<AcceleratorType, int> _acceleratorPrefOrder = new()
    {
        { AcceleratorType.Cuda, 2 },
        { AcceleratorType.OpenCL, 1 },
        { AcceleratorType.CPU, 0 }
    };
    public static Context Context => _context.Value;
    public static bool IsInitialized => _context.IsValueCreated;
    public static GPU Default
    {
        get
        {
            lock (_lock)
            {
                _defaultGPU ??= GetGPU();
            }
            return _defaultGPU;
        }
    }
    internal static Accelerator GetPreferedAccelerator(bool forceCPU)
    {
        var devices = Context.Devices;

        if (devices.Length == 0) throw new Exception("No Accelerators");

        Device? preferedAccelerator = null;
        for (int i = 0; i < devices.Length; i++)
        {
            if (forceCPU && devices[i].AcceleratorType == AcceleratorType.CPU)
                return devices[i].CreateAccelerator(Context);

            preferedAccelerator ??= devices[i];

            if (_acceleratorPrefOrder.TryGetValue(preferedAccelerator.AcceleratorType, out int Prefpriority))
                if (_acceleratorPrefOrder.TryGetValue(devices[i].AcceleratorType, out int Devicepriority))
                {
                    if (Devicepriority > Prefpriority)
                    {
                        preferedAccelerator = devices[i];
                        continue;
                    }

                    if (devices[i].MaxConstantMemory > preferedAccelerator.MaxConstantMemory)
                    {
                        preferedAccelerator = devices[i];
                        continue;
                    }
                }
        }
        if (preferedAccelerator == null)
            throw new Exception("No suitable accelerator found.");

        return preferedAccelerator.CreateAccelerator(Context);
    }

    public static GPU GetGPU(float memoryCap = 0.8f, bool forceCPU = false)
    {
        Console.WriteLine("Getting GPU...");
        return GetGPU<LRU>(memoryCap, forceCPU);
    }
    public static GPU GetGPU<TMem>(float memoryCap = 0.8f, bool forceCPU = false) where TMem : IMemoryManager, new()
    {
        if (memoryCap <= 0f || memoryCap >= 1f)
            throw new Exception($"Memory Cap CANNOT be less than 0 or more than 1. Recieved {memoryCap}");

        var accelerator = GetPreferedAccelerator(forceCPU);
        Console.WriteLine($"Device loaded: {accelerator.Name}");

        var memoryManager = new TMem()
        {
            AvailableMemory = (long)Math.Round(accelerator.MemorySize * memoryCap)
        };

        var gpu = new GPU(accelerator, memoryManager);

        gpu.LoadKernels();

        return gpu;
    }
}
