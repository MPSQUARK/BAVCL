using ILGPU.Runtime;
using System.Linq;

namespace BAVCL
{
    public partial class Vector
    {
        public static Vector Reverse(Vector vector)
        {
            return new Vector(vector.gpu, vector.Value.Reverse().ToArray(), vector.Columns);
        }
        public Vector Reverse_IP()
        {
            SyncCPU();
            UpdateCache(this.Value.Reverse().ToArray());
            return this;
        }
        public static Vector ReverseX(Vector vector)
        {
            return vector.Copy().ReverseX_IP();
        }
        public Vector ReverseX_IP()
        {
            IncrementLiveCount();

            // Check if the input & output are in Cache
            MemoryBuffer<float> buffer = GetBuffer(); // IO

            gpu.reverseKernel(gpu.accelerator.DefaultStream, buffer.Length >> 1, buffer.View);

            gpu.accelerator.Synchronize();

            DecrementLiveCount();

            return this;
        }

    }
}
