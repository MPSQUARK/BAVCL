using System.Numerics;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    public static Vec<T> operator +(Vec<T> vectorA) => vectorA.AbsXIP();
    public static Vec<T> operator +(Vec<T> vectorA, Vec<T> vectorB) => OP(vectorA, vectorB, Operations.add);
    public static Vec<T> operator +(Vec<T> vectorA, T scalar) => OP(vectorA, scalar, Operations.add);
    public static Vec<T> operator +(T scalar, Vec<T> vectorA) => OP(vectorA, scalar, Operations.add);

    public static Vec<T> operator -(Vec<T> vectorA, Vec<T> vectorB) => OP(vectorA, vectorB, Operations.subtract);
    public static Vec<T> operator -(Vec<T> vectorA, T scalar) => OP(vectorA, scalar, Operations.subtract);
    public static Vec<T> operator -(T scalar, Vec<T> vectorA) => OP(vectorA, scalar, Operations.flipSubtract);

    public static Vec<T> operator *(Vec<T> vectorA, Vec<T> vectorB) => OP(vectorA, vectorB, Operations.multiply);
    public static Vec<T> operator *(Vec<T> vectorA, T scalar) => OP(vectorA, scalar, Operations.multiply);
    public static Vec<T> operator *(T scalar, Vec<T> vectorA) => OP(vectorA, scalar, Operations.multiply);

    public static Vec<T> operator /(Vec<T> vectorA, Vec<T> vectorB) => OP(vectorA, vectorB, Operations.divide);
    public static Vec<T> operator /(Vec<T> vectorA, T scalar) => OP(vectorA, scalar, Operations.divide);
    public static Vec<T> operator /(T scalar, Vec<T> vectorA) => OP(vectorA, scalar, Operations.flipDivide);

    public static Vec<T> operator ^(Vec<T> vectorA, Vec<T> vectorB) => OP(vectorA, vectorB, Operations.pow);
    public static Vec<T> operator ^(Vec<T> vectorA, T scalar) => OP(vectorA, scalar, Operations.pow);
    public static Vec<T> operator ^(T scalar, Vec<T> vectorA) => OP(vectorA, scalar, Operations.flipPow);
}
