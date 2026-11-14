namespace BAVCL;

public enum ComplexSortMode
{
	/// <summary>Order by magnitude.</summary>
	Magnitude,
	/// <summary>NumPy-style lexicographic ordering: real, then imaginary.</summary>
	Complex,
	/// <summary>Order by real component.</summary>
	Real,
	/// <summary>Order by imaginary component.</summary>
	Imaginary,
}
