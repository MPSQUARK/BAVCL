using System;
using System.IO;
using BAVCL.Core.Interfaces;

namespace BAVCL.Modules.IO;

/// <summary>
/// Read or write a single file for type <typeparamref name="T"/> using formatter <typeparamref name="TFormatter"/>.
/// Writers allow one write per session.
/// </summary>
public sealed class FileSession<T, TFormatter>
	where TFormatter : class, IFormatter<T>, ISingleton<TFormatter>
{
	readonly string _path;
	readonly TFormatter _formatter;
	readonly bool _overwrite;
	bool _written;

	internal FileSession(string path, bool overwrite)
	{
		_formatter = TFormatter.Default;
		_path = path;
		_overwrite = overwrite;
	}

	public void Serialize(T value, int flags = 0)
	{
		ArgumentNullException.ThrowIfNull(value);
		Commit(_formatter.Serialize(value, flags));
	}

	public T Deserialize(GPU gpu)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		return _formatter.Deserialize(gpu, ReadRaw());
	}

	public void WriteRaw(string content) =>
		Commit(content);

	public string ReadRaw()
	{
		if (!File.Exists(_path))
			throw new FileNotFoundException($"File not found: {_path}", _path);

		return File.ReadAllText(_path);
	}

	void Commit(string content)
	{
		ArgumentNullException.ThrowIfNull(content);
		if (_written)
			throw new InvalidOperationException("This writer has already written a document.");

		if (!_overwrite && File.Exists(_path))
			throw new IOException($"File already exists: {_path}");

		File.WriteAllText(_path, content);
		_written = true;
	}
}
