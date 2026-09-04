using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using ILGPU.Util;
using BAVCL.Core;

namespace BAVCL;

public partial class GPU
{
	// Keep in sync with ReduceRowOps (host). ILGPU device bodies cannot share that module.
	static float ReduceRowElementFloat(float coeff, float matrixValue, Operations operation) =>
		operation switch
		{
			Operations.multiply => coeff * matrixValue,
			Operations.add => coeff + matrixValue,
			Operations.subtract => coeff - matrixValue,
			Operations.flipSubtract => matrixValue - coeff,
			Operations.divide => coeff / matrixValue,
			Operations.flipDivide => matrixValue / coeff,
			Operations.pow => XMath.Pow(coeff, matrixValue),
			Operations.flipPow => XMath.Pow(matrixValue, coeff),
			Operations.differenceSquared => XMath.Pow(coeff - matrixValue, 2f),
			Operations.distance => XMath.Pow(coeff - matrixValue, 2f),
			_ => 0f,
		};

	static bool ReduceRowUsesDistance(Operations operation) => operation == Operations.distance;

	static float AccumulateReduceRow(
		ArrayView<float> coeffs,
		ArrayView<float> inputB,
		int startidx,
		int cols,
		SpecializedValue<int> operation)
	{
		Operations op = (Operations)operation.Value;
		float sum = 0f;
		for (int i = 0; i < cols; i++)
			sum += ReduceRowElementFloat(coeffs[i], inputB[startidx + i], op);

		return ReduceRowUsesDistance(op) ? XMath.Sqrt(sum) : sum;
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

	static int ReduceRowElementInt(int coeff, int matrixValue, Operations operation) =>
		operation switch
		{
			Operations.multiply => coeff * matrixValue,
			Operations.add => coeff + matrixValue,
			Operations.subtract => coeff - matrixValue,
			Operations.flipSubtract => matrixValue - coeff,
			Operations.divide => coeff / matrixValue,
			Operations.flipDivide => matrixValue / coeff,
			_ => 0,
		};

	static int AccumulateReduceRowInt(
		ArrayView<int> coeffs,
		ArrayView<int> inputB,
		int startidx,
		int cols,
		SpecializedValue<int> operation)
	{
		Operations op = (Operations)operation.Value;
		int sum = 0;
		for (int i = 0; i < cols; i++)
			sum += ReduceRowElementInt(coeffs[i], inputB[startidx + i], op);

		return sum;
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
