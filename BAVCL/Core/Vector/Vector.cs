using System;
using BAVCL.Modules.Arithmetic;
using BAVCL.Modules.GpuOps;
using BAVCL.Modules.Structural;

namespace BAVCL;

/// <summary>
/// Class for 1D and 2D Vector support
/// Float Precision
/// </summary>
public sealed partial class Vector : VectorBase<float>
{
	public Vector(GPU gpu, float[] values, int columns = 0, bool cache = true) :
		base(gpu, values, columns, cache)
	{ }

	public Vector(GPU gpu, int length, int columns = 0) :
		base(gpu, length, columns)
	{ }

	public override void Print() => Console.WriteLine(this.ToStr());

	public void Print(byte decimalplaces = 2, bool syncCPU = true) => Console.WriteLine(this.ToStr(decimalplaces, syncCPU));

	public bool Equals(Vector vector)
	{
		if (Length != vector.Length)
			return false;

		ReadOnlySpan<float> left = RetrieveReadOnlySpan();
		ReadOnlySpan<float> right = vector.RetrieveReadOnlySpan();

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

	public void Flatten() => Columns = 0;

	public Geometric.Vector3 ToVector3()
	{
		if (Length % 3 != 0) { throw new Exception("Vector length must be a multiple of 3"); }
		if (ID != 0)
			return new Geometric.Vector3(Gpu, Pull());

		return new Geometric.Vector3(Gpu, ToArray());
	}

	public override string ToString() => this.ToStr(2, false);

	#region "OPERATORS"
	public static Vector operator +(Vector vector) =>
		vector.AbsX();
	public static Vector operator +(Vector vectorA, Vector vectorB) =>
		vectorA.OP(vectorB, Operations.add);
	public static Vector operator +(Vector vector, float Scalar) =>
		vector.OP(Scalar, Operations.add);
	public static Vector operator +(float Scalar, Vector vector) =>
		vector.OP(Scalar, Operations.add);

	public static Vector operator -(Vector vector) =>
		vector.OP(-1, Operations.multiply);
	public static Vector operator -(Vector vectorA, Vector vectorB) =>
		vectorA.OP(vectorB, Operations.subtract);
	public static Vector operator -(Vector vector, float scalar) =>
		vector.OP(scalar, Operations.subtract);
	public static Vector operator -(float scalar, Vector vector) =>
		vector.OP(scalar, Operations.flipSubtract);

	public static Vector operator *(Vector vectorA, Vector vectorB) =>
		vectorA.OP(vectorB, Operations.multiply);

	public static Vector operator *(Vector vector, float scalar) =>
		vector.OP(scalar, Operations.multiply);

	public static Vector operator *(float scalar, Vector vector) =>
		vector.OP(scalar, Operations.multiply);

	public static Vector operator /(Vector vectorA, Vector vectorB) =>
		vectorA.OP(vectorB, Operations.divide);
	public static Vector operator /(Vector vector, float scalar) =>
		vector.OP(scalar, Operations.divide);
	public static Vector operator /(float scalar, Vector vector) =>
		vector.OP(scalar, Operations.flipDivide);

	public static Vector operator ^(Vector vectorA, Vector vectorB) =>
		vectorA.OP(vectorB, Operations.pow);
	public static Vector operator ^(Vector vector, float scalar) =>
		vector.OP(scalar, Operations.pow);
	public static Vector operator ^(float Scalar, Vector vector) =>
		vector.OP(Scalar, Operations.flipPow);
	#endregion
}
