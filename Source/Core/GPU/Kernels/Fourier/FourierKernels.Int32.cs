using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>, int, int, int>
		fftRealInt = (_, _, _, _, _, _, _, _) =>
			throw new KernelNotCompiledException(nameof(fftRealInt), KernelDomain.Fourier);

	internal void LoadFourierInt32Kernels()
	{
		fftRealInt = accelerator.LoadAutoGroupedKernel<
			Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>, int, int, int>(RealIntFft_Kern);
	}

	static void RealIntFft_Kern(
		Index1D outputIndex,
		ArrayView<float> real,
		ArrayView<float> imaginary,
		ArrayView<int> input,
		int length,
		int columns,
		int inverse)
	{
		int output = outputIndex.X;
		if (columns != 0)
		{
			int rows = length / columns;
			int frequencyRow = output / columns;
			int frequencyColumn = output - frequencyRow * columns;
			float real2D = 0f;
			float imaginary2D = 0f;
			for (int row = 0; row < rows; row++)
				for (int column = 0; column < columns; column++)
				{
					float angle = 6.283185307179586f *
						((float)frequencyRow * row / rows + (float)frequencyColumn * column / columns);
					float sine = XMath.Sin(angle) * (inverse != 0 ? 1f : -1f);
					float value = input[row * columns + column];
					real2D += value * XMath.Cos(angle);
					imaginary2D += value * sine;
				}
			float scale2D = inverse != 0 ? 1f / (rows * columns) : 1f;
			real[output] = real2D * scale2D;
			imaginary[output] = imaginary2D * scale2D;
			return;
		}
		int lineLength = columns == 0 ? length : columns;
		int line = columns == 0 ? 0 : output / columns;
		int frequency = columns == 0 ? output : output - line * columns;
		int start = line * lineLength;
		float realSum = 0f;
		float imaginarySum = 0f;
		for (int index = 0; index < lineLength; index++)
		{
			float angle = 6.283185307179586f * frequency * index / lineLength;
			float sine = XMath.Sin(angle) * (inverse != 0 ? 1f : -1f);
			float value = input[start + index];
			realSum += value * XMath.Cos(angle);
			imaginarySum += value * sine;
		}
		float scale = inverse != 0 ? 1f / lineLength : 1f;
		real[output] = realSum * scale;
		imaginary[output] = imaginarySum * scale;
	}
}
