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
using BAVCL.Core;
using BAVCL.Experimental;
using System.Collections.Generic;

namespace BAVCL
{
	public class GPU
	{
		protected internal Context context;
		public Accelerator accelerator;


		// LRU 
		public ConcurrentDictionary<uint, WeakReference<ICacheable>> CPUrefs = new();  // Size, LiveCount
		public ConcurrentDictionary<uint, MemoryBuffer> GPUbuffers = new(); // Memory Buffer


		protected internal ConcurrentDictionary<uint, bool> ForceKeepFlags = new();
		protected internal ConcurrentQueue<uint> LRU = new();                                                
		
		protected internal uint CurrentVecId = 0;
		protected internal int LiveObjectCount = 0;

		// Device Memory
		public readonly long MaxMemory;
		protected internal long MemoryInUse = 0;
		protected internal float _memoryCap;
		public float Memorycap
		{ 
			get => this._memoryCap;
			set { if (0f < value && value < 1f) { this._memoryCap = value; } else { throw new Exception($"Memory Cap CANNOT be less than 0 or more than 1. Recieved {value}"); } } 
		}

		// Accelerator Preference Order
		Dictionary<AcceleratorType, int> AcceleratorPrefOrder = new()
		{
			{ AcceleratorType.Cuda, 2 },
			{ AcceleratorType.OpenCL, 1 },
			{ AcceleratorType.CPU, 0 }
		};



		// Variables - Kernels
		#region
		// Test KERNELS
		public Action<AcceleratorStream, Index1D, ArrayView<double>, ArrayView<float>> sumKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>> TestSQRTKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>> TestMYSQRTKernel;


		// ACTUAL KERNELS
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int> appendKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, float> nanToNumKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>> getSliceKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>> a_opFKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, float, SpecializedValue<int>> s_opFKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>> vectormatrixOpKernel;

		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, SpecializedValue<int>> a_FloatOPKernelIP;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, float, SpecializedValue<int>> s_FloatOPKernelIP;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>> diffKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>> reverseKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>> absKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>> rcpKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>> rsqrtKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>> crossKernel;
		public Action<AcceleratorStream, Index1D, ArrayView<float>, ArrayView<float>, int> transposekernel;



		public Action<AcceleratorStream, Index1D, ArrayView<float>, float> LogKernel;
		#endregion

		
		// Constructor
		public GPU(float memorycap = 0.8f, bool forceCPU = false)
		{
			// Create Context
			this.context = Context.Create(builder => builder.Default().EnableAlgorithms());
			// OptimizationLevel optimizationLevel = OptimizationLevel.Debug


			// Get Accelerator Device
			//this.accelerator = context.GetPreferredDevice(preferCPU: forceCPU).CreateAccelerator(context);
			this.accelerator = GetPreferedAccelerator(context, forceCPU);
			Console.WriteLine($"Device loaded: {accelerator.Name}");

			// Set Memory Usage Cap
			this.Memorycap = memorycap;
			this.MaxMemory = (long)Math.Round(this.accelerator.MemorySize * this._memoryCap);

			// Load Kernels
			LoadKernels();
			Console.WriteLine("Device Kernels Loaded");
		}

		private Accelerator GetPreferedAccelerator(Context context, bool forceCPU)
        {
			var devices = context.Devices;

			if (devices.Length == 0) throw new Exception("No Accelerators");

			Device preferedAccelerator = null;
			for (int i = 0; i < devices.Length; i++)
			{
				if (forceCPU && devices[i].AcceleratorType == AcceleratorType.CPU)
					return devices[i].CreateAccelerator(context);
				
				if (preferedAccelerator == null)
					preferedAccelerator = devices[i];

				if (AcceleratorPrefOrder.TryGetValue(preferedAccelerator.AcceleratorType, out int Prefpriority))
					if (AcceleratorPrefOrder.TryGetValue(devices[i].AcceleratorType, out int Devicepriority))
                    {
						if (Devicepriority > Prefpriority)
                        {
							preferedAccelerator = devices[i];
							continue;
						}

						if (devices[i].MaxConstantMemory > preferedAccelerator.MaxConstantMemory)
                        {
							preferedAccelerator = devices[i];
							continue;
						}
					}
			}
			return preferedAccelerator.CreateAccelerator(context);
		}

		private void LoadKernels()
		{
			Stopwatch timer = new();
			timer.Start();

			// TEST KERNELS
			sumKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<double>, ArrayView<float>>(SumKernel);
			TestSQRTKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>>(Testsqrtkernel);
			TestMYSQRTKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>>(Testmysqrtkernel);


			appendKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, int>(AppendKernel);
			nanToNumKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float>(Nan_to_numKernel);
			getSliceKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<int>>(AccessSliceKernel);
			
			a_opFKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, SpecializedValue<int>>(A_FloatOPKernel);
			s_opFKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, float, SpecializedValue<int>>(S_FloatOPKernel);
			vectormatrixOpKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>, int, SpecializedValue<int>>(VectorMatrixKernel);

			a_FloatOPKernelIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, SpecializedValue<int>>(A_FloatOPKernelIP);
			s_FloatOPKernelIP = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float, SpecializedValue<int>>(S_FloatOPKernelIP);

			diffKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>> (DiffKernel);
			reverseKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>> (ReverseKernel);
			absKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(AbsKernel);
			rcpKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(ReciprocalKernel);
			rsqrtKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>>(RsqrtKernel);

			crossKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>>(CrossKernel);
			transposekernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, ArrayView<float>, int>(TransposeKernel);


			LogKernel = accelerator.LoadAutoGroupedKernel<Index1D, ArrayView<float>, float>(LogKern);

			timer.Stop();
			Console.WriteLine($"Kernels Loaded in: {timer.Elapsed.TotalMilliseconds} MS");
		}


		#region "Memory Caching & Management"
		private uint GenerateId() => Interlocked.Increment(ref CurrentVecId);
		
		
		//private bool IsInCache(uint id)
		//{
		//    return this.CachedMemory.ContainsKey(id);
		//}
		
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
				if (this.LiveObjectCount == 0) 
				{
					throw new Exception(
						$"GPU states {this.LiveObjectCount} Live Tasks Running, while requiring {required >> 20} MB which is more than available {(this.MaxMemory - this.MemoryInUse) >> 20} MB. Potential cause: memory leak"); 
				}

				// Get the ID of the last item
				uint Id;
				if (!LRU.TryDequeue(out Id)) 
				{
					throw new Exception($"LRU Empty Cannot Continue DeCaching");
				}

				// Try Get Reference to and the object of ICacheable
				WeakReference<ICacheable> referenceCacheable;
				if (!this.CPUrefs.TryGetValue(Id, out referenceCacheable)) 
				{
                    // Object been GC'ed
                    try
                    {
						bool whatthehell = GPUbuffers.TryRemove(Id, out MemoryBuffer Buffer);
						Interlocked.Add(ref MemoryInUse, -Buffer.LengthInBytes);
						Buffer.Dispose();
						Interlocked.Decrement(ref LiveObjectCount);
						continue;
					}
                    catch (Exception)
                    {
						Console.WriteLine("HERE");
                    }
				} 
				ICacheable cacheable;
				if (!referenceCacheable.TryGetTarget(out cacheable)) 
				{
					// Object been GC'ed
					MemoryBuffer Buffer;
					GPUbuffers.TryRemove(Id, out Buffer);
					Interlocked.Add(ref MemoryInUse, -Buffer.LengthInBytes);
					Buffer.Dispose();
					Interlocked.Decrement(ref LiveObjectCount);
					continue;
				} 

				// Try to decache the data
				if (!cacheable.TryDeCache()) { LRU.Enqueue(Id); }

			}

			return;
		}

		private void UpdateMemoryUsage(long size) => Interlocked.Add(ref MemoryInUse, size); 
		
		public uint Allocate<T>(ICacheable Cacheable, T[] values) where T: unmanaged
		{
			DeCacheLRU(Cacheable.MemorySize);
			UpdateMemoryUsage(Cacheable.MemorySize);
			
			AddLiveTask();
			
			WeakReference<ICacheable> CacheableReference = new(Cacheable);
			MemoryBuffer1D<T, Stride1D.Dense> Buffer = this.accelerator.Allocate1D(values);
			
			uint Id = GenerateId();
			CPUrefs.TryAdd(Id, CacheableReference);
			GPUbuffers.TryAdd(Id, Buffer);
			LRU.Enqueue(Id);

			return Id;
		}
		
		
		private void RemoveFromLRU(uint Id)
		{
			if (LRU.IsEmpty || !LRU.Contains(Id)) { return; }

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
			CPUrefs.TryRemove(Id, out _);
			
			// Try to remove memory
			MemoryBuffer Buffer;
			if (GPUbuffers.TryRemove(Id, out Buffer))
			{
				UpdateMemoryUsage(-Buffer.LengthInBytes);
				Buffer.Dispose();
			}

			RemoveFromLRU(Id);
			SubtractLiveTask();
			return 0;
		}



		public void ShowMemoryUsage(bool percentage = true, string format = "F2")
		{
			if (percentage) { Console.WriteLine($"{(((double)this.MemoryInUse / (double)this.MaxMemory) * 100f).ToString(format)}%"); return; }

			Console.WriteLine( $"{this.MemoryInUse >> 20}/{this.MaxMemory >> 20} MB");
		}


		public void AddLiveTask() => Interlocked.Increment(ref LiveObjectCount);
		public void SubtractLiveTask() => Interlocked.Decrement(ref LiveObjectCount);


		#endregion



		// Test Kernels
		static void SumKernel(Index1D index, ArrayView<double> Output, ArrayView<float> Input)
		{
			double sum = 0;
			for (int i = index * 100000; i < (index + 1) * 100000; i++)
			{
				sum += Input[i];
			}
			Output[index] += sum;
		}




		//Kernels
		static void AppendKernel(Index1D index, ArrayView<float> Output, ArrayView<float> vecA, ArrayView<float> vecB, int vecAcol, int vecBcol)
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



		static void Nan_to_numKernel(Index1D index, ArrayView<float> IO, float num)
		{
			if (float.IsNaN(IO[index]) || float.IsInfinity(IO[index]))
			{
				IO[index] = num;
			}

		}

		static void AccessSliceKernel(Index1D index, ArrayView<float> OutPut, ArrayView<float> Input, ArrayView<int> ChangeSelectLength)
		{
			OutPut[index] = Input[
				index * ChangeSelectLength[1] +                         // iRcL
				ChangeSelectLength[0]];                                 // Cs
		}


		static void A_FloatOPKernel(Index1D index, ArrayView<float> OutPut, ArrayView<float> InputA, ArrayView<float> InputB, SpecializedValue<int> operation)
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
				case Operations.flipDivide:
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

		static void A_FloatOPKernelIP(Index1D index, ArrayView<float> IO, ArrayView<float> Input, SpecializedValue<int> operation)
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
				case Operations.flipDivide:
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


		static void S_FloatOPKernel(Index1D index, ArrayView<float> OutPut, ArrayView<float> Input, float Scalar, SpecializedValue<int> operation)
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
				case Operations.flipDivide:
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

		static void S_FloatOPKernelIP(Index1D index, ArrayView<float> IO, float Scalar, SpecializedValue<int> operation)
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
				case Operations.flipDivide:
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


		static void VectorMatrixKernel(Index1D index, ArrayView<float> OutPut, ArrayView<float> InputA, ArrayView<float> InputB, int Cols, SpecializedValue<int> operation)
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
				case Operations.flipDivide:
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





		static void DiffKernel(Index1D index, ArrayView<float> Output, ArrayView<float> Input)
		{
			Output[index] = Input[index + 1] - Input[index];
		}

		static void ReverseKernel(Index1D index, ArrayView<float> IO)
		{
			int idx = IO.IntLength - 1 - index;
			float temp = IO[idx];

			IO[idx] = IO[index];
			IO[index] = temp;
		}

		static void AbsKernel(Index1D index, ArrayView<float> IO)
		{
			IO[index] = XMath.Abs(IO[index]);
		}

		static void ReciprocalKernel(Index1D index, ArrayView<float> IO)
		{
			IO[index] = XMath.Rcp(IO[index]);
		}

		static void RsqrtKernel(Index1D index, ArrayView<float> IO)
		{
			IO[index] = XMath.Rsqrt(IO[index]);
		}



		static void CrossKernel(Index1D index, ArrayView<float> Output, ArrayView<float> InputA, ArrayView<float> InputB)
		{
			Output[index*3]     = InputA[index * 3 + 1] * InputB[index * 3 + 2] - InputA[index * 3 + 2] * InputB[index * 3 + 1];
			Output[index*3 + 1] = InputA[index * 3 + 2] * InputB[index * 3    ] - InputA[index * 3    ] * InputB[index * 3 + 2];
			Output[index*3 + 2] = InputA[index * 3    ] * InputB[index * 3 + 1] - InputA[index * 3 + 1] * InputB[index * 3    ];
		}

		static void TransposeKernel(Index1D index, ArrayView<float> Output, ArrayView<float> Input, int columns)
		{
			int rows = Input.IntLength / columns;
			int col = index % columns;
			int row = (int)XMath.Floor(index / columns);

			int idx = col * rows + row;

			Output[idx] = Input[index];
		}


		static void Testsqrtkernel(Index1D index, ArrayView<float> Output, ArrayView<float> Input)
		{
			Output[index] = XMath.Sqrt(Input[index]);
		}

		static void Testmysqrtkernel(Index1D index, ArrayView<float> Output, ArrayView<float> Input)
		{
			Output[index] = TestCls.sqrt_acc_v2(Input[index]);
		}



		public static void LogKern(Index1D index, ArrayView<float> IO, float @base)
		{
			IO[index] = XMath.Log(IO[index],@base);
		}



	}

	public enum Operations
	{
		multiply = 0,
		add = 1,
		subtract = 2,
		divide = 3,
		pow = 4,
		flipDivide = 5,
		flipSubtract = 6,
		flipPow = 7,
		subtractSquare = 8, // square the difference of two values
	}
}
