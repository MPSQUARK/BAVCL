using System;
using BAVCL.Core.Fourier;

namespace BAVCL.Modules.Fourier;

/// <summary>Real-input single-precision Fourier transform result.</summary>
public sealed class FftResult
{
	public FftResult(Vector real, Vector imaginary)
	{
		ArgumentNullException.ThrowIfNull(real);
		ArgumentNullException.ThrowIfNull(imaginary);
		if (real.Length != imaginary.Length || real.Columns != imaginary.Columns)
			throw new ArgumentException("Real and imaginary parts must have the same length and shape.");
		if (real.Gpu != imaginary.Gpu)
			throw new ArgumentException("Real and imaginary parts must use the same GPU.");
		if (real.Length < 1)
			throw new ArgumentOutOfRangeException(nameof(real), "FFT results must not be empty.");
		if (!real.IsRectangular())
			throw new ArgumentException("FFT results must use rectangular row-major storage.", nameof(real));
		Real = real;
		Imaginary = imaginary;
	}

	public Vector Real { get; }
	public Vector Imaginary { get; }
	public int Length => Real.Length;
	public int Columns => Real.Columns;
}

public static class FourierModule
{
	extension(Vector vector)
	{
		public FftResult Fft() => FourierCore.Forward(vector);
		public FftResult FftX() => FourierCore.ForwardX(vector);
	}

	extension(VectorInt vector)
	{
		public FftResult Fft() => FourierCore.Forward(vector);
		public FftResult FftX() => FourierCore.ForwardX(vector);
	}

	extension(FftResult result)
	{
		public Vector Ifft() => FourierCore.Inverse(result);
		public Vector IfftX() => FourierCore.InverseX(result);
	}
}
