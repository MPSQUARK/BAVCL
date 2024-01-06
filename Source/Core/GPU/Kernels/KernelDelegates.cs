using ILGPU;
using ILGPU.Runtime;

namespace BAVCL.Core;

public delegate void AppendKernel(AcceleratorStream stream, Index1D index, ArrayView<float> Output, ArrayView<float> vecA, ArrayView<float> vecB, int vecAcol, int vecBcol);

public delegate void AccessSliceKernel(AcceleratorStream stream, Index1D index, ArrayView<float> OutPut, ArrayView<float> Input, ArrayView<int> ChangeSelectLength);
public delegate void A_FloatOPKernel(AcceleratorStream stream, Index1D index, ArrayView<float> OutPut, ArrayView<float> InputA, ArrayView<float> InputB, SpecializedValue<int> operation);
public delegate void A_FloatOPKernelIP(AcceleratorStream stream, Index1D index, ArrayView<float> IO, ArrayView<float> Input, SpecializedValue<int> operation);
public delegate void S_FloatOPKernel(AcceleratorStream stream, Index1D index, ArrayView<float> OutPut, ArrayView<float> Input, float Scalar, SpecializedValue<int> operation);
public delegate void S_FloatOPKernelIP(AcceleratorStream stream, Index1D index, ArrayView<float> IO, float Scalar, SpecializedValue<int> operation);

public delegate void DiffKernel(AcceleratorStream stream, Index1D index, ArrayView<float> Output, ArrayView<float> Input);

public delegate void CrossKernel(AcceleratorStream stream, Index1D index, ArrayView<float> Output, ArrayView<float> InputA, ArrayView<float> InputB);
public delegate void TransposeKernel(AcceleratorStream stream, Index1D index, ArrayView<float> Output, ArrayView<float> Input, int columns);


public delegate void SIOKernel(AcceleratorStream stream, Index1D index, ArrayView<float> IO, float Scalar);
/// <summary>
/// Dual vector input kernel, supplying the operation as a specialized value
/// </summary>
/// <param name="stream"></param>
/// <param name="index"></param>
/// <param name="Output"></param>
/// <param name="InputA"></param>
/// <param name="InputB"></param>
/// <param name="Cols"></param>
/// <param name="operation"></param>
public delegate void DualVectorOPKernel(AcceleratorStream stream, Index1D index, ArrayView<float> Output, ArrayView<float> InputA, ArrayView<float> InputB, int Cols, SpecializedValue<int> operation);
public delegate void IOKernel(AcceleratorStream stream, Index1D index, ArrayView<float> IO);