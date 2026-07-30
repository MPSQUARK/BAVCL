using BAVCL.Types;

namespace BAVCL.Modules.Masking;

public static class MaskModule
{
	extension(Mask)
	{
		public static Mask OP(Mask left, Mask right, MaskOperation operation) =>
			MaskBitwiseOps.BinaryOp(left, right, operation);

		public static Mask IPOP(Mask left, Mask right, MaskOperation operation) =>
			MaskBitwiseOps.BinaryOpInPlace(left, right, operation);
	}
}

public static class MaskModuleExtensions
{
	extension(Mask mask)
	{
		public Mask OP(Mask other, MaskOperation operation) =>
			MaskBitwiseOps.BinaryOp(mask, other, operation);

		public Mask IPOP(Mask other, MaskOperation operation) =>
			MaskBitwiseOps.BinaryOpInPlace(mask, other, operation);

		public void SetAll() => MaskBitwiseOps.SetAll(mask);

		public void ClearAll() => MaskBitwiseOps.ClearAll(mask);

		public Mask Nand(Mask other) => MaskBitwiseOps.BinaryOp(mask, other, MaskOperation.Nand);

		public Mask Nor(Mask other) => MaskBitwiseOps.BinaryOp(mask, other, MaskOperation.Nor);

		public Mask Xnor(Mask other) => MaskBitwiseOps.BinaryOp(mask, other, MaskOperation.Xnor);
	}

	extension(Vector vector)
	{
		public Vector Filter(Mask mask, float fill) => MaskVectorOps.Filter(vector, mask, fill);

		public Vector Select(Mask mask) => MaskVectorOps.Select(vector, mask);
	}
}
