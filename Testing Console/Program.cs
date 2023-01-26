using System;
using BenchmarkDotNet.Running;

using BAVCL;
using BAVCL.Core;
using BAVCL.Geometric;
using System.Threading.Tasks;
using Testing_Console;
using System.Diagnostics;
using ILGPU.Runtime;
using ILGPU;

GPU gpu = new();

int len = 3;

float[] data = new float[500_000_000]; //Vector.Fill(gpu, 5f, 250_000_000, Cache: false).Value;
Vector[] vecs = new Vector[len];

Stopwatch sw = new();

vecs[0] = new Vector(gpu, data);
vecs[1] = new Vector(gpu, data);
vecs[2] = new Vector(gpu, data);

//~vecs[2];

for (int i = 0; i < 100; i++)
{
    //vecs[0] = vecs[2];
    sw.Restart();


    //MemoryBuffer1D<float, Stride1D.Dense> buffer = gpu.accelerator.Allocate1D<float>(500_000_000);

    //MemoryBuffer1D<float, Stride1D.Dense> buffer = gpu.accelerator.Allocate1D<float>(data); 



    vecs[0] = vecs[1] * vecs[2];


    //vecs[0].IPOP(vecs[1], Operations.multiply);
    Console.WriteLine($"Time Taken To Cache: {sw.ElapsedMilliseconds * 1e-3:F6} sec");
    vecs[0].DeCache();
    //gpu.GCLRU(gpu.MaxMemory);
    //buffer.Dispose();
}


//uint[] idsInCache;



//for (int i = 0; i < vecs.Length; i++)
//{
//    vecs[i] = Vector.Fill(gpu, 5f, 250_000_000);
//    Console.WriteLine($"\nLRU IDs:");
//    gpu.GetLRUIDs().Print();
//    idsInCache = new uint[gpu.Caches.Count];
//    gpu.Caches.Keys.CopyTo(idsInCache, 0);
//    idsInCache.Print();
//    gpu.ShowMemoryUsage(false);
//}

//Parallel.For(0, vecs.Length, i =>
//{
//    vecs[i] = Vector.Fill(gpu, 5f, 250_000_000);
//    Console.WriteLine($"\nLRU IDs:");
//    gpu.GetLRUIDs().Print();
//    idsInCache = new uint[gpu.Caches.Count];
//    gpu.Caches.Keys.CopyTo(idsInCache, 0);
//    idsInCache.Print();
//    gpu.ShowMemoryUsage(false);
//});

