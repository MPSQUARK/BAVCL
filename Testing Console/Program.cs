using DataScience;
using DataScience.Core;
using System;
using System.Diagnostics;
using DataScience.Geometric;
using DataScience.Utility;
using System.Linq;
using BenchmarkDotNet.Running;
using ILGPU.Algorithms;
using System.Threading.Tasks;
using DataScience.Experimental;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {

            GPU gpu = new GPU();
            Random rnd = new Random(522);


            BenchmarkRunner.Run<Benchmark>();



            #region "ACCURACY TEST"

            //Benchmark benchmark = new();
            //float actual;

            //actual = 1.2599210498948731647672106072782283505702514647015079800819751121f;
            //benchmark.NN = 2;
            //Console.WriteLine();
            //Console.WriteLine($"N      = {benchmark.NN}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFast()} | Diff : {benchmark.CbrtFast()-actual}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFastInterop()} | Diff : {benchmark.CbrtFastInterop() - actual}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFastInterop2()} | Diff : {benchmark.CbrtFastInterop2() - actual}");
            //Console.WriteLine($"RCP    : {benchmark.CbrtFastRcp()} | Diff : {benchmark.CbrtFastRcp() - actual}");
            //Console.WriteLine($"RCPPOW : {benchmark.CbrtFastRcpANDPow()} | Diff : {benchmark.CbrtFastRcpANDPow() - actual}");
            //Console.WriteLine();


            //actual = 2.1544346900318837217592935665193504952593449421921085824892355063f;
            //benchmark.NN = 10;
            //Console.WriteLine();
            //Console.WriteLine($"N      = {benchmark.NN}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFast()} | Diff : {benchmark.CbrtFast() - actual}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFastInterop()} | Diff : {benchmark.CbrtFastInterop() - actual}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFastInterop2()} | Diff : {benchmark.CbrtFastInterop2() - actual}");
            //Console.WriteLine($"RCP    : {benchmark.CbrtFastRcp()} | Diff : {benchmark.CbrtFastRcp() - actual}");
            //Console.WriteLine($"RCPPOW : {benchmark.CbrtFastRcpANDPow()} | Diff : {benchmark.CbrtFastRcpANDPow() - actual}");
            //Console.WriteLine();

            //actual = 3.4760266448864497867398652190045374340048385387869674214742239567f;
            //benchmark.NN = 42;
            //Console.WriteLine();
            //Console.WriteLine($"N      = {benchmark.NN}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFast()} | Diff : {benchmark.CbrtFast() - actual}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFastInterop()} | Diff : {benchmark.CbrtFastInterop() - actual}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFastInterop2()} | Diff : {benchmark.CbrtFastInterop2() - actual}");
            //Console.WriteLine($"RCP    : {benchmark.CbrtFastRcp()} | Diff : {benchmark.CbrtFastRcp() - actual}");
            //Console.WriteLine($"RCPPOW : {benchmark.CbrtFastRcpANDPow()} | Diff : {benchmark.CbrtFastRcpANDPow() - actual}");
            //Console.WriteLine();

            //actual = 3.000000000000000000000000000000000000000000000000000000000000000f;
            //benchmark.NN = 27;
            //Console.WriteLine();
            //Console.WriteLine($"N      = {benchmark.NN}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFast()} | Diff : {benchmark.CbrtFast() - actual}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFastInterop()} | Diff : {benchmark.CbrtFastInterop() - actual}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFastInterop2()} | Diff : {benchmark.CbrtFastInterop2() - actual}");
            //Console.WriteLine($"RCP    : {benchmark.CbrtFastRcp()} | Diff : {benchmark.CbrtFastRcp() - actual}");
            //Console.WriteLine($"RCPPOW : {benchmark.CbrtFastRcpANDPow()} | Diff : {benchmark.CbrtFastRcpANDPow() - actual}");
            //Console.WriteLine();

            //actual = 10.000000000000000000000000000000000000000000000000000000000000000f;
            //benchmark.NN = 1000;
            //Console.WriteLine();
            //Console.WriteLine($"N      = {benchmark.NN}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFast()} | Diff : {benchmark.CbrtFast() - actual}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFastInterop()} | Diff : {benchmark.CbrtFastInterop() - actual}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFastInterop2()} | Diff : {benchmark.CbrtFastInterop2() - actual}");
            //Console.WriteLine($"RCP    : {benchmark.CbrtFastRcp()} | Diff : {benchmark.CbrtFastRcp() - actual}");
            //Console.WriteLine($"RCPPOW : {benchmark.CbrtFastRcpANDPow()} | Diff : {benchmark.CbrtFastRcpANDPow() - actual}");
            //Console.WriteLine();



            //actual = 23.624359603032442804532367300266155060748262730647780394539011487f;
            //benchmark.NN = 13185;
            //Console.WriteLine();
            //Console.WriteLine($"N      = {benchmark.NN}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFast()} | Diff : {benchmark.CbrtFast() - actual}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFastInterop()} | Diff : {benchmark.CbrtFastInterop() - actual}");
            //Console.WriteLine($"Orig   : {benchmark.CbrtFastInterop2()} | Diff : {benchmark.CbrtFastInterop2() - actual}");
            //Console.WriteLine($"RCP    : {benchmark.CbrtFastRcp()} | Diff : {benchmark.CbrtFastRcp() - actual}");
            //Console.WriteLine($"RCPPOW : {benchmark.CbrtFastRcpANDPow()} | Diff : {benchmark.CbrtFastRcpANDPow() - actual}");
            //Console.WriteLine();

            #endregion


        }






    }
}
