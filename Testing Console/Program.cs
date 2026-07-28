using System;
using System.Diagnostics;
using BAVCL;
using BAVCL.Modules.Structural;
using BAVCL.Types;

GPU gpu = GPUManager.Default;

Mask mask = new(gpu, [true, false, false, true, true, false, false, true, true, false], 2);
mask.Print();

Vector vector = new(gpu, [1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f, 10.0f], 2);
vector.Print();

var result = vector & mask;
result.Print();

Vector vector1 = new(gpu, [2f, 3f, 42f, 53f, 5f, 1f, 3f, 12f, 9f, 131f], 2);

Mask mask1 = vector1 <= vector;
mask1.Print();

int itter = 200000;
Stopwatch sw = Stopwatch.StartNew();

for (int i = 0; i < itter; i++)
{
    Mask mask2 = mask & mask1;
}

Console.WriteLine($"time taken {(float)sw.ElapsedMilliseconds / (float)itter}ms (total: {sw.ElapsedMilliseconds}ms)");

Console.ReadLine();
