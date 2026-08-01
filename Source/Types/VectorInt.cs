using System;
using BAVCL.Core;
using BAVCL.Modules.Arithmetic;
using BAVCL.Modules.GpuOps;
using BAVCL.Modules.Masking;
using BAVCL.Modules.Structural;
using BAVCL.Types;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

/// <summary>
/// Class for 1D and 2D Vector support
/// Int32 Precision
/// </summary>
public sealed partial class VectorInt : VectorBase<int>
{
	public VectorInt(GPU gpu, int[] values, int columns = 0, bool cache = true) :
		base(gpu, values, columns, cache)
	{ }

	public VectorInt(GPU gpu, int length, int columns = 0) :
		base(gpu, length, columns)
	{ }

	public override void Print() => Console.WriteLine(this.ToStr());

	public bool Equals(VectorInt vector)
	{
		if (Length != vector.Length)
			return false;

		ReadOnlySpan<int> left = RetrieveReadOnlySpan();
		ReadOnlySpan<int> right = vector.RetrieveReadOnlySpan();

		for (int i = 0; i < Length; i++)
		{
			if (left[i] != right[i])
				return false;
		}

		return true;
	}

	public VectorInt Copy(bool cache = true)
	{
		if (ID == 0)
			return new VectorInt(Gpu, ToArray(), Columns, cache);

		return new VectorInt(Gpu, Pull(), Columns, cache);
	}

	public void Flatten() => Columns = 0;

	public override string ToString() => VectorStructuralExtensions.ToStr(this);

	public VectorInt this[Mask mask] => MaskVectorIntOps.Filter(this, mask);

	public Mask CompareEquals(VectorInt other) =>
		MaskVectorIntOps.Compare(this, other, VectorComparison.Equal);

	public Mask CompareNotEquals(VectorInt other) =>
		MaskVectorIntOps.Compare(this, other, VectorComparison.NotEqual);

	public Mask Compare(VectorInt other, VectorComparison comparison) =>
		MaskVectorIntOps.Compare(this, other, comparison);

	public Mask CompareEquals(int scalar) =>
		MaskVectorIntOps.Compare(this, scalar, VectorComparison.Equal);

	public Mask CompareNotEquals(int scalar) =>
		MaskVectorIntOps.Compare(this, scalar, VectorComparison.NotEqual);

	public Mask Compare(int scalar, VectorComparison comparison) =>
		MaskVectorIntOps.Compare(this, scalar, comparison);

	public static explicit operator VectorInt(Vector vector) => CastCore.ToVectorInt(vector);

	public static explicit operator Vector(VectorInt vector) => CastCore.ToVector(vector);

	#region OPERATORS

	/// <summary>
	/// Returns the absolute value of the vector (bitwise, sign bit cleared).
	/// </summary>
	public static VectorInt operator +(VectorInt vector) =>
		vector.AbsX();

	public static VectorInt operator +(VectorInt vectorA, VectorInt vectorB) =>
		vectorA.OP(vectorB, Operations.add);

	public static VectorInt operator +(VectorInt vector, int scalar) =>
		vector.OP(scalar, Operations.add);

	public static VectorInt operator +(int scalar, VectorInt vector) =>
		vector.OP(scalar, Operations.add);

	// In-place operator optimization
	public void operator +=(VectorInt vectorB) =>
		this.IPOP(vectorB, Operations.add);

	public void operator +=(int scalar) =>
		this.IPOP(scalar, Operations.add);

	/// <summary>
	/// Negates the vector via two's complement (no overflow exception).
	/// </summary>
	public static VectorInt operator -(VectorInt vector) =>
		vector.Negate();

	public static VectorInt operator -(VectorInt vectorA, VectorInt vectorB) =>
		vectorA.OP(vectorB, Operations.subtract);

	public static VectorInt operator -(VectorInt vector, int scalar) =>
		vector.OP(scalar, Operations.subtract);

	public static VectorInt operator -(int scalar, VectorInt vector) =>
		vector.OP(scalar, Operations.flipSubtract);

	public void operator -=(VectorInt vectorB) =>
		this.IPOP(vectorB, Operations.subtract);

	public void operator -=(int scalar) =>
		this.IPOP(scalar, Operations.subtract);

	public static VectorInt operator *(VectorInt vectorA, VectorInt vectorB) =>
		vectorA.OP(vectorB, Operations.multiply);

	public static VectorInt operator *(VectorInt vector, int scalar) =>
		vector.OP(scalar, Operations.multiply);

	public static VectorInt operator *(int scalar, VectorInt vector) =>
		vector.OP(scalar, Operations.multiply);

	public void operator *=(VectorInt vectorB) =>
		this.IPOP(vectorB, Operations.multiply);

	public void operator *=(int scalar) =>
		this.IPOP(scalar, Operations.multiply);

	public static VectorInt operator /(VectorInt vectorA, VectorInt vectorB) =>
		vectorA.OP(vectorB, Operations.divide);

	public static VectorInt operator /(VectorInt vector, int scalar) =>
		vector.OP(scalar, Operations.divide);

	public static VectorInt operator /(int scalar, VectorInt vector) =>
		vector.OP(scalar, Operations.flipDivide);

	public static (VectorInt TrueLanes, VectorInt FalseLanes) operator /(VectorInt vector, Mask mask) =>
		MaskVectorIntOps.Partition(vector, mask);

	public void operator /=(VectorInt vectorB) =>
		this.IPOP(vectorB, Operations.divide);

	public void operator /=(int scalar) =>
		this.IPOP(scalar, Operations.divide);

	/// <summary>
	/// Element-wise modulo (C# % semantics; divide-by-zero yields int.MaxValue + flag).
	/// </summary>
	public static VectorInt operator %(VectorInt vectorA, VectorInt vectorB) =>
		vectorA.OP(vectorB, Operations.modulo);

	public static VectorInt operator %(VectorInt vector, int scalar) =>
		vector.OP(scalar, Operations.modulo);

	public static VectorInt operator %(int scalar, VectorInt vector) =>
		vector.OP(scalar, Operations.flipModulo);

	public void operator %=(VectorInt vectorB) =>
		this.IPOP(vectorB, Operations.modulo);

	public void operator %=(int scalar) =>
		this.IPOP(scalar, Operations.modulo);

	/// <summary>
	/// Element-wise bitwise XOR.
	/// </summary>
	public static VectorInt operator ^(VectorInt vectorA, VectorInt vectorB) =>
		vectorA.OP(vectorB, Operations.bitwiseXor);

	public static VectorInt operator ^(VectorInt vector, int scalar) =>
		vector.OP(scalar, Operations.bitwiseXor);

	public static VectorInt operator ^(int scalar, VectorInt vector) =>
		vector.OP(scalar, Operations.bitwiseXor);

	public void operator ^=(VectorInt vectorB) =>
		this.IPOP(vectorB, Operations.bitwiseXor);

	public void operator ^=(int scalar) =>
		this.IPOP(scalar, Operations.bitwiseXor);

	/// <summary>
	/// Element-wise left shift. Broadcast rules match <see cref="OP"/>.
	/// </summary>
	public static VectorInt operator <<(VectorInt vector, int shift) =>
		vector.OP(shift, Operations.leftShift);

	public static VectorInt operator <<(VectorInt vector, VectorInt shiftCounts) =>
		vector.OP(shiftCounts, Operations.leftShift);

	public void operator <<=(int shift) =>
		this.IPOP(shift, Operations.leftShift);

	public void operator <<=(VectorInt shiftCounts) =>
		this.IPOP(shiftCounts, Operations.leftShift);

	/// <summary>
	/// Element-wise arithmetic (sign-extending) right shift.
	/// </summary>
	public static VectorInt operator >>(VectorInt vector, int shift) =>
		vector.OP(shift, Operations.rightShift);

	public static VectorInt operator >>(VectorInt vector, VectorInt shiftCounts) =>
		vector.OP(shiftCounts, Operations.rightShift);

	public void operator >>=(int shift) =>
		this.IPOP(shift, Operations.rightShift);

	public void operator >>=(VectorInt shiftCounts) =>
		this.IPOP(shiftCounts, Operations.rightShift);

	public static VectorInt operator &(VectorInt vectorA, VectorInt vectorB) =>
		vectorA.OP(vectorB, Operations.bitwiseAnd);

	public static VectorInt operator &(VectorInt vector, int scalar) =>
		vector.OP(scalar, Operations.bitwiseAnd);

	public static VectorInt operator &(int scalar, VectorInt vector) =>
		vector.OP(scalar, Operations.bitwiseAnd);

	public static VectorInt operator &(VectorInt vector, Mask mask) =>
		MaskVectorIntOps.Mask(vector, mask, 0);

	public static VectorInt operator &(VectorInt vector, (Mask mask, int fill) masked) =>
		MaskVectorIntOps.Mask(vector, masked.mask, masked.fill);

	public void operator &=(VectorInt vectorB) =>
		this.IPOP(vectorB, Operations.bitwiseAnd);

	public void operator &=(int scalar) =>
		this.IPOP(scalar, Operations.bitwiseAnd);

	public static VectorInt operator |(VectorInt vector, Mask mask) =>
		MaskVectorIntOps.Filter(vector, mask);

	public static Mask operator >(VectorInt left, VectorInt right) =>
		MaskVectorIntOps.Compare(left, right, VectorComparison.Greater);

	public static Mask operator <(VectorInt left, VectorInt right) =>
		MaskVectorIntOps.Compare(left, right, VectorComparison.Less);

	public static Mask operator >=(VectorInt left, VectorInt right) =>
		MaskVectorIntOps.Compare(left, right, VectorComparison.GreaterOrEqual);

	public static Mask operator <=(VectorInt left, VectorInt right) =>
		MaskVectorIntOps.Compare(left, right, VectorComparison.LessOrEqual);

	public static Mask operator >(VectorInt vector, int scalar) =>
		MaskVectorIntOps.Compare(vector, scalar, VectorComparison.Greater);

	public static Mask operator <(VectorInt vector, int scalar) =>
		MaskVectorIntOps.Compare(vector, scalar, VectorComparison.Less);

	public static Mask operator >=(VectorInt vector, int scalar) =>
		MaskVectorIntOps.Compare(vector, scalar, VectorComparison.GreaterOrEqual);

	public static Mask operator <=(VectorInt vector, int scalar) =>
		MaskVectorIntOps.Compare(vector, scalar, VectorComparison.LessOrEqual);

	#endregion

	internal VectorInt Negate()
	{
		VectorInt copy = Copy();
		NegateInPlace(copy);
		return copy;
	}

	internal void NegateInPlace(VectorInt vector)
	{
		using (GpuScope.Begin(vector))
		{
			MemoryBuffer1D<int, Stride1D.Dense> buffer = vector.GetBuffer();
			vector.Gpu.negateIntKernel(vector.Gpu.DefaultStream, buffer.IntExtent, buffer.View);
			vector.Gpu.Synchronize();
		}
	}
}
