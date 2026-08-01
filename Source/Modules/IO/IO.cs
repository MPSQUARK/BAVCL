using System;
using System.Collections.Generic;
using System.IO;
using BAVCL.Core.Interfaces;

namespace BAVCL.Modules.IO;

/// <summary>
/// Disk persistence for BAVCL data types.
/// Callers supply directory + file name; the library appends the formatter extension.
/// Default directory is the current working directory. Overwrite defaults to false.
/// </summary>
public static class IO
{
	public static void Serialize<T, TFormatter>(
		T value,
		string fileName,
		string? directory = null,
		bool overwrite = false,
		int flags = 0)
		where TFormatter : class, IFormatter<T>, ICollectionFormatter<T>, ISingleton<TFormatter>
	{
		ArgumentNullException.ThrowIfNull(value);
		using FileSession<T, TFormatter> writer = CreateWriter<T, TFormatter>(fileName, directory, overwrite);
		writer.Write(value, flags);
	}

	public static T Deserialize<T, TFormatter>(GPU gpu, string fileName, string? directory = null)
		where TFormatter : class, IFormatter<T>, ICollectionFormatter<T>, ISingleton<TFormatter>
	{
		var session = CreateReader<T, TFormatter>(fileName, directory);
		return session.Deserialize(gpu);
	}

	public static IReadOnlyList<T> DeserializeAll<T, TFormatter>(GPU gpu, string fileName, string? directory = null)
		where TFormatter : class, IFormatter<T>, ICollectionFormatter<T>, ISingleton<TFormatter>
	{
		var session = CreateReader<T, TFormatter>(fileName, directory);
		return session.DeserializeAll(gpu);
	}

	public static FileSession<T, TFormatter> CreateWriter<T, TFormatter>(
		string fileName,
		string? directory = null,
		bool overwrite = false)
		where TFormatter : class, IFormatter<T>, ICollectionFormatter<T>, ISingleton<TFormatter>
	{
		string path = ResolvePath<TFormatter>(directory, fileName);
		return new FileSession<T, TFormatter>(path, overwrite);
	}

	public static FileSession<T, TFormatter> CreateReader<T, TFormatter>(
		string fileName,
		string? directory = null)
		where TFormatter : class, IFormatter<T>, ICollectionFormatter<T>, ISingleton<TFormatter>
	{
		string path = ResolvePath<TFormatter>(directory, fileName);
		return new FileSession<T, TFormatter>(path, overwrite: true);
	}

	static string ResolvePath<TFormatter>(string? directory, string fileName)
		where TFormatter : class, ISingleton<TFormatter>
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

		string dir = string.IsNullOrWhiteSpace(directory)
			? Environment.CurrentDirectory
			: directory;

		return Path.Combine(dir, fileName + TFormatter.Default.Extension);
	}
}
