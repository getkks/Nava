using Nava.Collections.Pooled;
using Nava.Runtime;

namespace Nava.Collections.Pooled.Dictionary;
/// <summary> Interface for handling failure outcome of a search within a dictionary. </summary>
public interface ISearchFailure<TKey, TValue, TReturn> {
	/// <summary> Handle the failure outcome of a search within a dictionary. </summary>
	/// <param name="key">The key.</param>
	/// <param name="value">The value to be updated.</param>
	/// <param name="hashCode">The hash code.</param>
	/// <param name="primaryReference"> The primary reference to the entry array list. </param>
	/// <param name="entryReference"> The reference to the entry array list at the insertion point. </param>
	public TReturn? Handle(TKey key, TValue value, int hashCode, ref int primaryReference, ref Entry<TKey, TValue> entryReference);
}
/// <summary> Interface for handling successful outcome of a search within a dictionary. </summary>
public interface ISearchSuccess<TKey, TValue, TReturn> {
	/// <summary> Handle the successful outcome of a search within a dictionary. </summary>
	/// <param name="key">The key.</param>
	/// <param name="value">The value to be updated.</param>
	/// <param name="behavior">The insertion behavior.</param>
	/// <param name="primaryReference"> The primary reference to the entry array list. </param>
	/// <param name="entryReference"> The reference to the entry. </param>
	/// <param name="previousEntryReference"> The reference to the previous entry if any. </param>
	/// <returns> Depends on the type of operation. </returns>
	TReturn Handle(TKey key, TValue value, InsertionBehavior behavior, ref int primaryReference, ref Entry<TKey, TValue> entryReference, ref Entry<TKey, TValue> previousEntryReference);
}
/// <summary> Handle failure outcome within a dictionary for insertion operation. </summary>
public readonly struct InsertFailure<TKey, TValue> : ISearchFailure<TKey, TValue, bool> {
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Handle(TKey key, TValue value, int hashCode, ref int primaryReference, ref Entry<TKey, TValue> entryReference) {
		entryReference = new Entry<TKey, TValue>(key, value, hashCode, primaryReference);
		return true;
	}
}
/// <summary> Handle successful search outcome within a dictionary for insertion operation. </summary>
public readonly struct InsertSuccess<TKey, TValue> : ISearchSuccess<TKey, TValue, bool> {
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool Handle(TKey key, TValue value, InsertionBehavior behavior, ref int primaryReference, ref Entry<TKey, TValue> entryReference, ref Entry<TKey, TValue> previousEntryReference) {
		switch( behavior ) {
			case InsertionBehavior.OverwriteExisting:
				entryReference.Value = value;
				return true;
			case InsertionBehavior.ThrowOnExisting:
				ThrowHelpers.ThrowAddingDuplicateWithKeyArgumentException(key);
				break;
		}
		return false;
	}
}

/// <summary> Handle failure outcome within a dictionary. Performs no action. </summary>
public readonly struct NoOpFailure<TKey, TValue, TReturn> : ISearchFailure<TKey, TValue, TReturn> {
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TReturn? Handle(TKey key, TValue value, int hashCode, ref int primaryReference, ref Entry<TKey, TValue> entryReference) => default;
}
/// <summary> Handle successful search outcome within a dictionary. Performs no action. </summary>
public readonly struct NoOpSuccess<TKey, TValue, TReturn> : ISearchSuccess<TKey, TValue, TReturn> {
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TReturn Handle(TKey key, TValue value, InsertionBehavior behavior, ref int primaryReference, ref Entry<TKey, TValue> entryReference, ref Entry<TKey, TValue> previousEntryReference) {
		if( typeof(TReturn) == typeof(bool) ) {
			var returnValue = true;
			return Unsafe.As<bool, TReturn>(ref returnValue);
		} else {
			return default!;
		}
	}
}
/// <summary> Handle successful search outcome within a dictionary for removal operation. </summary>
public readonly struct RemoveSuccess<TKey, TValue> : ISearchSuccess<TKey, TValue, bool> {
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool Handle(TKey key, TValue value, InsertionBehavior behavior, ref int primaryReference, ref Entry<TKey, TValue> entryReference, ref Entry<TKey, TValue> previousEntryReference) {
		RemoveHandlers.DefaultHandle(key, value, behavior, ref primaryReference, ref entryReference, ref previousEntryReference);
		return true;
	}
}
/// <summary> Handle successful search outcome within a dictionary for removal operation. </summary>
public readonly struct RemoveSuccessReturnValue<TKey, TValue> : ISearchSuccess<TKey, TValue, TValue> {
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly TValue Handle(TKey key, TValue value, InsertionBehavior behavior, ref int primaryReference, ref Entry<TKey, TValue> entryReference, ref Entry<TKey, TValue> previousEntryReference) {
		var returnValue = entryReference.Value!;
		RemoveHandlers.DefaultHandle(key, value, behavior, ref primaryReference, ref entryReference, ref previousEntryReference);
		return returnValue;
	}
}
public static class RemoveHandlers {
	public static void DefaultHandle<TKey, TValue>(TKey key, TValue value, InsertionBehavior behavior, ref int primaryReference, ref Entry<TKey, TValue> entryReference, ref Entry<TKey, TValue> previousEntryReference) {
		if( Unsafe.IsNullRef(ref previousEntryReference) ) {
			primaryReference = entryReference.Next;
		} else {
			previousEntryReference.Next = entryReference.Next;
		}
		entryReference.Clear();
	}
}
