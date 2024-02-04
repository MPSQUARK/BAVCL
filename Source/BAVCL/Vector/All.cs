using System.Linq;

namespace BAVCL;

public partial class Vector
{
	/// <summary>
	/// Determines if All the values in the Vector are Non-Zero
	/// </summary>
	/// <returns></returns>
	public static bool All(Vector vector)
	{
		if (vector.ID != 0)
			return !vector.Pull().Contains(0f);

		return !vector.Value.Contains(0f);
	}

	/// <summary>
	/// Determines if All the values in this Vector are Non-Zero
	/// </summary>
	/// <returns></returns>
	public bool All()
	{
		if (ID != 0)
			return !Pull().Contains(0f);
		return !Value.Contains(0f);
	}

}