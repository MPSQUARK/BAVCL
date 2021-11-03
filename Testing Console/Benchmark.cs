using DataScience;
using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using ILGPU.Algorithms;
using System.Threading.Tasks;
using DataScience.Geometric;
using System.Collections.Generic;
using ILGPU;

namespace Testing_Console
{
    [MemoryDiagnoser]
    public class Benchmark
    {

        #region "GPU TEST"

        //public static GPU gpu = new GPU();
        //public static Vector vector = Vector.Fill(gpu, 2f, 1000000, 1);


        //[Benchmark]
        //public void cbrt()
        //{
        //    vector.TestCBRT(Operations.cbrt);
        //}

        //[Benchmark]
        //public void cbrtrcp()
        //{
        //    vector.TestCBRT(Operations.cbrtrcp);
        //}

        //[Benchmark]
        //public void cbrtrcppow()
        //{
        //    vector.TestCBRT(Operations.cbrtrcppow);
        //}

        //[Benchmark]
        //public void cbrtInterop()
        //{
        //    vector.TestCBRT(Operations.cbrtinterop);
        //}

        #endregion

        #region "CPU TESTING"

        //[Params(2,10,42,27)] - NOT NESSESSARY FOR PERFORMANCE TEST


        //public double NN = 2;
        //public bool checksafety = true;


        //[Benchmark]
        //public unsafe float CbrtFast()
        //{
        //    double N = NN;

        //    float n = (float)N;

        //    // Perform "Safety Check" for; -n, 0, +Inf, -Inf, NaN
        //    if (checksafety)
        //    {
        //        //if (n < 0) { return -CbrtFast(-n); }
        //        if (n == 0 || double.IsNaN(n) || double.IsInfinity(n)) { return n; }
        //    }

        //    // Initial approximation
        //    // Convert the binary representation of float into a positive int
        //    // Isolate & Convert mantissa into actual power it represents
        //    // Perform cube root on 2^P, to appoximate x using power law
        //    float x = 1 << (int)((((*(uint*)&N) >> 53) - 1023) * 0.33333333333333333333f) + 0b1;

        //    // Perform check if x^3 matches n
        //    float xcubed = x * x * x;
        //    if (xcubed == n) { return x; }

        //    // Perform 4 itterations of Halley algorithm for double accuracy
        //    float xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    return x;
        //}

        //[Benchmark]
        //public float CbrtFastInterop()
        //{
        //    double N = NN;

        //    float n = (float)N;

        //    // Perform "Safety Check" for; -n, 0, +Inf, -Inf, NaN
        //    if (checksafety)
        //    {
        //        //if (n < 0) { return -CbrtFastInterop(-n); }
        //        if (n == 0 || double.IsNaN(n) || double.IsInfinity(n)) { return n; }
        //    }

        //    // Initial approximation
        //    // Convert the binary representation of float into a positive int
        //    // Isolate & Convert mantissa into actual power it represents
        //    // Perform cube root on 2^P, to appoximate x using power law

        //    float x = 1 << ((int)(((Interop.FloatAsInt(N) >> 53) - 1023) * 0.33333333333333333333f) + 0b1);

        //    // Perform check if x^3 matches n
        //    float xcubed = x * x * x;
        //    if (xcubed == n) { return x; }

        //    // Perform 4 itterations of Halley algorithm for double accuracy
        //    float xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    return x;
        //}

        //[Benchmark]
        //public unsafe float CbrtFastRcp()
        //{
        //    double N = NN;

        //    float n = (float)N;

        //    // Perform "Safety Check" for; -n, 0, +Inf, -Inf, NaN
        //    if (checksafety)
        //    {
        //        //if (n < 0) { return -CbrtFast(-n); }
        //        if (n == 0 || double.IsNaN(n) || double.IsInfinity(n)) { return n; }
        //    }

        //    // Initial approximation
        //    // Convert the binary representation of float into a positive int
        //    // Isolate & Convert mantissa into actual power it represents
        //    // Perform cube root on 2^P, to appoximate x using power law
        //    float x = 1 << (int)((((*(uint*)&N) >> 53) - 1023) * 0.33333333333333333333f) + 0b1;

        //    // Perform check if x^3 matches n
        //    float xcubed = x * x * x;
        //    if (xcubed == n) { return x; }

        //    // Perform 4 itterations of Halley algorithm for double accuracy
        //    float xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

        //    return x;
        //}

        //[Benchmark]
        //public unsafe float CbrtFastRcpANDPow()
        //{
        //    double N = NN;

        //    float n = (float)N;

        //    // Perform "Safety Check" for; -n, 0, +Inf, -Inf, NaN
        //    if (checksafety)
        //    {
        //        //if (n < 0) { return -CbrtFast(-n); }
        //        if (n == 0 || double.IsNaN(n) || double.IsInfinity(n)) { return n; }
        //    }

        //    // Initial approximation
        //    // Convert the binary representation of float into a positive int
        //    // Isolate & Convert mantissa into actual power it represents
        //    // Perform cube root on 2^P, to appoximate x using power law
        //    float x = 1 << (int)((((*(uint*)&N) >> 53) - 1023) * 0.33333333333333333333f) + 0b1;

        //    // Perform check if x^3 matches n

        //    float xcubed = XMath.Pow(x, 3);
        //    if (xcubed == n) { return x; }

        //    // Perform 4 itterations of Halley algorithm for double accuracy
        //    float xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

        //    xcubed = XMath.Pow(x, 3);
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

        //    xcubed = XMath.Pow(x, 3);
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

        //    xcubed = XMath.Pow(x, 3);
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

        //    xcubed = XMath.Pow(x, 3);
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

        //    xcubed = XMath.Pow(x, 3);
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

        //    return x;
        //}

        #endregion




        #region "Algo Testing"

        //[Benchmark]
        //public float CbrtFastInterop2()
        //{
        //    double N = NN;

        //    float n = (float)N;

        //    // Perform "Safety Check" for; -n, 0, +Inf, -Inf, NaN
        //    if (checksafety)
        //    {
        //        //if (n < 0) { return -CbrtFastInterop2(-n); }
        //        if (n == 0 || double.IsNaN(n) || double.IsInfinity(n)) { return n; }
        //    }

        //    // Initial approximation
        //    // Convert the binary representation of float into a positive int
        //    // Isolate & Convert mantissa into actual power it represents
        //    // Perform cube root on 2^P, to appoximate x using power law

        //    float x = 1 << (int)(((Interop.FloatAsInt(N) >> 53) - 1023) * 0.33333333333333333333f) + 0b1;

        //    // Perform check if x^3 matches n
        //    float xcubed = x * x * x;
        //    if (xcubed == n) { return x; }

        //    // Perform 4 itterations of Halley algorithm for double accuracy
        //    float xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    xcubed = x * x * x;
        //    xcubedPlusN = xcubed + n;
        //    x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

        //    return x;
        //}

        #endregion

    }



}
