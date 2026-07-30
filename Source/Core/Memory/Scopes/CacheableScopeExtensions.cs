namespace BAVCL.Core;

public static class CacheableScopeExtensions
{
	public static CpuScope<T> CpuScope<T>(this ICacheable<T> cacheable, bool syncToGpu = false)
		where T : unmanaged =>
		global::BAVCL.Core.CpuScope.Begin(cacheable, syncToGpu);

	public static CpuScope<T> CpuScopeAndSync<T>(this ICacheable<T> cacheable) where T : unmanaged =>
		global::BAVCL.Core.CpuScope.Begin(cacheable, syncToGpu: true);

	public static GpuPinScope GpuScope(this ICacheable modified) =>
		global::BAVCL.Core.GpuScope.Begin(modified);

	public static GpuPinScope GpuScope(this ICacheable modified, ICacheable readOnly) =>
		global::BAVCL.Core.GpuScope.Begin(modified, readOnly);

	public static GpuPinScope GpuScope(this ICacheable modified, params ICacheable[] readOnly) =>
		global::BAVCL.Core.GpuScope.Begin(modified, readOnly);

	public static GpuPinScope GpuScopeReadOnly(this ICacheable readOnly) =>
		global::BAVCL.Core.GpuScope.BeginReadOnly(readOnly);

	public static GpuPinScope GpuScopeReadOnly(this ICacheable readOnly, params ICacheable[] more)
	{
		ICacheable[] all = new ICacheable[more.Length + 1];
		all[0] = readOnly;
		more.CopyTo(all, 1);
		return global::BAVCL.Core.GpuScope.BeginReadOnly(all);
	}
}
