using BAVCL.Core;

namespace BAVCL.Geometric;

public sealed partial class Vector3 : VectorBase<float>
{
    public static Vector3 Zeros(GPU gpu, int Length)
    {
        return new Vector3(gpu, new float[Length]);
    }
}
