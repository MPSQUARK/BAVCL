namespace BAVCL;

public enum Operations
{
	multiply = 0,
	add = 1,
	/// <summary>
	/// Vector - Scalar
	/// </summary>	
	subtract = 2,
	/// <summary>
	/// Vector - Scalar
	/// </summary>	
	divide = 3,
	/// <summary>
	/// Vector - Scalar
	/// </summary>	
	pow = 4,
	/// <summary>
	/// Scalar - Vector
	/// </summary>
	flipDivide = 5,
	/// <summary>
	/// Scalar - Vector
	/// </summary>
	flipSubtract = 6,
	/// <summary>
	/// Scalar - Vector
	/// </summary>
	flipPow = 7,
	differenceSquared = 8, // square the difference of two values
	distance = 9, // square root of the difference squared
	magnitude = 10, // square root of the sum of the squares
}