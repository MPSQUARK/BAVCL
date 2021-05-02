using ILGPU;
using ILGPU.Runtime;
using System;
using System.Linq;


namespace DataScience
{
    public class Setup
    {
        public static Accelerator GetGpu(Context context, bool prefCPU = false)
        {
            context.EnableAlgorithms();

            var groupedAccelerators = Accelerator.Accelerators
                    .GroupBy(x => x.AcceleratorType)
                    .ToDictionary(x => x.Key, x => x.ToList());

            if (prefCPU && groupedAccelerators.TryGetValue(AcceleratorType.CPU, out var cpu))
            {
                return Accelerator.Create(context, cpu[0]);
            }

            if (groupedAccelerators.TryGetValue(AcceleratorType.Cuda, out var nv))
            {
                return Accelerator.Create(context, nv[0]);
            }

            if (groupedAccelerators.TryGetValue(AcceleratorType.OpenCL, out var cl))
            {
                return Accelerator.Create(context, cl[0]);
            }

            throw new Exception("No accelerators found!");
        }


    }


}
