using ILGPU;

namespace BAVCL.Core;

public partial class VectorBase<T>
{
    public long CalculateMemorySize() => (long)Interop.SizeOf<T>() * (long)this.Length;
    public long CalculateMemorySize(T[] array) => (long)Interop.SizeOf<T>() * (long)array.Length;
}
