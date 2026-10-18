using BAVCL.Modules.GpuOps;

namespace BAVCL.Modules.Arithmetic;

// TODO: To be refactored further, Guards can be combined and Operations inlined.
internal static class MatrixOpsCore
{
	internal static Vector BinaryX(Vector matrixA, Vector matrixB, Operations operation)
	{
		Guard.IsVector2D(matrixA);
		Guard.IsVector2D(matrixB);
		Guard.MatchingDimensions(matrixA.Shape(), matrixB.Shape());
		return VectorVectorOp.VectorVectorOP(matrixA, matrixB, operation);
	}

	internal static VectorInt BinaryX(VectorInt matrixA, VectorInt matrixB, Operations operation)
	{
		Guard.IsVector2D(matrixA);
		Guard.IsVector2D(matrixB);
		Guard.MatchingDimensions(matrixA.Shape(), matrixB.Shape());
		return VectorVectorOp.VectorVectorOP(matrixA, matrixB, operation);
	}
}
