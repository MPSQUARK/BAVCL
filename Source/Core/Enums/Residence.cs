using System;

namespace BAVCL.Core;

[Flags]
public enum Residence : byte
{
	None = 0,
	Cpu = 1 << 0,
	Gpu = 1 << 1,
	InSync = Cpu | Gpu,
	Active = 1 << 2,
	ActiveCpu = Active | Cpu,
	ActiveGpu = Active | Gpu,
}
