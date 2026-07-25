using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

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

	static int BroadcastOperandIndex(
		Index1D flatOut,
		SpecializedValue<int> outCols,
		SpecializedValue<int> rows,
		SpecializedValue<int> cols)
	{
		int outColsValue = outCols.Value;
		int rowsValue = rows.Value;
		int colsValue = cols.Value;

		if (rowsValue == 1 && colsValue == 1)
			return 0;

		if (rowsValue == 1)
			return flatOut % outColsValue;

		if (colsValue == 1)
			return flatOut / outColsValue;

		return flatOut;
	}
}
