using ILGPU;
using ILGPU.Algorithms;
using ILGPU.IR.Transformations;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DataScience.Core;
using DataScience.Experimental;

namespace DataScience
{
    public class GPU
    {
        protected internal Context context;
        public Accelerator accelerator;


        // LRU 
        public ConcurrentDictionary<uint, WeakReference<ICacheable>> CachedInfo = new ConcurrentDictionary<uint, WeakReference<ICacheable>>();  // Size, LiveCount
        public ConcurrentDictionary<uint, MemoryBuffer> CachedMemory = new ConcurrentDictionary<uint, MemoryBuffer>(); // Memory Buffer


        protected internal ConcurrentDictionary<uint, bool> ForceKeepFlags = new ConcurrentDictionary<uint, bool>();
        protected internal ConcurrentQueue<uint> LRU = new ConcurrentQueue<uint>();                                                
        
        protected internal uint CurrentVecId = 0;
        protected internal int LiveTaskCount = 0;

        // Device Memory
        public readonly long MaxMemory;
        protected internal long MemoryInUse = 0;
        protected internal float MemoryCap;
        public float memorycap
        { 
            get { return this.MemoryCap; } 
            set { MemoryCap = Math.Clamp(value, 0f, 1f); } 
        }


        
        

        // Variables - Kernels
        #region
        public Action<AcceleratorStream, Index1, ArrayView<double>, ArrayView<float>> sumKernel;



        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int> appendKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>, float> nanToNumKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>, ArrayView<int>> accessSliceKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>> consecOpKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>, float, SpecializedValue<int>> scalarConsecOpKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>> vectormatrixOpKernel;

        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>, SpecializedValue<int>> consecOpKernelIP;
        public Action<AcceleratorStream, Index1, ArrayView<float>, float, SpecializedValue<int>> scalarConsecOpKernelIP;
        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>> diffKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>> reverseKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>> absKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>> reciprocalKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>> crossKernel;
        public Action<AcceleratorStream, Index1, ArrayView<float>, ArrayView<float>, int> transposekernel;



        #endregion


        // Constructor
        public GPU(float memorycap = 0.8f, bool forceCPU = false, ContextFlags flags = ContextFlags.None, OptimizationLevel optimizationLevel = OptimizationLevel.Debug)
        {
            // Create Context
            this.context = new Context(flags, optimizationLevel);
            this.context.EnableAlgorithms();

            // Get Accelerator Device
            this.accelerator = GetGpu(context, forceCPU);
            Console.WriteLine("Device loaded: " + accelerator.Name);

            // Set Memory Usage Cap
            this.memorycap = memorycap;
            this.MaxMemory = (long)Math.Round(this.accelerator.MemorySize * this.MemoryCap);

            // Load Kernels
            LoadKernels();
            Console.WriteLine("Device Kernels Loaded");
        }
        private void LoadKernels()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            sumKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<double>, ArrayView<float>>(SumKernel);
            testkern = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, SpecializedValue<int>>(TESTKERNEL);


            appendKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int>(AppendKernel);
            nanToNumKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, float>(Nan_to_numKernel);
            accessSliceKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<int>>(AccessSliceKernel);
            
            consecOpKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>>(ConsecutiveOperationKernel);
            scalarConsecOpKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, float, SpecializedValue<int>>(ScalarConsecutiveOperationKernel);
            vectormatrixOpKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>>(VectorMatrixKernel);

            consecOpKernelIP = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, SpecializedValue<int>>(ConsecutiveOperationKernelIP);
            scalarConsecOpKernelIP = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, float, SpecializedValue<int>>(ScalarConsecutiveOperationKernelIP);

            diffKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>> (DiffKernel);
            reverseKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>> (ReverseKernel);
            absKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>>(AbsKernel);
            reciprocalKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>>(ReciprocalKernel);
            crossKernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, ArrayView<float>>(CrossKernel);
            transposekernel = accelerator.LoadAutoGroupedKernel<Index1, ArrayView<float>, ArrayView<float>, int>(TransposeKernel);

            timer.Stop();
            Console.WriteLine("Kernels Loaded in: " + timer.Elapsed.TotalMilliseconds + " MS");
        }


        // Get 'Best' GPU
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


        #region "Memory Caching & Management"
        private uint GenerateId()
        {
            return Interlocked.Increment(ref CurrentVecId);
        }
        private bool IsInCache(uint id)
        {
            return this.CachedMemory.ContainsKey(id);
        }
        public void DeCacheLRU(long required)
        {
            // Check if the memory required doesn't exceed the Maximum available
            if (required > this.MaxMemory)
            {
                throw new Exception($"Cannot cache this data onto the GPU, required memory : {required >> 20} MB, max memory available : {this.MaxMemory >> 20} MB.\n " +
                                    $"Consider spliting/breaking the data into multiple smaller sets OR \n Caching to a GPU with more available memory.");
            }


            // Keep decaching untill enough space is made to accomodate the data
            while (required > (this.MaxMemory - this.MemoryInUse))
            {
                if (this.LiveTaskCount == 0) 
                {
                    throw new Exception(
                        $"GPU states {this.LiveTaskCount} Live Tasks Running, while requiring {required >> 20} MB which is more than available {(this.MaxMemory - this.MemoryInUse) >> 20} MB. Potential cause: memory leak"); 
                }

                // Get the ID of the last item
                uint Id;
                if (!LRU.TryDequeue(out Id)) 
                {
                    throw new Exception($"LRU Empty Cannot Continue DeCaching");
                }

                // Try Get Reference to and the object of ICacheable
                WeakReference<ICacheable> referenceCacheable;
                if (!this.CachedInfo.TryGetValue(Id, out referenceCacheable)) 
                {
                    // Object been GC'ed
                    MemoryBuffer Buffer;
                    CachedMemory.TryRemove(Id, out Buffer);
                    Interlocked.Add(ref MemoryInUse, -Buffer.LengthInBytes);
                    Buffer.Dispose();
                    Interlocked.Decrement(ref LiveTaskCount);
                    continue;
                } 
                ICacheable cacheable;
                if (!referenceCacheable.TryGetTarget(out cacheable)) 
                {
                    // Object been GC'ed
                    MemoryBuffer Buffer;
                    CachedMemory.TryRemove(Id, out Buffer);
                    Interlocked.Add(ref MemoryInUse, -Buffer.LengthInBytes);
                    Buffer.Dispose();
                    Interlocked.Decrement(ref LiveTaskCount);
                    continue;
                } 

                // Try to decache the data
                if (!cacheable.TryDeCache()) { LRU.Enqueue(Id); }

            }

            return;
        }


        public uint Allocate(WeakReference<ICacheable> CacheableReference, MemoryBuffer Buffer, long Size)
        {

            // Increase the Memory in Use
            Interlocked.Add(ref MemoryInUse, Size);

            // generate Id for data so it can be found by the vector
            uint Id = GenerateId();
            while (!CachedInfo.TryAdd(Id, CacheableReference))
            {
                Id = GenerateId();
            }
            CachedMemory.TryAdd(Id, Buffer);

            // Put the Id in a queue
            LRU.Enqueue(Id);

            return Id;
        }
        private void RemoveFromLRU(uint Id)
        {
            if (LRU.Count == 0) { return; }
            if (!LRU.Contains(Id)) { return; }

            uint DequeuedId;
            LRU.TryDequeue(out DequeuedId);

            if (DequeuedId == Id) { return; }


            // Get the number of items in LRU
            int length = LRU.Count;

            // Put back the De-queued Id since it didn't match the Disposed Id
            LRU.Enqueue(DequeuedId);

            // shuffle through LRU to remove the disposed Id
            for (int i = 0; i < length; i++)
            {
                LRU.TryDequeue(out DequeuedId);

                // Id matching the one disposed will not be re-enqueued 
                // Order will be preserved
                if (Id != DequeuedId)
                {
                    LRU.Enqueue(DequeuedId);
                }
            }
            return;
        }
        public uint DeCache(uint Id)
        {
            // Try to remove weak reference
            CachedInfo.TryRemove(Id, out _);
            
            // Try to remove memory
            MemoryBuffer Buffer;
            if (CachedMemory.TryRemove(Id, out Buffer))
            {
                Interlocked.Add(ref MemoryInUse, -Buffer.LengthInBytes);
                Buffer.Dispose();
            }

            RemoveFromLRU(Id);
            Interlocked.Decrement(ref LiveTaskCount);
            return 0;
        }



        public void ShowMemoryUsage(bool percentage = true, string format = "F2")
        {
            if (percentage) { Console.WriteLine($"{(((double)this.MemoryInUse / (double)this.MaxMemory) * 100f).ToString(format)}%"); return; }

            Console.WriteLine( $"{this.MemoryInUse >> 20}/{this.MaxMemory >> 20} MB");
        }


        public void AddLiveTask()
        {
            Interlocked.Increment(ref LiveTaskCount);
        }


        #endregion



        // Test Kernels
        static void SumKernel(Index1 index, ArrayView<double> Output, ArrayView<float> Input)
        {
            double sum = 0;
            for (int i = index * 100000; i < (index + 1) * 100000; i++)
            {
                sum += Input[i];
            }
            Output[index] += sum;
        }




        //Kernels
        static void AppendKernel(Index1 index, ArrayView<float> Output, ArrayView<float> vecA, ArrayView<float> vecB, int vecAcol, int vecBcol)
        {

            for (int i = 0, j=0; j < vecBcol; i++)
            {
                if (i < vecAcol)
                {
                    Output[index * (vecAcol + vecBcol) + i] = vecA[index * vecAcol + i];
                    continue;
                }

                Output[index * (vecAcol + vecBcol) + i] = vecB[index * vecBcol + j];
                j++;

            }

        }



        static void Nan_to_numKernel(Index1 index, ArrayView<float> IO, float num)
        {
            if (float.IsNaN(IO[index]) || float.IsInfinity(IO[index]))
            {
                IO[index] = num;
            }

        }

        //static void AccessSliceKernel(Index1 index, ArrayView<float> OutPut, ArrayView<float> Input, ArrayView<int> ChangeSelectLength)
        //{
        //    OutPut[index] = Input[
        //        index * ChangeSelectLength[0] * ChangeSelectLength[4] + // iRcL
        //        index * ChangeSelectLength[1] +                         // iCc
        //        ChangeSelectLength[2] * ChangeSelectLength[4] +         // RsL
        //        ChangeSelectLength[3]];                                 // Cs
        //}

        static void AccessSliceKernel(Index1 index, ArrayView<float> OutPut, ArrayView<float> Input, ArrayView<int> ChangeSelectLength)
        {
            OutPut[index] = Input[
                index * ChangeSelectLength[1] +                         // iRcL
                ChangeSelectLength[0]];                                 // Cs
        }


        static void ConsecutiveOperationKernel(Index1 index, ArrayView<float> OutPut, ArrayView<float> InputA, ArrayView<float> InputB, SpecializedValue<int> operation)
        {
            switch ((Operations)operation.Value)
            {
                case Operations.multiply:
                    OutPut[index] = InputA[index] * InputB[index];
                    break;
                case Operations.add:
                    OutPut[index] = InputA[index] + InputB[index];
                    break;
                case Operations.subtract:
                    OutPut[index] = InputA[index] - InputB[index];
                    break;
                case Operations.flipSubtract:
                    OutPut[index] = InputB[index] - InputA[index];
                    break;
                case Operations.divide:
                    OutPut[index] = InputA[index] / InputB[index];
                    break;
                case Operations.invDivide:
                    OutPut[index] = InputB[index] / InputA[index];
                    break;
                case Operations.pow:
                    OutPut[index] = XMath.Pow(InputA[index], InputB[index]);
                    break;
                case Operations.flipPow:
                    OutPut[index] = XMath.Pow(InputB[index], InputA[index]);
                    break;
                case Operations.subtractSquare:
                    OutPut[index] = XMath.Pow((InputA[index] - InputB[index]), 2f);
                    break;

            }
        }

        static void ConsecutiveOperationKernelIP(Index1 index, ArrayView<float> IO, ArrayView<float> Input, SpecializedValue<int> operation)
        {
            switch ((Operations)operation.Value)
            {
                case Operations.multiply:
                    IO[index] = IO[index] * Input[index];
                    break;
                case Operations.add:
                    IO[index] = IO[index] + Input[index];
                    break;
                case Operations.subtract:
                    IO[index] = IO[index] - Input[index];
                    break;
                case Operations.flipSubtract:
                    IO[index] = IO[index] - Input[index];
                    break;
                case Operations.divide:
                    IO[index] = IO[index] / Input[index];
                    break;
                case Operations.invDivide:
                    IO[index] = IO[index] / Input[index];
                    break;
                case Operations.pow:
                    IO[index] = XMath.Pow(IO[index], Input[index]);
                    break;
                case Operations.flipPow:
                    IO[index] = XMath.Pow(IO[index], Input[index]);
                    break;
                case Operations.subtractSquare:
                    IO[index] = XMath.Pow((IO[index] - Input[index]), 2f);
                    break;
            }
        }


        static void ScalarConsecutiveOperationKernel(Index1 index, ArrayView<float> OutPut, ArrayView<float> Input, float Scalar, SpecializedValue<int> operation)
        {
            switch ((Operations)operation.Value)
            {
                case Operations.multiply:
                    OutPut[index] = Input[index] * Scalar;
                    break;
                case Operations.add:
                    OutPut[index] = Input[index] + Scalar;
                    break;
                case Operations.subtract:
                    OutPut[index] = Input[index] - Scalar;
                    break;
                case Operations.flipSubtract:
                    OutPut[index] = Scalar - Input[index];
                    break;
                case Operations.divide:
                    OutPut[index] = Input[index] / Scalar;
                    break;
                case Operations.invDivide:
                    OutPut[index] = Scalar / Input[index];
                    break;
                case Operations.pow:
                    OutPut[index] = XMath.Pow(Input[index], Scalar);
                    break;
                case Operations.flipPow:
                    OutPut[index] = XMath.Pow(Scalar, Input[index]);
                    break;
                case Operations.subtractSquare:
                    OutPut[index] = XMath.Pow((Input[index] - Scalar), 2f);
                    break;
            }
        }

        static void ScalarConsecutiveOperationKernelIP(Index1 index, ArrayView<float> IO, float Scalar, SpecializedValue<int> operation)
        {
            switch ((Operations)operation.Value)
            {
                case Operations.multiply:
                    IO[index] = IO[index] * Scalar;
                    break;
                case Operations.add:
                    IO[index] = IO[index] + Scalar;
                    break;
                case Operations.subtract:
                    IO[index] = IO[index] - Scalar;
                    break;
                case Operations.flipSubtract:
                    IO[index] = Scalar - IO[index];
                    break;
                case Operations.divide:
                    IO[index] = IO[index] / Scalar;
                    break;
                case Operations.invDivide:
                    IO[index] = Scalar / IO[index];
                    break;
                case Operations.pow:
                    IO[index] = XMath.Pow(IO[index], Scalar);
                    break;
                case Operations.flipPow:
                    IO[index] = XMath.Pow(Scalar, IO[index]);
                    break;
                case Operations.subtractSquare:
                    IO[index] = XMath.Pow((IO[index] - Scalar), 2f);
                    break;
            }
        }


        static void VectorMatrixKernel(Index1 index, ArrayView<float> OutPut, ArrayView<float> InputA, ArrayView<float> InputB, int Cols, SpecializedValue<int> operation)
        {
            int startidx = index * Cols;

            switch ((Operations)operation.Value)
            {
                case Operations.multiply:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += InputA[i] * InputB[startidx + i];
                    }
                    break;
                case Operations.add:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += InputA[i] + InputB[startidx + i];
                    }
                    break;
                case Operations.subtract:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += InputA[i] - InputB[startidx + i];
                    }
                    break;
                case Operations.flipSubtract:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += InputB[startidx + i] - InputA[i];
                    }
                    break;
                case Operations.divide:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += InputA[i] / InputB[startidx + i];
                    }
                    break;
                case Operations.invDivide:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += InputB[startidx + i] / InputA[i];
                    }
                    break;
                case Operations.pow:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += XMath.Pow(InputA[i], InputB[startidx + i]);
                    }
                    break;
                case Operations.flipPow:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += XMath.Pow(InputB[startidx + i], InputA[i]);
                    }
                    break;
                case Operations.subtractSquare:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += XMath.Pow((InputA[i] - InputB[startidx + i]), 2f);
                    }
                    break;

            }
        }





        static void DiffKernel(Index1 index, ArrayView<float> Output, ArrayView<float> Input)
        {
            Output[index] = Input[index + 1] - Input[index];
        }

        static void ReverseKernel(Index1 index, ArrayView<float> IO)
        {
            int idx = IO.Length - 1 - index;
            float temp = IO[idx];

            IO[idx] = IO[index];
            IO[index] = temp;
        }

        static void AbsKernel(Index1 index, ArrayView<float> IO)
        {
            IO[index] = XMath.Abs(IO[index]);
        }

        static void ReciprocalKernel(Index1 index, ArrayView<float> IO)
        {
            IO[index] = XMath.Rcp(IO[index]);
        }

        static void CrossKernel(Index1 index, ArrayView<float> Output, ArrayView<float> InputA, ArrayView<float> InputB)
        {
            Output[index*3]     = InputA[index * 3 + 1] * InputB[index * 3 + 2] - InputA[index * 3 + 2] * InputB[index * 3 + 1];
            Output[index*3 + 1] = InputA[index * 3 + 2] * InputB[index * 3    ] - InputA[index * 3    ] * InputB[index * 3 + 2];
            Output[index*3 + 2] = InputA[index * 3    ] * InputB[index * 3 + 1] - InputA[index * 3 + 1] * InputB[index * 3    ];
        }

        static void TransposeKernel(Index1 index, ArrayView<float> Output, ArrayView<float> Input, int columns)
        {
            int rows = Input.IntLength / columns;
            int col = index % columns;
            int row = (int)XMath.Floor(index / columns);

            int idx = col * rows + row;

            Output[idx] = Input[index];

            // (int)Math.Floor(Input.Length / columns) => The Row
            // (int)(Input.Length % columns) => The Column
            
            //float invcol = 1f / columns;
            //Output[(index % columns) * ((int)(Input.Length * invcol)) + ((int)XMath.Floor(index * invcol))] = Input[index];
        }









        public Action<AcceleratorStream, Index1, ArrayView<float>, SpecializedValue<int>> testkern;
        static void TESTKERNEL(Index1 index, ArrayView<float> IO, SpecializedValue<int> operation)
        {
            switch ((Operations)operation.Value)
            {
                case Operations.cbrt:
                    IO[index] = TestCls.CbrtFast(IO[index]);
                    break;
                case Operations.cbrtrcp:
                    IO[index] = TestCls.CbrtFastRcp(IO[index]);
                    break;
                case Operations.cbrtrcppow:
                    IO[index] = TestCls.CbrtFastRcpANDPow(IO[index]);
                    break;
                case Operations.cbrtinterop:
                    IO[index] = TestCls.CbrtFastInterop(IO[index]);
                    break;
            }
        }



    }

    public enum Operations
    {
        multiply = 0,
        add = 1,
        subtract = 2,
        divide = 3,
        pow = 4,
        invDivide = 5,
        flipSubtract = 6,
        flipPow = 7,
        subtractSquare = 8, // square the difference of two values


        cbrt = 9,
        cbrtrcp = 10,
        cbrtrcppow = 11,
        cbrtinterop = 12,
    }
}
