using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Core
{
	public partial class VectorBase<T>
	{
		/*
		* TODO: [OPTIMISATION] If the gpu memory manager can store a flag to track data divergence between CPU/GPU,
		* then this can be used to avoid unnecessary data transfers on this function call
		*/ 
		public void SyncCPU()
		{
			if (ID != 0) Value = Pull();
			Length = Value.Length;
		}

		public void SyncCPU(MemoryBuffer buffer)
		{
			if (Value == null || Value.Length != buffer.Length)
				Value = new T[buffer.Length];

			buffer.AsArrayView<T>(0, buffer.Length).CopyToCPU(Value);
			Length = Value.Length;

			//buffer.CopyToCPU<T>(ref Value);

			//Value = ((MemoryBuffer1D<T, Stride1D.Dense>)buffer).GetAsArray1D();
		}


	}


}
