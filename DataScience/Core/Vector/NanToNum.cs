

using ILGPU.Runtime;

namespace DataScience
{
    public partial class Vector
    {

        public static Vector Nan_to_num(Vector vector, float num)
        {
            Vector vec = vector.Copy();
            vec.Nan_to_num_IP(num);
            return vec;
        }
        public void Nan_to_num_IP(float num)
        {
            MemoryBuffer<float> buffer = GetBuffer();

            gpu.nanToNumKernel(gpu.accelerator.DefaultStream, this.Value.Length, buffer.View, num);

            gpu.accelerator.Synchronize();

            buffer.CopyTo(this.Value, 0, 0, this.Value.Length);

            return;
        }


    }
}
