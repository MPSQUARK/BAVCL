namespace BAVCL;

public partial class Matrix : VectorBase<float>
{

    public (int, int) MatrixShape;

    public Matrix(float[] value, (int, int) shape, int columns = 1, Upload upload = Upload.Now, GPU? gpu = null) : base(value, columns, upload, gpu)
    {
        this.MatrixShape = shape;

        ID = Gpu.GCItem(ID);
        throw new System.NotImplementedException();
    }

    public int MatrixLength()
    {
        return MatrixShape.Item1 * MatrixShape.Item2;
    }



}
