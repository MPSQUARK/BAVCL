using System;

namespace BAVCL.Core
{
    public partial class VectorBase<T>
    {
        public void SyncCPU()
        {
            if (this._id != 0)
            {
                this.Value = this.Pull();
                return;
            }

            if (this.Value != null)
            {
                return;
            }

            throw new Exception($"No Data Found On GPU. Vector Cache ID = {this._id}");
        }


    }


}
