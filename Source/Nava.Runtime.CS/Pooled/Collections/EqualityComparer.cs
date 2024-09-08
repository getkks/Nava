using System.Diagnostics.CodeAnalysis;
namespace Nava.Collections;
/// <summary> Struct based optimized for Interface access to Equality comparer for <typeparamref name="T"/> based on <see cref="System.Collections.Generic.EqualityComparer{T}.Default"/>. </summary>
public readonly struct CustomEqualityComparer<E, T>(E comparer) : IEqualityComparer<T> where E : IEqualityComparer<T> {
	/// <inheritdoc/>
	public bool Equals(T? x, T? y) => comparer.Equals(x, y);
	/// <inheritdoc/>
	public int GetHashCode([DisallowNull] T obj) => comparer.GetHashCode(obj);
}
/// <summary> Struct based optimized for Interface access to Equality comparer for <typeparamref name="T"/> based on <see cref="System.Collections.Generic.EqualityComparer{T}.Default"/>. </summary>
public readonly struct DefaultEqualityComparer<E, T> : IEqualityComparer<T> where E : IEqualityComparer<T> {
	/// <inheritdoc/>
	public bool Equals(T? x, T? y) => EqualityComparer<T>.Default.Equals(x, y);
	/// <inheritdoc/>
	public int GetHashCode([DisallowNull] T obj) => EqualityComparer<T>.Default.GetHashCode(obj);
}
public static class HashHelpers {
	public static TKeyComparer ApplyComparer<TKey, EqualityComparer, TKeyComparer>(EqualityComparer comparer) where EqualityComparer : IEqualityComparer<TKey> where TKeyComparer : struct, IEqualityComparer<TKey> {
		var keyType = typeof(TKey);
		if( keyType.IsValueType ) {
			if( comparer is null or EqualityComparer<TKey> ) {
				var result = new DefaultEqualityComparer<EqualityComparer, TKey>();
				return Unsafe.As<DefaultEqualityComparer<EqualityComparer, TKey>, TKeyComparer>(ref result);
			}
		}
		var res = new CustomEqualityComparer<EqualityComparer, TKey>(comparer ?? (EqualityComparer)(object)EqualityComparer<TKey>.Default);
		return Unsafe.As<CustomEqualityComparer<EqualityComparer, TKey>, TKeyComparer>(ref res);
	}
}
