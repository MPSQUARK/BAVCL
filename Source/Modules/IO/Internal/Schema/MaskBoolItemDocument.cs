using System.Text.Json.Serialization;
using BAVCL.Modules.IO.Internal;

namespace BAVCL.Modules.IO.Internal.Schema;

/// <summary>Per-item bool-mask JSON payload inside a collection file.</summary>
internal sealed class MaskBoolItemDocument
{
	[JsonPropertyName(IoSchema.Field.Type)]
	public string? Type { get; set; }

	[JsonPropertyName(IoSchema.Field.Dtype)]
	public string? Dtype { get; set; }

	[JsonPropertyName(IoSchema.Field.Columns)]
	public int Columns { get; set; }

	[JsonPropertyName(IoSchema.Field.Data)]
	public bool[] Data { get; set; } = [];
}
