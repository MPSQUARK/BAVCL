using System;
using BAVCL.Core.Exceptions;
using ILGPU;
using ILGPU.Runtime;

namespace BAVCL;

public partial class GPU
{
	// No module provides these; they stay uncompiled until an experimental module is added.
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>> TestSQRTKernel
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(TestSQRTKernel));
	public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>> TestMYSQRTKernel
		= (_, _, _, _) => throw new KernelNotCompiledException(nameof(TestMYSQRTKernel));
}
