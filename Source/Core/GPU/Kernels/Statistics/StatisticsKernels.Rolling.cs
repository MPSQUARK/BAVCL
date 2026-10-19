using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, int> rollingSumFloat1D
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(rollingSumFloat1D), KernelDomain.Statistics);

	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, int, int> rollingSumFloat2D
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(rollingSumFloat2D), KernelDomain.Statistics);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int> rollingSumInt1D
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(rollingSumInt1D), KernelDomain.Statistics);

	public Action<AcceleratorStream, Index1D, ArrayView<int>, ArrayView<int>, int, int> rollingSumInt2D
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(rollingSumInt2D), KernelDomain.Statistics);

	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, int> rollingAverageFloat1D
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(rollingAverageFloat1D), KernelDomain.Statistics);

	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, int, int> rollingAverageFloat2D
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(rollingAverageFloat2D), KernelDomain.Statistics);

	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<int>, int> rollingAverageFromInt1D
		= (_, _, _, _, _) => throw new KernelNotCompiledException(nameof(rollingAverageFromInt1D), KernelDomain.Statistics);

	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<int>, int, int> rollingAverageFromInt2D
		= (_, _, _, _, _, _) => throw new KernelNotCompiledException(nameof(rollingAverageFromInt2D), KernelDomain.Statistics);

	internal void LoadStatisticsRollingKernels()
	{
		rollingSumFloat1D = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, int>(RollingSumFloat1D_Kern);
		rollingSumFloat2D = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, int, int>(RollingSumFloat2D_Kern);
		rollingSumInt1D = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int>(RollingSumInt1D_Kern);
		rollingSumInt2D = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<int>, ArrayView<int>, int, int>(RollingSumInt2D_Kern);
		rollingAverageFloat1D = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, int>(RollingAverageFloat1D_Kern);
		rollingAverageFloat2D = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, int, int>(RollingAverageFloat2D_Kern);
		rollingAverageFromInt1D = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<int>, int>(RollingAverageFromInt1D_Kern);
		rollingAverageFromInt2D = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<int>, int, int>(RollingAverageFromInt2D_Kern);
	}

	static void RollingSumFloat1D_Kern(Index1D index, ArrayView<float> output, ArrayView<float> cum, int window)
	{
		int i = index.X;
		float previous = i >= window ? cum[i - window] : 0f;
		output[i] = cum[i] - previous;
	}

	static void RollingSumFloat2D_Kern(Index1D index, ArrayView<float> output, ArrayView<float> cum, int window, int columns)
	{
		int i = index.X;
		int col = i % columns;
		float previous = col >= window ? cum[i - window] : 0f;
		output[i] = cum[i] - previous;
	}

	static void RollingSumInt1D_Kern(Index1D index, ArrayView<int> output, ArrayView<int> cum, int window)
	{
		int i = index.X;
		int previous = i >= window ? cum[i - window] : 0;
		output[i] = cum[i] - previous;
	}

	static void RollingSumInt2D_Kern(Index1D index, ArrayView<int> output, ArrayView<int> cum, int window, int columns)
	{
		int i = index.X;
		int col = i % columns;
		int previous = col >= window ? cum[i - window] : 0;
		output[i] = cum[i] - previous;
	}

	static void RollingAverageFloat1D_Kern(Index1D index, ArrayView<float> output, ArrayView<float> cum, int window)
	{
		int i = index.X;
		float previous = i >= window ? cum[i - window] : 0f;
		float sum = cum[i] - previous;
		int effective = XMath.Min(i + 1, window);
		output[i] = sum / effective;
	}

	static void RollingAverageFloat2D_Kern(Index1D index, ArrayView<float> output, ArrayView<float> cum, int window, int columns)
	{
		int i = index.X;
		int col = i % columns;
		float previous = col >= window ? cum[i - window] : 0f;
		float sum = cum[i] - previous;
		int effective = XMath.Min(col + 1, window);
		output[i] = sum / effective;
	}

	static void RollingAverageFromInt1D_Kern(Index1D index, ArrayView<float> output, ArrayView<int> rolling, int window)
	{
		int i = index.X;
		int effective = XMath.Min(i + 1, window);
		output[i] = rolling[i] / (float)effective;
	}

	static void RollingAverageFromInt2D_Kern(Index1D index, ArrayView<float> output, ArrayView<int> rolling, int window, int columns)
	{
		int i = index.X;
		int col = i % columns;
		int effective = XMath.Min(col + 1, window);
		output[i] = rolling[i] / (float)effective;
	}
}
