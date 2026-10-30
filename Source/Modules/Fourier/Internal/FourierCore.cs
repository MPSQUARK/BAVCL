using System;
using BAVCL.Core.Fourier;
using BAVCL.Core.Memory;

namespace BAVCL.Modules.Fourier;

internal static class FourierCore
{
	public static FftResult Forward(Vector input) => TransformCpu(input, inverse: false);
	public static FftResult Forward(VectorInt input) => TransformCpu(input, inverse: false);

	public static Vector Inverse(FftResult spectrum)
	{
		ArgumentNullException.ThrowIfNull(spectrum);
		return TransformCpuInverse(spectrum);
	}

	public static FftResult ForwardX(Vector input) => TransformGpu(input);
	public static FftResult ForwardX(VectorInt input) => TransformGpu(input);

	public static Vector InverseX(FftResult spectrum)
	{
		ArgumentNullException.ThrowIfNull(spectrum);
		// The inverse consumes two real-valued vectors. The GPU path is intentionally
		// not exposed until a complex-vector storage contract exists.
		return Inverse(spectrum);
	}

	private static FftResult TransformCpu(Vector input, bool inverse)
	{
		Validate(input);
		float[] values = input.ToArray();
		FftComplex[] source = new FftComplex[values.Length];
		for (int i = 0; i < values.Length; i++)
			source[i] = new(values[i], 0f);
		FftComplex[] transformed = new FftComplex[source.Length];
		if (input.Is2D())
			new FftPlan2D(input.RowCount(), input.Columns).Transform(source, transformed, inverse);
		else
			FftPlan.Create(source.Length).Transform(source, transformed, inverse);
		return CreateResult(input.Gpu, input.Columns, transformed);
	}

	private static FftResult TransformCpu(VectorInt input, bool inverse)
	{
		Validate(input);
		int[] values = input.ToArray();
		FftComplex[] source = new FftComplex[values.Length];
		for (int i = 0; i < values.Length; i++)
			source[i] = new(values[i], 0f);
		FftComplex[] transformed = new FftComplex[source.Length];
		if (input.Is2D())
			new FftPlan2D(input.RowCount(), input.Columns).Transform(source, transformed, inverse);
		else
			FftPlan.Create(source.Length).Transform(source, transformed, inverse);
		return CreateResult(input.Gpu, input.Columns, transformed);
	}

	private static FftResult TransformCpu(Vector input, FftComplex[] source, bool inverse)
	{
		FftComplex[] transformed = new FftComplex[source.Length];
		if (input.Is2D())
			new FftPlan2D(input.RowCount(), input.Columns).Transform(source, transformed, inverse);
		else
			FftPlan.Create(source.Length).Transform(source, transformed, inverse);
		return CreateResult(input, transformed);
	}

	private static Vector TransformCpuInverse(FftResult spectrum)
	{
		Validate(spectrum.Real);
		if (spectrum.Real.Gpu != spectrum.Imaginary.Gpu)
			throw new ArgumentException("The real and imaginary vectors must use the same GPU.");
		float[] real = spectrum.Real.ToArray();
		float[] imaginary = spectrum.Imaginary.ToArray();
		FftComplex[] source = new FftComplex[real.Length];
		for (int i = 0; i < source.Length; i++)
			source[i] = new(real[i], imaginary[i]);
		FftComplex[] transformed = new FftComplex[source.Length];
		if (spectrum.Columns > 1)
			new FftPlan2D(spectrum.Real.RowCount(), spectrum.Columns).Transform(source, transformed, inverse: true);
		else
			FftPlan.Create(source.Length).Transform(source, transformed, inverse: true);
		float[] output = new float[source.Length];
		for (int i = 0; i < output.Length; i++)
			output[i] = transformed[i].Real;
		return new Vector(spectrum.Real.Gpu, output, spectrum.Columns);
	}

	private static FftResult TransformGpu(Vector input)
	{
		Validate(input);
		if (input.Is2D())
			return TransformCpu(input, inverse: false);
		Vector real = new(input.Gpu, input.Length, input.Columns);
		Vector imaginary = new(input.Gpu, input.Length, input.Columns);
		using (GpuScope.Begin([real, imaginary], [input]))
		{
			input.Gpu.fftRealFloat(
				input.Gpu.DefaultStream, input.Length, real.GetBuffer().View,
				imaginary.GetBuffer().View, input.GetBuffer().View, input.RowCount(),
				input.Is2D() ? input.Columns : 0, 0);
			input.Gpu.Synchronize();
		}
		return new FftResult(real, imaginary);
	}

	private static FftResult TransformGpu(VectorInt input)
	{
		Validate(input);
		if (input.Is2D())
		{
			int[] values = input.ToArray();
			FftComplex[] source = new FftComplex[values.Length];
			for (int i = 0; i < values.Length; i++)
				source[i] = new(values[i], 0f);
			FftComplex[] transformed = new FftComplex[source.Length];
			new FftPlan2D(input.RowCount(), input.Columns).Transform(source, transformed);
			return CreateResult(input.Gpu, input.Columns, transformed);
		}
		Vector real = new(input.Gpu, input.Length, input.Columns);
		Vector imaginary = new(input.Gpu, input.Length, input.Columns);
		using (GpuScope.Begin([real, imaginary], [input]))
		{
			input.Gpu.fftRealInt(
				input.Gpu.DefaultStream, input.Length, real.GetBuffer().View,
				imaginary.GetBuffer().View, input.GetBuffer().View, input.RowCount(),
				input.Is2D() ? input.Columns : 0, 0);
			input.Gpu.Synchronize();
		}
		return new FftResult(real, imaginary);
	}

	private static FftResult CreateResult(Vector input, FftComplex[] values)
	{
		return CreateResult(input.Gpu, input.Columns, values);
	}

	private static FftResult CreateResult(GPU gpu, int columns, FftComplex[] values)
	{
		float[] real = new float[values.Length];
		float[] imaginary = new float[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			real[i] = values[i].Real;
			imaginary[i] = values[i].Imaginary;
		}
		return new FftResult(
			new Vector(gpu, real, columns),
			new Vector(gpu, imaginary, columns));
	}

	private static void Validate(VectorBase<float> input)
	{
		if (input.Length < 1)
			throw new ArgumentOutOfRangeException(nameof(input), "FFT input must not be empty.");
		if (input.Is2D() && !input.IsRectangular())
			throw new ArgumentException("A 2D FFT requires rectangular row-major storage.", nameof(input));
	}

	private static void Validate(VectorBase<int> input)
	{
		if (input.Length < 1)
			throw new ArgumentOutOfRangeException(nameof(input), "FFT input must not be empty.");
		if (input.Is2D() && !input.IsRectangular())
			throw new ArgumentException("A 2D FFT requires rectangular row-major storage.", nameof(input));
	}
}
