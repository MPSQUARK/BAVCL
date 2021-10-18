using DataScience;
using System;
using System.Diagnostics;
using DataScience.Geometric;
using ILGPU.Runtime;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {

            GPU gpu = new GPU();
            Random rnd = new Random(522);


            // SAMPLE AND TEST CODE
            int length = (int)1e9;
            int reps = 1;
            float dt = 0.1f;
            float Error = 0.2f;
            float chisquared;
            float time = 0f;

            float[] pertubation = new float[length];
            float[] data = new float[length];

            for (int i = 0; i < length; i++)
            {
                pertubation[i] = (float)rnd.NextDouble();
                data[i] = (float)rnd.NextDouble();
            }


            Vector Data = new Vector(gpu, data, 1);
            Vector Noise = new Vector(gpu, pertubation, 1);

            gpu.ShowMemoryUsage();

            Vector NoiseyData = Data + Noise;

            

            chisquared = (NoiseyData - Data).ConsecutiveOP_IP(1f / Error, Operations.multiplication).ConsecutiveOP_IP(2f, Operations.power).Sum();


            Stopwatch sw = new();
            sw.Start();
            for (int i = 0; i < reps; i++)
            {
                chisquared = (NoiseyData - Data).ConsecutiveOP_IP(1f/Error, Operations.multiplication).ConsecutiveOP_IP(2f, Operations.power).Sum();
            }
            time = sw.ElapsedMilliseconds;
            sw.Reset();
            Console.WriteLine($"Chi squared ans : {chisquared} , time taken : {time} ms , data length : {length} , for reps : {reps}");

            //float[] time = new float[2] { 0f,0f};
            //double result = 0f;
            //double warm = 0f;

            //double cubroot2 = 1.2599210498948731647672106072782283505702514647015079800819751121;
            //int reps = 100_000_000;

            //float[] testvals = new float[reps];
            //for (int i = 0; i < reps; i++)
            //{
            //    testvals[i] = (float)rnd.NextDouble();
            //}
            //testvals[^1] = 2f;




            //// Allocate data to GPU
            //MemoryBuffer<float> buffer2 = gpu.accelerator.Allocate<float>(testvals.Length);
            //MemoryBuffer<double> buffer = gpu.accelerator.Allocate<double>(testvals.Length);
            //buffer2.CopyFrom(testvals, 0, 0, testvals.Length);
            //var cbrtkern = gpu.accelerator.LoadAutoGroupedKernel<Index1, ArrayView<double>, ArrayView<float>>(TestCls.cbrtKernel);

            //cbrtkern(gpu.accelerator.DefaultStream, testvals.Length, buffer.View, buffer2.View);
            //gpu.accelerator.Synchronize();


            ////for (int i = 0; i < reps; i++)
            ////{
            //    cbrtkern(gpu.accelerator.DefaultStream, testvals.Length, buffer.View, buffer2.View);
            //    gpu.accelerator.Synchronize();

            //    //TestCls.cbrtMagic(2);
            ////}


            //sw.Start();
            //cbrtkern(gpu.accelerator.DefaultStream, testvals.Length, buffer.View, buffer2.View);
            //gpu.accelerator.Synchronize();
            ////for (int i = 0; i < reps; i++)
            ////{
            ////    TestCls.cbrtMagic(testvals[i], false);
            ////}
            //float time = sw.ElapsedMilliseconds;
            //Console.WriteLine($"Time taken is : {time} ms Total for {reps} calculations or {time / (reps / 1e6)} ns avg per itteration for method {"cbrt"}");

            //double[] Output = buffer.GetAsArray();
            ////Console.WriteLine($"The result of cbrt -8 is : {TestCls.cbrtMagic(-8,true)}");
            //Console.WriteLine($"The result of cbrt 2 is : {Output[^1]}");


            #region
            //for (int i = 0; i < reps; i++)
            //{
            //    warm = Math.Cbrt(2);
            //}

            //sw.Start();
            //for (int i = 0; i < reps; i++)
            //{
            //    result = Math.Cbrt(testvals[i]);
            //}
            //time[0] = sw.ElapsedMilliseconds;


            //Console.WriteLine($"\n Time taken for cbrt : {time[0]/reps * 1e6: 0.000} ns \n and the result : {result}\n The accuracy (log) is {Math.Log10(Math.Abs(warm-cubroot2)):0.00}\n");



            //for (int i = 0; i < reps; i++)
            //{
            //    warm = TestCls.cbrt(2);
            //}

            //sw.Restart();

            //for (int i = 0; i < reps; i++)
            //{
            //    result = TestCls.cbrt(testvals[i]);
            //}
            //time[1] = sw.ElapsedMilliseconds;
            //Console.WriteLine($" Time taken for cbrt using log10 method : {time[1] / reps * 1e6: 0.000} ns \n and the result : {result}\n The accuracy (log) is {Math.Log10(Math.Abs(warm - cubroot2)):0.00}\n");






            //for (int i = 0; i < reps; i++)
            //{
            //    warm = TestCls.cbrtln(2);
            //}

            //sw.Restart();

            //for (int i = 0; i < reps; i++)
            //{
            //    result = TestCls.cbrtln(testvals[i]);
            //}
            //time[1] = sw.ElapsedMilliseconds;
            //Console.WriteLine($" Time taken for cbrt using log E method : {time[1] / reps * 1e6: 0.000} ns \n and the result : {result}\n The accuracy (log) is {Math.Log10(Math.Abs(warm - cubroot2)):0.00}\n");




            //for (int i = 0; i < reps; i++)
            //{
            //    warm = TestCls.cbrt2(2);
            //}

            //sw.Restart();

            //for (int i = 0; i < reps; i++)
            //{
            //    result = TestCls.cbrt2(testvals[i]);
            //}
            //time[1] = sw.ElapsedMilliseconds;
            //Console.WriteLine($" Time taken for cbrt using log 2 method : {time[1] / reps * 1e6: 0.000} ns \n and the result : {result}\n The accuracy (log) is {Math.Log10(Math.Abs(warm - cubroot2)):0.00}\n");





            //for (int i = 0; i < reps; i++)
            //{
            //    warm = TestCls.cbrtHalley(2);
            //}

            //sw.Restart();

            //for (int i = 0; i < reps; i++)
            //{
            //    result = TestCls.cbrtHalley(testvals[i]);
            //}
            //time[1] = sw.ElapsedMilliseconds;
            //Console.WriteLine($" Time taken for cbrt using Halley opti method : {time[1] / reps * 1e6: 0.000} ns \n and the result : {result}\n The accuracy (raw) is {(Math.Abs(warm - cubroot2)):0.00}\n");



            //for (int i = 0; i < reps; i++)
            //{
            //    warm = TestCls.cbrtHalley(2);
            //}

            //sw.Restart();

            //for (int i = 0; i < reps; i++)
            //{
            //    result = TestCls.cbrtHalley(testvals[i], x:2);
            //}
            //time[1] = sw.ElapsedMilliseconds;
            //Console.WriteLine($" Time taken for cbrt using Halley method : {time[1] / reps * 1e6: 0.000} ns \n and the result : {result}\n The accuracy (raw) is {(Math.Abs(warm - cubroot2)):0.00}\n");








            //for (int i = 0; i < reps; i++)
            //{
            //    warm = Math.Pow(2f,1f/3f);
            //}

            //sw.Restart();

            //for (int i = 0; i < reps; i++)
            //{
            //    result = Math.Pow(testvals[i], 1f / 3f);
            //}
            //time[1] = sw.ElapsedMilliseconds;
            //Console.WriteLine($" Time taken for cbrt using Math.Pow method : {time[1] / reps * 1e6: 0.000} ns \n and the result : {result}\n The accuracy (log) is {Math.Log10(Math.Abs(warm - cubroot2)):0.00}\n");


            //for (int i = 0; i < reps; i++)
            //{
            //    warm = TestCls.cbrtMagic(2f);
            //}

            //sw.Restart();

            //for (int i = 0; i < reps; i++)
            //{
            //    result = TestCls.cbrtMagic(testvals[i]);
            //}
            //time[1] = sw.ElapsedMilliseconds;
            //Console.WriteLine($" Time taken for cbrt using Magic cbrt method : {time[1] / reps * 1e6: 0.000} ns \n and the result : {result}\n The accuracy (raw) is {Math.Abs(warm - cubroot2):0.00}\n");






            //sw.Stop();


            //Console.WriteLine($"\n Percentage improvement = {(1f - (time[1] / time[0])) * 100:0.00} %");

            //Console.WriteLine($"\n The last number was : {testvals[testvals.Length-1]}");


            #endregion
            Console.Read();


            








        }






    }
}
