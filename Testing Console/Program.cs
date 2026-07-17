using System;
using BAVCL;
using BAVCL.Core;
using BAVCL.Geometric;
using BAVCL.Services;

GPU gpu = GPUManager.Default;
Vector vec = new(gpu, [1, 2, 3, 4, 5, 6], 2);
Vector vec2 = new(gpu, [5, 5, 5, 5, 5, 5], 3);

vec.Print();
vec2.Print();

Vector vec3 = Vector.Cross(vec, vec2);
Vector vec4 = Vector.Cross(vec2, vec);

vec3.Print();
vec4.Print();
