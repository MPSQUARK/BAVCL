using ILGPU.Runtime;

namespace BAVCL.Core
{
	public partial class VectorBase<T>
	{
		/// <summary>
		/// Store info about data to LRU
		/// </summary>
		internal MemoryBuffer Cache()
        {
			MemoryBuffer buffer;
			(ID, buffer) = gpu.Allocate(this, Value);
			return buffer;
		}
			

		/// <summary>
		/// Store info about data to LRU
		/// </summary>
		internal MemoryBuffer Cache(T[] array)
        {
			MemoryBuffer buffer;
			(ID, buffer) = gpu.Allocate(this, array);
			return buffer;
		}

	}


}
