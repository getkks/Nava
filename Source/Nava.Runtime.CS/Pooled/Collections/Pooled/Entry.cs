namespace Nava.Collections.Pooled;
/// <summary> Insertion behavior for dictionary operations. </summary>
public enum InsertionBehavior {
	/// <summary> Specifies that an existing entry with the same key should be overwritten if encountered. </summary>
	OverwriteExisting,
	/// <summary> Specifies that if an existing entry with the same key is encountered, an exception should be thrown. </summary>
	ThrowOnExisting,
	/// <summary> The default insertion behavior. </summary>
	None,
}/// <summary> Entry in a dictionary. </summary>
 /// <typeparam name="TKey"> Type of the key. </typeparam>
 /// <typeparam name="TValue"> Type of the value. </typeparam>
 /// <remarks> Initializes a new instance of the <see cref="Entry{TKey, TValue}"/> struct. </remarks>
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public struct Entry<TKey, TValue>(TKey key, TValue value, int hashCode, int next) {
	/// <summary> Hash code of the entry. </summary>
	public int HashCode = hashCode;
	/// <summary> Collision index of the entry. </summary>
	public int Next = next;
	/// <summary> Key of the entry. </summary>
	public TKey Key = key;
	/// <summary> Value of the entry. </summary>
	public TValue Value = value;
	/// <summary> Clears the entry. </summary>
	public void Clear() {
		if( RuntimeHelpers.IsReferenceOrContainsReferences<TKey>() ) {
			Key = default!;
		}

		if( RuntimeHelpers.IsReferenceOrContainsReferences<TValue>() ) {
			Value = default!;
		}
		Next = int.MinValue;
	}
}
