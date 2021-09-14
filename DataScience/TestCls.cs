using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using ILGPU.Algorithms;
using ILGPU;
using System.Runtime.CompilerServices;

namespace DataScience
{
    public class TestCls
    {

        private const double third = 1 / 3f; 


        public static double cbrt(double x)
        {

            double log = Math.Log10(x) * third;

            return Math.Pow(10, log);

        }


        public static double cbrtln(double x)
        {

            double log = Math.Log(x) * third;

            return Math.Pow(MathF.E, log);

        }

        public static double cbrt2(double x)
        {

            double log = Math.Log2(x) * third;

            return Math.Pow(2, log);

        }


        private static double prevx = 2;



        public static double cbrtHalley(double n, double x, double e = 1e-6)
        {
            double xcubed = x * x * x;
            double xcubedPlusN;

            while (Math.Abs(xcubed - n) > e)
            {
                xcubedPlusN = xcubed + n;
                x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));
                xcubed = x * x * x;
            }

            return x;

        }





        // https://rosettacode.org/wiki/Nth_root#C.23
        public static double NthRoot(double A, int n, double p)
        {
            double _n = (double)n;
            double[] x = new double[2];
            x[0] = A;
            x[1] = A / _n;
            while (Math.Abs(x[0] - x[1]) > p)
            {
                x[1] = x[0];
                x[0] = (1 / _n) * (((_n - 1) * x[1]) + (A / Math.Pow(x[1], _n - 1)));

            }
            return x[0];
        }


        /// <summary>
        /// Calculates the cube root of a number.
        /// </summary>
        /// <param name="n">The number whose cube root you wish to find.</param>
        /// <param name="checksafety">Enables checks for edge cases: 
        /// 0, NaN, Inf and negative values of n. 
        /// Disable to gain minor speed increase at your own risk.</param>
        /// <param name="itter">Determines the number of itterations performed 
        /// using Halley's algorithm, default : 3 should provide double precision
        /// level of accuracy. A value of 2 will approximately give the result to float precision
        /// level of accuracy. Higher number increases accuracy and time nessessary 
        /// for computation. Lower values will be less accurate but faster.</param>
        /// <returns>The cube root of the value n as a double.</returns>
        public unsafe static double cbrtMagic(double n, bool checksafety = true, int itter = 3)
        {
            // Edge cases
            //if (checksafety)
            //{
            //    if (n == 0) { return 0; }
            //    if (n < 0) { return -cbrtMagic(-n); }
            //    if (float.IsNaN(n) || float.IsInfinity(n))
            //    { throw new Exception("Cannot perform cube root on NaN or Inf values"); }
            //}

            // Initial approximation
            uint N = *(uint*)&n;         // Convert the binary representation of float into a positive int
            N >>= 23;                         // Isolate the mantissa 
            N -= 127;                         // Convert mantissa into actual power it represents
            double x = 1 << (int)(N / 3);     // Perform cube root on 2^P, to appoximate x

            // Perform 3 itterations of Halley algorithm for double accuracy
            // 2 itterations for approximately float accuracy
            double xcubed = x * x * x;
            if (xcubed == n) { return x; }

            double xcubedPlusN;

            for (int i = 0; i < itter; i++)
            {
                xcubedPlusN = xcubed + n;
                x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));
                xcubed = x * x * x;
            }

            return x;
        }


        public unsafe static float cbrtfloat(float n, bool checksafety = true, int itter = 3)
        {
            // Edge cases
            //if (checksafety)
            //{
            //    if (n == 0) { return 0; }
            //    if (n < 0) { return -cbrtMagic(-n); }
            //    if (float.IsNaN(n) || float.IsInfinity(n))
            //    { throw new Exception("Cannot perform cube root on NaN or Inf values"); }
            //}

            // Initial approximation
            uint N = *(uint*)&n;         // Convert the binary representation of float into a positive int
            N >>= 23;                         // Isolate the mantissa 
            N -= 127;                         // Convert mantissa into actual power it represents
            float x = 1 << (int)(N / 3);     // Perform cube root on 2^P, to appoximate x

            // Perform 3 itterations of Halley algorithm for double accuracy
            // 2 itterations for approximately float accuracy
            float xcubed = x * x * x;
            if (xcubed == n) { return x; }

            float xcubedPlusN;

            for (int i = 0; i < itter; i++)
            {
                xcubedPlusN = xcubed + n;
                x = x * ((xcubedPlusN + n) / (xcubed + xcubedPlusN));
                xcubed = x * x * x;
            }

            return x;
        }


        public static void cbrtKernel(Index1 index, ArrayView<double> OutPut, ArrayView<float> Input)
        {
            OutPut[index] = cbrtfloat(Input[index]);
            //OutPut[index] = CbrtRust(Input[index]);
        }


        //This is a port from Rust https://github.com/rust-lang/libm/blob/master/src/math/cbrt.rs
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double CbrtRust(double x)
        {
            const int b1 = 715094163; /* B1 = (1023-1023/3-0.03306235651)*2**20 */
            const int b2 = 696219795; /* B2 = (1023-1023/3-54/3-0.03306235651)*2**20 */

            const double p0 = 1.87595182427177009643; /* 0x3ffe03e6, 0x0f61e692 */
            const double p1 = -1.88497979543377169875; /* 0xbffe28e0, 0x92f02420 */
            const double p2 = 1.621429720105354466140; /* 0x3ff9f160, 0x4a49d6c2 */
            const double p3 = -0.758397934778766047437; /* 0xbfe844cb, 0xbee751d9 */
            const double p4 = 0.145996192886612446982; /* 0x3fc2b000, 0xd4e4edd7 */

            double x1p54 = Interop.IntAsFloat(0x4350000000000000); // 0x1p54 === 2 ^ 54
            ulong ui = Interop.FloatAsInt(x);
            double r;
            double s;
            double t;
            double w;
            uint hx = ((uint)(ui >> 32)) & 0x7fffffff;

            if (hx >= 0x7ff00000)
            {
                /* cbrt(NaN,INF) is itself */
                return x + x;
            }

            /*
             * Rough cbrt to 5 bits:
             *    cbrt(2**e*(1+m) ~= 2**(e/3)*(1+(e%3+m)/3)
             * where e is integral and >= 0, m is real and in [0, 1), and "/" and
             * "%" are integer division and modulus with rounding towards minus
             * infinity.  The RHS is always >= the LHS and has a maximum relative
             * error of about 1 in 16.  Adding a bias of -0.03306235651 to the
             * (e%3+m)/3 term reduces the error to about 1 in 32. With the IEEE
             * floating point representation, for finite positive normal values,
             * ordinary integer divison of the value in bits magically gives
             * almost exactly the RHS of the above provided we first subtract the
             * exponent bias (1023 for doubles) and later add it back.  We do the
             * subtraction virtually to keep e >= 0 so that ordinary integer
             * division rounds towards minus infinity; this is also efficient.
             */
            if (hx < 0x00100000)
            {
                /* zero or subnormal? */
                ui = Interop.FloatAsInt(x * x1p54);
                hx = (uint)(ui >> 32) & 0x7fffffff;
                if (hx == 0) return x; /* cbrt(0) is itself */
                hx = hx / 3 + b2;
            }
            else
            {
                hx = hx / 3 + b1;
            }
            ui &= (ulong)1 << 63;
            ui |= (ulong)hx << 32;
            t = Interop.IntAsFloat(ui);

            /*
             * New cbrt to 23 bits:
             *    cbrt(x) = t*cbrt(x/t**3) ~= t*P(t**3/x)
             * where P(r) is a polynomial of degree 4 that approximates 1/cbrt(r)
             * to within 2**-23.5 when |r - 1| < 1/10.  The rough approximation
             * has produced t such than |t/cbrt(x) - 1| ~< 1/32, and cubing this
             * gives us bounds for r = t**3/x.
             *
             * Try to optimize for parallel evaluation as in __tanf.c.
             */
            r = (t * t) * (t / x);
            t = t * ((p0 + r * (p1 + r * p2)) + ((r * r) * r) * (p3 + r * p4));

            /*
             * Round t away from zero to 23 bits (sloppily except for ensuring that
             * the result is larger in magnitude than cbrt(x) but not much more than
             * 2 23-bit ulps larger).  With rounding towards zero, the error bound
             * would be ~5/6 instead of ~4/6.  With a maximum error of 2 23-bit ulps
             * in the rounded t, the infinite-precision error in the Newton
             * approximation barely affects third digit in the final error
             * 0.667; the error in the rounded t can be up to about 3 23-bit ulps
             * before the final error is larger than 0.667 ulps.
             */
            ui = Interop.FloatAsInt(t);
            ui = (ui + 0x80000000) & 0xffffffffc0000000;
            t = Interop.IntAsFloat(ui);

            /* one step Newton iteration to 53 bits with error < 0.667 ulps */
            s = t * t; /* t*t is exact */
            r = x / s; /* error <= 0.5 ulps; |r| < |t| */
            w = t + t; /* t+t is exact */
            r = (r - t) / (w + r); /* r-t is exact; w+r ~= 3*t */
            t = t + t * r; /* error <= 0.5 + 0.5/3 + epsilon */
            return t;
        }






    }
}
