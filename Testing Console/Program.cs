using System;
using BAVCL;
using BAVCL.Modules.Structural;
using BAVCL.Types;

GPU gpu = GPUManager.Default;

Mask mask = new(gpu,
[
    true,
    false,
    true,
    false,
    true,
    false,
    true,
    false,
    true,
    false
],2);

//Vector vector = new(gpu,
//[
//    1.0f,
//    2.0f,
//    3.0f,
//    4.0f,
//    5.0f,
//    6.0f,
//    7.0f,
//    8.0f,
//    9.0f,
//    10.0f
//], 2);


mask.Print();
//vector.Print();

Console.ReadLine();