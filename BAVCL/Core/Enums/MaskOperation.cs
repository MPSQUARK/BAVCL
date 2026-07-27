namespace BAVCL.Core;

/// <summary>
/// Binary lane operations dispatched to the mask kernels as a specialized constant.
/// Unary forms (complement, set-all, clear-all) are expressed as an operation against a
/// constant word, so they need no enum member of their own.
/// </summary>
public enum MaskOperation
{
	And = 0,
	Or = 1,
	Xor = 2,
	Nand = 3,
	Nor = 4,
	Xnor = 5,
}
