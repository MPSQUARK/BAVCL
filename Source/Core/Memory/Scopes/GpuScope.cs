namespace BAVCL.Core;

public static class GpuScope
{
	public static GpuPinScope Begin(ICacheable modified) =>
		new([modified], []);

	public static GpuPinScope Begin(params ICacheable[] modified) =>
		new(modified, []);

	public static GpuPinScope Begin(ICacheable modified, ICacheable readOnly) =>
		new([modified], [readOnly]);

	public static GpuPinScope Begin(ICacheable modified, params ICacheable[] readOnly) =>
		new([modified], readOnly);

	public static GpuPinScope Begin(ICacheable[] modified, ICacheable[] readOnly) =>
		new(modified, readOnly);

	public static GpuPinScope BeginReadOnly(ICacheable readOnly) =>
		new([], [readOnly]);

	public static GpuPinScope BeginReadOnly(params ICacheable[] readOnly) =>
		new([], readOnly);
}
