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

		public VectorInt Where() => MaskCompactionCpu.Where(mask);

		public VectorInt WhereX() => MaskCompaction.WhereX(mask);

		public int CountTrue() => MaskCpu.CountTrue(mask);

		public int CountTrueX() => MaskGpu.CountTrueX(mask);

		public bool Any() => MaskCpu.Any(mask);

		public bool AnyX() => MaskGpu.AnyX(mask);

		public bool All() => MaskCpu.All(mask);

		public bool AllX() => MaskGpu.AllX(mask);

		public int FirstTrueIndex() => MaskCpu.FirstTrueIndex(mask);

		public int FirstTrueIndexX() => MaskGpu.FirstTrueIndexX(mask);

		public int LastTrueIndex() => MaskCpu.LastTrueIndex(mask);

		public int LastTrueIndexX() => MaskGpu.LastTrueIndexX(mask);
	}

	extension(Vector vector)
	{
		public Vector Mask(Mask mask, float fill = 0f) => MaskCompactionCpu.Mask(vector, mask, fill);

		public Vector MaskX(Mask mask, float fill = 0f) => MaskGpu.MaskX(vector, mask, fill);

		public Vector Filter(Mask mask) => MaskCompactionCpu.Filter(vector, mask);

		public Vector FilterX(Mask mask) => MaskCompaction.FilterX(vector, mask);

		public (Vector TrueLanes, Vector FalseLanes) Partition(Mask mask) =>
			MaskCompactionCpu.Partition(vector, mask);

		public (Vector TrueLanes, Vector FalseLanes) PartitionX(Mask mask) => MaskCompaction.PartitionX(vector, mask);
	}

	extension(VectorInt vector)
	{
		public VectorInt Mask(Mask mask, int fill = 0) => MaskCompactionCpu.Mask(vector, mask, fill);

		public VectorInt MaskX(Mask mask, int fill = 0) => MaskGpu.MaskX(vector, mask, fill);

		public VectorInt Filter(Mask mask) => MaskCompactionCpu.Filter(vector, mask);

		public VectorInt FilterX(Mask mask) => MaskCompaction.FilterX(vector, mask);

		public (VectorInt TrueLanes, VectorInt FalseLanes) Partition(Mask mask) =>
			MaskCompactionCpu.Partition(vector, mask);

		public (VectorInt TrueLanes, VectorInt FalseLanes) PartitionX(Mask mask) =>
			MaskCompaction.PartitionX(vector, mask);
	}
}
