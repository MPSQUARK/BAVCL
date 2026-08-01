using System.Text.Json.Serialization;
using BAVCL.Modules.IO.Internal;

namespace BAVCL.Modules.IO.Internal.Schema;

internal sealed class IntArrayDocument
{
	[JsonPropertyName(IoSchema.Field.SchemaVersion)]
	public int SchemaVersion { get; set; } = StructuredIoValidation.CurrentSchemaVersion;

	[JsonPropertyName(IoSchema.Field.Type)]
	public string? Type { get; set; }

	[JsonPropertyName(IoSchema.Field.Dtype)]
	public string? Dtype { get; set; }

	[JsonPropertyName(IoSchema.Field.Columns)]
	public int Columns { get; set; }

	[JsonPropertyName(IoSchema.Field.Data)]
	public int[] Data { get; set; } = [];
}
