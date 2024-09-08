using System.Buffers;
namespace Nava.Runtime.Pooled.Collections;
/// <summary> List implementation using <see cref="ArrayPool{T}"/> for storing elements. This implementation is based on <see cref="List{T}"/> and <see href="https://github.com/jtmueller/Collections.Pooled"/>.</summary>
/// <typeparam name="T">Type of element.</typeparam>
public struct PooledList<T> {
	/// <summary> Initial capacity of the <see cref="PooledList{T}"/>. </summary>
	public const int InitialCapacity = 16;
	internal readonly SharedBuffer<T> pool;

	internal T[] array;

	internal int count = 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public PooledList(int capacity, SharedBuffer<T> pool) {
		this.pool = pool;
		array = pool.Rent(capacity);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public PooledList(SharedBuffer<T> pool)
		: this(InitialCapacity, pool) {
	}

	/// <summary>Creates a new instance of <see cref="PooledList{T}" /> with the specified capacity.</summary>
	/// <param name="capacity">Initial capacity.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public PooledList(int capacity)
		: this(capacity, new SharedBuffer<T>()) {
	}

	/// <summary>Creates a new instance of <see cref="PooledList{T}" /> with the specified span.</summary>
	/// <param name="span">Span of elements.</param>
	public PooledList(ReadOnlySpan<T> span) : this(span.Length) {
		span.CopyTo(array.GetSpan(0, span.Length));
		count = span.Length;
	}

	/// <summary>Creates a new instance of <see cref="PooledList{T}" /> with the specified list.</summary>
	/// <param name="list">List of elements.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public PooledList(PooledList<T> list)
		: this(list.array.GetReadOnlySpan(0, list.count)) {
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal PooledList(T[] array, int count, SharedBuffer<T> pool) {
		this.pool = pool;
		this.count = count;
		this.array = array;
	}

	/// <summary>Gets the number of elements contained in the <see cref="PooledList{T}" />.</summary>
	/// <returns>The number of elements contained in the <see cref="PooledList{T}" />.</returns>
	public readonly int Count => count;

	/// <summary>Gets a value indicating whether the <see cref="PooledList{T}" /> is read-only.</summary>
	public readonly bool IsReadOnly => false;

	/// <summary> Gets a <see cref="Span{T}" /> of elements contained in <see cref="PooledList{T}" />.</summary>
	/// <returns><see cref="Span{T}" /> of elements.</returns>
	public readonly Span<T> Span => array.GetSpan(count);

	/// <summary>Gets or sets the element at the specified index.</summary>
	/// <returns>The element at the specified index.</returns>
	public readonly T this[int index] {
		get => array[index];
		set => array[index] = value;
	}

	/// <summary>Gets the number of elements contained in the <see cref="PooledList{T}" />.</summary>
	/// <param name="item">Value to add.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(T item) {
		var count = this.count;
		var array = this.array;
		if( count == array.Length ) {
			this.array = array = pool.Grow(array);
		}
		array.GetReference(count) = item;
		this.count = count + 1;
	}

	/// <summary>Adds the elements of the given <see cref="IEnumerable{T}" /> at the end of <see cref="PooledList{T}" />.</summary>
	/// <param name="sequence">Sequence of elements.</param>
	public void AddRange(IEnumerable<T> sequence) {
		ArgumentNullException.ThrowIfNull(sequence);
		foreach( var item in sequence ) {
			Add(item);
		}
	}

	/// <summary>Adds the elements of the given <see cref="ICollection{T}" /> at the end of <see cref="PooledList{T}" />.</summary>
	/// <param name="collection">Collection of elements.</param>
	public void AddRange(ICollection<T> collection) {
		ArgumentNullException.ThrowIfNull(collection);
		var count = this.count;
		var newCount = count + collection.Count;
		if( array.Length <= newCount ) {
			array = pool.Grow(array, newCount);
		}
		ref var reference = ref array.GetReference();
		foreach( var item in collection ) {
			Unsafe.Add(ref reference, count) = item;
			++count;
		}
		this.count = newCount;
	}

	/// <summary>Adds the elements of the given <see cref="ReadOnlySpan{T}" /> at the end of <see cref="PooledList{T}" />.</summary>
	/// <param name="span">Span of elements.</param>
	public void AddRange(ReadOnlySpan<T> span) {
		var count = this.count;
		var newCount = count + span.Length;
		if( array.Length <= newCount )
			array = pool.Grow(array, newCount);
		span.CopyTo(array.GetSpan(count, span.Length));
		this.count = newCount;
	}

	/// <summary>Adds the elements of the given <see cref="T:System.Array" /> at the end of <see cref="PooledList{T}" />.</summary>
	/// <param name="array">Array of elements.</param>
	public void AddRange(T[] array) {
		ArgumentNullException.ThrowIfNull(array);
		AddRange((ReadOnlySpan<T>)array);
	}
	/// <summary>
	/// Disposes the <see cref="PooledList{T}"/> by returning it to the <see cref="SharedBuffer{T}"/>.
	/// </summary>
	public void Dispose() {
		pool.Return(array);
		array = default!;
	}

	public Span<T> GetSpan(int index, int length) {
		ArgumentOutOfRangeException.ThrowIfNegative(index);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(index + length, count);
		return array.GetSpan(index, length);
	}
}
