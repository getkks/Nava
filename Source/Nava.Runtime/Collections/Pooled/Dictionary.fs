namespace Nava.Collections.Pooled

open System
open System.Buffers
open System.Numerics
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open System.Runtime.Intrinsics

open Nava.Runtime
open Nava.Runtime.Pooled
open Nava.Collections.Pooled.Dictionary

open InlineIL

type OperationSuccess<'TStore, 'TKey, 'TValue, 'TStoreAccess, 'TSuccess, 'TReturn
    when Structure<'TSuccess>
    and 'TSuccess :> ISearchSuccess<'TStore, 'TKey, 'TValue, 'TStoreAccess, 'TReturn>
    and StoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess>> = 'TSuccess

type OperationFailure<'TStore, 'TKey, 'TValue, 'TStoreAccess, 'TFailure, 'TReturn
    when Structure<'TFailure>
    and 'TFailure :> ISearchFailure<'TStore, 'TKey, 'TValue, 'TStoreAccess, 'TReturn>
    and StoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess>> = 'TFailure

module DictionaryModule =
    /// <summary> End of index. </summary>
    [<Literal>]
    let EndOfIndex = -1

    /// <summary> Golden ratio. </summary>
    [<Literal>]
    let GoldenRatio = -1640531527

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    let hashToIndex hashCode bitShift =
        ((uint GoldenRatio * uint hashCode) >>> bitShift) |> int

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    let powerOf2(capacity: int) =
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity)

        if BitOperations.IsPow2 capacity then
            capacity
        else
            ArgumentOutOfRangeException.ThrowIfGreaterThan(capacity, Array.MaxLength >>> 1)
            capacity |> uint |> BitOperations.RoundUpToPowerOf2 |> int

    [<MethodImpl(MethodImplOptions.AggressiveInlining
                 ||| MethodImplOptions.AggressiveOptimization)>]
    let wrapAroundMask(capacity: int) =
        (sizeof<int> <<< 3) - (capacity |> uint |> BitOperations.Log2)

// [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
// let prepareIndex(state: DictionaryBase<'TKey, 'TValue, 'TKeyComparer, 'TEntryBuffer, 'TIndexBuffer> byref, release) =
//     let capacity = state.entries.Length

//     if release then
//         state.indexBuffer.Return state.index

//     state.index <- state.indexBuffer.Rent capacity
//     state.bitShift <- wrapAroundMask capacity
[<Struct; NoComparison; NoEquality>]
type DictionaryBase<'TKey, 'TValue, 'TKeyComparer, 'TEntryBuffer, 'TIndexBuffer, 'TStore, 'TStoreAccess
    when Structure<'TKeyComparer>
    and 'TKeyComparer :> Collections.Generic.IEqualityComparer<'TKey>
    and Structure<'TEntryBuffer>
    and 'TEntryBuffer :> IBuffer<Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess>> //IBuffer<KeyValueStore<'TKey, 'TValue>>
    and Structure<'TIndexBuffer>
    and 'TIndexBuffer :> IBuffer<int>
    and 'TStore: struct
    and Structure<'TStoreAccess>
    and 'TStoreAccess :> IStoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess>> =
    // inherit Disposable
    /// <summary> Number of elements contained in the dictionary. </summary>
    val mutable internal count: int
    val mutable internal freeCount: int
    /// <summary> Hash comparer. </summary>
    val mutable internal comparer: 'TKeyComparer
    /// <summary> Entries buffer. </summary>
    val mutable internal entries: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess>[]
    val mutable internal entriesBuffer: 'TEntryBuffer
    val mutable internal indexBuffer: 'TIndexBuffer
    /// <summary> Number of bits to shift based on capacity. </summary>
    val mutable internal bitShift: int
    /// <summary> Index buffer. </summary>
    val mutable internal index: int[]

    new(capacity: int, comparer: 'TKeyComparer, entriesBuffer: 'TEntryBuffer, indexBuffer: 'TIndexBuffer) =
        let entries = capacity |> DictionaryModule.powerOf2 |> entriesBuffer.Rent
        entries.Length |> IL.Push
        IL.Emit.Starg(nameof capacity)

        {
            bitShift = DictionaryModule.wrapAroundMask capacity
            comparer = comparer |> Nava.Collections.HashHelpers.ApplyComparer
            count = 0
            entries = entries
            entriesBuffer = entriesBuffer
            freeCount = 0
            index = indexBuffer.Rent(capacity, DictionaryModule.EndOfIndex)
            indexBuffer = indexBuffer

        }

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    new(capacity, comparer) =
        if sizeof<'TEntryBuffer> <> 1 || sizeof<'TIndexBuffer> <> 1 then
            ThrowHelpers.ThrowNotSupportedException Argument_TypeNotSupported

        DictionaryBase(capacity, comparer, new 'TEntryBuffer(), new 'TIndexBuffer())

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    new(capacity) = DictionaryBase(capacity, Unchecked.defaultof<'TKeyComparer>)

    // [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    // new() = DictionaryBase(1)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member inline internal this.HashToIndex hashCode =
        ((uint DictionaryModule.GoldenRatio * uint hashCode) >>> this.bitShift) |> int

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member inline internal this.PrepareIndex release =
        let capacity = this.entries.Length

        if release then
            this.indexBuffer.Return this.index

        this.index <- this.indexBuffer.Rent(capacity, DictionaryModule.EndOfIndex)
        this.bitShift <- DictionaryModule.wrapAroundMask capacity

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member inline internal this.GetBucket(indexPtr: int nativeptr, hashCode) =
        // PointerHelpers.Add(indexPtr, this.HashToIndex hashCode)
        hashCode |> this.HashToIndex |> NativeInterop.NativePtr.add indexPtr

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member inline internal this.GetBucket hashCode =
        &this.index.GetReference(this.HashToIndex hashCode)

    /// <summary> Gets the capacity of the dictionary. </summary>
    member this.Capacity = this.index.Length
    /// <summary> Gets the number of elements contained in the dictionary. </summary>
    member this.Count = this.count - this.freeCount

    member internal this.CopyEntries(oldEntries: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> Span, newEntries: _ Span) =
        use indexPtr = fixed &this.index.GetReference()
        let newEntriesRef = &newEntries.GetReference()
        let oldEntriesRef = &oldEntries.GetReference()

        for i = 0 to oldEntries.Length - 1 do
            let entry = &Unsafe.Add(&newEntriesRef, i)
            entry <- Unsafe.Add(&oldEntriesRef, i)
            let bucket = this.GetBucket(indexPtr, entry.HashCode)
            entry.Next <- PointerHelpers.GetItem bucket //NativeInterop.NativePtr.read bucket
            NativeInterop.NativePtr.write bucket i

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member internal this.CopyEntries(oldEntries: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> Span, newEntries: _ Span, resizeMode) =
        let count = this.count
        let mutable newCount = 0
        use indexPtr = fixed &this.index.GetReference()
        let newEntriesRef = &newEntries.GetReference()
        let oldEntriesRef = &oldEntries.GetReference()

        for i = 0 to oldEntries.Length - 1 do
            let entry = &Unsafe.Add(&newEntriesRef, i)
            entry <- Unsafe.Add(&oldEntriesRef, i)

            if entry.Next >= DictionaryModule.EndOfIndex then
                let bucket = this.GetBucket(indexPtr, entry.HashCode)
                entry.Next <- NativeInterop.NativePtr.read bucket
                NativeInterop.NativePtr.write bucket (if resizeMode then newCount else count + newCount)
                newCount <- newCount + 1

    member internal this.Resize(capacity: int) =
        ArgumentOutOfRangeException.ThrowIfGreaterThan(capacity, Array.MaxLength - 1)
        let array = this.entries
        this.entries <- this.entriesBuffer.Rent capacity
        this.PrepareIndex true

        if this.count > 0 then
            let oldSpan = array.GetSpan this.count
            let newSpan = this.entries.GetSpan()

            if this.freeCount > 0 then
                this.CopyEntries(oldSpan, newSpan, true)

            this.CopyEntries(oldSpan, newSpan)

        this.entriesBuffer.Return array

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member internal this.Search<'TSuccess, 'TFailure, 'TReturn
        when OperationSuccess<'TStore, 'TKey, 'TValue, 'TStoreAccess, 'TSuccess, 'TReturn>
        and OperationFailure<'TStore, 'TKey, 'TValue, 'TStoreAccess, 'TFailure, 'TReturn>>
        (key: 'TKey, value: 'TValue, behavior: InsertionBehavior)
        : 'TReturn =
        if not typeof<'TKey>.IsValueType then
            ArgumentNullException.ThrowIfNull key

        let mutable previousIndex = DictionaryModule.EndOfIndex
        let entriesReference = &this.entries.GetReference()
        let hashCode = this.comparer.GetHashCode key
        let mutable primaryReference = &this.GetBucket hashCode
        let mutable index = primaryReference

        while uint index < uint this.count do
            let entry = &Unsafe.Add(&entriesReference, index)

            if
                entry.HashCode = hashCode
                && this.comparer.Equals(Unchecked.defaultof<'TStoreAccess>.GetKey entry, key)
            then
                Unchecked.defaultof<'TSuccess>
                    .Handle(key, value, behavior, &primaryReference, &entry, &entriesReference, previousIndex)
                |> IL.Push<'TReturn>

                IL.Emit.Ret()
                raise(IL.Unreachable())
            else
                previousIndex <- index
                index <- entry.Next

        Unchecked.defaultof<'TFailure>
            .Handle(key, value, hashCode, &primaryReference, &entriesReference, &this.count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.TryInsert(key: 'TKey, value: 'TValue, behavior: InsertionBehavior) =
        if this.count = this.entries.Length then
            this.Resize(this.count <<< 1)

        this.Search<InsertSuccess<_, _, _, _>, InsertFailure<_, _, _, _>, _>(key, value, behavior)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.Add(key: 'TKey, value: 'TValue) =
        this.TryInsert(key, value, ThrowOnExisting) |> ignore

    // member this.GetEnumerator() =
    //     seq {
    //         let entries = this.entries

    //         for i = 0 to this.count - 1 do
    //             let entry = &entries[i]

    //             if entry.Next >= Dictionary.EndOfIndex then
    //                 yield Collections.Generic.KeyValuePair(entry.Key, entry.Value)
    //     }

    // override this.Dispose disposing =
    //     if disposing then
    //         this.entriesBuffer.Return this.entries
    //         this.indexBuffer.Return this.index
    //         this.entries <- Unchecked.defaultof<_>
    //         this.index <- Unchecked.defaultof<_>

    member this.Dispose() =
        this.entriesBuffer.Return this.entries
        this.indexBuffer.Return this.index
        this.entries <- Unchecked.defaultof<_>
        this.index <- Unchecked.defaultof<_>

// type Dictionary<'TKey, 'TValue>(capacity, comparer) =
//     inherit
//         DictionaryBase<'TKey, 'TValue, 'TKey Collections.DefaultEqualityComparer, Entry<'TKey, 'TValue> SharedBuffer, int SharedBuffer>(
//             capacity,
//             comparer,
//             SharedBuffer(),
//             SharedBuffer()
//         )

//     new() = Dictionary(1, Collections.DefaultEqualityComparer())
//     new(capacity) = Dictionary(capacity, Collections.DefaultEqualityComparer())