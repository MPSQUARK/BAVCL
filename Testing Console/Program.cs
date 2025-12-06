using System;
using BAVCL;
using BAVCL.Core;
using BAVCL.Geometric;

GPU gpu = new();
Vector3 vec = new(gpu, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
Vector3 vec2 = new(gpu, [5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5]);

vec.Print();
vec.Magnitude().Print(6);
vec.Distance(vec2).Print(6);

vec[1, 2].Print();