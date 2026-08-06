namespace BAVCL;

/// <summary>
/// Sort direction. <see cref="Descending"/> must remain enum value <c>-1</c> — comparer helpers
/// use <c>(int)order</c> as an all-zeros / all-ones flip mask for branchless sign reversal.
/// </summary>
public enum SortOrder
{
	Ascending = 0,
	Descending = -1,
}
