using BAVCL;
//using BAVCL.Core;
using System;

using BenchmarkDotNet.Running;

using BAVCL.Experimental;
using BAVCL.Geometric;
using BAVCL.Core;
//using BAVCL.Geometric;
//using ILGPU.Algorithms;
//using BAVCL.Plotting;

namespace Testing_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            GPU gpu = new GPU();


            Vector vector = new Vector(gpu,new float[5],cache:false);


            vector.UpdateCache(new float[7] { 1f,2f,3f,4f,5f,6f,7f});

            //vector.SyncCPU();
            vector.Print();

        }







    }

}