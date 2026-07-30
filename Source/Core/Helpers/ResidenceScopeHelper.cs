using System;

namespace BAVCL.Core;

internal static class ResidenceScopeHelper
{
	public static bool TryTransition(ICacheable cacheable, Residence expected, Residence next) =>
		cacheable.TrySetResidence(expected, next);

	public static void TransitionOrReconcile(ICacheable cacheable, Residence expected, Residence next)
	{
		if (cacheable.Residence == next)
			return;

		if (TryTransition(cacheable, expected, next))
			return;

		Residence current = cacheable.Residence;
		if (current == next)
			return;

		if (TryTransition(cacheable, current, next))
			return;

		throw new InvalidOperationException(
			$"Failed to transition residence to {next} (expected {expected}, actual {cacheable.Residence}).");
	}
}
