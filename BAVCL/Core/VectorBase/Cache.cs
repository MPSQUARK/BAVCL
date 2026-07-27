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
			(ID, MemoryBuffer buffer) = Gpu.Allocate(this);
			return buffer;
		}


		/// <summary>
		/// Store info about data to LRU
		/// </summary>
		internal MemoryBuffer Cache(T[] array)
		{
			(ID, MemoryBuffer buffer) = Gpu.Allocate(this, array);
			return buffer;
		}


		internal MemoryBuffer CacheEmpty(int length)
		{
			(ID, MemoryBuffer buffer) = Gpu.AllocateEmpty<T>(this, length);
			return buffer;
		}


	}


}
