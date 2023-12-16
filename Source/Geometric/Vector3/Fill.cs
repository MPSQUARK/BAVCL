using System.Linq;
using BAVCL.Core;

namespace BAVCL.Geometric;

public sealed partial class Vector3 : VectorBase<float>
{
    public static Vector3 Fill(GPU gpu, float Value, int Length)
    {
        return new Vector3(gpu, Enumerable.Repeat(Value, Length).ToArray());
    }
}
