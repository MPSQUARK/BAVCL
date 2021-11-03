using System;
using ILGPU;
using ILGPU.Algorithms;

namespace DataScience.Experimental
{
    public class TestCls
    {


        public unsafe static float CbrtFast(double N, bool checksafety = true)
        {

            float n = (float)N;

            // Perform "Safety Check" for; -n, 0, +Inf, -Inf, NaN
            if (checksafety)
            {
                if (n < 0) { return -CbrtFast(-n); }
                if (n == 0 || double.IsNaN(n) || double.IsInfinity(n)) { return n; }
            }

            // Initial approximation
            // Convert the binary representation of float into a positive int
            // Isolate & Convert mantissa into actual power it represents
            // Perform cube root on 2^P, to appoximate x using power law
            float x = 1 << (int)((((*(uint*)&N) >> 53) - 1023) * 0.33333333333333333333f) + 0b1;

            // Perform check if x^3 matches n
            float xcubed = x * x * x;
            if (xcubed == n) { return x; }

            // Perform 4 itterations of Halley algorithm for double accuracy
            float xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            return x;
        }


        public static float CbrtFastInterop(double N, bool checksafety = true)
        {

            float n = (float)N;

            // Perform "Safety Check" for; -n, 0, +Inf, -Inf, NaN
            if (checksafety)
            {
                if (n < 0) { return -CbrtFastInterop(-n); }
                if (n == 0 || double.IsNaN(n) || double.IsInfinity(n)) { return n; }
            }

            // Initial approximation
            // Convert the binary representation of float into a positive int
            // Isolate & Convert mantissa into actual power it represents
            // Perform cube root on 2^P, to appoximate x using power law

            float x = 1 << ((int)(((Interop.FloatAsInt(N) >> 53) - 1023) * 0.33333333333333333333f) + 0b1);

            // Perform check if x^3 matches n
            float xcubed = x * x * x;
            if (xcubed == n) { return x; }

            // Perform 4 itterations of Halley algorithm for double accuracy
            float xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            return x;
        }

        public static float CbrtFastInterop2(double N, bool checksafety = true)
        {

            float n = (float)N;

            // Perform "Safety Check" for; -n, 0, +Inf, -Inf, NaN
            if (checksafety)
            {
                if (n < 0) { return -CbrtFastInterop2(-n); }
                if (n == 0 || double.IsNaN(n) || double.IsInfinity(n)) { return n; }
            }

            // Initial approximation
            // Convert the binary representation of float into a positive int
            // Isolate & Convert mantissa into actual power it represents
            // Perform cube root on 2^P, to appoximate x using power law

            float x = 1 << (int)(((Interop.FloatAsInt(N) >> 53) - 1023) * 0.33333333333333333333f) + 0b1;

            // Perform check if x^3 matches n
            float xcubed = x * x * x;
            if (xcubed == n) { return x; }

            // Perform 4 itterations of Halley algorithm for double accuracy
            float xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));

            return x;
        }




        public unsafe static float CbrtFastRcp(double N, bool checksafety = true)
        {

            float n = (float)N;

            // Perform "Safety Check" for; -n, 0, +Inf, -Inf, NaN
            if (checksafety)
            {
                if (n < 0) { return -CbrtFastRcp(-n); }
                if (n == 0 || double.IsNaN(n) || double.IsInfinity(n)) { return n; }
            }

            // Initial approximation
            // Convert the binary representation of float into a positive int
            // Isolate & Convert mantissa into actual power it represents
            // Perform cube root on 2^P, to appoximate x using power law
            float x = 1 << (int)((((*(uint*)&N) >> 53) - 1023) * 0.33333333333333333333f) + 0b1;

            // Perform check if x^3 matches n
            float xcubed = x * x * x;
            if (xcubed == n) { return x; }

            // Perform 4 itterations of Halley algorithm for double accuracy
            float xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

            xcubed = x * x * x;
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

            return x;
        }

        public unsafe static float CbrtFastRcpANDPow(double N, bool checksafety = true)
        {

            float n = (float)N;

            // Perform "Safety Check" for; -n, 0, +Inf, -Inf, NaN
            if (checksafety)
            {
                if (n < 0) { return -CbrtFastRcpANDPow(-n); }
                if (n == 0 || double.IsNaN(n) || double.IsInfinity(n)) { return n; }
            }

            // Initial approximation
            // Convert the binary representation of float into a positive int
            // Isolate & Convert mantissa into actual power it represents
            // Perform cube root on 2^P, to appoximate x using power law
            float x = 1 << (int)((((*(uint*)&N) >> 53) - 1023) * 0.33333333333333333333f) + 0b1;

            // Perform check if x^3 matches n

            float xcubed = XMath.Pow(x, 3);
            if (xcubed == n) { return x; }

            // Perform 4 itterations of Halley algorithm for double accuracy
            float xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

            xcubed = XMath.Pow(x, 3);
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

            xcubed = XMath.Pow(x, 3);
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

            xcubed = XMath.Pow(x, 3);
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

            xcubed = XMath.Pow(x, 3);
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

            xcubed = XMath.Pow(x, 3);
            xcubedPlusN = xcubed + n;
            x = x * ((xcubedPlusN + n) * XMath.Rcp(xcubed + xcubedPlusN));

            return x;
        }



        //public static double cbrtHalley(double n, double x, double e = 1e-6)
        //{
        //    double xcubed = x * x * x;
        //    double xcubedPlusN;

        //    while (Math.Abs(xcubed - n) > e)
        //    {
        //        xcubedPlusN = xcubed + n;
        //        x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));
        //        xcubed = x * x * x;
        //    }

        //    return x;

        //}

        //// https://rosettacode.org/wiki/Nth_root#C.23
        //public static double NthRoot(double A, int n, double p)
        //{
        //    double _n = (double)n;
        //    double[] x = new double[2];
        //    x[0] = A;
        //    x[1] = A / _n;
        //    while (Math.Abs(x[0] - x[1]) > p)
        //    {
        //        x[1] = x[0];
        //        x[0] = (1 / _n) * (((_n - 1) * x[1]) + (A / Math.Pow(x[1], _n - 1)));

        //    }
        //    return x[0];
        //}


        ///// <summary>
        ///// Calculates the cube root of a number.
        ///// </summary>
        ///// <param name="n">The number whose cube root you wish to find.</param>
        ///// <param name="checksafety">Enables checks for edge cases: 
        ///// 0, NaN, Inf and negative values of n. 
        ///// Disable to gain minor speed increase at your own risk.</param>
        ///// <param name="itter">Determines the number of itterations performed 
        ///// using Halley's algorithm, default : 3 should provide double precision
        ///// level of accuracy. A value of 2 will approximately give the result to float precision
        ///// level of accuracy. Higher number increases accuracy and time nessessary 
        ///// for computation. Lower values will be less accurate but faster.</param>
        ///// <returns>The cube root of the value n as a double.</returns>
        //public unsafe static double cbrtMagic(double n, bool checksafety = true, int itter = 3)
        //{
        //    // Edge cases
        //    //if (checksafety)
        //    //{
        //    //    if (n == 0) { return 0; }
        //    //    if (n < 0) { return -cbrtMagic(-n); }
        //    //    if (float.IsNaN(n) || float.IsInfinity(n))
        //    //    { throw new Exception("Cannot perform cube root on NaN or Inf values"); }
        //    //}

        //    // Initial approximation
        //    uint N = *(uint*)&n;         // Convert the binary representation of float into a positive int
        //    N >>= 23;                         // Isolate the mantissa 
        //    N -= 127;                         // Convert mantissa into actual power it represents
        //    double x = 1 << (int)(N / 3);     // Perform cube root on 2^P, to appoximate x

        //    // Perform 3 itterations of Halley algorithm for double accuracy
        //    // 2 itterations for approximately float accuracy
        //    double xcubed = x * x * x;
        //    if (xcubed == n) { return x; }

        //    double xcubedPlusN;

        //    for (int i = 0; i < itter; i++)
        //    {
        //        xcubedPlusN = xcubed + n;
        //        x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));
        //        xcubed = x * x * x;
        //    }

        //    return x;
        //}


        //public unsafe static float cbrtfloat(float n, bool checksafety = true, int itter = 3)
        //{
        //    // Edge cases
        //    //if (checksafety)
        //    //{
        //    //    if (n == 0) { return 0; }
        //    //    if (n < 0) { return -cbrtMagic(-n); }
        //    //    if (float.IsNaN(n) || float.IsInfinity(n))
        //    //    { throw new Exception("Cannot perform cube root on NaN or Inf values"); }
        //    //}

        //    // Initial approximation
        //    uint N = *(uint*)&n;         // Convert the binary representation of float into a positive int
        //    N >>= 23;                         // Isolate the mantissa 
        //    N -= 127;                         // Convert mantissa into actual power it represents
        //    float x = 1 << (int)(N / 3);     // Perform cube root on 2^P, to appoximate x

        //    // Perform 3 itterations of Halley algorithm for double accuracy
        //    // 2 itterations for approximately float accuracy
        //    float xcubed = x * x * x;
        //    if (xcubed == n) { return x; }

        //    float xcubedPlusN;

        //    for (int i = 0; i < itter; i++)
        //    {
        //        xcubedPlusN = xcubed + n;
        //        x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));
        //        xcubed = x * x * x;
        //    }

        //    return x;
        //}


        //public static void cbrtKernel(Index1 index, ArrayView<double> OutPut, ArrayView<float> Input)
        //{
        //    OutPut[index] = cbrtfloat(Input[index]);
        //}









    }
}
