namespace BAVCL;

/// <summary>ILGPU-safe storage for one <see cref="System.Numerics.Complex"/> value.</summary>
public struct Complex64Device
{
	public double Real;
	public double Imaginary;

	public Complex64Device(double real, double imaginary)
	{
		Real = real;
		Imaginary = imaginary;
	}
}
