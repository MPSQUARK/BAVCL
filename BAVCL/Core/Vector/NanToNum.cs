using ILGPU;
using ILGPU.Runtime;

namespace BAVCL
{
    public partial class Vector
    {

        public static Vector Nan_to_num(Vector vector, float num) =>
            vector.Copy().Nan_to_num_IP(num);

        public Vector Nan_to_num_IP(float num)
        {
            IncrementLiveCount();

            MemoryBuffer1D<float, Stride1D.Dense> buffer = GetBuffer();

            Gpu.nanToNumKernel(Gpu.accelerator.DefaultStream, Length, buffer.View, num);

            Gpu.accelerator.Synchronize();

            DecrementLiveCount();

            return this;
        }


    }
}
