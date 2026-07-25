using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BAVCL;

int itter = 100000;

GPU gpu = GPUManager.Default;

Stopwatch sw = Stopwatch.StartNew();

Vector vec = new(gpu, [1, 2, 3, 4, 5, 6]);
Vector vec2 = new(gpu, [5, 5, 5, 5, 5, 5]);

Parallel.For(0, itter, i =>
{
    Vector vec3 = vec * vec2;
    Vector vec4 = vec2 * vec;
}
);

var time = sw.ElapsedMilliseconds;
Console.WriteLine($"Time taken: {(float)time/(float)itter} ms (total: {time} ms)");

vec.Print();
vec2.Print();

Vector vec5 = vec * vec2;
Vector vec6 = vec2 * vec;
vec5.Print();
vec6.Print();