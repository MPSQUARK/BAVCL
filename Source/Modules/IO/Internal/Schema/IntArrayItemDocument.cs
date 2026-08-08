using System.Text.Json.Serialization;
using BAVCL.Modules.IO.Internal;

namespace BAVCL.Modules.IO.Internal.Schema;

/// <summary>Per-item JSON payload inside a collection file (no <see cref="IoSchema.Field.SchemaVersion"/>).</summary>
internal sealed class IntArrayItemDocument
{
	[JsonPropertyName(IoSchema.Field.Type)]
	public string? Type { get; set; }

	[JsonPropertyName(IoSchema.Field.Dtype)]
	public string? Dtype { get; set; }

	[JsonPropertyName(IoSchema.Field.Columns)]
	public int Columns { get; set; }

	[JsonPropertyName(IoSchema.Field.Data)]
	public int[] Data { get; set; } = [];
}
