namespace Nava.Collections.Pooled.Dictionary

open System
open System.Runtime.CompilerServices
open Nava.Collections.Pooled

/// <summary> Interface for handling failure outcome of a search. </summary>
type ISearchFailure<'TStore, 'TKey, 'TValue, 'TStoreAccess, 'TReturn when StoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess>> =
    interface
        /// <summary> Handle the failure outcome of a search. </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value to be updated.</param>
        /// <param name="hashCode">The hash code.</param>
        /// <param name="primaryReference"> The primary reference to the entry array list. </param>
        /// <param name="entryReference"> The reference to the entry array list at the insertion point. </param>
        /// <param name="count"> The number of entries in the dictionary. </param>
        abstract member Handle:
            key: 'TKey *
            value: 'TValue *
            hashCode: int *
            primaryReference: int byref *
            entryReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref *
            count: int byref ->
                'TReturn
    end

/// <summary> Handle failure outcome for insertion operation. </summary>
[<Struct; NoComparison; NoEquality>]
type InsertFailure<'TStore, 'TKey, 'TValue, 'TStoreAccess when StoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess>> =
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.Handle
        (
            key: 'TKey,
            value: 'TValue,
            hashCode,
            primaryReference: int byref,
            entryReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref,
            count: int byref
        ) =
        Unsafe.Add(&entryReference, count) <-
            Unchecked.defaultof<'TStoreAccess>
                .Create(key, value, hashCode, primaryReference)

        primaryReference <- count
        count <- count + 1
        true

    interface ISearchFailure<'TStore, 'TKey, 'TValue, 'TStoreAccess, bool> with
        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.Handle(key, value, hashCode, primaryReference, entryReference, count) =
            this.Handle(key, value, hashCode, &primaryReference, &entryReference, &count)

/// <summary> Handle failure outcome within a dictionary. Performs no action. </summary>
[<Struct; NoComparison; NoEquality>]
type NoOpFailure<'TStore, 'TKey, 'TValue, 'TStoreAccess, 'TReturn when StoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess>> =
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.Handle
        (
            key: 'TKey,
            value: 'TValue,
            hashCode,
            primaryReference: int byref,
            entryReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref,
            count: int byref
        ) =
        Unchecked.defaultof<'TReturn>

    interface ISearchFailure<'TStore, 'TKey, 'TValue, 'TStoreAccess, 'TReturn> with
        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.Handle(key, value, hashCode, primaryReference, entryReference, count) =
            this.Handle(key, value, hashCode, &primaryReference, &entryReference, &count)