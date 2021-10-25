using DataScience;
using BenchmarkDotNet.Attributes;
using System;
using System.Linq;
using ILGPU.Algorithms;
using System.Threading.Tasks;
using DataScience.Geometric;

namespace Testing_Console
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        float[] arr = new float[3] { 2,3,4};
        float[] arr2 = new float[3] { 8,5,11};

        [Benchmark]
        public float Distance()
        {
            return arr[0] * arr2[0] + arr[1] * arr2[1] + arr[2] * arr2[2];
        }

        [Benchmark]
        public float DistanceVec()
        {
            System.Numerics.Vector3 vec = new System.Numerics.Vector3(arr[0], arr[1], arr[2]);
            System.Numerics.Vector3 vec2 = new System.Numerics.Vector3(arr2[0], arr2[1], arr2[2]);

            return System.Numerics.Vector3.Dot(vec, vec2);
        }




    }



}
