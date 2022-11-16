using ILGPU;

namespace BAVCL.Core
{
	public partial class VectorBase<T>
	{

		public long CalculateMemorySize() => (long)Interop.SizeOf<T>() * (long)this.Value.Length;
		public long CalculateMemorySize(T[] array) => (long)Interop.SizeOf<T>() * (long)array.Length;

		public void UpdateMemorySize() => this.MemorySize = CalculateMemorySize();
		public void UpdateMemorySize(T[] array) => this.MemorySize = CalculateMemorySize(array);
	
	}
}
