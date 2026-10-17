using System;
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

	public Vector Copy(bool Cache = true) =>
		new(Gpu, ToArray(), Columns, Cache);

	public void Flatten() => Columns = 0;

	public Geometric.Vector3 ToVector3()
	{
		if (Length % 3 != 0) { throw new Exception("Vector length must be a multiple of 3"); }
		if (ID != 0)
			return new Geometric.Vector3(Gpu, Pull());

		return new Geometric.Vector3(Gpu, ToArray());
	}

	public override string ToString() => VectorStructuralExtensions.ToStr(this);

	/// <summary>Compact true lanes (accelerated scan pipeline; same as <c>| mask</c>).</summary>
	public Vector this[Mask mask] => MaskGpu.FilterX(this, mask);

	public Mask CompareEqualsX(Vector other) =>
		MaskGpu.CompareX(this, other, VectorComparison.Equal);

	public Mask CompareNotEqualsX(Vector other) =>
		MaskGpu.CompareX(this, other, VectorComparison.NotEqual);

	public Mask CompareX(Vector other, VectorComparison comparison) =>
		MaskGpu.CompareX(this, other, comparison);

	public Mask CompareEqualsX(float scalar) =>
		MaskGpu.CompareX(this, scalar, VectorComparison.Equal);

	public Mask CompareNotEqualsX(float scalar) =>
		MaskGpu.CompareX(this, scalar, VectorComparison.NotEqual);

	public Mask CompareX(float scalar, VectorComparison comparison) =>
		MaskGpu.CompareX(this, scalar, comparison);

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

	/// <summary>Partition by mask (accelerated scan pipeline). Vector/vector <c>/</c> still uses element-wise divide.</summary>
	public static (Vector TrueLanes, Vector FalseLanes) operator /(Vector vector, Mask mask) =>
		MaskGpu.PartitionX(vector, mask);

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

	/// <summary>Mask fill false lanes (accelerated <c>maskFilter</c> kernel).</summary>
	public static Vector operator &(Vector vector, Mask mask) =>
		MaskGpu.MaskX(vector, mask, 0f);

	public static Vector operator &(Vector vector, (Mask mask, float fill) masked) =>
		MaskGpu.MaskX(vector, masked.mask, masked.fill);

	/// <summary>Compact true lanes (accelerated scan pipeline; CPU host path: <see cref="BAVCL.Modules.Masking.MaskModuleExtensions.Filter"/>).</summary>
	public static Vector operator |(Vector vector, Mask mask) =>
		MaskGpu.FilterX(vector, mask);

	public static Mask operator >(Vector left, Vector right) =>
		MaskGpu.CompareX(left, right, VectorComparison.Greater);

	public static Mask operator <(Vector left, Vector right) =>
		MaskGpu.CompareX(left, right, VectorComparison.Less);

	public static Mask operator >=(Vector left, Vector right) =>
		MaskGpu.CompareX(left, right, VectorComparison.GreaterOrEqual);

	public static Mask operator <=(Vector left, Vector right) =>
		MaskGpu.CompareX(left, right, VectorComparison.LessOrEqual);

	public static Mask operator >(Vector vector, float scalar) =>
		MaskGpu.CompareX(vector, scalar, VectorComparison.Greater);

	public static Mask operator <(Vector vector, float scalar) =>
		MaskGpu.CompareX(vector, scalar, VectorComparison.Less);

	public static Mask operator >=(Vector vector, float scalar) =>
		MaskGpu.CompareX(vector, scalar, VectorComparison.GreaterOrEqual);

	public static Mask operator <=(Vector vector, float scalar) =>
		MaskGpu.CompareX(vector, scalar, VectorComparison.LessOrEqual);

    #endregion
}
