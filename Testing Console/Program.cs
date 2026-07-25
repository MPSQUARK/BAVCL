using System;
using System.Diagnostics;
using BAVCL;
using BAVCL.Modules.Arithmetic;

GPU gpu = GPUManager.Default;

Stopwatch sw = Stopwatch.StartNew();

Vector vec = new(gpu, [1, 2, 3, 4, 5, 6], 2);
Vector vec2 = new(gpu, [5, 5, 5, 5, 5, 5], 3);

vec.Print();
vec2.Print();

Vector vec3 = vec.Cross(vec2);
Vector vec4 = vec2.Cross(vec);
Vector.Cross(vec, vec2).Print();
vec3.Print();
vec4.Print();

var time = sw.ElapsedMilliseconds;
Console.WriteLine($"Time taken: {time} ms");