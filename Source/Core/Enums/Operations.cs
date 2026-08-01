namespace BAVCL.Core;

public enum Operations
{
	multiply = 0,
	add = 1,
	subtract = 2,
	divide = 3,
	/// <summary>
	/// Vector ^ Scalar
	/// </summary>
	pow = 4,
	/// <summary>
	/// Scalar / Vector
	/// </summary>
	flipDivide = 5,
	/// <summary>
	/// Scalar - Vector
	/// </summary>
	flipSubtract = 6,
	/// <summary>
	/// Scalar ^ Vector
	/// </summary>
	flipPow = 7,
	/// <summary>
	/// (a - b)^2
	/// </summary>
	differenceSquared = 8, // square the difference of two values
	/// <summary>
	/// Sqrt((a - b)^2)
	/// </summary>
	distance = 9, // square root of the difference squared
	/// <summary>
	/// Sqrt(a^2 + b^2)
	/// </summary>
	magnitude = 10, // square root of the sum of the squares
	/// <summary>
	/// VectorInt only: a % b
	/// </summary>
	modulo = 11,
	/// <summary>
	/// VectorInt only: a &lt;&lt; b
	/// </summary>
	leftShift = 12,
	/// <summary>
	/// VectorInt only: a &gt;&gt; b (arithmetic, sign-extending)
	/// </summary>
	rightShift = 13,
	/// <summary>
	/// VectorInt only: a ^ b (bitwise XOR)
	/// </summary>
	bitwiseXor = 14,
	/// <summary>
	/// VectorInt only: b % a
	/// </summary>
	flipModulo = 15,
	/// <summary>
	/// VectorInt only: a &amp; b (bitwise AND)
	/// </summary>
	bitwiseAnd = 16,
}
