using ILGPU.Runtime;
using ILGPU.Algorithms;
using System;
using BAVCL.Core;
using BAVCL.Ext;
using ILGPU;

namespace BAVCL;


/// <summary>
/// Class for 1D and 2D Vector support
/// Float Precision
/// </summary>
public sealed partial class Vector : VectorBase<float>
{

	// CONSTRUCTOR
	/// <summary>
	/// Constructs a Vector class object.
	/// </summary>
	/// <param name="gpu">The device to use when computing this Vector.</param>
	/// <param name="values">The array of data contained in this Vector.</param>
	/// <param name="columns">0 = 1D row (default), 1 = column vector, N&gt;1 = matrix width</param>
	public Vector(GPU gpu, float[] values, int columns = 0, bool cache = true) :
		base(gpu, values, columns, cache)
	{ }

	/// <summary>
	/// Constructs a Vector object of length 'length' with uninitialized values.
	/// Ideal for creating output vectors. JUST REMEMBER TO SET/UPDATE ALL VALUES.
	/// WARNING: Values are NOT initialized to zero, and you may get random leftover data.
	/// </summary>
	/// <param name="gpu"></param>
	/// <param name="length"></param>
	/// <param name="columns"></param>
	/// <returns></returns>
	public Vector(GPU gpu, int length, int columns = 0) :
		base(gpu, length, columns)
	{ }

	public override void Print() => Console.WriteLine(ToStr());

	public void Print(byte decimalplaces = 2, bool syncCPU = true) => Console.WriteLine(ToStr(decimalplaces, syncCPU));

	// METHODS
	public bool Equals(Vector vector)
	{
		if (Length != vector.Length)
			return false;

		ReadOnlySpan<float> left = RetrieveReadOnlySpan();
		ReadOnlySpan<float> right = vector.RetrieveReadOnlySpan();

        // TODO: Use SIMD for comparison
        for (int i = 0; i < Length; i++)
		{
			if (left[i] != right[i])
				return false;
		}

		return true;
	}

	public Vector Copy(bool Cache = true)
	{
		if (ID == 0)
			return new Vector(Gpu, ToArray(), Columns, Cache);

		return new Vector(Gpu, Pull(), Columns, Cache);
	}

	#region "MATHEMATICAL PROPERTIES"
	public override float Mean() => Sum() / Length;

	public float Std() => XMath.Sqrt(Var());

	public float Var()
	{
		if (Length < 10000)
		{
			int
				vectorSize = System.Numerics.Vector<float>.Count,
				i = 0;

			ReadOnlySpan<float> data = RetrieveReadOnlySpan();
			float mean = Mean();

			System.Numerics.Vector<float> meanvec = new(mean);

			System.Numerics.Vector<float> sumVector = System.Numerics.Vector<float>.Zero;

			for (; i <= data.Length - vectorSize; i += vectorSize)
			{
				System.Numerics.Vector<float> input = new(data.Slice(i, vectorSize));
				System.Numerics.Vector<float> difference = input - meanvec;

				sumVector += (difference * difference);
			}

			float sum = 0;

			for (int j = 0; j < vectorSize; j++)
				sum += sumVector[j];

			for (; i < data.Length; i++)
				sum += XMath.Pow((data[i] - mean), 2f);

			return sum / Length;
		}

		return OP(this, Mean(), Operations.differenceSquared).Sum() / Length;
	}
	public override float Range() => Max() - Min();
	public void Flatten() => Columns = 0;

	#endregion


	#region "CONVERSION"

	public Geometric.Vector3 ToVector3()
	{
		if (Length % 3 != 0) { throw new Exception("Vector length must be a multiple of 3"); }
		if (ID != 0)
			return new Geometric.Vector3(Gpu, Pull());

		return new Geometric.Vector3(Gpu, ToArray());
	}

	#endregion


	#region "OPERATORS"
	public static Vector operator +(Vector vector) =>
		AbsX(vector);
	public static Vector operator +(Vector vectorA, Vector vectorB) =>
		OP(vectorA, vectorB, Operations.add);
	public static Vector operator +(Vector vector, float Scalar) =>
		OP(vector, Scalar, Operations.add);
	public static Vector operator +(float Scalar, Vector vector) =>
		OP(vector, Scalar, Operations.add);

	public static Vector operator -(Vector vector) =>
		OP(vector, -1, Operations.multiply);
	public static Vector operator -(Vector vectorA, Vector vectorB) =>
		OP(vectorA, vectorB, Operations.subtract);
	public static Vector operator -(Vector vector, float scalar) =>
		OP(vector, scalar, Operations.subtract);
	public static Vector operator -(float scalar, Vector vector) =>
		OP(vector, scalar, Operations.flipSubtract);

	public static Vector operator *(Vector vectorA, Vector vectorB) =>
		OP(vectorA, vectorB, Operations.multiply);

	public static Vector operator *(Vector vector, float scalar) =>
		OP(vector, scalar, Operations.multiply);

	public static Vector operator *(float scalar, Vector vector) =>
		OP(vector, scalar, Operations.multiply);

	public static Vector operator /(Vector vectorA, Vector vectorB) =>
		OP(vectorA, vectorB, Operations.divide);
	public static Vector operator /(Vector vector, float scalar) =>
		OP(vector, scalar, Operations.divide);
	public static Vector operator /(float scalar, Vector vector) =>
		OP(vector, scalar, Operations.flipDivide);

	public static Vector operator ^(Vector vectorA, Vector vectorB) =>
		OP(vectorA, vectorB, Operations.pow);
	public static Vector operator ^(Vector vector, float scalar) =>
		OP(vector, scalar, Operations.pow);
	public static Vector operator ^(float Scalar, Vector vector) =>
		OP(vector, Scalar, Operations.flipPow);


    #endregion



    // FUNCTIONS
    public static Vector OP(Vector vectorA, Vector vectorB, Operations operation) => _BroadcastOP(vectorA, vectorB, operation);

    public Vector IPOP(Vector vectorB, Operations operation) => _BroadcastOP_IP(vectorB, operation);

    public static Vector OP(Vector vector, float scalar, Operations operation)
	{
		GPU gpu = vector.Gpu;
		Vector output = new(gpu, vector.Length, vector.Columns);

		using (GpuScope.Begin(output, vector))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer();

			gpu.s_opFKernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, scalar, new SpecializedValue<int>((int)operation));
			gpu.accelerator.Synchronize();
		}

		return output;
	}

	public Vector IPOP(float scalar, Operations operation)
	{
		using (GpuScope.Begin(this))
		{
			MemoryBuffer1D<float, Stride1D.Dense> buffer = GetBuffer();
			Gpu.s_FloatOPKernelIP(Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, scalar, new SpecializedValue<int>((int)operation));
			Gpu.accelerator.Synchronize();
		}

		return this;
	}


	internal static Vector _VectorVectorOP(Vector vectorA, Vector vectorB, Operations operation)
	{
		GPU gpu = vectorA.Gpu;
		Vector output = new(gpu, vectorA.Length, vectorA.Columns);

		using (GpuScope.Begin(output, vectorA, vectorB))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vectorA.GetBuffer(),
				buffer3 = vectorB.GetBuffer();

			gpu.a_opFKernel(gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, buffer3.View, new SpecializedValue<int>((int)operation));
			gpu.accelerator.Synchronize();
		}

		return output;
	}

	internal Vector _VectorVectorOP_IP(Vector vectorB, Operations operation)
	{
		using (GpuScope.Begin(this, vectorB))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = GetBuffer(),
				buffer2 = vectorB.GetBuffer();

			Gpu.a_FloatOPKernelIP(Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, buffer2.View, new SpecializedValue<int>((int)operation));
			Gpu.accelerator.Synchronize();
		}

		return this;
	}

	internal static Vector RunReduceRowOp(Vector vector, Vector matrix, Operations operation)
	{
		GPU gpu = vector.Gpu;
		Vector output = new(gpu, matrix.RowCount(), 1);

		using (GpuScope.Begin(output, vector, matrix))
		{
			MemoryBuffer1D<float, Stride1D.Dense>
				buffer = output.GetBuffer(),
				buffer2 = vector.GetBuffer(),
				buffer3 = matrix.GetBuffer();

			gpu.reduceRowOpKernel(
				gpu.accelerator.DefaultStream,
				matrix.RowCount(),
				buffer.View,
				buffer2.View,
				buffer3.View,
				matrix.Columns,
				new SpecializedValue<int>((int)operation));

			gpu.accelerator.Synchronize();
		}

		return output;
	}



	public Vector Log_IP(float @base)
	{
		using (GpuScope.Begin(this))
		{
			MemoryBuffer1D<float, Stride1D.Dense> buffer = GetBuffer();
			Gpu.LogKernel(Gpu.accelerator.DefaultStream, buffer.IntExtent, buffer.View, @base);
			Gpu.accelerator.Synchronize();
		}

		return this;
	}

}
