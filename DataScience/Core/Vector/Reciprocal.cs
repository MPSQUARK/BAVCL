using ILGPU.Runtime;

namespace DataScience
{
    public partial class Vector
    {
        public static Vector Reciprocal(Vector vector)
        {
            Vector vec = vector.Copy();
            vec.Reciprocal_IP();
            return vec;
        }
        public void Reciprocal_IP()
        {
            // Check if the input & output are in Cache
            MemoryBuffer<float> buffer = this.GetBuffer(); // IO

            gpu.reciprocalKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View);

            gpu.accelerator.Synchronize();

            buffer.CopyTo(this.Value, 0, 0, this.Value.Length);

            return;
        }

    }
}
