namespace BAVCL.Core
{
	public partial class VectorBase<T>
	{
		/// <summary>
		/// Store info about data to LRU
		/// </summary>
		internal void Cache() => ID = gpu.Allocate(this, Value);

		/// <summary>
		/// Store info about data to LRU
		/// </summary>
		internal void Cache(T[] array) => ID = gpu.Allocate(this, array);

	}


}
