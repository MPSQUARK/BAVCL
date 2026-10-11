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

		public VectorInt Where() => MaskCompaction.Where(mask);

		public VectorInt WhereX() => MaskCompaction.WhereX(mask);
	}

	extension(Vector vector)
	{
		public Vector Mask(Mask mask, float fill = 0f) => MaskCompactionReference.Mask(vector, mask, fill);

		public Vector MaskX(Mask mask, float fill = 0f) => MaskVectorOps.MaskX(vector, mask, fill);

		public Vector Filter(Mask mask) => MaskCompactionReference.Filter(vector, mask);

		public Vector FilterX(Mask mask) => MaskVectorOps.FilterX(vector, mask);

		public (Vector TrueLanes, Vector FalseLanes) Partition(Mask mask) =>
			MaskCompactionReference.Partition(vector, mask);

		public (Vector TrueLanes, Vector FalseLanes) PartitionX(Mask mask) => MaskVectorOps.PartitionX(vector, mask);
	}

	extension(VectorInt vector)
	{
		public VectorInt Mask(Mask mask, int fill = 0) => MaskCompactionReference.Mask(vector, mask, fill);

		public VectorInt MaskX(Mask mask, int fill = 0) => MaskVectorIntOps.MaskX(vector, mask, fill);

		public VectorInt Filter(Mask mask) => MaskCompactionReference.Filter(vector, mask);

		public VectorInt FilterX(Mask mask) => MaskVectorIntOps.FilterX(vector, mask);

		public (VectorInt TrueLanes, VectorInt FalseLanes) Partition(Mask mask) =>
			MaskCompactionReference.Partition(vector, mask);

		public (VectorInt TrueLanes, VectorInt FalseLanes) PartitionX(Mask mask) =>
			MaskVectorIntOps.PartitionX(vector, mask);
	}
}
