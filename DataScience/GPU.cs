using ILGPU;
using ILGPU.Algorithms;
using ILGPU.IR.Transformations;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataScience
{
    public class GPU
    {
        Context context;
        public Accelerator accelerator;

        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int> appendKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>, float> nanToNumKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>, ArrayView<int>> accessSliceKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>> consecutiveOperationKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>, float, SpecializedValue<int>> scalarConsecutiveOperationKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>> diffKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>> reverseKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>> absKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>> reciprocalKernel;

        public GPU(bool forceCPU = false, ContextFlags flags = ContextFlags.None, OptimizationLevel optimizationLevel = OptimizationLevel.Debug)
        {
            this.context = new Context(flags, optimizationLevel);
            this.context.EnableAlgorithms();

            this.accelerator = GetGpu(context, forceCPU);
            Console.WriteLine("Device loaded: " + accelerator.Name);

            LoadKernels();
            Console.WriteLine("Device Kernels Loaded");
        }

        private void LoadKernels()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            appendKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int>(AppendKernel);
            nanToNumKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, float>(Nan_to_numKernel);
            accessSliceKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<int>>(AccessSliceKernel);
            consecutiveOperationKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>>(ConsecutiveOperationKernel);
            scalarConsecutiveOperationKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, float, SpecializedValue<int>>(ScalarConsecutiveOperationKernel);
            diffKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>> (DiffKernel);
            reverseKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>> (ReverseKernel);
            absKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>>(AbsKernel);
            reciprocalKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>>(ReciprocalKernel);

            timer.Stop();
            Console.WriteLine("Kernels Loaded in: " + timer.Elapsed.TotalMilliseconds + " MS");
        }

        private Accelerator GetGpu(Context context, bool prefCPU = false)
        {
            var groupedAccelerators = Accelerator.Accelerators
                    .GroupBy(x => x.AcceleratorType)
                    .ToDictionary(x => x.Key, x => x.ToList());

            if (prefCPU)
            {
                return new CPUAccelerator(context);
            }

            if (groupedAccelerators.TryGetValue(AcceleratorType.Cuda, out var nv))
            {
                return Accelerator.Create(context, nv[0]);
            }

            if (groupedAccelerators.TryGetValue(AcceleratorType.OpenCL, out var cl))
            {
                return Accelerator.Create(context, cl[0]);
            }

            //fallback
            Console.WriteLine("Warning : Could not find gpu, falling back to Default device");
            return new CPUAccelerator(context);
        }

        //Kernels

        static void AppendKernel(Index1 index, ArrayView<float> Output, ArrayView<float> vecA, ArrayView<float> vecB, int vecAcol, int vecBcol)
        {
            for (int i = 0; i < vecAcol; i++)
            {
                Output[index * (vecAcol + vecBcol) + i] = vecA[index * vecAcol + i];
            }
            for (int i = 0; i < vecBcol; i++)
            {
                Output[index * (vecAcol + vecBcol) + i + vecAcol] = vecB[index * vecBcol + i];
            }
        }

        static void Nan_to_numKernel(Index1 index, ArrayView<float> IO, float num)
        {
            if (float.IsNaN(IO[index]) || float.IsInfinity(IO[index]))
            {
                IO[index] = num;
            }

        }

        static void AccessSliceKernel(Index1 index, ArrayView<float> OutPut, ArrayView<float> Input, ArrayView<int> ChangeSelectLength)
        {
            OutPut[index] = Input[
                index * ChangeSelectLength[0] * ChangeSelectLength[4] + // iRcL
                index * ChangeSelectLength[1] +                         // iCc
                ChangeSelectLength[2] * ChangeSelectLength[4] +         // RsL
                ChangeSelectLength[3]];                                 // Cs
        }

        static void ConsecutiveOperationKernel(Index1 index, ArrayView<float> InputA, ArrayView<float> InputB, ArrayView<float> OutPut, SpecializedValue<int> operation)
        {
            switch ((Operations)operation.Value)
            {
                case Operations.multiplication:
                    OutPut[index] = InputA[index] * InputB[index];
                    break;
                case Operations.addition:
                    OutPut[index] = InputA[index] + InputB[index];
                    break;
                case Operations.subtraction:
                    OutPut[index] = InputA[index] - InputB[index];
                    break;
                case Operations.division:
                    OutPut[index] = InputA[index] / InputB[index];
                    break;
                case Operations.inverseDivision:
                    OutPut[index] = InputB[index] / InputA[index];
                    break;
            }
        }

        static void ScalarConsecutiveOperationKernel(Index1 index, ArrayView<float> OutPut, ArrayView<float> Input, float Scalar, SpecializedValue<int> operation)
        {
            switch ((Operations)operation.Value)
            {
                case Operations.multiplication:
                    OutPut[index] = Input[index] * Scalar;
                    break;
                case Operations.addition:
                    OutPut[index] = Input[index] + Scalar;
                    break;
                case Operations.subtraction:
                    OutPut[index] = Input[index] - Scalar;
                    break;
                case Operations.division:
                    OutPut[index] = Input[index] / Scalar;
                    break;
                case Operations.inverseDivision:
                    OutPut[index] = Scalar / Input[index];
                    break;
            }
        }

        static void DiffKernel(Index1 index, ArrayView<float> Output, ArrayView<float> Input)
        {
            Output[index] = Input[index + 1] - Input[index];
        }

        static void ReverseKernel(Index1 index, ArrayView<float> Output, ArrayView<float> Input)
        {
            Output[index] = Input[Input.Length - 1 - index];
        }

        static void AbsKernel(Index1 index, ArrayView<float> IO)
        {
            IO[index] = XMath.Abs(IO[index]);
        }

        static void ReciprocalKernel(Index1 index, ArrayView<float> IO)
        {
            IO[index] = XMath.Rcp(IO[index]);
        }
    }

    public enum Operations
    {
        multiplication = 0,
        addition = 1,
        subtraction = 2,
        division = 3,
        inverseDivision = 4,
    }
}
