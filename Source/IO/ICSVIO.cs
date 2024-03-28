namespace BAVCL.IO.Interfaces;

public interface ICSVIO<T> where T : unmanaged
{
	public abstract string ToCsv();
	public abstract bool ToCsv(string path, bool overwrite = false);

	public abstract T FromCsv(string csv);
	public abstract T ReadCsv(string path);
}
