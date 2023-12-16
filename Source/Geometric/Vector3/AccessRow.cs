using BAVCL.Core;

namespace BAVCL.Geometric;

public sealed partial class Vector3 : VectorBase<float>
{
    public static Vector3 AccessRow(Vector3 vector, int vert_row)
    {
        vector.SyncCPU();
        return new Vector3(vector.gpu, vector.Value[(vert_row * 3)..((vert_row + 1) * 3)]);
    }
}
