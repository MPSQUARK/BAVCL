using ILGPU;
using ILGPU.Algorithms;
using ILGPU.IR.Transformations;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DataScience.Core;


namespace DataScience
{
    public class GPU
    {
        Context context;
        public Accelerator accelerator;


        public struct GPUData
        {
            public MemoryBuffer buffer;
            public long size;
        }
        // LRU 
        public ConcurrentDictionary<uint, GPUData> GData = new ConcurrentDictionary<uint, GPUData>();
        internal protected ConcurrentDictionary<uint, bool> ForceKeepFlags = new ConcurrentDictionary<uint, bool>();
        private ConcurrentQueue<uint> LRU = new ConcurrentQueue<uint>();                                       // GPU-side memory caching                                                     
        private uint CurrentVecId = 0;


        // Device Memory
        public readonly long MaxMemory;
        private long MemoryInUse = 0;
        private float MemoryCap;
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
            return this.GData.ContainsKey(id);
        }

        public void DeCacheLRU(long required, bool HasFlags=true)
        {
            // Check if the memory required doesn't exceed the Maximum available
            if (required > this.MaxMemory)
            {
                throw new Exception($"Cannot cache this data onto the GPU, required memory : {required / (1024 * 1024)} MB, max memory available : {this.MaxMemory / (1024 * 1024)} MB.\n " +
                                    $"Consider spliting/breaking the data into multiple smaller sets OR \n Caching to a GPU with more available memory.");
            }

            // Keep decaching untill enough space is made to accomodate the data
            while (required > (this.MaxMemory - this.MemoryInUse))
            {

                if (LRU.Count == ForceKeepFlags.Keys.Count)
                {
                    throw new Exception($"Cannot DeCache any more data, keep flags : {ForceKeepFlags.Keys.Count}, LRU entries : {LRU.Count}");
                }

                // Get the ID of the last item
                uint Id;
                LRU.TryDequeue(out Id); // Returns bool - could be useful for something

                // If the Id is Flagged - Do not dispose and re-enqueue
                if (ForceKeepFlags.ContainsKey(Id))
                {
                    LRU.Enqueue(Id);
                    continue;
                } 

                // Get the Buffer corresponding to the ID
                GPUData data;
                if (GData.TryRemove(Id, out data))
                {
                    // Dispose of the Buffer
                    data.buffer.Dispose();

                    // Reduce the Memory in Use tracker by the size
                    Interlocked.Add(ref MemoryInUse, -data.size);
                }
            }
            return;
        }
        public void DeCacheLRU(long required)
        {
            // Check if the memory required doesn't exceed the Maximum available
            if (required > this.MaxMemory)
            {
                throw new Exception($"Cannot cache this data onto the GPU, required memory : {required / (1024 * 1024)} MB, max memory available : {this.MaxMemory / (1024 * 1024)} MB.\n " +
                                    $"Consider spliting/breaking the data into multiple smaller sets OR \n Caching to a GPU with more available memory.");
            }


            // Get the ID of the last item
            uint Id;

            // Keep decaching untill enough space is made to accomodate the data
            while (required > (this.MaxMemory - this.MemoryInUse))
            {

                LRU.TryDequeue(out Id); // Returns bool - could be useful for something

                // Get the Buffer corresponding to the ID
                GPUData data;
                if (GData.TryRemove(Id, out data))
                {
                    // Dispose of the Buffer
                    data.buffer.Dispose();

                    // Reduce the Memory in Use tracker by the size
                    Interlocked.Add(ref MemoryInUse, -data.size);
                }
            }
            return;
        }

        public uint Cache(ICacheable cacheable)
        {
            // Calculates how much memory needed
            long size = cacheable.MemorySize();

            // check if the amount required is available
            // decache last and check again untill enough space is made
            DeCacheLRU(size);

            // Allocate data to GPU
            MemoryBuffer buffer = cacheable.Allocate();

            // Increase the Memory in Use
            Interlocked.Add(ref MemoryInUse, size);

            GPUData data = new GPUData { buffer = buffer, size = size};

            // generate Id for data so it can be found by the vector
            uint Id = GenerateId();
            while (!GData.TryAdd(Id, data))
            {
                Id = GenerateId();
            }

            // Put the Id in a queue
            LRU.Enqueue(Id);

            return Id;
        }

        public void DeCache(uint Id)
        {
            // Get the Memory Buffer
            GPUData data;
            if (GData.TryRemove(Id, out data))
            {
                // Dispose of buffer
                data.buffer.Dispose();
                // Reduces the Memory in use tracker by the size
                Interlocked.Add(ref MemoryInUse, -data.size);
            }

            // If the Vector being disposed is the last Vector, then don't shuffle
            uint DequeuedId;
            LRU.TryDequeue(out DequeuedId);

            if (DequeuedId == Id)
            {
                return;
            }

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
            
        }
        public void ShowMemoryUsage(bool percentage = true)
        {
            if (percentage) { Console.WriteLine($"{((double)this.MemoryInUse / (double)this.MaxMemory) * 100f:0.00}%"); return; }

            Console.WriteLine( $"{this.MemoryInUse / (1024 * 1024)}/{this.MaxMemory / (1024 * 1024)} MB");
        }
        public MemoryBuffer GetMemoryBuffer(ICacheable cacheable)
        {
            GPUData data;
            bool inputexists = this.GData.TryGetValue(cacheable.Id, out data);

            if (!inputexists)
            {
                uint Id = this.Cache(cacheable);
                this.GData.TryGetValue(Id, out data);
            }
            return data.buffer;
        }

        public void AddFlags(uint Id)
        {
            ForceKeepFlags.TryAdd(Id, true);
        }
        public void AddFlags(uint[] Ids)
        {
            for (int i = 0; i < Ids.Length; i++)
            {
                ForceKeepFlags.TryAdd(Ids[i], true);
            }
        }
        public void RemoveFlags(uint Id)
        {
            ForceKeepFlags.TryRemove(Id, out _);
        }
        public void RemoveFlags(uint[] Ids)
        {
            for (int i = 0; i < Ids.Length; i++)
            {
                ForceKeepFlags.TryRemove(Ids[i], out _);
            }
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
                case Operations.multiplication:
                    OutPut[index] = InputA[index] * InputB[index];
                    break;
                case Operations.addition:
                    OutPut[index] = InputA[index] + InputB[index];
                    break;
                case Operations.subtraction:
                    OutPut[index] = InputA[index] - InputB[index];
                    break;
                case Operations.flipSubtraction:
                    OutPut[index] = InputB[index] - InputA[index];
                    break;
                case Operations.division:
                    OutPut[index] = InputA[index] / InputB[index];
                    break;
                case Operations.inverseDivision:
                    OutPut[index] = InputB[index] / InputA[index];
                    break;
                case Operations.power:
                    OutPut[index] = XMath.Pow(InputA[index], InputB[index]);
                    break;
                case Operations.powerFlipped:
                    OutPut[index] = XMath.Pow(InputB[index], InputA[index]);
                    break;
                case Operations.squareOfDiffs:
                    OutPut[index] = XMath.Pow((InputA[index] - InputB[index]), 2f);
                    break;

            }
        }

        static void ConsecutiveOperationKernelIP(Index1 index, ArrayView<float> IO, ArrayView<float> Input, SpecializedValue<int> operation)
        {
            switch ((Operations)operation.Value)
            {
                case Operations.multiplication:
                    IO[index] = IO[index] * Input[index];
                    break;
                case Operations.addition:
                    IO[index] = IO[index] + Input[index];
                    break;
                case Operations.subtraction:
                    IO[index] = IO[index] - Input[index];
                    break;
                case Operations.flipSubtraction:
                    IO[index] = IO[index] - Input[index];
                    break;
                case Operations.division:
                    IO[index] = IO[index] / Input[index];
                    break;
                case Operations.inverseDivision:
                    IO[index] = IO[index] / Input[index];
                    break;
                case Operations.power:
                    IO[index] = XMath.Pow(IO[index], Input[index]);
                    break;
                case Operations.powerFlipped:
                    IO[index] = XMath.Pow(IO[index], Input[index]);
                    break;
                case Operations.squareOfDiffs:
                    IO[index] = XMath.Pow((IO[index] - Input[index]), 2f);
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
                case Operations.flipSubtraction:
                    OutPut[index] = Scalar - Input[index];
                    break;
                case Operations.division:
                    OutPut[index] = Input[index] / Scalar;
                    break;
                case Operations.inverseDivision:
                    OutPut[index] = Scalar / Input[index];
                    break;
                case Operations.power:
                    OutPut[index] = XMath.Pow(Input[index], Scalar);
                    break;
                case Operations.powerFlipped:
                    OutPut[index] = XMath.Pow(Scalar, Input[index]);
                    break;
                case Operations.squareOfDiffs:
                    OutPut[index] = XMath.Pow((Input[index] - Scalar), 2f);
                    break;
            }
        }

        static void ScalarConsecutiveOperationKernelIP(Index1 index, ArrayView<float> IO, float Scalar, SpecializedValue<int> operation)
        {
            switch ((Operations)operation.Value)
            {
                case Operations.multiplication:
                    IO[index] = IO[index] * Scalar;
                    break;
                case Operations.addition:
                    IO[index] = IO[index] + Scalar;
                    break;
                case Operations.subtraction:
                    IO[index] = IO[index] - Scalar;
                    break;
                case Operations.flipSubtraction:
                    IO[index] = Scalar - IO[index];
                    break;
                case Operations.division:
                    IO[index] = IO[index] / Scalar;
                    break;
                case Operations.inverseDivision:
                    IO[index] = Scalar / IO[index];
                    break;
                case Operations.power:
                    IO[index] = XMath.Pow(IO[index], Scalar);
                    break;
                case Operations.powerFlipped:
                    IO[index] = XMath.Pow(Scalar, IO[index]);
                    break;
                case Operations.squareOfDiffs:
                    IO[index] = XMath.Pow((IO[index] - Scalar), 2f);
                    break;
            }
        }


        static void VectorMatrixKernel(Index1 index, ArrayView<float> OutPut, ArrayView<float> InputA, ArrayView<float> InputB, int Cols, SpecializedValue<int> operation)
        {
            int startidx = index * Cols;

            switch ((Operations)operation.Value)
            {
                case Operations.multiplication:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += InputA[i] * InputB[startidx + i];
                    }
                    break;
                case Operations.addition:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += InputA[i] + InputB[startidx + i];
                    }
                    break;
                case Operations.subtraction:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += InputA[i] - InputB[startidx + i];
                    }
                    break;
                case Operations.flipSubtraction:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += InputB[startidx + i] - InputA[i];
                    }
                    break;
                case Operations.division:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += InputA[i] / InputB[startidx + i];
                    }
                    break;
                case Operations.inverseDivision:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += InputB[startidx + i] / InputA[i];
                    }
                    break;
                case Operations.power:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += XMath.Pow(InputA[i], InputB[startidx + i]);
                    }
                    break;
                case Operations.powerFlipped:
                    for (int i = 0; i < Cols; i++)
                    {
                        OutPut[index] += XMath.Pow(InputB[startidx + i], InputA[i]);
                    }
                    break;
                case Operations.squareOfDiffs:
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












    }

    public enum Operations
    {
        multiplication = 0,
        addition = 1,
        subtraction = 2,
        division = 3,
        power = 4,
        inverseDivision = 5,
        flipSubtraction = 6,
        powerFlipped = 7,
        squareOfDiffs = 8,
    }
}
