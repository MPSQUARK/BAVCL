using System;

namespace BAVCL.Core.Interfaces;

/// <summary>
/// Marks <typeparamref name="T"/> as a singleton type with a shared <see cref="Default"/> instance.
/// </summary>
public interface ISingleton<T>
{
	static virtual T Default =>
		throw new InvalidOperationException($"No default instance configured for '{typeof(T).Name}'.");

	string Extension { get; }
}
