using ILGPU.Algorithms.ScanReduceOperations;

namespace BAVCL.GpuAlgorithms.ScanReduceOperations;

/// <summary>ILGPU scan/reduce operator for cumulative product on <see cref="int"/> (identity 1, op multiply).</summary>
readonly struct MulInt32 : IScanReduceOperation<int>
{
	public string CLCommand => "*";

	public int Identity => 1;

	public int Apply(int first, int second) => first * second;

	public void AtomicApply(ref int target, int value) =>
		target = Apply(target, value);
}
