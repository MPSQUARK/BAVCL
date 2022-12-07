namespace BAVCL.Core
{
	public partial class VectorBase<T>
	{
		internal void Cache()
		{	
			// Store info about data to LRU
			this.ID = this.gpu.Allocate(this, this.Value);
		}

		internal void Cache(T[] array)
		{
			// Store info about data to LRU
			this.ID = this.gpu.Allocate(this, array);
		}



	}


}
