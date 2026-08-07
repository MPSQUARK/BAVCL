using System;
using BAVCL;
using BAVCL.Geometric;
using BAVCL.Modules.Geometric;
using BAVCL.Modules.Sorting;
using BAVCL.Modules.Structural;
using BAVCL.Types;

GPU gpu = GPUManager.Default;
KernelModuleLoader.Load<float>(gpu, KernelWorkloads.Sorting);

Vector vec = new(gpu, [1,324,312,5645,756,890,123,456,789,012,198],2);

vec.Print();
vec.SortDescXIP();
vec.Print();

Console.ReadLine();
