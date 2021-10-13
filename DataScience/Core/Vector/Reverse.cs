using ILGPU.Runtime;
using System.Linq;

namespace DataScience
{
    public partial class Vector
    {
        public static Vector Reverse(Vector vector)
        {
            return new Vector(vector.gpu, vector.Value.Reverse().ToArray(), vector.Columns);
        }
        public void Reverse_IP()
        {
            this.Value = this.Value.Reverse().ToArray();
            UpdateCache();
            return;
        }
        public static Vector ReverseX(Vector vector)
        {
            Vector vec = vector.Copy();
            vec.ReverseX_IP();
            return vec;
        }
        public void ReverseX_IP()
        {
            // Check if the input & output are in Cache
            MemoryBuffer<float> buffer = this.GetBuffer(); // IO

            gpu.reverseKernel(gpu.accelerator.DefaultStream, buffer.Length >> 1, buffer.View);

            gpu.accelerator.Synchronize();

            buffer.CopyTo(this.Value, 0, 0, this.Value.Length);

            return;
        }

    }
}
