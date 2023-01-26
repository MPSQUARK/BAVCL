using BAVCL.Core;


namespace BAVCL
{
    public partial class Matrix : VectorBase<float>
    {

        public (int, int) MatrixShape; 

        public Matrix(GPU gpu, float[] value, (int, int) shape, int columns = 1, bool Cache = true) : base(gpu, value, columns, Cache)
        {
            this.MatrixShape = shape;

            ID = gpu.GCItem(ID);
            throw new System.NotImplementedException();
        }

        public override float Mean()
        {
            throw new System.NotImplementedException();
        }

        public override float Range()
        {
            throw new System.NotImplementedException();
        }

        public override float Sum()
        {
            throw new System.NotImplementedException();
        }


        public int MatrixLength()
        {
            return MatrixShape.Item1 * MatrixShape.Item2;
        }



    }


}
