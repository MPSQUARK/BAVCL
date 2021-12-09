using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ILGPU;
using ILGPU.Algorithms;

namespace BAVCL.Experimental
{
    public partial class TestCls
    {

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
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




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float sqrt_simplif_v5(float N)
        {
            int n = (int)Interop.FloatAsInt(N);

            float result = 1 << ((n >> 23) - 127 >> 1);

            if ((~n & 0b100000000000000000000000) == 0b100000000000000000000000)
            {
                result *= 1.414213562373095f;
            }

            uint D = (uint)(n << 9) >> 9;

            if (D == 0) { return result; }

            float d = Interop.IntAsFloat(0b0_01111111_00000000000000000000000 | D);

            result *= d * (d * (d * ((0.00492272f * d - 0.0471846f) * d + 0.194967f) - 0.474473f) + 1.02803f) + 0.293744f;

            return result;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float sqrt_taylor_v2(float N)
        {
            int n = (int)Interop.FloatAsInt(N);

            float result = 1 << ((n >> 23) - 127 >> 1);

            if ((~n & 0b100000000000000000000000) == 0b100000000000000000000000)
            {
                result *= 1.414213562373095f;
            }

            uint D = (uint)(n << 9) >> 9;

            if (D == 0) { return result; }

            float d = Interop.IntAsFloat(0b0_01111111_00000000000000000000000 | D);

            result = result * 0.00390625f * (d * (d * (d * (d * (7f * d - 45f) + 126f) - 210f) + 315) + 63);

            return result;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float sqrt_acc_v1(float N)
        {
            int n = (int)Interop.FloatAsInt(N);

            float result = 1 << ((n >> 23) - 127 >> 1);

            if ((~n & 0b100000000000000000000000) == 0b100000000000000000000000)
            {
                result *= 1.414213562373095f;
            }

            uint D = (uint)(n << 9) >> 9;

            if (D == 0) { return result; }


            float d = Interop.IntAsFloat(0b0_01111111_00000000000000000000000 | D);

            // 1.5 <= d < 2 
            if ((D & 0b0_00000000_10000000000000000000000) == 0b0_00000000_10000000000000000000000)
            {
                result *= (0.41166212f + (0.71114645f * d) - (0.13609814f * d * d) + (0.01558198f * d * d * d));
                return 0.5f * (result + (N/result));
            }

            // 1 <= d < 1.5
            result *= (0.3464898f + (0.84429014f * d) - (0.22725081f * d * d) + (0.03648872f * d * d * d));
            return 0.5f * (result + (N / result));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float sqrt_acc_v2(float N)
        {
            int n = (int)Interop.FloatAsInt(N);

            float result = 1 << ((n >> 23) - 127 >> 1);

            if ((~n & 0b100000000000000000000000) == 0b100000000000000000000000)
            {
                result *= 1.414213562373095f;
            }

            uint D = (uint)(n << 9) >> 9;

            if (D == 0) { return result; }


            float d = Interop.IntAsFloat(0b0_01111111_00000000000000000000000 | D);

            // 1.5 <= d < 2 
            if ((D & 0b0_00000000_10000000000000000000000) == 0b0_00000000_10000000000000000000000)
            {
                result *= (d * ((0.015582f * d - 0.136098f) * d + 0.711146f) + 0.411662f);
                return 0.5f * (result + (N / result));
            }

            // 1 <= d < 1.5
            result *= (d * ((0.0364887f * d - 0.227251f) * d + 0.84429f) + 0.34649f);
            return 0.5f * (result + (N / result));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float sqrt_acc_v3(float N)
        {
            int n = (int)Interop.FloatAsInt(N);

            float result = 1 << ((n >> 23) - 127 >> 1);

            if ((~n & 0b100000000000000000000000) == 0b100000000000000000000000)
            {
                result *= 1.414213562373095f;
            }

            uint D = (uint)(n << 9) >> 9;

            if (D == 0) { return result; }


            float d = Interop.IntAsFloat(0b0_01111111_00000000000000000000000 | D);

            // 1.5 <= d < 2 
            if ((D & 0b0_00000000_10000000000000000000000) == 0b0_00000000_10000000000000000000000)
            {
                result *= (d * ((0.015582f * d - 0.136098f) * d + 0.711146f) + 0.411662f);
                return 0.5f * (result + (N / result));
            }

            // 1 <= d < 1.5
            result *= (d * ((0.0364887f * d - 0.227251f) * d + 0.84429f) + 0.34649f);
            return 0.5f * (result + (N / result));
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float[] sqrtarr(int len, float[] res, float[] Ns)
        {
            Parallel.For(0, len, i => 
            {
                res[i] = sqrt_acc_vcpu(Ns[i]);
            });
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static float sqrt_acc_vcpu(float N)
        {
            int n = (*(int*)&N);

            float result = 1 << ((n >> 23) - 127 >> 1);

            if ((~n & 0b100000000000000000000000) == 0b100000000000000000000000)
            {
                result *= 1.414213562373095f;
            }

            uint D = (uint)(n << 9) >> 9;

            if (D == 0) { return result; }


            float d = Interop.IntAsFloat(0b0_01111111_00000000000000000000000 | D);

            // 1.5 <= d < 2 
            if ((D & 0b0_00000000_10000000000000000000000) == 0b0_00000000_10000000000000000000000)
            {
                result *= (d * ((0.015582f * d - 0.136098f) * d + 0.711146f) + 0.411662f);
                return 0.5f * (result + (N / result));
            }

            // 1 <= d < 1.5
            result *= (d * ((0.0364887f * d - 0.227251f) * d + 0.84429f) + 0.34649f);
            return 0.5f * (result + (N / result));
        }


        // Alternatively
        // result = result * ((n & 0b0_00000001_00000000000000000000000) >> 23) 1.414213562373095f



    }
}
