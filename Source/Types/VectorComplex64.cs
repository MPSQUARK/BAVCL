using System;
using System.Linq;
using System.Numerics;
using BAVCL.Core;
using BAVCL.Core.Exceptions;
using BAVCL.Core.Memory;
using BAVCL.Types;
using ILGPU;
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
			if (!ComponentEquals(left[i], right[i]))
				return false;
		return true;
	}

	public Mask CompareEqualsX(VectorComplex64 other) => CompareX(other, equal: true);

	public Mask CompareNotEqualsX(VectorComplex64 other) =>
		CompareX(other, equal: false);

	public Mask CompareEquals(VectorComplex64 other) =>
		Compare(other, ComponentEquals);

	public Mask Compare(VectorComplex64 other, Func<Complex, Complex, bool> predicate)
	{
		ArgumentNullException.ThrowIfNull(other);
		ArgumentNullException.ThrowIfNull(predicate);
		Shape shape = Shape().BroadcastWith(other.Shape());
		Complex[] left = RetrieveReadOnlySpan().ToArray();
		Complex[] right = other.RetrieveReadOnlySpan().ToArray();
		bool[] result = new bool[shape.ElementCount];
		for (int i = 0; i < result.Length; i++)
			result[i] = predicate(left[BroadcastIndex(i, shape, Shape())], right[BroadcastIndex(i, shape, other.Shape())]);
		return new Mask(Gpu, result, shape.ToStorageColumns());
	}

	public VectorComplex64 Filter(Mask mask) => FilterCore(mask, partition: false).TrueLanes;

	public (VectorComplex64 TrueLanes, VectorComplex64 FalseLanes) Partition(Mask mask) =>
		FilterCore(mask, partition: true);

	public static VectorComplex64 operator |(VectorComplex64 vector, Mask mask) => vector.FilterX(mask);

	public static (VectorComplex64 TrueLanes, VectorComplex64 FalseLanes) operator /(VectorComplex64 vector, Mask mask) =>
		vector.PartitionX(mask);

	public VectorComplex64 this[Mask mask] => FilterX(mask);
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

	public VectorComplex64 MaskX(Mask mask, Complex fill)
	{
		ArgumentNullException.ThrowIfNull(mask);
		mask.ValidateShapeMatches(Shape(), Length);
		if (Length == 0)
			return new VectorComplex64(Gpu, [], Columns);
		using BufferEntity<Complex64Device> input = BufferPools.For(Gpu).Complex64.Rent(Length);
		using BufferEntity<Complex64Device> output = BufferPools.For(Gpu).Complex64.Rent(Length);
		using BufferEntity<int> words = BufferPools.For(Gpu).Int.Rent(Math.Max(mask.Length, 1));
		Upload(input, ToDevice(RetrieveReadOnlySpan()));
		Upload(words, mask.ToWordArray());
		Complex64Device[] values = new Complex64Device[Length];
		using (GpuScope.Begin(output.PinTarget, input.PinTarget, words.PinTarget))
		{
			Gpu.complexMaskFilter(Gpu.DefaultStream, Length, output.View, input.View, words.View,
				new Complex64Device(fill.Real, fill.Imaginary));
			Gpu.Synchronize();
			output.View.CopyToCPU(values);
		}
		return FromDevice(Gpu, values, Columns);
	}

	public Mask CompareEqualsX(Complex scalar) => CompareScalarX(scalar, equal: true);
	public Mask CompareNotEqualsX(Complex scalar) => CompareScalarX(scalar, equal: false);

	public VectorComplex64 FilterX(Mask mask) => FilterGpu(mask, partition: false).TrueLanes;

	public (VectorComplex64 TrueLanes, VectorComplex64 FalseLanes) PartitionX(Mask mask) =>
		FilterGpu(mask, partition: true);

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
			Complex value = view[left];
			view[left] = view[right];
			view[right] = value;
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
		ArgumentNullException.ThrowIfNull(other);
		Shape outputShape = Shape().BroadcastWith(other.Shape());
		if (outputShape.ElementCount == 0)
			return new VectorComplex64(Gpu, [], outputShape.ToStorageColumns());
		KernelModuleLoader.Load<Complex>(Gpu, KernelDomain.Arithmetic);
		using BufferEntity<Complex64Device> left = BufferPools.For(Gpu).Complex64.Rent(outputShape.ElementCount);
		using BufferEntity<Complex64Device> right = BufferPools.For(Gpu).Complex64.Rent(outputShape.ElementCount);
		using BufferEntity<Complex64Device> output = BufferPools.For(Gpu).Complex64.Rent(outputShape.ElementCount);
		Upload(left, ExpandToDevice(RetrieveReadOnlySpan(), Shape(), outputShape));
		Upload(right, ExpandToDevice(other.RetrieveReadOnlySpan(), other.Shape(), outputShape));
		Complex64Device[] values = new Complex64Device[output.Length];
		using (GpuScope.Begin(output.PinTarget, left.PinTarget, right.PinTarget))
		{
			Gpu.complexVectorOp(Gpu.DefaultStream, output.Length, output.View, left.View, right.View, new SpecializedValue<int>((int)operation));
			Gpu.Synchronize();
			output.View.CopyToCPU(values);
		}
		return FromDevice(Gpu, values, outputShape.ToStorageColumns());
	}

	VectorComplex64 ScalarX(Complex scalar, Operations operation)
	{
		if (Length == 0)
			return new VectorComplex64(Gpu, [], Columns);
		KernelModuleLoader.Load<Complex>(Gpu, KernelDomain.Arithmetic);
		using BufferEntity<Complex64Device> input = BufferPools.For(Gpu).Complex64.Rent(Length);
		using BufferEntity<Complex64Device> output = BufferPools.For(Gpu).Complex64.Rent(Length);
		Upload(input, ToDevice(RetrieveReadOnlySpan()));
		Complex64Device[] values = new Complex64Device[Length];
		using (GpuScope.Begin(output.PinTarget, input.PinTarget))
		{
			Gpu.complexScalarOp(Gpu.DefaultStream, Length, output.View, input.View,
				new Complex64Device(scalar.Real, scalar.Imaginary),
				new SpecializedValue<int>((int)operation));
			Gpu.Synchronize();
			output.View.CopyToCPU(values);
		}
		return FromDevice(Gpu, values, Columns);
	}

	void VectorXIP(VectorComplex64 other, Operations operation)
	{
		ArgumentNullException.ThrowIfNull(other);
		Shape outputShape = Shape().BroadcastWith(other.Shape());
		if (!outputShape.MatchesDimensions(Shape()))
			throw new PerformanceException("Swap operand order OR use allocating overload 'OPX'.");
		if (Length == 0)
			return;
		KernelModuleLoader.Load<Complex>(Gpu, KernelDomain.Arithmetic);
		using BufferEntity<Complex64Device> left = BufferPools.For(Gpu).Complex64.Rent(Length);
		using BufferEntity<Complex64Device> right = BufferPools.For(Gpu).Complex64.Rent(Length);
		Upload(left, ToDevice(RetrieveReadOnlySpan()));
		Upload(right, ExpandToDevice(other.RetrieveReadOnlySpan(), other.Shape(), outputShape));
		Complex64Device[] values = new Complex64Device[Length];
		using (GpuScope.Begin(left.PinTarget, right.PinTarget))
		{
			Gpu.complexVectorOpIP(Gpu.DefaultStream, Length, left.View, right.View, new SpecializedValue<int>((int)operation));
			Gpu.Synchronize();
			left.View.CopyToCPU(values);
		}
		ReplaceCpuValues(values);
	}

	void ScalarXIP(Complex scalar, Operations operation)
	{
		if (Length == 0)
			return;
		KernelModuleLoader.Load<Complex>(Gpu, KernelDomain.Arithmetic);
		using BufferEntity<Complex64Device> input = BufferPools.For(Gpu).Complex64.Rent(Length);
		Upload(input, ToDevice(RetrieveReadOnlySpan()));
		Complex64Device[] values = new Complex64Device[Length];
		using (GpuScope.Begin(input.PinTarget))
		{
			Gpu.complexScalarOpIP(Gpu.DefaultStream, Length, input.View,
				new Complex64Device(scalar.Real, scalar.Imaginary),
				new SpecializedValue<int>((int)operation));
			Gpu.Synchronize();
			input.View.CopyToCPU(values);
		}
		ReplaceCpuValues(values);
	}

	Mask CompareX(VectorComplex64 other, bool equal)
	{
		ArgumentNullException.ThrowIfNull(other);
		Shape outputShape = Shape().BroadcastWith(other.Shape());
		if (outputShape.ElementCount == 0)
			throw new ArgumentException("A comparison cannot produce an empty mask.", nameof(other));
		KernelModuleLoader.Load<Complex>(Gpu, KernelDomain.Mask);
		int[] flags = new int[outputShape.ElementCount];
		using BufferEntity<Complex64Device> leftBuffer = BufferPools.For(Gpu).Complex64.Rent(flags.Length);
		using BufferEntity<Complex64Device> rightBuffer = BufferPools.For(Gpu).Complex64.Rent(flags.Length);
		using BufferEntity<int> flagBuffer = BufferPools.For(Gpu).Int.Rent(flags.Length);
		Upload(leftBuffer, ExpandToDevice(RetrieveReadOnlySpan(), Shape(), outputShape));
		Upload(rightBuffer, ExpandToDevice(other.RetrieveReadOnlySpan(), other.Shape(), outputShape));
		using (GpuScope.Begin(flagBuffer.PinTarget, leftBuffer.PinTarget, rightBuffer.PinTarget))
		{
			Gpu.complexCompareEquals(Gpu.DefaultStream, flags.Length, flagBuffer.View,
				leftBuffer.View, rightBuffer.View, new SpecializedValue<int>(equal ? 0 : 1));
			Gpu.Synchronize();
			flagBuffer.View.CopyToCPU(flags);
		}
		return new Mask(Gpu, FlagsToBool(flags), outputShape.ToStorageColumns());
	}

	Mask CompareScalarX(Complex scalar, bool equal)
	{
		if (Length == 0)
			throw new ArgumentException("A comparison cannot produce an empty mask.", nameof(scalar));
		KernelModuleLoader.Load<Complex>(Gpu, KernelDomain.Mask);
		int[] flags = new int[Length];
		using BufferEntity<Complex64Device> inputBuffer = BufferPools.For(Gpu).Complex64.Rent(Length);
		using BufferEntity<int> flagBuffer = BufferPools.For(Gpu).Int.Rent(Length);
		Upload(inputBuffer, ToDevice(RetrieveReadOnlySpan()));
		using (GpuScope.Begin(flagBuffer.PinTarget, inputBuffer.PinTarget))
		{
			Gpu.complexCompareScalar(Gpu.DefaultStream, Length, flagBuffer.View, inputBuffer.View,
				new Complex64Device(scalar.Real, scalar.Imaginary),
				new SpecializedValue<int>(equal ? 0 : 1));
			Gpu.Synchronize();
			flagBuffer.View.CopyToCPU(flags);
		}
		return new Mask(Gpu, FlagsToBool(flags), Columns);
	}

	(VectorComplex64 TrueLanes, VectorComplex64 FalseLanes) FilterGpu(Mask mask, bool partition)
	{
		ArgumentNullException.ThrowIfNull(mask);
		mask.ValidateShapeMatches(Shape(), Length);
		if (Length == 0)
			return (new VectorComplex64(Gpu, []), new VectorComplex64(Gpu, []));
		KernelModuleLoader.Load<Complex>(Gpu, KernelDomain.Mask);
		Complex64Device[] input = ToDevice(RetrieveReadOnlySpan());
		bool[] bits = mask.ToBoolArray();
		int trueCount = 0;
		for (int i = 0; i < bits.Length; i++)
			if (bits[i]) trueCount++;
		int falseCount = Length - trueCount;
		using BufferEntity<Complex64Device> inputBuffer = BufferPools.For(Gpu).Complex64.Rent(Length);
		using BufferEntity<Complex64Device> trueBuffer = BufferPools.For(Gpu).Complex64.Rent(Math.Max(trueCount, 1));
		using BufferEntity<Complex64Device> falseBuffer = BufferPools.For(Gpu).Complex64.Rent(Math.Max(falseCount, 1));
		using BufferEntity<int> flags = BufferPools.For(Gpu).Int.Rent(Length);
		using BufferEntity<int> positions = BufferPools.For(Gpu).Int.Rent(Length);
		int[] flagValues = new int[Length];
		int[] positionValues = new int[Length];
		int position = 0;
		for (int i = 0; i < Length; i++)
		{
			flagValues[i] = bits[i] ? 1 : 0;
			positionValues[i] = position;
			if (bits[i]) position++;
		}
		Upload(inputBuffer, input);
		Upload(flags, flagValues);
		Upload(positions, positionValues);
		Complex64Device[] trueValues = new Complex64Device[Math.Max(trueCount, 1)];
		Complex64Device[] falseValues = new Complex64Device[Math.Max(falseCount, 1)];
		using (GpuScope.Begin(
			partition ? [trueBuffer.PinTarget, falseBuffer.PinTarget] : [trueBuffer.PinTarget],
			[inputBuffer.PinTarget, flags.PinTarget, positions.PinTarget]))
		{
			if (partition)
				Gpu.complexPartitionGather(Gpu.DefaultStream, Length, trueBuffer.View, falseBuffer.View,
					inputBuffer.View, flags.View, positions.View);
			else
				Gpu.complexGather(Gpu.DefaultStream, Length, trueBuffer.View, inputBuffer.View, flags.View, positions.View);
			Gpu.Synchronize();
			if (trueCount > 0) trueBuffer.View.SubView(0, trueCount).CopyToCPU(trueValues);
			if (partition && falseCount > 0) falseBuffer.View.SubView(0, falseCount).CopyToCPU(falseValues);
		}
		return (
			FromDevice(Gpu, trueValues[..trueCount], 0),
			partition ? FromDevice(Gpu, falseValues[..falseCount], 0) : new VectorComplex64(Gpu, []));
	}

	void ReplaceCpuValues(Complex64Device[] values)
	{
		Complex[] converted = FromDeviceArray(values);
		using var scope = this.CpuScope();
		EditableView<Complex> view = scope.View;
		for (int i = 0; i < converted.Length; i++)
			view[i] = converted[i];
	}

	static Complex64Device[] ToDevice(ReadOnlySpan<Complex> values)
	{
		Complex64Device[] result = new Complex64Device[values.Length];
		for (int i = 0; i < values.Length; i++)
			result[i] = new Complex64Device(values[i].Real, values[i].Imaginary);
		return result;
	}

	static void Upload(BufferEntity<Complex64Device> destination, Complex64Device[] values) =>
		destination.CopyFromCPU(values);

	static void Upload(BufferEntity<int> destination, int[] values) =>
		destination.CopyFromCPU(values);

	static Complex64Device[] ExpandToDevice(ReadOnlySpan<Complex> values, Shape input, Shape output)
	{
		Complex64Device[] result = new Complex64Device[output.ElementCount];
		for (int flat = 0; flat < result.Length; flat++)
		{
			int row = flat / output.Cols;
			int column = flat % output.Cols;
			int inputRow = input.Rows == 1 ? 0 : row;
			int inputColumn = input.Cols == 1 ? 0 : column;
			result[flat] = new Complex64Device(
				values[inputRow * input.Cols + inputColumn].Real,
				values[inputRow * input.Cols + inputColumn].Imaginary);
		}
		return result;
	}

	static VectorComplex64 FromDevice(GPU gpu, Complex64Device[] values, int columns) =>
		new(gpu, FromDeviceArray(values), columns);

	static Complex[] FromDeviceArray(Complex64Device[] values)
	{
		Complex[] result = new Complex[values.Length];
		for (int i = 0; i < values.Length; i++)
			result[i] = new Complex(values[i].Real, values[i].Imaginary);
		return result;
	}

	static Complex64Device[] Compact(Complex64Device[] values, bool[] bits, bool expected)
	{
		var result = new System.Collections.Generic.List<Complex64Device>();
		for (int i = 0; i < values.Length; i++)
			if (bits[i] == expected)
				result.Add(values[i]);
		return result.ToArray();
	}

	static bool[] FlagsToBool(int[] flags)
	{
		bool[] result = new bool[flags.Length];
		for (int i = 0; i < flags.Length; i++)
			result[i] = flags[i] != 0;
		return result;
	}

	static bool ComponentEquals(Complex left, Complex right) =>
		left.Real == right.Real && left.Imaginary == right.Imaginary;

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
		for (int i = 0; i < output.Length; i++)
			output[i] = operation(left[BroadcastIndex(i, outputShape, Shape())], right[BroadcastIndex(i, outputShape, other.Shape())]);
		return new VectorComplex64(Gpu, output, outputShape.ToStorageColumns());
	}

	(VectorComplex64 TrueLanes, VectorComplex64 FalseLanes) FilterCore(Mask mask, bool partition)
	{
		ArgumentNullException.ThrowIfNull(mask);
		mask.ValidateShapeMatches(Shape(), Length);
		ReadOnlySpan<Complex> values = RetrieveReadOnlySpan();
		bool[] bits = mask.ToBoolArray();
		var trueValues = new System.Collections.Generic.List<Complex>();
		var falseValues = new System.Collections.Generic.List<Complex>();
		for (int i = 0; i < values.Length; i++)
		{
			if (bits[i])
				trueValues.Add(values[i]);
			else if (partition)
				falseValues.Add(values[i]);
		}
		if (!partition)
			return (new VectorComplex64(Gpu, trueValues.ToArray()), new VectorComplex64(Gpu, []));
		return (new VectorComplex64(Gpu, trueValues.ToArray()), new VectorComplex64(Gpu, falseValues.ToArray()));
	}

	static int BroadcastIndex(int flatIndex, Shape output, Shape input)
	{
		int row = flatIndex / output.Cols;
		int column = flatIndex % output.Cols;
		int inputRow = input.Rows == 1 ? 0 : row;
		int inputColumn = input.Cols == 1 ? 0 : column;
		return inputRow * input.Cols + inputColumn;
	}
}
