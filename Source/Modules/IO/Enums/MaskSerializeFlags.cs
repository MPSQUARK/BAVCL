namespace BAVCL.Modules.IO.Enums;

/// <summary>Serialize flags for <see cref="IFormatter{T}.Serialize"/> when <c>T</c> is <see cref="BAVCL.Types.Mask"/>.</summary>
public static class MaskSerializeFlags
{
	public const int Packed = 0;
	public const int Bool = 1;
}
