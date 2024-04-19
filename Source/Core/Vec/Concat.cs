using System;
using System.Numerics;
using BAVCL.Core.Enums;
using BAVCL.MemoryManagement;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
	/// <summary>
	/// Concatinates VectorB onto the end of VectorA.
	/// Preserves the value of Columns of VectorA.
	/// </summary>
	/// <param name="vectorA"></param>
	/// <param name="vectorB"></param>
	/// <returns></returns>
	public static Vec<T> Concat(Vec<T> vectorA, Vec<T> vectorB, char axis = 'r', bool warp = false)
	{
		return vectorA.Copy().Concat_IP(vectorB, axis, warp);
	}
	public Vec<T> Concat_IP(Vec<T> vector, char axis = 'r', bool warp = false)
	{
		if (axis == 'r')
		{
			this.Append_IP(vector);
			return this;
		}

		// IF Concat in COLUMN mode

		if (Is2D() && vector.Is2D())
		{
			if ((Rows != vector.Rows) && (Rows != vector.Columns))
			{
				throw new Exception(
					$"Vectors CANNOT be appended. " +
					$"This Vector has the shape ({this.Rows},{this.Columns}). " +
					$"The 2D Vector being appended has the shape ({vector.Rows},{vector.Columns})");
			}

			if (Rows == vector.Columns)
			{
				if (!warp)
				{
					vector.Transpose_IP();
				}

				if (warp && (vector.Length % Rows == 0))
				{
					vector.Columns = (uint)(vector.Values.Length / Rows);
				}

			}

		}
		// IF 1D
		if (vector.Is1D())
		{

			if (vector.Values.Length % Rows != 0)
			{
				throw new Exception($"Vectors CANNOT be appended. " +
					$"This array has shape ({Rows},{Columns}), 1D vector being appended has {vector.Length} Length");
			}

			vector.Columns = (uint)(vector.Values.Length / this.Rows);

		}

		Vec<T> Output = new(Gpu, vector.Length + Length);

		IncrementLiveCount();
		vector.IncrementLiveCount();
		Output.IncrementLiveCount();

		MemoryBuffer1D<T, Stride1D.Dense>
			buffer = Output.GetBuffer(),        // Output
			buffer2 = GetBuffer(),              // Input
			buffer3 = vector.GetBuffer();       // Input


		var kernel = (Action<AcceleratorStream, Index1D, ArrayView<T>, ArrayView<T>, ArrayView<T>, int, int>)Gpu.GetKernel<T>(KernelType.Append);
		kernel(Gpu.accelerator.DefaultStream, Rows, buffer.View, buffer2.View, buffer3.View, (int)Columns, (int)vector.Columns);

		Gpu.accelerator.Synchronize();

		DecrementLiveCount();
		vector.DecrementLiveCount();
		Output.DecrementLiveCount();

		this.Columns += vector.Columns;

		return TransferBuffer(Output);
	}
}
