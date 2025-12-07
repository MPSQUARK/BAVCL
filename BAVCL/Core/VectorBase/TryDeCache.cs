namespace BAVCL.Core
{
    public partial class VectorBase<T>
    {
        public void DeCache()
        {
            // If the vector is not cached - it's rechnically already decached
            if (ID == 0) return;

            // If the vector is live - Fail
            if (LiveCount != 0) return;

            // Else Decache
            Value = Pull();
            ID = Gpu.GCItem(ID);
        }


    }


}
