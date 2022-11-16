using System;
using BenchmarkDotNet.Running;
using BAVCL;
using BAVCL.Core;
using BAVCL.Geometric;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Testing_Console;
using BAVCL.Experimental;


GPU gpu = new();
Vector vec = Vector.Arange(gpu,0f, 20000000f, 0.5f,5);

vec.Print();


Console.WriteLine(TestCls.Sqrt(311.3f));
Console.ReadLine();




//double[] arr = new double[2000000];
//Random rnd = new Random();

//for (int i = 0; i < arr.Length; i+=2)
//{
//    arr[i] = rnd.Next(2,(int)1e8) + rnd.NextDouble();
//    arr[i+1] = Math.Cbrt(arr[i]);
//}

//double[] acc = new double[8];

//for (int i = 0; i < arr.Length; i+=2)
//{
//    acc[1] += Math.Abs(TestCls.CBRT_FAST(arr[i]) - arr[i + 1]);
//    acc[3] += Math.Abs(TestCls.CBRT_v2(arr[i]) - arr[i + 1]);
//    acc[5] += Math.Abs(TestCls.CbrtRust(arr[i]) - arr[i + 1]);
//    acc[7] += Math.Abs(Math.Cbrt(arr[i]) - arr[i + 1]);
//}
//Console.WriteLine();
//for (int i = 0; i < acc.Length; i++)
//{
//    Console.Write($"{acc[i]}, ");

//}

//Console.WriteLine($"CBRT v3:  {TestCls.CBRT_v3(124512)}");
//Console.WriteLine($"CBRT_FAST {TestCls.CBRT_FAST(124512)}");
//Console.WriteLine($"CbrtFast  {TestCls.CbrtFast(124512)}");
//Console.WriteLine($"CBRT_v2   {TestCls.CBRT_v2(124512)}");
//Console.WriteLine($"CBRT      {TestCls.CBRT(124512)}");
//Console.WriteLine($"CbrtRust  {TestCls.CbrtRust(124512)}");
//Console.WriteLine($"Math      {Math.Cbrt(124512)}");
//Console.WriteLine($"Actual    {49.934848475784521335244675506973788632625373546702390337469711516}");

//TestCls.TSTCODE();

//TestCls.LOG2(7f).Print(12);
//TestCls.LOG2_v2(7f).Print(12);
//Math.Log2(7f).Print(12);

BenchmarkRunner.Run<Benchmark>();



Console.ReadLine();

//GPU gpu = new();

//float[] vector = Vector.Arange(1f, 500000f, 0.1f);

//float[] _math = new float[vector.Length];
//float[] _cbrt_new = new float[vector.Length];
//float[] _cbrt_original = new float[vector.Length];


//for (int i = 0; i < vector.Length; i++)
//{
//    _math[i] = MathF.Cbrt(vector[i]);
//    _cbrt_new[i] = TestCls.CBRT(vector[i]);
//    _cbrt_original[i] = TestCls.CbrtFast(vector[i]);
//}

//Vector vecMath = new(gpu, _math, 10);
//Vector vecNEW = new(gpu, _cbrt_new, 10);
//Vector vecOrig = new(gpu, _cbrt_original, 10);


//Vector newError = vecMath - vecNEW;
//Vector origError = vecMath - vecOrig;


//Console.WriteLine("Original METHOD:");
//Console.WriteLine($"MIN: {origError.Min()}");
//Console.WriteLine($"MAX: {origError.Max()}");
//Console.WriteLine($"AVG: {origError.Mean()}");

//Console.WriteLine("NEW METHOD:");
//Console.WriteLine($"MIN: {newError.Min()}");
//Console.WriteLine($"MAX: {newError.Max()}");
//Console.WriteLine($"AVG: {newError.Mean()}");


//float value = 58135183131f;
//Console.WriteLine();
//Console.WriteLine(MathF.Cbrt(value));
//Console.WriteLine(Math.Cbrt(value));
//Console.WriteLine(TestCls.CBRT(value));