using BAVCL.Core;

namespace BAVCL.Geometric;

public sealed partial class Vector3 : VectorBase<float>
{
    public Vector3 Copy()
    {
        if (_id != 0)
        {
            return new Vector3(gpu, Pull());
        }
        return new Vector3(gpu, Value[..]);
    }
}
