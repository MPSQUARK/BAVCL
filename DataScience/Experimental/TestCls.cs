using System;
using ILGPU;
using ILGPU.Algorithms;

namespace BAVCL.Experimental
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



        public static float Sqrt(float N)
        {
            // if N < 0 throw error, no support for imaginary numbers
            int n = (int)Interop.FloatAsInt(N);

            int M = (n >> 23) - 127;
            float result = 1 << (M >> 1);

            if ((M & 0b1) == 0b1)
            {
                result *= 1.414213562373095f;
            }

            uint D = (uint)(n << 9) >> 9;
            
            if (D == 0) { return result; }

            float d = Interop.IntAsFloat((uint)(0b0_01111111_00000000000000000000000 | D));

            result = result * (0.0239878f * d * d * d - 0.17811632f * d * d + 0.78060805f * d + 0.37368985f);

            return result;
        }

        // Alternatively
        // result = result * ((n & 0b0_00000001_00000000000000000000000) >> 23) 1.414213562373095f



    }
}
