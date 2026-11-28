using System;
using System.Linq;
using System.Numerics;
using BAVCL.Core.Exceptions;
using BAVCL.Modules.Masking;
using BAVCL.Types;
using ILGPU.Runtime;

namespace BAVCL;

/// <summary>Vector of IEEE-754 double precision complex values.</summary>
/// <remarks>
/// Complex has no natural ordering. Arithmetic and transcendental operations are
/// evaluated by <see cref="System.Numerics.Complex"/> on the CPU when no GPU kernel
/// exists. Arithmetic kernels use the vector's direct <see cref="Complex"/> buffer.
/// </remarks>
public sealed class VectorComplex64 : VectorBase<Complex>
{
	public VectorComplex64(GPU gpu, Complex[] values, int columns = 0, bool cache = true)
		: base(gpu, values, columns, cache && values.Length > 0) { }

	public VectorComplex64(GPU gpu, int length, int columns = 0)
		: base(gpu, new Complex[length], columns, cache: false) { }

	private VectorComplex64(GPU gpu, int columns) : base(gpu, [], columns, cache: false) { }

	public static Vessel<VectorComplex64> CreateVessel(GPU gpu, int columns = 0) =>
		new(new VectorComplex64(gpu, columns), gpu);

	public VectorComplex64 Copy(bool cache = true) =>
		new(Gpu, ToArray(), Columns, cache);

	public override void Print() => Console.WriteLine(string.Join(", ", RetrieveReadOnlySpan().ToArray()));

	public bool Equals(VectorComplex64 other)
	{
		ArgumentNullException.ThrowIfNull(other);
		if (Length != other.Length)
			return false;

		ReadOnlySpan<Complex> left = RetrieveReadOnlySpan();
		ReadOnlySpan<Complex> right = other.RetrieveReadOnlySpan();
		for (int i = 0; i < left.Length; i++)
			if (left[i] != right[i])
				return false;
		return true;
	}

	public Mask CompareEqualsX(VectorComplex64 other) => CompareX(other, equal: true);

	public Mask CompareNotEqualsX(VectorComplex64 other) =>
		CompareX(other, equal: false);

	public Mask CompareEquals(VectorComplex64 other)
	{
		ArgumentNullException.ThrowIfNull(other);
		Shape shape = Shape();
		Guard.MatchingDimensions(shape, other.Shape());
		ReadOnlySpan<Complex> left = RetrieveReadOnlySpan();
		ReadOnlySpan<Complex> right = other.RetrieveReadOnlySpan();
		bool[] values = new bool[left.Length];
		for (int i = 0; i < left.Length; i++)
			values[i] = left[i] == right[i];
		return new Mask(Gpu, values, Columns);
	}

	public VectorComplex64 Mask(Mask mask, Complex fill = default) =>
		MaskCompactionCpu.Mask(this, mask, fill);

	public VectorComplex64 MaskX(Mask mask, Complex fill = default) =>
		MaskGpu.MaskX(this, mask, fill);

	public VectorComplex64 Filter(Mask mask) =>
		MaskCompactionCpu.Filter(this, mask);

	public VectorComplex64 FilterX(Mask mask) =>
		MaskCompaction.FilterX(this, mask);

	public (VectorComplex64 TrueLanes, VectorComplex64 FalseLanes) Partition(Mask mask) =>
		MaskCompactionCpu.Partition(this, mask);

	public (VectorComplex64 TrueLanes, VectorComplex64 FalseLanes) PartitionX(Mask mask) =>
		MaskCompaction.PartitionX(this, mask);

	public static VectorComplex64 operator |(VectorComplex64 vector, Mask mask) => vector.FilterX(mask);

	public static (VectorComplex64 TrueLanes, VectorComplex64 FalseLanes) operator /(VectorComplex64 vector, Mask mask) =>
		vector.PartitionX(mask);

	public VectorComplex64 this[Mask mask] => this.FilterX(mask);
	public static VectorComplex64 operator &(VectorComplex64 vector, Mask mask) =>
		vector.MaskX(mask, Complex.Zero);
	public static VectorComplex64 operator &(VectorComplex64 vector, (Mask mask, Complex fill) masked) =>
		vector.MaskX(masked.mask, masked.fill);

	public VectorComplex64 AddX(VectorComplex64 other) => VectorX(other, Operations.add);
	public VectorComplex64 SubtractX(VectorComplex64 other) => VectorX(other, Operations.subtract);
	public VectorComplex64 MultiplyX(VectorComplex64 other) => VectorX(other, Operations.multiply);
	public VectorComplex64 DivideX(VectorComplex64 other) => VectorX(other, Operations.divide);
	public VectorComplex64 OP(VectorComplex64 other, Operations operation) => VectorX(other, operation);
	public VectorComplex64 OPX(VectorComplex64 other, Operations operation) => VectorX(other, operation);
	public VectorComplex64 AddX(Complex scalar) => ScalarX(scalar, Operations.add);
	public VectorComplex64 SubtractX(Complex scalar) => ScalarX(scalar, Operations.subtract);
	public VectorComplex64 MultiplyX(Complex scalar) => ScalarX(scalar, Operations.multiply);
	public VectorComplex64 DivideX(Complex scalar) => ScalarX(scalar, Operations.divide);
	public VectorComplex64 OP(Complex scalar, Operations operation) => ScalarX(scalar, operation);
	public VectorComplex64 OPX(Complex scalar, Operations operation) => ScalarX(scalar, operation);
	public void AddXIP(VectorComplex64 other) => VectorXIP(other, Operations.add);
	public void SubtractXIP(VectorComplex64 other) => VectorXIP(other, Operations.subtract);
	public void MultiplyXIP(VectorComplex64 other) => VectorXIP(other, Operations.multiply);
	public void DivideXIP(VectorComplex64 other) => VectorXIP(other, Operations.divide);
	public void IPOP(VectorComplex64 other, Operations operation) => VectorXIP(other, operation);
	public void IPOPX(VectorComplex64 other, Operations operation) => VectorXIP(other, operation);
	public void AddXIP(Complex scalar) => ScalarXIP(scalar, Operations.add);
	public void SubtractXIP(Complex scalar) => ScalarXIP(scalar, Operations.subtract);
	public void MultiplyXIP(Complex scalar) => ScalarXIP(scalar, Operations.multiply);
	public void DivideXIP(Complex scalar) => ScalarXIP(scalar, Operations.divide);
	public void IPOP(Complex scalar, Operations operation) => ScalarXIP(scalar, operation);
	public void IPOPX(Complex scalar, Operations operation) => ScalarXIP(scalar, operation);

	public Mask CompareEqualsX(Complex scalar) => CompareScalarX(scalar, equal: true);
	public Mask CompareNotEqualsX(Complex scalar) => CompareScalarX(scalar, equal: false);

	public static VectorComplex64 operator +(VectorComplex64 vector) => vector.Copy();
	public static VectorComplex64 operator -(VectorComplex64 vector) => vector.Map(static value => -value);
	public static VectorComplex64 operator +(VectorComplex64 left, VectorComplex64 right) => left.Binary(right, static (a, b) => a + b);
	public static VectorComplex64 operator -(VectorComplex64 left, VectorComplex64 right) => left.Binary(right, static (a, b) => a - b);
	public static VectorComplex64 operator *(VectorComplex64 left, VectorComplex64 right) => left.Binary(right, static (a, b) => a * b);
	public static VectorComplex64 operator /(VectorComplex64 left, VectorComplex64 right) => left.Binary(right, static (a, b) => a / b);
	public static VectorComplex64 operator +(VectorComplex64 vector, Complex scalar) => vector.Map(value => value + scalar);
	public static VectorComplex64 operator +(Complex scalar, VectorComplex64 vector) => vector + scalar;
	public static VectorComplex64 operator -(VectorComplex64 vector, Complex scalar) => vector.Map(value => value - scalar);
	public static VectorComplex64 operator -(Complex scalar, VectorComplex64 vector) => vector.Map(value => scalar - value);
	public static VectorComplex64 operator *(VectorComplex64 vector, Complex scalar) => vector.Map(value => value * scalar);
	public static VectorComplex64 operator *(Complex scalar, VectorComplex64 vector) => vector * scalar;
	public static VectorComplex64 operator /(VectorComplex64 vector, Complex scalar) => vector.Map(value => value / scalar);
	public static VectorComplex64 operator /(Complex scalar, VectorComplex64 vector) => vector.Map(value => scalar / value);

	public static VectorComplex64 Zeros(GPU gpu, int length, int columns = 0) =>
		new(gpu, new Complex[length], columns);

	public static VectorComplex64 Ones(GPU gpu, int length, int columns = 0) =>
		Fill(gpu, Complex.One, length, columns);

	public static VectorComplex64 Fill(GPU gpu, Complex value, int length, int columns = 0)
	{
		Complex[] values = new Complex[length];
		Array.Fill(values, value);
		return new VectorComplex64(gpu, values, columns);
	}

	public void Flatten() => Columns = 0;

	public static VectorComplex64 Concat(VectorComplex64 left, VectorComplex64 right)
	{
		ArgumentNullException.ThrowIfNull(left);
		ArgumentNullException.ThrowIfNull(right);
		if (left.Columns != right.Columns)
			throw new ShapeMismatchException(left.Shape(), right.Shape());
		Complex[] values = new Complex[left.Length + right.Length];
		left.RetrieveReadOnlySpan().CopyTo(values);
		right.RetrieveReadOnlySpan().CopyTo(values.AsSpan(left.Length));
		return new VectorComplex64(left.Gpu, values, left.Columns);
	}

	public void ConcatIP(VectorComplex64 other)
	{
		VectorComplex64 result = Concat(this, other);
		using var scope = CpuScope.Begin(this, syncToGpu: true);
		Value = result.ToArray();
		Length = Value.Length;
	}

	public VectorComplex64 Append(Complex value) => Concat(this, new VectorComplex64(Gpu, [value]));
	public VectorComplex64 Prepend(Complex value) => Concat(new VectorComplex64(Gpu, [value]), this);

	public VectorComplex64 Reverse() => new(Gpu, RetrieveReadOnlySpan().ToArray().Reverse().ToArray(), Columns);

	public void ReverseIP()
	{
		using var scope = CpuScope.Begin(this);
		EditableView<Complex> view = scope.View;
		for (int left = 0, right = Length - 1; left < right; left++, right--)
		{
            (view[right], view[left]) = (view[left], view[right]);
        }
    }

	public Complex[] GetSliceAsArray(int start, int length) =>
		RetrieveReadOnlySpan().Slice(start, length).ToArray();

	public VectorComplex64 GetSliceAsVector(int start, int length) =>
		new(Gpu, GetSliceAsArray(start, length));

	public override string ToString() => string.Join(", ", RetrieveReadOnlySpan().ToArray());

	public VectorComplex64 Conjugate() => Map(Complex.Conjugate);
	public VectorComplex64 Magnitude() => Map(value => new Complex(value.Magnitude, 0));
	public VectorComplex64 Phase() => Map(value => new Complex(value.Phase, 0));
	public VectorComplex64 Reciprocal() => Map(Complex.Reciprocal);
	public VectorComplex64 Exp() => Map(Complex.Exp);
	public VectorComplex64 Log() => Map(Complex.Log);
	public VectorComplex64 Sqrt() => Map(Complex.Sqrt);
	public VectorComplex64 Sin() => Map(Complex.Sin);
	public VectorComplex64 Cos() => Map(Complex.Cos);
	public VectorComplex64 Tan() => Map(Complex.Tan);
	public VectorComplex64 Sinh() => Map(Complex.Sinh);
	public VectorComplex64 Cosh() => Map(Complex.Cosh);
	public VectorComplex64 Tanh() => Map(Complex.Tanh);

	public VectorComplex64 UnsupportedGpuOperation(string operation) =>
		throw new NotSupportedException($"GPU operation '{operation}' is not supported for {nameof(VectorComplex64)}; use the CPU method.");

	VectorComplex64 VectorX(VectorComplex64 other, Operations operation)
	{
		ValidateOperation(operation);
		ArgumentNullException.ThrowIfNull(other);
		Shape leftShape = Shape();
		Shape rightShape = other.Shape();
		Shape outputShape = leftShape.BroadcastWith(rightShape);
		if (outputShape.ElementCount == 0)
			return new VectorComplex64(Gpu, [], outputShape.ToStorageColumns());
		VectorComplex64 result = new(Gpu, outputShape.ElementCount, outputShape.ToStorageColumns());
		var op = new SpecializedValue<int>((int)operation);
		using (GpuScope.Begin(result, this, other))
		{
			if (leftShape.MatchesDimensions(rightShape) && Columns == other.Columns)
			{
				Gpu.complexVectorOp(Gpu.DefaultStream, result.Length,
					result.GetBuffer().View, GetBuffer().View, other.GetBuffer().View, op);
			}
			else
			{
				Gpu.complexBroadcast(Gpu.DefaultStream, result.Length,
					result.GetBuffer().View, GetBuffer().View, other.GetBuffer().View,
					outputShape.Cols, BroadcastStrides.For(leftShape), BroadcastStrides.For(rightShape), op);
			}

			Gpu.Synchronize();
		}
		return result;
	}

	VectorComplex64 ScalarX(Complex scalar, Operations operation)
	{
		ValidateOperation(operation);
		if (Length == 0)
			return new VectorComplex64(Gpu, [], Columns);
		VectorComplex64 result = new(Gpu, Length, Columns);
		using (GpuScope.Begin(result, this))
		{
			Gpu.complexScalarOp(Gpu.DefaultStream, Length, result.GetBuffer().View,
				GetBuffer().View, scalar, new SpecializedValue<int>((int)operation));
			Gpu.Synchronize();
		}
		return result;
	}

	void VectorXIP(VectorComplex64 other, Operations operation)
	{
		ValidateOperation(operation);
		ArgumentNullException.ThrowIfNull(other);
		Shape leftShape = Shape();
		Shape rightShape = other.Shape();
		Shape outputShape = leftShape.BroadcastWith(rightShape);
		if (!outputShape.MatchesDimensions(leftShape))
			throw new PerformanceException("Swap operand order OR use allocating overload 'OPX'.");
		if (Length == 0)
			return;
		var op = new SpecializedValue<int>((int)operation);
		using (GpuScope.Begin(this, other))
		{
			if (leftShape.MatchesDimensions(rightShape) && Columns == other.Columns)
				Gpu.complexVectorOpIP(Gpu.DefaultStream, Length, GetBuffer().View, other.GetBuffer().View, op);
			else
				Gpu.complexBroadcastIP(Gpu.DefaultStream, Length, GetBuffer().View, other.GetBuffer().View,
					leftShape.Cols, BroadcastStrides.For(rightShape), op);
			Gpu.Synchronize();
		}
	}

	void ScalarXIP(Complex scalar, Operations operation)
	{
		ValidateOperation(operation);
		if (Length == 0)
			return;
		using (GpuScope.Begin(this))
		{
			Gpu.complexScalarOpIP(Gpu.DefaultStream, Length, GetBuffer().View, scalar,
				new SpecializedValue<int>((int)operation));
			Gpu.Synchronize();
		}
	}

	static void ValidateOperation(Operations operation)
	{
		switch (operation)
		{
			case Operations.add:
			case Operations.subtract:
			case Operations.multiply:
			case Operations.divide:
				return;
			default:
				throw new UnsupportedOperationException(operation.ToString());
		}
	}

	Mask CompareX(VectorComplex64 other, bool equal)
		=> MaskGpu.CompareX(this, other, equal);

	Mask CompareScalarX(Complex scalar, bool equal)
		=> MaskGpu.CompareX(this, scalar, equal);

	VectorComplex64 Map(Func<Complex, Complex> operation)
	{
		Complex[] input = RetrieveReadOnlySpan().ToArray();
		Complex[] output = new Complex[input.Length];
		for (int i = 0; i < input.Length; i++)
			output[i] = operation(input[i]);
		return new VectorComplex64(Gpu, output, Columns);
	}

	VectorComplex64 Binary(VectorComplex64 other, Func<Complex, Complex, Complex> operation)
	{
		ArgumentNullException.ThrowIfNull(other);
		Shape outputShape = Shape().BroadcastWith(other.Shape());
		Complex[] left = RetrieveReadOnlySpan().ToArray();
		Complex[] right = other.RetrieveReadOnlySpan().ToArray();
		Complex[] output = new Complex[outputShape.ElementCount];
		BroadcastStrides leftStrides = BroadcastStrides.For(Shape());
		BroadcastStrides rightStrides = BroadcastStrides.For(other.Shape());
		int columns = outputShape.Cols;
		for (int i = 0; i < output.Length; i++)
		{
			int row = i / columns;
			int column = i - (row * columns);
			output[i] = operation(left[leftStrides.IndexOf(row, column)], right[rightStrides.IndexOf(row, column)]);
		}
		return new VectorComplex64(Gpu, output, outputShape.ToStorageColumns());
	}
}
