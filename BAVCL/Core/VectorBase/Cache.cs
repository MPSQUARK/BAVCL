namespace BAVCL.Core
{
	public partial class VectorBase<T>
	{
		internal void Cache()
		{
			this.UpdateMemorySize();
			this.Length = this.Value.Length;
			
			// Store info about data to LRU
			this.ID = this.gpu.Allocate(this, this.Value);
		}

		internal void Cache(T[] array)
		{
			this.MemorySize = this.CalculateMemorySize(array);
			this.Length = array.Length;

			// Store info about data to LRU
			this.ID = this.gpu.Allocate(this, array);
		}



	}


}
