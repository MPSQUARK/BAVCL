using System;
using BAVCL.Core;
using BAVCL.Modules.Arithmetic;
using BAVCL.Modules.GpuOps;
using BAVCL.Modules.Masking;
using BAVCL.Modules.Structural;
using BAVCL.Types;

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

	private Vector(GPU gpu, int columns) : base(gpu, columns) { }

	/// <summary>Creates an empty vessel shell with no GPU buffer.</summary>
	public static Vessel<Vector> CreateVessel(GPU gpu, int columns = 0) =>
		new(new Vector(gpu, columns), gpu);

	public override void Print() => Console.WriteLine(this.ToStr());

	public void Print(byte decimalplaces = 2) => Console.WriteLine(VectorStructuralExtensions.ToStr(this, decimalplaces));

	public bool Equals(Vector vector)
	{
		if (Length != vector.Length)
			return false;

		ReadOnlySpan<float> left = RetrieveReadOnlySpan();
		ReadOnlySpan<float> right = vector.RetrieveReadOnlySpan();

		// TODO: Can use SIMD
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

		// TODO: I don't think PULL is the right API to use here. Need to investigate.
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

	public override string ToString() => VectorStructuralExtensions.ToStr(this);

	public Vector this[Mask mask] => MaskVectorOps.Filter(this, mask);

	public Mask CompareEquals(Vector other) =>
		MaskVectorOps.Compare(this, other, VectorComparison.Equal);

	public Mask CompareNotEquals(Vector other) =>
		MaskVectorOps.Compare(this, other, VectorComparison.NotEqual);

	public Mask Compare(Vector other, VectorComparison comparison) =>
		MaskVectorOps.Compare(this, other, comparison);

	public Mask CompareEquals(float scalar) =>
		MaskVectorOps.Compare(this, scalar, VectorComparison.Equal);

	public Mask CompareNotEquals(float scalar) =>
		MaskVectorOps.Compare(this, scalar, VectorComparison.NotEqual);

	public Mask Compare(float scalar, VectorComparison comparison) =>
		MaskVectorOps.Compare(this, scalar, comparison);

    #region OPERATORS
    /// <summary>
    /// Returns the absolute value of the vector.
    /// </summary>
    public static Vector operator +(Vector vector) =>
		vector.AbsX();
	public static Vector operator +(Vector vectorA, Vector vectorB) =>
		vectorA.OP(vectorB, Operations.add);
	public static Vector operator +(Vector vector, float Scalar) =>
		vector.OP(Scalar, Operations.add);
	public static Vector operator +(float Scalar, Vector vector) =>
		vector.OP(Scalar, Operations.add);
    // In-place operator optimization
    public void operator +=(Vector vectorB) =>
		this.IPOP(vectorB, Operations.add);
	public void operator +=(float Scalar) =>
        this.IPOP(Scalar, Operations.add);

    /// <summary>
    /// Negates the vector by multiplying it by -1.
    /// </summary>
    public static Vector operator -(Vector vector) =>
		vector.OP(-1, Operations.multiply);
	public static Vector operator -(Vector vectorA, Vector vectorB) =>
		vectorA.OP(vectorB, Operations.subtract);
	public static Vector operator -(Vector vector, float scalar) =>
		vector.OP(scalar, Operations.subtract);
	public static Vector operator -(float scalar, Vector vector) =>
		vector.OP(scalar, Operations.flipSubtract);
    // In-place operator optimization
    public void operator -=(Vector vectorB) =>
        this.IPOP(vectorB, Operations.subtract);
    public void operator -=(float Scalar) =>
        this.IPOP(Scalar, Operations.subtract);

    public static Vector operator *(Vector vectorA, Vector vectorB) =>
		vectorA.OP(vectorB, Operations.multiply);
	public static Vector operator *(Vector vector, float scalar) =>
		vector.OP(scalar, Operations.multiply);
	public static Vector operator *(float scalar, Vector vector) =>
		vector.OP(scalar, Operations.multiply);
    // In-place operator optimization
    public void operator *=(Vector vectorB) =>
        this.IPOP(vectorB, Operations.multiply);
    public void operator *=(float Scalar) =>
        this.IPOP(Scalar, Operations.multiply);

    public static Vector operator /(Vector vectorA, Vector vectorB) =>
		vectorA.OP(vectorB, Operations.divide);
	public static Vector operator /(Vector vector, float scalar) =>
		vector.OP(scalar, Operations.divide);
	public static Vector operator /(float scalar, Vector vector) =>
		vector.OP(scalar, Operations.flipDivide);

	public static (Vector TrueLanes, Vector FalseLanes) operator /(Vector vector, Mask mask) =>
		MaskVectorOps.Partition(vector, mask);

    // In-place operator optimization
    public void operator /=(Vector vectorB) =>
        this.IPOP(vectorB, Operations.divide);
    public void operator /=(float Scalar) =>
        this.IPOP(Scalar, Operations.divide);

    public static Vector operator ^(Vector vectorA, Vector vectorB) =>
		vectorA.OP(vectorB, Operations.pow);
	public static Vector operator ^(Vector vector, float scalar) =>
		vector.OP(scalar, Operations.pow);
	public static Vector operator ^(float Scalar, Vector vector) =>
		vector.OP(Scalar, Operations.flipPow);
    // In-place operator optimization
    public void operator ^=(Vector vectorB) =>
        this.IPOP(vectorB, Operations.pow);
	public void operator ^=(float Scalar) =>
        this.IPOP(Scalar, Operations.pow);

	public static Vector operator &(Vector vector, Mask mask) =>
		MaskVectorOps.Mask(vector, mask, 0f);

	public static Vector operator &(Vector vector, (Mask mask, float fill) masked) =>
		MaskVectorOps.Mask(vector, masked.mask, masked.fill);

	public static Vector operator |(Vector vector, Mask mask) =>
		MaskVectorOps.Filter(vector, mask);

	public static Mask operator >(Vector left, Vector right) =>
		MaskVectorOps.Compare(left, right, VectorComparison.Greater);

	public static Mask operator <(Vector left, Vector right) =>
		MaskVectorOps.Compare(left, right, VectorComparison.Less);

	public static Mask operator >=(Vector left, Vector right) =>
		MaskVectorOps.Compare(left, right, VectorComparison.GreaterOrEqual);

	public static Mask operator <=(Vector left, Vector right) =>
		MaskVectorOps.Compare(left, right, VectorComparison.LessOrEqual);

	public static Mask operator >(Vector vector, float scalar) =>
		MaskVectorOps.Compare(vector, scalar, VectorComparison.Greater);

	public static Mask operator <(Vector vector, float scalar) =>
		MaskVectorOps.Compare(vector, scalar, VectorComparison.Less);

	public static Mask operator >=(Vector vector, float scalar) =>
		MaskVectorOps.Compare(vector, scalar, VectorComparison.GreaterOrEqual);

	public static Mask operator <=(Vector vector, float scalar) =>
		MaskVectorOps.Compare(vector, scalar, VectorComparison.LessOrEqual);

    #endregion
}
