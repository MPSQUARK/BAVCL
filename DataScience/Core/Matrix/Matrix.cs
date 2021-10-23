using DataScience.Core;


namespace DataScience
{
    public partial class Matrix : VectorBase<float>
    {
        public Matrix(GPU gpu, float[] value, int columns = 1, bool Cache = true) : base(gpu, value, columns, Cache)
        {
            gpu.DeCache(_id);
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
    }


}
