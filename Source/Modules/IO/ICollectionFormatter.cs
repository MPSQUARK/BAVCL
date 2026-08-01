using System;
using System.Collections.Generic;
using System.Text;

namespace BAVCL.Modules.IO;

/// <summary>
/// Multi-document persistence for a formatter. Complements <see cref="IFormatter{T}"/>, which
/// serializes a single item, by combining items into one valid collection file (JSON array,
/// XML root wrapper, CSV header + rows, or TXT --- separated segments) and splitting a
/// collection file back into its individual items.
/// </summary>
public interface ICollectionFormatter<T>
{
	/// <summary>Text that starts a new collection file, embedding <paramref name="first"/> as its first item.</summary>
	string OpenCollection(T first, int flags);

	/// <summary>Text appended to an already-open collection file to add another item.</summary>
	string AppendItem(T value, int flags);

	/// <summary>Text appended to finalize (close) a collection file containing <paramref name="itemCount"/> items.</summary>
	string CloseCollection(int itemCount);

	/// <summary>Reads every document from a finalized collection file.</summary>
	IReadOnlyList<T> DeserializeAll(GPU gpu, string text);

	/// <summary>
	/// Serializes <paramref name="values"/> to a finalized collection file, without touching disk.
	/// Symmetrical to <see cref="DeserializeAll"/>.
	/// </summary>
	string SerializeAll(IReadOnlyList<T> values, int flags = 0)
	{
		ArgumentNullException.ThrowIfNull(values);
		if (values.Count == 0)
			throw new ArgumentException("Collection must contain at least one item.", nameof(values));

		var text = new StringBuilder(OpenCollection(values[0], flags));
		for (int i = 1; i < values.Count; i++)
			text.Append(AppendItem(values[i], flags));

		text.Append(CloseCollection(values.Count));
		return text.ToString();
	}
}
