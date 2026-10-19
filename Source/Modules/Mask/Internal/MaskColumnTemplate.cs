using System;

namespace BAVCL.Modules.Masking;

/// <summary>Selected / unselected column indices for row-broadcast mask compaction.</summary>
internal readonly struct MaskColumnTemplate(int[] trueColumns, int[] falseColumns)
{
	public ReadOnlySpan<int> TrueColumns => trueColumns;
	public ReadOnlySpan<int> FalseColumns => falseColumns;
}
