namespace DataScience.Core
{
    public partial class VectorBase<T>
    {
        public bool TryDeCache()
        {
            // If the vector is live - Fail
            if (this._livecount != 0) { return false; }

            // If the vector is not cached - it's rechnically already decached
            if (this._id == 0) { return true; }

            // Else Decache
            this.Value = this.Pull();
            this._id = this.gpu.DeCache(this._id);
            return true;
        }


    }


}
