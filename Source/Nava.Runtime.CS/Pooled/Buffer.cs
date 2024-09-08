using System.Buffers;

namespace Nava.Runtime.Pooled;

/// <summary> Interface for handling an <see cref="Array"/> buffer. </summary>
public interface IBuffer<T> {

	/// <summary> Creates an <see cref="IBuffer{T}"/> implementation based on <paramref name="pool"/>. </summary>
	/// <typeparam name="TBuffer">Type of <see cref="IBuffer{T}"/> implementation.</typeparam>
	/// <param name="pool"> <see cref="ArrayPool{T}"/> used for creating <see cref="IBuffer{T}"/> implementation. </param>
	/// <returns> <see cref="SharedBuffer{T}"/> if <paramref name="pool"/> is <see cref="ArrayPool{T}.Shared"/>, otherwise <see cref="PooledBuffer{T}"/></returns>
	public static TBuffer Create<TBuffer>(ArrayPool<T> pool) where TBuffer : struct, IBuffer<T> {
		if( pool == ArrayPool<T>.Shared ) {
			var buffer = new SharedBuffer<T>();
			return Unsafe.As<SharedBuffer<T>, TBuffer>(ref buffer);
		} else {
			var buffer = new PooledBuffer<T>(pool);
			return Unsafe.As<PooledBuffer<T>, TBuffer>(ref buffer);
		}
	}

	/// <summary> Returns an <see cref="Array"/> of double the size of <paramref name="array"/>. Copies the old <paramref name="array"/> to the new one. </summary>
	/// <param name="array">Array to grow.</param>
	/// <param name="index">Index to start to start copying data.</param>
	/// <param name="length">Number of elements to copy.</param>
	/// <param name="capacity">Capacity of the new <see cref="Array"/>.</param>
	/// <returns><see cref="Array"/> with double the size of <paramref name="array"/>.</returns>
	/// <remarks> No checks are done to validate <paramref name="array"/>. </remarks>
	abstract T[] Grow(T[] array, int index, int length, int capacity);

	/// <summary> Returns an <see cref="Array"/> of size <paramref name="capacity"/>. Copies the old <paramref name="array"/> to the new one. </summary>
	/// <param name="array">Array to grow.</param>
	/// <param name="capacity">Capacity of the new <see cref="Array"/>.</param>
	/// <returns><see cref="Array"/> of size <paramref name="capacity"/>.</returns>
	/// <remarks> No checks are done to validate <paramref name="array"/>. </remarks>
	abstract T[] Grow(T[] array, int capacity);

	/// <summary> Returns an <see cref="Array"/> of double the size of <paramref name="array"/>. Copies the old <paramref name="array"/> to the new one. </summary>
	/// <param name="array">Array to grow.</param>
	/// <returns><see cref="Array"/> with double the size of <paramref name="array"/>.</returns>
	/// <remarks> No checks are done to validate <paramref name="array"/>. </remarks>
	abstract T[] Grow(T[] array);

	/// <summary> Gets an <see cref="Array"/> of <typeparamref name="T"/> with the specified <paramref name="capacity"/>. </summary>
	/// <param name="capacity">Capacity of the <see cref="Array"/>.</param>
	/// <param name="clear"><see langword="true"/> if the <see cref="Array"/> should be cleared; otherwise, <see langword="false"/>.</param>
	/// <returns><see cref="Array"/> of <typeparamref name="T"/>.</returns>
	abstract T[] Rent(int capacity, bool clear);

	/// <summary> Gets an <see cref="Array"/> of <typeparamref name="T"/> with the specified <paramref name="capacity"/>. </summary>
	/// <param name="capacity">Capacity of the <see cref="Array"/>.</param>
	/// <param name="clear"><see langword="true"/> if the <see cref="Array"/> should be cleared; otherwise, <see langword="false"/>.</param>
	/// <returns><see cref="Array"/> of <typeparamref name="T"/>.</returns>
	abstract T[] Rent(uint capacity, bool clear);

	/// <summary> Gets an <see cref="Array"/> of <typeparamref name="T"/> with the specified <paramref name="capacity"/>. </summary>
	/// <param name="capacity">Capacity of the <see cref="Array"/>.</param>
	/// <returns><see cref="Array"/> of <typeparamref name="T"/>.</returns>
	abstract T[] Rent(int capacity);

	/// <summary> Gets an <see cref="Array"/> of <typeparamref name="T"/> with the specified <paramref name="capacity"/>. </summary>
	/// <param name="capacity">Capacity of the <see cref="Array"/>.</param>
	/// <returns><see cref="Array"/> of <typeparamref name="T"/>.</returns>
	abstract T[] Rent(uint capacity);

	/// <summary> Gets an <see cref="Array"/> of <typeparamref name="T"/> with the specified <paramref name="capacity"/>. </summary>
	/// <param name="capacity">Capacity of the <see cref="Array"/>.</param>
	/// <param name="value">Value to fill in the <see cref="Array"/>.</param>
	/// <returns><see cref="Array"/> of <typeparamref name="T"/>.</returns>
	abstract T[] Rent(int capacity, T value);

	/// <summary> Gets an <see cref="Array"/> of <typeparamref name="T"/> with the specified <paramref name="capacity"/>. </summary>
	/// <param name="capacity">Capacity of the <see cref="Array"/>.</param>
	/// <param name="value">Value to fill in the <see cref="Array"/>.</param>
	/// <returns><see cref="Array"/> of <typeparamref name="T"/>.</returns>
	abstract T[] Rent(uint capacity, T value);

	/// <summary> Returns the specified <paramref name="array"/> back to the pool. </summary>
	/// <param name="array">Array to return.</param>
	abstract void Return(T[] array);
}

/// <summary> Buffer implementation using specified <see cref="ArrayPool{T}"/>. </summary>
public readonly struct PooledBuffer<T>(ArrayPool<T> pool) : IBuffer<T> {

	/// <summary> Gets the <see cref="ArrayPool{T}"/> for this <see cref="PooledBuffer{T}"/>. </summary>
	public ArrayPool<T> Pool { get; } = pool;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Grow(T[] array, int index, int length, int capacity) {
		var result = Pool.Rent(capacity);
		Array.Copy(array, index, result, 0, length);
		Return(array);
		return result;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Grow(T[] array, int capacity) => Grow(array, 0, array.Length, capacity);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly T[] Grow(T[] array) => Grow(array, array.Length << 1);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Rent(int capacity) => Pool.Rent(capacity);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Rent(uint capacity) => Rent((int)capacity);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Rent(int capacity, T value) {
		var array = Rent(capacity);
		MemoryHelpers.GetSpan(array).Fill(value);
		return array;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Rent(uint capacity, T value) => Rent((int)capacity, value);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Rent(int capacity, bool clear) {
		var array = Pool.Rent(capacity);
		if( clear ) {
			MemoryHelpers.GetSpan(array).Clear();
		}
		return array;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Rent(uint capacity, bool clear) => Rent((int)capacity, clear);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly void Return(T[] array) => Pool.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
}

/// <summary> Buffer implementation using shared <see cref="ArrayPool{T}"/>. </summary>
[StructLayout(LayoutKind.Sequential, Size = 0)]
public struct SharedBuffer<T> : IBuffer<T> {

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Grow(T[] array, int index, int length, int capacity) {
		var result = ArrayPool<T>.Shared.Rent(capacity);
		Array.Copy(array, index, result, 0, length);
		Return(array);
		return result;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Grow(T[] array, int capacity) => Grow(array, 0, array.Length, capacity);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Grow(T[] array) => Grow(array, array.Length << 1);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Rent(int capacity) => ArrayPool<T>.Shared.Rent(capacity);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Rent(uint capacity) => Rent((int)capacity);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Rent(int capacity, T value) {
		var array = Rent(capacity);
		MemoryHelpers.GetSpan(array).Fill(value);
		return array;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Rent(uint capacity, T value) => Rent((int)capacity, value);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Rent(int capacity, bool clear) {
		var array = ArrayPool<T>.Shared.Rent(capacity);
		if( clear ) {
			MemoryHelpers.GetSpan(array).Clear();
		}
		return array;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly T[] Rent(uint capacity, bool clear) => Rent((int)capacity, clear);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly void Return(T[] array) => ArrayPool<T>.Shared.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
}
/// <summary> Extension methods for <see cref="ArrayPool{T}"/>. </summary>
public static class ArrayPoolExtensions {

	/// <summary> Grows <paramref name="array"/> by <paramref name="capacity"/> and copies elements. </summary>
	/// <typeparam name="T"> Type of the array elements. </typeparam>
	/// <param name="pool"> <see cref="ArrayPool{T}"/> used for renting new <see cref="Array"/>. </param>
	/// <param name="array"> <see cref="Array"/> to grow.</param>
	/// <param name="index"> Index to start copying from.</param>
	/// <param name="length"> Number of elements to copy.</param>
	/// <param name="capacity"> New capacity.</param>
	/// <returns> Rented <see cref="Array"/>. </returns>
	/// <exception cref="ArgumentNullException"> <paramref name="pool"/> or <paramref name="array"/> is <see langword="null"/>. </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] Grow<T>(this ArrayPool<T> pool, T[] array, int index, int length, int capacity) {
		var result = pool.Rent(capacity);
		Array.Copy(array, index, result, 0, length);
		Return(pool, array);
		return result;
	}

	/// <summary> Grows <paramref name="array"/> by <paramref name="capacity"/> and copies elements. </summary>
	/// <typeparam name="T"> Type of the array elements. </typeparam>
	/// <param name="pool"> <see cref="ArrayPool{T}"/> used for renting new <see cref="Array"/>. </param>
	/// <param name="array"> <see cref="Array"/> to grow.</param>
	/// <param name="capacity"> New capacity.</param>
	/// <returns> Rented <see cref="Array"/>. </returns>
	/// <exception cref="ArgumentNullException"> <paramref name="pool"/> or <paramref name="array"/> is <see langword="null"/>. </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] Grow<T>(this ArrayPool<T> pool, T[] array, int capacity) => pool.Grow(array, 0, array.Length, capacity);

	/// <summary> Grows <paramref name="array"/> by double its size and copies elements. </summary>
	/// <typeparam name="T"> Type of the array elements. </typeparam>
	/// <param name="pool"> <see cref="ArrayPool{T}"/> used for renting new <see cref="Array"/>. </param>
	/// <param name="array"> <see cref="Array"/> to grow.</param>
	/// <returns> Rented <see cref="Array"/>. </returns>
	/// <exception cref="ArgumentNullException"> <paramref name="pool"/> or <paramref name="array"/> is <see langword="null"/>. </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] Grow<T>(this ArrayPool<T> pool, T[] array) => pool.Grow(array, array.Length << 1);

	/// <summary> Rent an <see cref="Array"/> of <typeparamref name="T"/> and fills it with <paramref name="value"/>. </summary>
	/// <typeparam name="T"> Type of the array elements. </typeparam>
	/// <param name="pool"> <see cref="ArrayPool{T}"/> used for renting new <see cref="Array"/>. </param>
	/// <param name="capacity"> Minimum size of <see cref="Array"/> to rent.</param>
	/// <param name="value"> Value to fill <see cref="Array"/> with.</param>
	/// <returns> Rented <see cref="Array"/>. </returns>
	/// <exception cref="ArgumentNullException"> <paramref name="pool"/> is <see langword="null"/>. </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] Rent<T>(this ArrayPool<T> pool, int capacity, T value) {
		var array = pool.Rent(capacity);
		MemoryHelpers.GetSpan(array).Fill(value);
		return array;
	}

	/// <summary> Rent an <see cref="Array"/> of <typeparamref name="T"/> and fills it with <paramref name="value"/>. </summary>
	/// <typeparam name="T"> Type of the array elements. </typeparam>
	/// <param name="pool"> <see cref="ArrayPool{T}"/> used for renting new <see cref="Array"/>. </param>
	/// <param name="capacity"> Minimum size of <see cref="Array"/> to rent.</param>
	/// <param name="value"> Value to fill <see cref="Array"/> with.</param>
	/// <returns> Rented <see cref="Array"/>. </returns>
	/// <exception cref="ArgumentNullException"> <paramref name="pool"/> is <see langword="null"/>. </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] Rent<T>(this ArrayPool<T> pool, uint capacity, T value) => pool.Rent((int)capacity, value);

	/// <summary> Rent an <see cref="Array"/> of <typeparamref name="T"/>. </summary>
	/// <typeparam name="T"> Type of the array elements. </typeparam>
	/// <param name="pool"> <see cref="ArrayPool{T}"/> used for renting new <see cref="Array"/>. </param>
	/// <param name="capacity"> Minimum size of <see cref="Array"/> to rent.</param>
	/// <returns> Rented <see cref="Array"/>. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] Rent<T>(this ArrayPool<T> pool, uint capacity) => pool.Rent((int)capacity);
	/// <summary> Rents an <see cref="Array"/> of <typeparamref name="T"/> and clears it. </summary>
	/// <typeparam name="T"> Type of the array elements. </typeparam>
	/// <param name="pool"> <see cref="ArrayPool{T}"/> used for renting new <see cref="Array"/>. </param>
	/// <param name="capacity"> Minimum size of <see cref="Array"/> to rent.</param>
	/// <param name="clear"> <see langword="true"/> if <see cref="Array"/> should be cleared, otherwise <see langword="false"/>. </param>
	/// <returns></returns>

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] Rent<T>(this ArrayPool<T> pool, int capacity, bool clear) {
		var array = pool.Rent(capacity);
		if( clear ) {
			array.GetSpan().Clear();
		}
		return array;
	}

	/// <summary> Rents an <see cref="Array"/> of <typeparamref name="T"/> and clears it. </summary>
	/// <typeparam name="T"> Type of the array elements. </typeparam>
	/// <param name="pool"> <see cref="ArrayPool{T}"/> used for renting new <see cref="Array"/>. </param>
	/// <param name="capacity"> Minimum size of <see cref="Array"/> to rent.</param>
	/// <param name="clear"> <see langword="true"/> if <see cref="Array"/> should be cleared, otherwise <see langword="false"/>.</param>
	/// <returns> Rented <see cref="Array"/>. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] Rent<T>(this ArrayPool<T> pool, uint capacity, bool clear) => pool.Rent((int)capacity, clear);

	/// <summary> Returns an <see cref="Array"/> of <typeparamref name="T"/>. </summary>
	/// <typeparam name="T"> Type of the array elements. </typeparam>
	/// <param name="pool"> <see cref="ArrayPool{T}"/> used for returning <see cref="Array"/>. </param>
	/// <param name="array"> <see cref="Array"/> to return.</param>
	/// <exception cref="ArgumentNullException"> <paramref name="pool"/> or <paramref name="array"/> is <see langword="null"/>. </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Return<T>(this ArrayPool<T> pool, T[] array) => pool.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
}
