using ILGPU;
using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using BAVCL.Core.Interfaces;
using BAVCL.Core;

namespace BAVCL;

public partial class GPU
{
	protected internal Context context;
	public Accelerator accelerator;
	public AcceleratorStream DefaultStream => accelerator.DefaultStream;
	public void Synchronize() => accelerator.Synchronize();
	private IMemoryManager _memoryManager;

	private Delegate[] Kernels { get; set; } = Array.Empty<Delegate>();

	public T GetKernel<T>(Kernels kernel) where T : Delegate
		=> (T)Kernels[(int)kernel];

	// Accelerator Preference Order
	Dictionary<AcceleratorType, int> AcceleratorPrefOrder = new()
		{
			{ AcceleratorType.Cuda, 2 },
			{ AcceleratorType.OpenCL, 1 },
			{ AcceleratorType.CPU, 0 }
		};

	// Constructor
	public GPU(float memorycap = 0.8f, bool forceCPU = false)
	{
		// Create Context
		context = Context.Create(builder => builder.Default().EnableAlgorithms());
		// OptimizationLevel optimizationLevel = OptimizationLevel.Debug

		// Get Accelerator Device
		//this.accelerator = context.GetPreferredDevice(preferCPU: forceCPU).CreateAccelerator(context);
		accelerator = GetPreferedAccelerator(context, forceCPU);
		Console.WriteLine($"Device loaded: {accelerator.Name}");

		// Set Memory Usage Cap
		_memoryManager = new LRU(accelerator.MemorySize, memorycap);

		AddKernels();
	}

	// Wrappers for Memory Manager
	public (uint, MemoryBuffer) Allocate<T>(ICacheable<T> cacheable) where T : unmanaged => _memoryManager.Allocate(cacheable, accelerator);
	public (uint, MemoryBuffer) Allocate<T>(ICacheable cacheable, T[] values) where T : unmanaged => _memoryManager.Allocate(cacheable, values, accelerator);
	public (uint, MemoryBuffer) AllocateEmpty<T>(ICacheable cacheable, int length) where T : unmanaged => _memoryManager.AllocateEmpty<T>(cacheable, length, accelerator);
	public MemoryBuffer? TryGetBuffer<T>(uint Id) where T : unmanaged => _memoryManager.GetBuffer(Id);
	public (uint, MemoryBuffer) UpdateBuffer<T>(ICacheable cacheable, T[] values) where T : unmanaged => _memoryManager.UpdateBuffer(cacheable, values, accelerator);
	public (uint, MemoryBuffer) UpdateBuffer<T>(ICacheable<T> cacheable) where T : unmanaged => _memoryManager.UpdateBuffer(cacheable, accelerator);
	public bool IsStored(uint Id) => _memoryManager.IsStored(Id);
	public HashSet<uint> StoredIDs() => _memoryManager.StoredIDs();
	public uint GCItem(uint Id) => _memoryManager.GCItem(Id);
	public string PrintMemoryUsage(bool percentage, string format = "F2") => _memoryManager.PrintMemoryUsage(percentage, format);
	public string GetMemUsage() => _memoryManager.MemoryUsed.ToString();

	private Accelerator GetPreferedAccelerator(Context context, bool forceCPU)
	{
		var devices = context.Devices;

		if (devices.Length == 0) throw new Exception("No Accelerators");

		Device preferedAccelerator = devices[0];

		for (int i = 1; i < devices.Length; i++)
		{
			if (forceCPU && devices[i].AcceleratorType == AcceleratorType.CPU)
			{
				preferedAccelerator = devices[i];
				break;
			}

			if (AcceleratorPrefOrder.TryGetValue(preferedAccelerator.AcceleratorType, out int Prefpriority))
				if (AcceleratorPrefOrder.TryGetValue(devices[i].AcceleratorType, out int Devicepriority))
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

		return preferedAccelerator.CreateAccelerator(context);
	}

}
