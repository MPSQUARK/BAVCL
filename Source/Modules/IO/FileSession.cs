using System;
using System.Collections.Generic;
using System.IO;
using BAVCL.Core.Interfaces;

namespace BAVCL.Modules.IO;

/// <summary>
/// Read or write a file for type <typeparamref name="T"/> using formatter <typeparamref name="TFormatter"/>.
/// A session either performs a single <see cref="Write"/>, or one or more <see cref="Append"/> calls
/// followed by <see cref="Flush"/> (or <see cref="Dispose"/>) to finalize a multi-document collection file.
/// </summary>
public sealed class FileSession<T, TFormatter> : IDisposable
	where TFormatter : class, IFormatter<T>, ICollectionFormatter<T>, ISingleton<TFormatter>
{
	readonly string _path;
	readonly TFormatter _formatter;
	readonly bool _overwrite;

	bool _started;
	bool _finalized;
	int _itemCount;
	int? _flags;

	internal FileSession(string path, bool overwrite)
	{
		_formatter = TFormatter.Default;
		_path = path;
		_overwrite = overwrite;
	}

	/// <summary>Serializes <paramref name="value"/> to text without touching disk.</summary>
	public string Serialize(T value, int flags = 0)
	{
		ArgumentNullException.ThrowIfNull(value);
		return _formatter.Serialize(value, flags);
	}

	/// <summary>Replaces the file with a finalized collection containing exactly <paramref name="value"/>.</summary>
	public void Write(T value, int flags = 0)
	{
		ArgumentNullException.ThrowIfNull(value);
		EnsureNotFinalized();
		if (_started)
			throw new InvalidOperationException("Cannot call Write after Append has started a collection. Call Flush() to finalize it, or create a new writer.");

		CheckOverwriteConflict();
		File.WriteAllText(_path, _formatter.SerializeAll([value], flags));
		_started = true;
		_finalized = true;
		_flags = flags;
		_itemCount = 1;
	}

	/// <summary>Adds <paramref name="value"/> to the file, opening a new collection if this is the first write.</summary>
	public void Append(T value, int flags = 0)
	{
		ArgumentNullException.ThrowIfNull(value);
		EnsureNotFinalized();
		ValidateFlags(flags);

		if (!_started)
		{
			CheckOverwriteConflict();
			File.WriteAllText(_path, _formatter.OpenCollection(value, flags));
			_started = true;
		}
		else
		{
			File.AppendAllText(_path, _formatter.AppendItem(value, flags));
		}

		_flags ??= flags;
		_itemCount++;
	}

	/// <summary>Finalizes the file, closing any open collection wrapper. Safe to call more than once.</summary>
	public void Flush()
	{
		if (_finalized || !_started)
			return;

		string closing = _formatter.CloseCollection(_itemCount);
		if (closing.Length > 0)
			File.AppendAllText(_path, closing);

		_finalized = true;
	}

	public void Dispose() => Flush();

	/// <summary>Reads exactly one document. Throws if the file contains zero or more than one document.</summary>
	public T Deserialize(GPU gpu)
	{
		IReadOnlyList<T> all = DeserializeAll(gpu);
		if (all.Count != 1)
			throw new InvalidOperationException(
				$"Expected exactly one document but found {all.Count}. Use DeserializeAll to read multiple documents.");

		return all[0];
	}

	/// <summary>Reads every document from the file.</summary>
	public IReadOnlyList<T> DeserializeAll(GPU gpu)
	{
		ArgumentNullException.ThrowIfNull(gpu);
		return _formatter.DeserializeAll(gpu, ReadRaw());
	}

	public void WriteRaw(string content)
	{
		ArgumentNullException.ThrowIfNull(content);
		EnsureNotFinalized();
		if (_started)
			throw new InvalidOperationException("Cannot write raw content after Append has started a collection. Call Flush() first or create a new writer.");

		CheckOverwriteConflict();
		File.WriteAllText(_path, content);
		_finalized = true;
	}

	public string ReadRaw()
	{
		if (!File.Exists(_path))
			throw new FileNotFoundException($"File not found: {_path}", _path);

		return File.ReadAllText(_path);
	}

	void EnsureNotFinalized()
	{
		if (_finalized)
			throw new InvalidOperationException("This writer has already finalized its output. Create a new writer to write again.");
	}

	void ValidateFlags(int flags)
	{
		if (_flags is int existing && existing != flags)
			throw new InvalidOperationException(
				$"Serialize flags must be consistent across items appended to the same file. First item used flags={existing}; this item used flags={flags}.");
	}

	void CheckOverwriteConflict()
	{
		if (!_overwrite && File.Exists(_path))
			throw new IOException($"File already exists: {_path}");
	}
}
