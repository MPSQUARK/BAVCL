using BAVCL.Types;

namespace BAVCL.Modules.IO;

public interface IFormatter<T>
{
	string Serialize(T value, int flags = 0);
	T Deserialize(GPU gpu, string text);
}
