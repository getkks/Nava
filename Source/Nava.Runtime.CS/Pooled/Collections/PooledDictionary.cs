using System.Numerics;

using Nava.Collections;
using Nava.Collections.Pooled;
using Nava.Collections.Pooled.Dictionary;

namespace Nava.Runtime.Pooled.Collections;
/// <summary> Dictionary implementation using <see cref="System.Buffers.ArrayPool{T}"/> for storing entries. </summary>
/// <typeparam name="TKey"> Type of the key. </typeparam>
/// <typeparam name="TValue"> Type of the value. </typeparam>
/// <typeparam name="TKeyComparer"> Type of the key comparer. </typeparam>
public struct PooledDictionary<TKey, TValue, TKeyComparer> where TKeyComparer : struct, IEqualityComparer<TKey> {//: Disposable where TKeyComparer : struct, IEqualityComparer<TKey> {
	/// <summary> End of index. </summary>
	public const int EndOfIndex = -1;
	/// <summary> Golden ratio. </summary>
	public const int GoldenRatio = -1640531527;
	/// <summary> Number of bits to shift based on capacity. </summary>
	private int bitShift;
	/// <summary> Hash comparer. </summary>
	private readonly TKeyComparer comparer;
	/// <summary> Number of elements contained in the dictionary. </summary>
	private int count = 0;
	/// <summary> <see cref="Array"/> of entries. </summary>
	private Entry<TKey, TValue>[] entries;
	private readonly SharedBuffer<Entry<TKey, TValue>> entriesBuffer = default;

	private int freeCount = 0;
	private int[] index;

	private readonly SharedBuffer<int> indexBuffer = default;
	/// <summary> Initializes a new instance of the <see cref="PooledDictionary{TKey, TValue, TKeyComparer}"/> class. </summary>
	/// <param name="capacity"> Initial capacity. </param>
	/// <param name="comparer"> Hash comparer. </param>
	public PooledDictionary(int capacity, IEqualityComparer<TKey> comparer) {
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(capacity, Array.MaxLength - 1);
		capacity = PowerOf2(capacity);
		entries = entriesBuffer.Rent(capacity);
		PrepareIndex();
		this.comparer = HashHelpers.ApplyComparer<TKey, IEqualityComparer<TKey>, TKeyComparer>(comparer);
	}
	/// <summary> Initializes a new instance of the <see cref="PooledDictionary{TKey, TValue, TKeyComparer}"/> class. </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public PooledDictionary() : this(1) { }
	/// <summary> Initializes a new instance of the <see cref="PooledDictionary{TKey, TValue, TKeyComparer}"/> class. </summary>
	/// <param name="capacity"> Initial capacity. </param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public PooledDictionary(int capacity)
		: this(capacity, default!) {
	}
	/// <summary> Gets the capacity of the dictionary. </summary>
	public readonly int Capacity => index.Length;
	/// <summary> Gets the number of elements contained in the dictionary. </summary>
	public readonly int Count => count - freeCount;
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int WrapAroundMask(int capacity) => (sizeof(int) << 3) - BitOperations.Log2((uint)capacity);
	/// <summary> Adds the provided <paramref name="key"/> and <paramref name="value"/>. </summary>
	/// <param name="key"> Key to add. </param>
	/// <param name="value"> Value to add. </param>
	/// <exception cref="ArgumentNullException"> <paramref name="key"/> is <c>null</c>. </exception>
	/// <exception cref="ArgumentException"> <paramref name="key"/> is already in the dictionary. </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(TKey key, TValue value) => _ = TryInsert(key, value, InsertionBehavior.ThrowOnExisting);

	public readonly IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
		var entries = this.entries;
		for( var i = 0; i < count; i++ ) {
			var entry = entries.GetElement(i);
			if( entry.Next >= EndOfIndex )
				yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryAdd(TKey key, TValue value) => TryInsert(key, value, InsertionBehavior.None);
	/// <summary> Scales the <paramref name="hashCode"/> to an offset with in <see cref="Capacity"/>">. </summary>
	/// <param name="hashCode"> Hash code to scale. </param>
	/// <param name="bitShift"> Number of bits to shift based on capacity. </param>
	/// <returns> Offset in the <see cref="index"/> array. </returns>
	/// <remarks> There is a posibility for collision and as such collision should be handled. </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int HashToIndex(int hashCode, int bitShift) => GoldenRatio * hashCode >> bitShift;
	/// <summary>
	/// Get power of 2 for given <paramref name="capacity"/>.
	/// </summary>
	/// <param name="capacity"> Requested capacity.</param>
	/// <returns> Power of 2 for given <paramref name="capacity"/>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int PowerOf2(int capacity) => BitOperations.IsPow2(capacity) ? capacity : (int)BitOperations.RoundUpToPowerOf2((uint)capacity);

	private unsafe void CopyEntries(Span<Entry<TKey, TValue>> oldEntries, Span<Entry<TKey, TValue>> newEntries, bool resizeMode) {
		var count = this.count;
		var newCount = 0;
		fixed( int* indexPointer = index ) {
			ref var entriesRef = ref newEntries.GetReference();
			ref var oldEntriesRef = ref oldEntries.GetReference();
			for( var i = 0; i < oldEntries.Length; i++ ) {
				ref var entry = ref Unsafe.Add(ref entriesRef, newCount);
				entry = Unsafe.Add(ref oldEntriesRef, i);
				if( entry.Next >= EndOfIndex ) {
					var bucket = GetBucket(indexPointer, entry.HashCode);
					entry.Next = *bucket;
					*bucket = resizeMode ? newCount : (count + newCount);
					newCount++;
				}
			}
		}
	}
	private unsafe void CopyEntries(Span<Entry<TKey, TValue>> oldEntries, Span<Entry<TKey, TValue>> newEntries) {
		fixed( int* indexPointer = index ) {
			ref var entriesRef = ref newEntries.GetReference();
			ref var oldEntriesRef = ref oldEntries.GetReference();
			for( var i = 0; i < oldEntries.Length; i++ ) {
				ref var entry = ref Unsafe.Add(ref entriesRef, i);
				entry = Unsafe.Add(ref oldEntriesRef, i);
				var bucket = GetBucket(indexPointer, entry.HashCode);
				entry.Next = *bucket;
				*bucket = i;
			}
		}
	}
	/// <inheritdoc/>
	// protected override void Dispose(bool disposing) {
	// 	if( disposing ) {
	// 		indexBuffer.Return(index);
	// 		index = default!;
	// 		entriesBuffer.Return(entries);
	// 		entries = default!;
	// 	}
	// }
	public void Dispose() {
		indexBuffer.Return(index);
		index = default!;
		entriesBuffer.Return(entries);
		entries = default!;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private readonly unsafe int* GetBucket(int* indexPtr, int hashCode) => indexPtr + ((uint)(GoldenRatio * hashCode) >> bitShift);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private readonly ref int GetBucket(int hashCode) => ref index.GetReference((uint)(GoldenRatio * hashCode) >> bitShift);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PrepareIndex(bool release = false) {
		var capacity = entries.Length;
		// index = new int[capacity];
		if( release )
			indexBuffer.Return(index);
		index = indexBuffer.Rent(capacity, EndOfIndex);
		bitShift = WrapAroundMask(capacity);
	}

	private void Resize(int capacity) {
		// ArgumentOutOfRangeException.ThrowIfLessThan(capacity, count - freeCount);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(capacity, Array.MaxLength - 1);
		var arr = entries;
		entries = entriesBuffer.Rent(capacity);
		PrepareIndex(true);
		if( count > 0 ) {
			if( freeCount > 0 ) {
				CopyEntries(arr.GetSpan(count), entries.GetSpan(), resizeMode: true);
			}
			CopyEntries(arr.GetSpan(0, count), entries.GetSpan());
		}
		entriesBuffer.Return(arr);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private TReturn? Search<TSuccess, TFailure, TReturn>(TKey key, TValue value, InsertionBehavior behavior) where TSuccess : struct, ISearchSuccess<TKey, TValue, TReturn> where TFailure : struct, ISearchFailure<TKey, TValue, TReturn> {
		if( !typeof(TKey).IsValueType )
			ArgumentNullException.ThrowIfNull(key);
		var elementOffset = 0;
		ref var entriesReference = ref entries.GetReference();
		var count = this.count;
		var hashCode = comparer.GetHashCode(key!);
		ref var primmaryReference = ref GetBucket(hashCode);
		var index = primmaryReference;
		while( (uint)index < (uint)count ) {
			ref var entry = ref Unsafe.Add(ref entriesReference, index);
			if( entry.HashCode == hashCode && comparer.Equals(entry.Key, key) ) {
				return default(TSuccess).Handle(key, value, behavior, ref primmaryReference, ref entry, ref Unsafe.Add(ref entriesReference, elementOffset));
			}
			elementOffset = index;
			index = entry.Next;
		};
		var result = default(TFailure).Handle(key, value, hashCode, ref primmaryReference, ref Unsafe.Add(ref entriesReference, count));
		primmaryReference = this.count++;
		return result;
	}

	private bool TryInsert(TKey key, TValue value, InsertionBehavior behavior) {
		if( count == entries.Length ) {
			Resize(count << 1);
		}
		return Search<InsertSuccess<TKey, TValue>, InsertFailure<TKey, TValue>, bool>(key, value, behavior);
	}
}
