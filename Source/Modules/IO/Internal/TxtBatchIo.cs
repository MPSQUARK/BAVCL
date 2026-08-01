using System;
using System.Collections.Generic;

namespace BAVCL.Modules.IO.Internal;

/// <summary>Splits a multi-document TXT file into its individual pipe-grid segments.</summary>
internal static class TxtBatchIo
{
	internal static IReadOnlyList<string> SplitDocuments(string text)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(text);

		string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
		string[] lines = normalized.Split('\n');

		var documents = new List<string>();
		var current = new List<string>();

		foreach (string line in lines)
		{
			if (line.Trim() == IoSchema.Collection.TxtBoundary)
			{
				documents.Add(string.Join('\n', current));
				current.Clear();
				continue;
			}

			current.Add(line);
		}

		documents.Add(string.Join('\n', current));

		return documents;
	}
}
