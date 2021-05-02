using ILGPU;
using ILGPU.IR.Transformations;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScience
{
    public class GPU
    {
        Context context;
        public Accelerator accelerator;

        public GPU(bool forceCPU = false, ContextFlags flags = ContextFlags.None, OptimizationLevel optimizationLevel = OptimizationLevel.Debug)
        {
            this.context = new Context(flags, optimizationLevel);
            this.context.EnableAlgorithms();

            this.accelerator = GetGpu(context, forceCPU);
            Console.WriteLine("Device loaded: " + accelerator.Name);
        }

        private Accelerator GetGpu(Context context, bool prefCPU = false)
        {
            var groupedAccelerators = Accelerator.Accelerators
                    .GroupBy(x => x.AcceleratorType)
                    .ToDictionary(x => x.Key, x => x.ToList());

            if (prefCPU)
            {
                return new CPUAccelerator(context);
            }

            if (groupedAccelerators.TryGetValue(AcceleratorType.Cuda, out var nv))
            {
                return Accelerator.Create(context, nv[0]);
            }

            if (groupedAccelerators.TryGetValue(AcceleratorType.OpenCL, out var cl))
            {
                return Accelerator.Create(context, cl[0]);
            }


            //fallback
            Console.WriteLine("Warning : Could not find gpu, falling back to Default device");
            return new CPUAccelerator(context);
        }
    }
}
