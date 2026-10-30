namespace BAVCL;

/// <summary>Explicit ordering used by <see cref="VectorComplex64"/> sorting.</summary>
public enum ComplexSortMode
{
	/// <summary>Order by magnitude, then real and imaginary components.</summary>
	Magnitude,
	/// <summary>NumPy-style lexicographic ordering: real, then imaginary.</summary>
	Complex,
	/// <summary>Order by real component, then imaginary component.</summary>
	Real,
	/// <summary>Order by imaginary component, then real component.</summary>
	Imaginary,
}
