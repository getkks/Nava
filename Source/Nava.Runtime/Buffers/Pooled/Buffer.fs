namespace Nava.Runtime.Pooled

open System
open System.Buffers
open System.Runtime.CompilerServices

open type Nava.Runtime.MemoryHelpers

/// <summary> Extension methods for <see cref="T:System.Buffers.ArrayPool`1" />. </summary>
[<Extension>]
type ArrayPool =
    /// <summary> Returns an <see cref="T:System.Array" /> of <typeparamref name="T" />. </summary>
    /// <typeparam name="T"> Type of the array elements. </typeparam>
    /// <param name="pool"> <see cref="T:System.Buffers.ArrayPool`1" /> used for returning <see cref="T:System.Array" />. </param>
    /// <param name="array"> <see cref="T:System.Array" /> to return.</param>
    /// <exception cref="T:System.ArgumentNullException"> <paramref name="pool" /> or <paramref name="array" /> is <see langword="null" />. </exception>
    [<MethodImpl(MethodImplOptions.AggressiveInlining); Extension>]
    static member Return(pool: 'T ArrayPool, array) =
        pool.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<'T>())

    /// <summary> Grows <paramref name="array" /> by <paramref name="capacity" /> and copies elements. </summary>
    /// <typeparam name="T"> Type of the array elements. </typeparam>
    /// <param name="pool"> <see cref="T:System.Buffers.ArrayPool`1" /> used for renting new <see cref="T:System.Array" />. </param>
    /// <param name="array"> <see cref="T:System.Array" /> to grow.</param>
    /// <param name="index"> Index to start copying from.</param>
    /// <param name="length"> Number of elements to copy.</param>
    /// <param name="capacity"> New capacity.</param>
    /// <returns> Rented <see cref="T:System.Array" />. </returns>
    /// <exception cref="T:System.ArgumentNullException"> <paramref name="pool" /> or <paramref name="array" /> is <see langword="null" />. </exception>
    [<MethodImpl(MethodImplOptions.AggressiveInlining); Extension>]
    static member Grow(pool: 'T ArrayPool, array, index, length, capacity) =
        let result = pool.Rent(capacity)
        Array.Copy(array, index, result, 0, length)
        ArrayPool.Return(pool, array)
        result

    /// <summary> Grows <paramref name="array" /> by <paramref name="capacity" /> and copies elements. </summary>
    /// <typeparam name="T"> Type of the array elements. </typeparam>
    /// <param name="pool"> <see cref="T:System.Buffers.ArrayPool`1" /> used for renting new <see cref="T:System.Array" />. </param>
    /// <param name="array"> <see cref="T:System.Array" /> to grow.</param>
    /// <param name="capacity"> New capacity.</param>
    /// <returns> Rented <see cref="T:System.Array" />. </returns>
    /// <exception cref="T:System.ArgumentNullException"> <paramref name="pool" /> or <paramref name="array" /> is <see langword="null" />. </exception>
    [<MethodImpl(MethodImplOptions.AggressiveInlining); Extension>]
    static member Grow(pool: 'T ArrayPool, array: _[], capacity) =
        pool.Grow(array, 0, array.Length, capacity)

    /// <summary> Grows <paramref name="array" /> by double its size and copies elements. </summary>
    /// <typeparam name="T"> Type of the array elements. </typeparam>
    /// <param name="pool"> <see cref="T:System.Buffers.ArrayPool`1" /> used for renting new <see cref="T:System.Array" />. </param>
    /// <param name="array"> <see cref="T:System.Array" /> to grow.</param>
    /// <returns> Rented <see cref="T:System.Array" />. </returns>
    /// <exception cref="T:System.ArgumentNullException"> <paramref name="pool" /> or <paramref name="array" /> is <see langword="null" />. </exception>
    [<MethodImpl(MethodImplOptions.AggressiveInlining); Extension>]
    static member Grow(pool: 'T ArrayPool, array: _[]) = pool.Grow(array, array.Length <<< 1)

    /// <summary> Rent an <see cref="T:System.Array" /> of <typeparamref name="T" /> and fills it with <paramref name="value" />. </summary>
    /// <typeparam name="T"> Type of the array elements. </typeparam>
    /// <param name="pool"> <see cref="T:System.Buffers.ArrayPool`1" /> used for renting new <see cref="T:System.Array" />. </param>
    /// <param name="capacity"> Minimum size of <see cref="T:System.Array" /> to rent.</param>
    /// <param name="value"> Value to fill <see cref="T:System.Array" /> with.</param>
    /// <returns> Rented <see cref="T:System.Array" />. </returns>
    /// <exception cref="T:System.ArgumentNullException"> <paramref name="pool" /> is <see langword="null" />. </exception>
    [<MethodImpl(MethodImplOptions.AggressiveInlining); Extension>]
    static member Rent(pool: 'T ArrayPool, capacity: int, value: 'T) =
        let array = pool.Rent(capacity, value)
        array.GetSpan().Fill value
        array

    /// <summary> Rent an <see cref="T:System.Array" /> of <typeparamref name="T" /> and fills it with <paramref name="value" />. </summary>
    /// <typeparam name="T"> Type of the array elements. </typeparam>
    /// <param name="pool"> <see cref="T:System.Buffers.ArrayPool`1" /> used for renting new <see cref="T:System.Array" />. </param>
    /// <param name="capacity"> Minimum size of <see cref="T:System.Array" /> to rent.</param>
    /// <param name="value"> Value to fill <see cref="T:System.Array" /> with.</param>
    /// <returns> Rented <see cref="T:System.Array" />. </returns>
    /// <exception cref="T:System.ArgumentNullException"> <paramref name="pool" /> is <see langword="null" />. </exception>
    [<MethodImpl(MethodImplOptions.AggressiveInlining); Extension>]
    static member Rent(pool: 'T ArrayPool, capacity: uint, value: 'T) = pool.Rent(int capacity, value)

    /// <summary> Rent an <see cref="T:System.Array" /> of <typeparamref name="T" />. </summary>
    /// <typeparam name="T"> Type of the array elements. </typeparam>
    /// <param name="pool"> <see cref="T:System.Buffers.ArrayPool`1" /> used for renting new <see cref="T:System.Array" />. </param>
    /// <param name="capacity"> Minimum size of <see cref="T:System.Array" /> to rent.</param>
    /// <returns> Rented <see cref="T:System.Array" />. </returns>
    /// <exception cref="T:System.ArgumentNullException"> <paramref name="pool" /> is <see langword="null" />. </exception>
    [<MethodImpl(MethodImplOptions.AggressiveInlining); Extension>]
    static member Rent(pool: 'T ArrayPool, capacity: uint) = int capacity |> pool.Rent

    /// <summary> Rents an <see cref="T:System.Array" /> of <typeparamref name="T" /> and clears it. </summary>
    /// <typeparam name="T"> Type of the array elements. </typeparam>
    /// <param name="pool"> <see cref="T:System.Buffers.ArrayPool`1" /> used for renting new <see cref="T:System.Array" />. </param>
    /// <param name="capacity"> Minimum size of <see cref="T:System.Array" /> to rent.</param>
    /// <param name="clear"> <see langword="true" /> if <see cref="T:System.Array" /> should be cleared, otherwise <see langword="false" />. </param>
    /// <returns> Rented <see cref="T:System.Array" />. </returns>
    /// <exception cref="T:System.ArgumentNullException"> <paramref name="pool" /> is <see langword="null" />. </exception>
    [<MethodImpl(MethodImplOptions.AggressiveInlining); Extension>]
    static member Rent(pool: 'T ArrayPool, capacity: int, clear: bool) =
        let array = pool.Rent(capacity)

        if clear then
            array.GetSpan().Clear()

        array

    /// <summary> Rents an <see cref="T:System.Array" /> of <typeparamref name="T" /> and clears it. </summary>
    /// <typeparam name="T"> Type of the array elements. </typeparam>
    /// <param name="pool"> <see cref="T:System.Buffers.ArrayPool`1" /> used for renting new <see cref="T:System.Array" />. </param>
    /// <param name="capacity"> Minimum size of <see cref="T:System.Array" /> to rent.</param>
    /// <param name="clear"> <see langword="true" /> if <see cref="T:System.Array" /> should be cleared, otherwise <see langword="false" />. </param>
    /// <returns> Rented <see cref="T:System.Array" />. </returns>
    /// <exception cref="T:System.ArgumentNullException"> <paramref name="pool" /> is <see langword="null" />. </exception>
    [<MethodImpl(MethodImplOptions.AggressiveInlining); Extension>]
    static member Rent(pool: 'T ArrayPool, capacity: uint, clear: bool) = pool.Rent(int capacity, clear)
