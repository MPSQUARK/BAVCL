using System;
using System.Buffers;
using System.Collections.Generic;

namespace BAVCL.Core.Fourier;

internal enum FftAlgorithm
{
	MixedRadix,
	Bluestein,
}

internal readonly record struct FftComplex(float Real, float Imaginary)
{
	public static FftComplex operator +(FftComplex left, FftComplex right) =>
		new(left.Real + right.Real, left.Imaginary + right.Imaginary);

	public static FftComplex operator *(FftComplex left, FftComplex right) =>
		new(
			left.Real * right.Real - left.Imaginary * right.Imaginary,
			left.Real * right.Imaginary + left.Imaginary * right.Real);

	public static FftComplex operator *(FftComplex value, float scalar) =>
		new(value.Real * scalar, value.Imaginary * scalar);
}

/// <summary>
/// Reusable internal single-precision complex transform plan.
/// </summary>
internal sealed class FftPlan
{
	private const float TwoPi = 2f * MathF.PI;
	private readonly int[] factors;
	private readonly FftPlan? convolutionPlan;
	private readonly FftComplex[]? bluesteinKernel;

	private FftPlan(int length, FftAlgorithm algorithm, int[] factors)
	{
		Length = length;
		Algorithm = algorithm;
		this.factors = factors;

		if (algorithm != FftAlgorithm.Bluestein)
			return;

		int convolutionLength = NextPowerOfTwo(checked(2 * length - 1));
		convolutionPlan = CreateMixedRadix(convolutionLength);
		bluesteinKernel = new FftComplex[convolutionLength];
		for (int index = 0; index < length; index++)
		{
			FftComplex value = Chirp(index, length, inverse: true);
			bluesteinKernel[index] = value;
			if (index != 0)
				bluesteinKernel[convolutionLength - index] = value;
		}

		convolutionPlan.Transform(bluesteinKernel, bluesteinKernel, inverse: false);
	}

	public int Length { get; }

	public FftAlgorithm Algorithm { get; }

	public static FftPlan Create(int length)
	{
		if (length < 1)
			throw new ArgumentOutOfRangeException(nameof(length), "FFT length must be positive.");
		if (length > int.MaxValue / 2)
			throw new ArgumentOutOfRangeException(nameof(length), "FFT length is too large.");

		List<int> factors = Factorize(length);
		return length != 1 && factors.Count == 0
			? new FftPlan(length, FftAlgorithm.Bluestein, [])
			: new FftPlan(length, FftAlgorithm.MixedRadix, [.. factors]);
	}

	public void Transform(ReadOnlySpan<FftComplex> input, Span<FftComplex> output, bool inverse = false)
	{
		if (input.Length != Length)
			throw new ArgumentException("Input length does not match the FFT plan.", nameof(input));
		if (output.Length != Length)
			throw new ArgumentException("Output length does not match the FFT plan.", nameof(output));

		if (Algorithm == FftAlgorithm.Bluestein)
		{
			TransformBluestein(input, output, inverse);
			return;
		}

		TransformMixedRadix(input, output, inverse);
		if (inverse)
			Scale(output, 1f / Length);
	}

	private void TransformBluestein(
		ReadOnlySpan<FftComplex> input,
		Span<FftComplex> output,
		bool inverse)
	{
		int convolutionLength = convolutionPlan!.Length;
		FftComplex[] values = ArrayPool<FftComplex>.Shared.Rent(convolutionLength);
		try
		{
			Array.Clear(values, 0, convolutionLength);
			for (int index = 0; index < Length; index++)
				values[index] = input[index] * Chirp(index, Length, inverse);

			convolutionPlan.Transform(values.AsSpan(0, convolutionLength), values.AsSpan(0, convolutionLength), inverse: false);
			for (int index = 0; index < convolutionLength; index++)
			{
				FftComplex kernel = inverse ? Conjugate(bluesteinKernel![index]) : bluesteinKernel![index];
				values[index] = values[index] * kernel;
			}
			convolutionPlan.Transform(values.AsSpan(0, convolutionLength), values.AsSpan(0, convolutionLength), inverse: true);

			for (int index = 0; index < Length; index++)
				output[index] = values[index] * Chirp(index, Length, inverse);
			if (inverse)
				Scale(output, 1f / Length);
		}
		finally
		{
			ArrayPool<FftComplex>.Shared.Return(values, clearArray: false);
		}
	}

	private void TransformMixedRadix(
		ReadOnlySpan<FftComplex> input,
		Span<FftComplex> output,
		bool inverse)
	{
		if (Length == 1)
		{
			output[0] = input[0];
			return;
		}

		FftComplex[] values = ArrayPool<FftComplex>.Shared.Rent(Length);
		try
		{
			input.CopyTo(values);
			TransformMixedRadix(values, output, 0, 1, Length, inverse);
		}
		finally
		{
			ArrayPool<FftComplex>.Shared.Return(values, clearArray: false);
		}
	}

	private void TransformMixedRadix(
		ReadOnlySpan<FftComplex> input,
		Span<FftComplex> output,
		int offset,
		int stride,
		int length,
		bool inverse)
	{
		if (length == 1)
		{
			output[0] = input[offset];
			return;
		}

		int radix = factors[FactorIndex(length)];
		int subLength = length / radix;
		FftComplex[] subTransforms = ArrayPool<FftComplex>.Shared.Rent(length);
		try
		{
			for (int branch = 0; branch < radix; branch++)
				TransformMixedRadix(
					input,
					subTransforms.AsSpan(branch * subLength, subLength),
					offset + branch * stride,
					stride * radix,
					subLength,
					inverse);

			for (int index = 0; index < length; index++)
			{
				FftComplex sum = default;
				for (int branch = 0; branch < radix; branch++)
				{
					FftComplex value = subTransforms[branch * subLength + index % subLength];
					sum += value * Twiddle(branch * index, length, inverse);
				}

				output[index] = sum;
			}
		}
		finally
		{
			ArrayPool<FftComplex>.Shared.Return(subTransforms, clearArray: false);
		}
	}

	private int FactorIndex(int length)
	{
		int currentLength = Length;
		int index = 0;
		while (currentLength != length)
		{
			currentLength /= factors[index];
			index++;
		}

		return index;
	}

	private static List<int> Factorize(int length)
	{
		List<int> factors = [];
		foreach (int radix in new[] { 2, 3, 5, 7 })
			while (length % radix == 0)
			{
				factors.Add(radix);
				length /= radix;
			}

		if (length != 1)
			factors.Clear();
		return factors;
	}

	private static FftComplex Chirp(int index, int length, bool inverse)
	{
		float angle = MathF.PI * index * (float)index / length;
		return Unit(angle, inverse);
	}

	private static FftComplex Twiddle(int exponent, int length, bool inverse) =>
		Unit(TwoPi * exponent / length, inverse);

	private static FftComplex Unit(float angle, bool inverse) =>
		new(MathF.Cos(angle), (inverse ? 1f : -1f) * MathF.Sin(angle));

	private static FftComplex Conjugate(FftComplex value) =>
		new(value.Real, -value.Imaginary);

	private static void Scale(Span<FftComplex> values, float factor)
	{
		for (int index = 0; index < values.Length; index++)
			values[index] *= factor;
	}

	private static int NextPowerOfTwo(int value)
	{
		int result = 1;
		while (result < value)
		{
			if (result > 1 << 30)
				throw new ArgumentOutOfRangeException(nameof(value), "FFT convolution length is too large.");
			result <<= 1;
		}
		return result;
	}

	private static FftPlan CreateMixedRadix(int length) =>
		new(length, FftAlgorithm.MixedRadix, [.. Factorize(length)]);
}

internal sealed class FftPlan2D
{
	private readonly FftPlan rowPlan;
	private readonly FftPlan columnPlan;
	private readonly int totalLength;

	public FftPlan2D(int rows, int columns)
	{
		if (rows < 1)
			throw new ArgumentOutOfRangeException(nameof(rows));
		if (columns < 1)
			throw new ArgumentOutOfRangeException(nameof(columns));
		if (rows > int.MaxValue / columns)
			throw new ArgumentOutOfRangeException(nameof(columns), "2D FFT size is too large.");

		Rows = rows;
		Columns = columns;
		totalLength = checked(rows * columns);
		rowPlan = FftPlan.Create(columns);
		columnPlan = FftPlan.Create(rows);
	}

	public int Rows { get; }

	public int Columns { get; }

	public void Transform(ReadOnlySpan<FftComplex> input, Span<FftComplex> output, bool inverse = false)
	{
		if (input.Length != totalLength)
			throw new ArgumentException("Input length does not match the 2D FFT plan.", nameof(input));
		if (output.Length != input.Length)
			throw new ArgumentException("Output length does not match the 2D FFT plan.", nameof(output));

		int lineLength = Math.Max(Rows, Columns);
		FftComplex[] work = ArrayPool<FftComplex>.Shared.Rent(totalLength);
		FftComplex[] lineInput = ArrayPool<FftComplex>.Shared.Rent(lineLength);
		FftComplex[] lineOutput = ArrayPool<FftComplex>.Shared.Rent(lineLength);
		try
		{
			input.CopyTo(work);
			for (int row = 0; row < Rows; row++)
			{
				work.AsSpan(row * Columns, Columns).CopyTo(lineInput);
				rowPlan.Transform(lineInput.AsSpan(0, Columns), lineOutput.AsSpan(0, Columns), inverse);
				lineOutput.AsSpan(0, Columns).CopyTo(work.AsSpan(row * Columns, Columns));
			}

			for (int column = 0; column < Columns; column++)
			{
				for (int row = 0; row < Rows; row++)
					lineInput[row] = work[row * Columns + column];
				columnPlan.Transform(lineInput.AsSpan(0, Rows), lineOutput.AsSpan(0, Rows), inverse);
				for (int row = 0; row < Rows; row++)
					output[row * Columns + column] = lineOutput[row];
			}
		}
		finally
		{
			ArrayPool<FftComplex>.Shared.Return(work, clearArray: false);
			ArrayPool<FftComplex>.Shared.Return(lineInput, clearArray: false);
			ArrayPool<FftComplex>.Shared.Return(lineOutput, clearArray: false);
		}
	}
}
