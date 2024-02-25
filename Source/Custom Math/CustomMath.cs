using System;
using System.Numerics;
using ILGPU.Algorithms;

namespace BAVCL.CustomMath;

/// <summary>
/// Custom Math - Mapping class for generic math operations.
/// </summary>
public class CMath
{
	public static T Pow<T>(T @base, T exponent) where T : INumber<T> => T.One;
	
	public static T Square<T>(T number) where T : INumber<T> => number * number;
	public static T Cube<T>(T number) where T : INumber<T> => number * number * number;
	
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
		
	public static T Abs<T>(T value) where T : INumber<T> => T.Abs(value);
	public static T Rcp<T>(T value) where T : INumber<T> => T.One / value;
	
	public static T Sqrt<T>(T value) where T : INumber<T> => T.One;	
	public static float Sqrt(float value) => XMath.Sqrt(value);
	public static double Sqrt(double value) => XMath.Sqrt(value);
	public static int Sqrt(int value) => (int)XMath.Sqrt(value);
	public static long Sqrt(long value) => (long)XMath.Sqrt(value);
	public static byte Sqrt(byte value) => (byte)XMath.Sqrt(value);
	public static short Sqrt(short value) => (short)XMath.Sqrt(value);
	public static sbyte Sqrt(sbyte value) => (sbyte)XMath.Sqrt(value);
	public static ushort Sqrt(ushort value) => (ushort)XMath.Sqrt(value);
	public static uint Sqrt(uint value) => (uint)XMath.Sqrt(value);
	public static ulong Sqrt(ulong value) => (ulong)XMath.Sqrt(value);
	
	public static T Rsqrt<T>(T value) where T : INumber<T> => T.One / Sqrt(value);
	public static float Rsqrt(float value) => XMath.Rsqrt(value);
	public static double Rsqrt(double value) => XMath.Rsqrt(value);
	
	public static T Log<T>(T value) where T : INumber<T> => T.One;
	public static T Log<T>(T value, T @base) where T : INumber<T> => T.One;
	public static float Log(float value) => XMath.Log(value);
	public static double Log(double value) => XMath.Log(value);
	public static int Log(int value) => (int)XMath.Log(value);
	public static long Log(long value) => (long)XMath.Log(value);
	public static byte Log(byte value) => (byte)XMath.Log(value);
	public static short Log(short value) => (short)XMath.Log(value);
	public static sbyte Log(sbyte value) => (sbyte)XMath.Log(value);
	public static ushort Log(ushort value) => (ushort)XMath.Log(value);
	public static uint Log(uint value) => (uint)XMath.Log(value);
	public static ulong Log(ulong value) => (ulong)XMath.Log(value);
	public static float Log(float value, float @base) => XMath.Log(value, @base);
	public static double Log(double value, double @base) => XMath.Log(value, @base);
	public static int Log(int value, int @base) => (int)XMath.Log(value, @base);
	public static long Log(long value, long @base) => (long)XMath.Log(value, @base);
	public static byte Log(byte value, byte @base) => (byte)XMath.Log(value, @base);
	public static short Log(short value, short @base) => (short)XMath.Log(value, @base);
	public static sbyte Log(sbyte value, sbyte @base) => (sbyte)XMath.Log(value, @base);
	public static ushort Log(ushort value, ushort @base) => (ushort)XMath.Log(value, @base);
	public static uint Log(uint value, uint @base) => (uint)XMath.Log(value, @base);
	public static ulong Log(ulong value, ulong @base) => (ulong)XMath.Log(value, @base);
	

}
