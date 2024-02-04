using System;
using System.Numerics;
using ILGPU.Algorithms;

namespace BAVCL.CustomMath;

/// <summary>
/// Custom Math - Mapping class for generic math operations.
/// </summary>
public class CMath
{
	public static T Pow<T>(T @base, T exponent) where T : INumber<T> => 
		throw new NotImplementedException();
	
	public static T Square<T>(T number) where T : INumber<T> => number * number;
	
	public static float Pow(float @base, float exponent) =>
		XMath.Pow(@base, exponent);
	
	public static double Pow(double @base, double exponent) =>
		XMath.Pow(@base, exponent);
		
	public static int Pow(int @base, int exponent) =>
		(int)XMath.Pow(@base, exponent);
	
	public static long Pow(long @base, long exponent) =>
		(long)XMath.Pow(@base, exponent);
		
	public static byte Pow(byte @base, byte exponent) =>
		(byte)XMath.Pow(@base, exponent);
	
	public static short Pow(short @base, short exponent) =>
		(short)XMath.Pow(@base, exponent);
		
	public static sbyte Pow(sbyte @base, sbyte exponent) =>
		(sbyte)XMath.Pow(@base, exponent);
		
	public static ushort Pow(ushort @base, ushort exponent) =>
		(ushort)XMath.Pow(@base, exponent);
		
	public static uint Pow(uint @base, uint exponent) =>
		(uint)XMath.Pow(@base, exponent);
		
	public static ulong Pow(ulong @base, ulong exponent) =>
		(ulong)XMath.Pow(@base, exponent);
		
}
