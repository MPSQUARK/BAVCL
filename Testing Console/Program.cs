using System;
using BenchmarkDotNet.Running;

using BAVCL;
using BAVCL.Core;
using BAVCL.Geometric;
using System.Threading.Tasks;
using Testing_Console;

GPU gpu = new();

Vector vector = new (gpu, new float[2] { 1f,2f});

BenchmarkRunner.Run<Benchmark>();
