using ILGPU.Runtime;
using System;
using System.Linq;

namespace DataScience
{
    public partial class Vector
    {
        /// <summary>
        /// Takes the absolute value of all values in the Vector.
        /// IMPORTANT : Use this method for Vectors of Length less than 100,000
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector Abs(Vector vector)
        {
            Vector vec = vector.Copy();
            vec.Abs_IP();
            return vec;
        }
        /// <summary>
        /// Takes the absolute value of all values in this Vector.
        /// IMPORTANT : Use this method for Vectors of Length less than 100,000
        /// </summary>
        public void Abs_IP()
        {
            if (this.Value.Min() > 0f)
            {
                return;
            }

            for (int i = 0; i < this.Value.Length; i++)
            {
                this.Value[i] = MathF.Abs(this.Value[i]);
            }

            if (this.Id != 0)
            {
                UpdateCache();
            }

            return;
        }
        /// <summary>
        /// Runs on Accelerator. (GPU : Default)
        /// Takes the absolute value of all the values in the Vector.
        /// IMPORTANT : Use this method for Vectors of Length more than 100,000
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector AbsX(Vector vector)
        {
            Vector vec = vector.Copy();
            vec.AbsX_IP();
            return vec;
        }
        /// <summary>
        /// Runs on Accelerator. (GPU : Default)
        /// Takes the absolute value of all the values in this Vector.
        /// IMPORTANT : Use this method for Vectors of Length more than 100,000
        /// </summary>
        public void AbsX_IP()
        {
            // Check if the input & output are in Cache
            MemoryBuffer<float> buffer = this.GetBuffer(); // IO

            gpu.absKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View);

            gpu.accelerator.Synchronize();

            buffer.CopyTo(this.Value, 0, 0, this.Value.Length);

            return;
        }


    }
}
