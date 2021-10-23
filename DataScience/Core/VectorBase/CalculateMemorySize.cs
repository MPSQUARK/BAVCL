using ILGPU;

namespace DataScience.Core
{
    public partial class VectorBase<T>
    {

        public long CalculateMemorySize()
        {
            return (long)Interop.SizeOf<T>() * (long)this.Value.Length;
        }
        public long CalculateMemorySize(T[] array)
        {
            return (long)Interop.SizeOf<T>() * (long)array.Length;
        }


    }


}
