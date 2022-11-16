using BAVCL;
using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using ILGPU.Algorithms;
using System.Threading.Tasks;
using BAVCL.Geometric;
using System.Collections.Generic;
using ILGPU;
using BAVCL.Experimental;
using ILGPU.Runtime;

namespace Testing_Console
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        //[Params(8, 100)]
        [Params(2, 8, 16, 49, 100, 64, 256, 529, 165518, 5131, 123, 71645, 12.1518f)]
        public float val;

        //public static GPU gpu = new();
        //public static MemoryBuffer1D<float, Stride1D.Dense> input = Vector.Arange(gpu, 1, 1_000_001, 1).GetBuffer();
        //public static int len = (int)input.Length;

        //public static MemoryBuffer1D<float, Stride1D.Dense> output = Vector.Fill(gpu, 0, len).GetBuffer();

        //public static float[] vals = input.GetAsArray1D();
        //public static float[] _output = new float[len];

        //[Benchmark]
        //public void XMathSqrt()
        //{
        //    gpu.TestSQRTKernel(gpu.accelerator.DefaultStream, output.IntExtent, output.View, input.View);
        //    gpu.accelerator.Synchronize();
        //}


        //[Benchmark]
        //public void MyCbrt()
        //{
        //    gpu.TestMYSQRTKernel(gpu.accelerator.DefaultStream, output.IntExtent, output.View, input.View);
        //    gpu.accelerator.Synchronize();
        //}

        //[Benchmark]
        //public void NETLOG2()
        //{
        //    Math.Log2(val);
        //}

        //[Benchmark]
        //public void MYLOG2()
        //{
        //    TestCls.LOG2(val);
        //}

        //[Benchmark]
        //public void MYLOG2_V2()
        //{
        //    TestCls.LOG2_v2(val);
        //}

        //[Benchmark]
        //public void NETLOG10()
        //{
        //    Math.Log10(val);
        //}

        //[Benchmark]
        //public void MYLOG10()
        //{
        //    TestCls.LOG10(val);
        //}


        //[Benchmark]
        //public void NETCBRT()
        //{
        //    Math.Cbrt(val);
        //}

        //[Benchmark]
        //public void MY_CBRT_NEW_V2()
        //{
        //    TestCls.CBRT_v2(val);
        //}

        [Benchmark]
        public void MY_CBRT()
        {
            TestCls.CBRT(val);
        }

        [Benchmark]
        public void MY_CBRT_v2()
        {
            //TestCls.CBRT_v2(val);
        }

        //[Benchmark]
        //public void RUST_CBRT()
        //{
        //    TestCls.CbrtRust(val);
        //}

    }



}
