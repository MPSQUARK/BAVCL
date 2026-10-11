using ILGPU.Algorithms.ScanReduceOperations;

namespace BAVCL.GpuAlgorithms.ScanReduceOperations;

/// <summary>ILGPU scan/reduce operator for cumulative product on <see cref="float"/> (identity 1, op multiply).</summary>
readonly struct MulFloat : IScanReduceOperation<float>
{
	public string CLCommand => "*";

	public float Identity => 1f;

	public float Apply(float first, float second) => first * second;

	public void AtomicApply(ref float target, float value) =>
		target = Apply(target, value);
}
