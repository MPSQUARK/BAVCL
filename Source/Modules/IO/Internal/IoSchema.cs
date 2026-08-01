using System;
using System.Collections.Generic;
using BAVCL.Geometric;
using BAVCL.Types;

namespace BAVCL.Modules.IO.Internal;

internal static class IoSchema
{
	internal static class Field
	{
		internal const string SchemaVersion = "schemaVersion";
		internal const string Type = "type";
		internal const string Dtype = "dtype";
		internal const string Columns = "columns";
		internal const string Data = "data";
		internal const string Count = "count";

		internal static readonly string[] DefaultHeader = [SchemaVersion, Type, Dtype, Columns, Data];
		internal static readonly string[] MaskPackedHeader = [SchemaVersion, Type, Dtype, Columns, Count, Data];
	}

	internal static class Document
	{
		static readonly HashSet<System.Type> Allowed = [typeof(Vector), typeof(Vector3), typeof(Mask)];

		internal static string Of<T>() => Of(typeof(T));

		internal static string Of(System.Type type) => Name(type, Allowed, "document type");

		internal static string XmlRootOf<T>() => XmlRootOf(typeof(T));

		internal static string XmlRootOf(System.Type type)
		{
			_ = Of(type);
			return type.Name.ToLowerInvariant();
		}

		internal static bool Is(string? name, System.Type type) => Matches(name, type);
	}

	internal static class Dtype
	{
		static readonly HashSet<System.Type> Allowed = [typeof(float), typeof(int), typeof(bool)];

		internal static string Of<T>() => Of(typeof(T));

		internal static string Of(System.Type type) => Name(type, Allowed, "element type");

		internal static bool Is(string? name, System.Type type) => Matches(name, type);
	}

	static string Name(System.Type type, HashSet<System.Type> allowed, string category)
	{
		if (!allowed.Contains(type))
			throw new ArgumentException($"Unsupported IO {category} '{type.Name}'.", nameof(type));

		return type.Name;
	}

	static bool Matches(string? name, System.Type type) =>
		string.Equals(name, type.Name, StringComparison.Ordinal);
}
