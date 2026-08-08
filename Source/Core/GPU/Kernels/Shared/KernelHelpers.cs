using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using ILGPU.Util;
using BAVCL.Core;

namespace BAVCL;

public partial class GPU
{
	static float AccumulateReduceRow(
		ArrayView<float> coeffs,
		ArrayView<float> inputB,
		int startidx,
		int cols,
		SpecializedValue<int> operation)
	{
		switch ((Operations)operation.Value)
		{
			case Operations.multiply:
				{
					float sum = 0f;
					for (int i = 0; i < cols; i++)
						sum += coeffs[i] * inputB[startidx + i];
					return sum;
				}
			case Operations.add:
				{
					float sum = 0f;
					for (int i = 0; i < cols; i++)
						sum += coeffs[i] + inputB[startidx + i];
					return sum;
				}
			case Operations.subtract:
				{
					float sum = 0f;
					for (int i = 0; i < cols; i++)
						sum += coeffs[i] - inputB[startidx + i];
					return sum;
				}
			case Operations.flipSubtract:
				{
					float sum = 0f;
					for (int i = 0; i < cols; i++)
						sum += inputB[startidx + i] - coeffs[i];
					return sum;
				}
			case Operations.divide:
				{
					float sum = 0f;
					for (int i = 0; i < cols; i++)
						sum += coeffs[i] / inputB[startidx + i];
					return sum;
				}
			case Operations.flipDivide:
				{
					float sum = 0f;
					for (int i = 0; i < cols; i++)
						sum += inputB[startidx + i] / coeffs[i];
					return sum;
				}
			case Operations.pow:
				{
					float sum = 0f;
					for (int i = 0; i < cols; i++)
						sum += XMath.Pow(coeffs[i], inputB[startidx + i]);
					return sum;
				}
			case Operations.flipPow:
				{
					float sum = 0f;
					for (int i = 0; i < cols; i++)
						sum += XMath.Pow(inputB[startidx + i], coeffs[i]);
					return sum;
				}
			case Operations.differenceSquared:
				{
					float sum = 0f;
					for (int i = 0; i < cols; i++)
						sum += XMath.Pow(coeffs[i] - inputB[startidx + i], 2f);
					return sum;
				}
			case Operations.distance:
				{
					float sum = 0f;
					for (int i = 0; i < cols; i++)
						sum += XMath.Pow(coeffs[i] - inputB[startidx + i], 2f);
					return XMath.Sqrt(sum);
				}
			default:
				return 0f;
		}
	}

	static void ApplyBroadcastOp(ref float target, float a, float b, SpecializedValue<int> operation)
	{
		switch ((Operations)operation.Value)
		{
			case Operations.multiply:
				target = a * b;
				break;
			case Operations.add:
				target = a + b;
				break;
			case Operations.subtract:
				target = a - b;
				break;
			case Operations.flipSubtract:
				target = b - a;
				break;
			case Operations.divide:
				target = a / b;
				break;
			case Operations.flipDivide:
				target = b / a;
				break;
			case Operations.pow:
				target = XMath.Pow(a, b);
				break;
			case Operations.flipPow:
				target = XMath.Pow(b, a);
				break;
			case Operations.differenceSquared:
				target = XMath.Pow(a - b, 2f);
				break;
		}
	}

	const int MaskWordShift = 5;
	const int MaskLaneMask = 31;

	static int MaskLane(ArrayView<int> words, int element) =>
		(words[element >> MaskWordShift] >> (element & MaskLaneMask)) & 1;

	// Lanes of one word are owned by 32 distinct threads, so the packed write is an atomic
	// merge into storage the host allocated zeroed.
	static void WriteMaskLane(ArrayView<int> words, int element, int lane) =>
		Atomic.Or(ref words[element >> MaskWordShift], lane << (element & MaskLaneMask));

	static int ApplyMaskWordOp(int left, int right, MaskOperation operation) => operation switch
	{
		MaskOperation.And => left & right,
		MaskOperation.Or => left | right,
		MaskOperation.Xor => left ^ right,
		MaskOperation.Nand => ~(left & right),
		MaskOperation.Nor => ~(left | right),
		MaskOperation.Xnor => ~(left ^ right),
		_ => 0,
	};

	// Lanes above the logical element count must stay clear so packed words remain comparable.
	static int PaddingKeepMask(Index1D word, int lastWord, int tailMask) =>
		Utilities.Select(word.X == lastWord, tailMask, -1);

	// IEEE ordering is already false whenever an operand is NaN, so only equality needs a NaN rule.
	static int CompareLane(float left, float right, VectorComparison comparison) => comparison switch
	{
		VectorComparison.Greater => AsLane(left > right),
		VectorComparison.Less => AsLane(left < right),
		VectorComparison.GreaterOrEqual => AsLane(left >= right),
		VectorComparison.LessOrEqual => AsLane(left <= right),
		VectorComparison.Equal => AsLane(NaNAwareEquals(left, right)),
		VectorComparison.NotEqual => AsLane(!NaNAwareEquals(left, right)),
		_ => 0,
	};

	static bool NaNAwareEquals(float left, float right) =>
		(left == right) | (float.IsNaN(left) & float.IsNaN(right));

	static int AsLane(bool set) => Utilities.Select(set, 1, 0);

	static int AccumulateReduceRowInt(
		ArrayView<int> coeffs,
		ArrayView<int> inputB,
		int startidx,
		int cols,
		SpecializedValue<int> operation)
	{
		switch ((Operations)operation.Value)
		{
			case Operations.multiply:
				{
					int sum = 0;
					for (int i = 0; i < cols; i++)
						sum += coeffs[i] * inputB[startidx + i];
					return sum;
				}
			case Operations.add:
				{
					int sum = 0;
					for (int i = 0; i < cols; i++)
						sum += coeffs[i] + inputB[startidx + i];
					return sum;
				}
			case Operations.subtract:
				{
					int sum = 0;
					for (int i = 0; i < cols; i++)
						sum += coeffs[i] - inputB[startidx + i];
					return sum;
				}
			case Operations.flipSubtract:
				{
					int sum = 0;
					for (int i = 0; i < cols; i++)
						sum += inputB[startidx + i] - coeffs[i];
					return sum;
				}
			case Operations.divide:
				{
					int sum = 0;
					for (int i = 0; i < cols; i++)
						sum += coeffs[i] / inputB[startidx + i];
					return sum;
				}
			case Operations.flipDivide:
				{
					int sum = 0;
					for (int i = 0; i < cols; i++)
						sum += inputB[startidx + i] / coeffs[i];
					return sum;
				}
			default:
				return 0;
		}
	}

	static void ApplyBroadcastOpInt(ref int target, int a, int b, SpecializedValue<int> operation)
	{
		switch ((Operations)operation.Value)
		{
			case Operations.multiply:
				target = a * b;
				break;
			case Operations.add:
				target = a + b;
				break;
			case Operations.subtract:
				target = a - b;
				break;
			case Operations.flipSubtract:
				target = b - a;
				break;
			case Operations.divide:
				target = a / b;
				break;
			case Operations.flipDivide:
				target = b / a;
				break;
			case Operations.differenceSquared:
				{
					int diff = a - b;
					target = diff * diff;
					break;
				}
			case Operations.modulo:
				target = a % b;
				break;
			case Operations.flipModulo:
				target = b % a;
				break;
			case Operations.leftShift:
				target = a << b;
				break;
			case Operations.rightShift:
				target = a >> b;
				break;
			case Operations.bitwiseXor:
				target = a ^ b;
				break;
			case Operations.bitwiseAnd:
				target = a & b;
				break;
		}
	}

	static int AbsIntBitwise(int value) => value & int.MaxValue;

	static int NegateIntBitwise(int value) => unchecked(~value + 1);

	static int CompareLaneInt(int left, int right, VectorComparison comparison) => comparison switch
	{
		VectorComparison.Greater => AsLane(left > right),
		VectorComparison.Less => AsLane(left < right),
		VectorComparison.GreaterOrEqual => AsLane(left >= right),
		VectorComparison.LessOrEqual => AsLane(left <= right),
		VectorComparison.Equal => AsLane(left == right),
		VectorComparison.NotEqual => AsLane(left != right),
		_ => 0,
	};
}
