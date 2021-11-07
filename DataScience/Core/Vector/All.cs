using System.Linq;

namespace DataScience
{
    public partial class Vector
    {
        /// <summary>
        /// Determines if All the values in the Vector are Non-Zero
        /// </summary>
        /// <returns></returns>
        public static bool All(Vector vector)
        {
            if (vector._id != 0)
            {
                return !vector.Pull().Contains(0f);
            }
            return !vector.Value.Contains(0f);
        }

        /// <summary>
        /// Determines if All the values in this Vector are Non-Zero
        /// </summary>
        /// <returns></returns>
        public bool All()
        {
            if (this._id != 0)
            {
                return !this.Pull().Contains(0f);
            }
            return !this.Value.Contains(0f);
        }

    }
}
