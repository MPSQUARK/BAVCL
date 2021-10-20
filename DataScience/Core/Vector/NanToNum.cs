using ILGPU.Runtime;

namespace DataScience
{
    public partial class Vector
    {

        public static Vector Nan_to_num(Vector vector, float num)
        {
            return vector.Copy().Nan_to_num_IP(num);
        }
        public Vector Nan_to_num_IP(float num)
        {
            IncrementLiveCount();

            MemoryBuffer<float> buffer = GetBuffer();

            gpu.nanToNumKernel(gpu.accelerator.DefaultStream, this._length, buffer.View, num);

            gpu.accelerator.Synchronize();

            DecrementLiveCount();

            return this;
        }


    }
}
