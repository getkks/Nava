namespace Nava.Collections.Pooled

open System
open System.Runtime.CompilerServices

open Nava.Runtime
open Nava.Runtime.Pooled

type Structure<'T when 'T: (new: unit -> 'T) and 'T: struct and 'T :> ValueType> = 'T

/// <summary> List implementation using <see cref="T:System.Buffers.ArrayPool`1" /> for storing elements. This implementation is based on <see cref="T:System.Collections.Generic.List`1" /> and <see href="https://github.com/jtmueller/Collections.Pooled" />.</summary>
/// <typeparam name="T">Type of element.</typeparam>
[<Struct; NoComparison; NoEquality>]
type List<'T, 'TBuffer when Structure<'TBuffer> and 'TBuffer :> IBuffer<'T>> =
    val mutable internal pool: 'TBuffer //'T Buffers.ArrayPool
    val mutable internal count: int
    val mutable internal array: 'T[]

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    internal new(array, count, pool: 'TBuffer) = //_ Buffers.ArrayPool) =
        {
            count = count
            array = array
            pool = pool
        }

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    new(capacity: int, pool: 'TBuffer) = List(pool.Rent capacity, 0, pool)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    new(pool: 'TBuffer) = List(4, pool)

    /// <summary>Creates a new instance of <see cref="T:Nava.Collections.Pooled.List`1" /> with the specified capacity.</summary>
    /// <param name="capacity">Initial capacity.</param>
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    new(capacity) = List(capacity, Unchecked.defaultof<'TBuffer>) //Buffers.ArrayPool.Shared)

    /// <summary>Creates a new instance of <see cref="T:Nava.Collections.Pooled.List`1" /> with the specified span.</summary>
    /// <param name="span">Span of elements.</param>
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    new(span: 'T ReadOnlySpan) =
        let pool = Unchecked.defaultof<'TBuffer> // Buffers.ArrayPool.Shared
        let array = pool.Rent span.Length
        span.CopyTo(MemoryHelpers.GetSpan(array, 0, span.Length))

        {
            count = span.Length
            array = array
            pool = pool
        }

    /// <summary>Creates a new instance of <see cref="T:Nava.Collections.Pooled.List`1" /> with the specified list.</summary>
    /// <param name="list">List of elements.</param>
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    new(list: List<_, 'TBuffer>) = List(MemoryHelpers.GetReadOnlySpan(list.array, 0, list.count))

    /// <summary>Gets the number of elements contained in the <see cref="T:Nava.Collections.Pooled.List`1" />.</summary>
    /// <returns>The number of elements contained in the <see cref="T:Nava.Collections.Pooled.List`1" />.</returns>
    member this.Count = this.count
    /// <summary>Gets a value indicating whether the <see cref="T:Nava.Collections.Pooled.List`1" /> is read-only.</summary>
    member this.IsReadOnly = false

    /// <summary> Gets a <see cref="T:System.Span`1" /> of elements contained in <see cref="T:Nava.Collections.Pooled.List`1" />.</summary>
    /// <returns><see cref="T:System.Span`1" /> of elements.</returns>
    member this.Span = this.array.GetSpan(this.count)

    /// <summary>Gets or sets the element at the specified index.</summary>
    /// <returns>The element at the specified index.</returns>
    member this.Item
        with get index = this.array[index]
        and set index value = this.array[index] <- value

    /// <summary>Gets the number of elements contained in the <see cref="T:Nava.Collections.Pooled.List`1" />.</summary>
    /// <param name="item">Value to add.</param>
    [<MethodImpl(MethodImplOptions.AggressiveInlining
                 ||| MethodImplOptions.AggressiveOptimization)>]
    member this.Add item =
        let count = this.count
        let mutable array = this.array

        if count = array.Length then
            array <- this.pool.Grow array
            this.array <- array

        array.GetReference(count) <- item
        this.count <- count + 1

    /// <summary>Adds the elements of the given <see cref="T:System.Collections.Generic.IEnumerable`1" /> at the end of <see cref="T:Nava.Collections.Pooled.List`1" />.</summary>
    /// <param name="sequence">Sequence of elements.</param>
    member this.AddRange(sequence: 'T seq) =
        ArgumentNullException.ThrowIfNull sequence
        let enumerator = sequence.GetEnumerator()

        try
            while enumerator.MoveNext() do
                this.Add enumerator.Current
        finally
            enumerator.Dispose()

    /// <summary>Adds the elements of the given <see cref="T:System.Collections.Generic.ICollection`1" /> at the end of <see cref="T:Nava.Collections.Pooled.List`1" />.</summary>
    /// <param name="collection">Collection of elements.</param>
    member this.AddRange(collection: 'T Collections.Generic.ICollection) =
        ArgumentNullException.ThrowIfNull collection
        let mutable count = this.count
        let newCount = count + collection.Count
        let mutable array = this.array

        if array.Length <= newCount then
            array <- this.pool.Grow(array, newCount)
            this.array <- array

        let reference = &array.GetReference()
        let enumerator = collection.GetEnumerator()

        try
            while enumerator.MoveNext() do
                Unsafe.Add(&reference, count) <- enumerator.Current
                count <- count + 1
        finally
            enumerator.Dispose()

        this.count <- newCount

    /// <summary>Adds the elements of the given <see cref="T:System.ReadOnlySpan`1" /> at the end of <see cref="T:Nava.Collections.Pooled.List`1" />.</summary>
    /// <param name="span">Span of elements.</param>
    member this.AddRange(span: 'T ReadOnlySpan) =
        let mutable count = this.count
        let newCount = count + span.Length
        this.array <- this.pool.Grow(this.array, newCount)
        span.CopyTo(MemoryHelpers.GetSpan(this.array, count, span.Length))
        this.count <- newCount

    /// <summary>Adds the elements of the given <see cref="T:System.Array" /> at the end of <see cref="T:Nava.Collections.Pooled.List`1" />.</summary>
    /// <param name="array">Array of elements.</param>
    member this.AddRange(array: 'T[]) =
        ArgumentNullException.ThrowIfNull array
        this.AddRange(array: 'T ReadOnlySpan)

    /// <summary> Disposes the <see cref="T:Nava.Runtime.Pooled.Collections.PooledList`1" /> by returning it to the <see cref="T:Nava.Runtime.Pooled.SharedBuffer`1" />. </summary>
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.Dispose() =
        this.pool.Return this.array
        this.array <- Unchecked.defaultof<_>

    member this.GetSpan(index, count) =
        ArgumentOutOfRangeException.ThrowIfNegative index
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero count

        if index + count > this.count then
            IndexOutOfRangeException() |> raise

        MemoryHelpers.GetSpan(this.array, index, count)