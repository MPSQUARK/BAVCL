using System.Numerics;
using BAVCL.MemoryManagement;

namespace BAVCL;

public partial class Vec<T> : ICacheable<T> where T : unmanaged, INumber<T>
{
    public static Vec<T> Normalise(Vec<T> vectorA) =>
        OP(vectorA, T.One / vectorA.Sum(), Operations.multiply);

    public Vec<T> Normalise_IP() =>
        IPOP(T.One / Sum(), Operations.multiply);

}
