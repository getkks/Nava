namespace Nava.Collections.Pooled.Dictionary

open System
open System.Runtime.CompilerServices
open Nava.Runtime
open Nava.Collections.Pooled
open InlineIL

/// <summary> Interface for handling successful outcome of a search within a dictionary. </summary>
type ISearchSuccess<'TStore, 'TKey, 'TValue, 'TStoreAccess, 'TReturn when StoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess>> =
    /// <summary> Handle the successful outcome of a search within a dictionary. </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value to be updated.</param>
    /// <param name="behavior">The insertion behavior.</param>
    /// <param name="primaryReference"> The primary reference to the entry array list. </param>
    /// <param name="entryReference"> The reference to the entry. </param>
    /// <param name="entriesReference"> The reference to the entries. </param>
    /// <param name="previousEntryIndex"> The index to the previous entry if any. </param>
    /// <returns> Depends on the type of operation. </returns>
    abstract member Handle:
        key: 'TKey *
        value: 'TValue *
        behavior: InsertionBehavior *
        primaryReference: int byref *
        entryReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref *
        entriesReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref *
        previousEntryIndex: int ->
            'TReturn

/// <summary> Handle successful search outcome within a dictionary for insertion operation. </summary>
[<Struct; NoComparison; NoEquality>]
type InsertSuccess<'TStore, 'TKey, 'TValue, 'TStoreAccess when StoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess>> =
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.Handle
        (
            key: 'TKey,
            value: 'TValue,
            behavior: InsertionBehavior,
            primaryReference: int byref,
            entryReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref,
            entriesReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref,
            previousEntryIndex: int
        ) =
        match behavior with
        | OverwriteExisting ->
            Unchecked.defaultof<'TStoreAccess>.SetValue(&entryReference, value)
            true
        | ThrowOnExisting ->
            ThrowHelpers.ThrowAddingDuplicateWithKeyArgumentException(key)
            false
        | _ -> false

    interface ISearchSuccess<'TStore, 'TKey, 'TValue, 'TStoreAccess, bool> with
        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.Handle(key, value, behavior, primaryReference, entryReference, entriesReference, previousEntryIndex) =
            this.Handle(key, value, behavior, &primaryReference, &entryReference, &entriesReference, previousEntryIndex)

module RemoveHandlers =
    let defaultHandler
        (
            key: 'TKey,
            value: 'TValue,
            behavior: InsertionBehavior,
            primaryReference: int byref,
            entryReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref,
            entriesReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref,
            previousEntryIndex: int
        ) =
        if previousEntryIndex < 0 then
            primaryReference <- entryReference.Next
        else
            Unsafe.Add(&entriesReference, previousEntryIndex).Next <- entryReference.Next

        entryReference.Clear()

/// <summary> Handle successful search outcome for removal operation. </summary>
type RemoveSuccess<'TStore, 'TKey, 'TValue, 'TStoreAccess when StoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess>> =
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.Handle
        (
            key: 'TKey,
            value: 'TValue,
            behavior: InsertionBehavior,
            primaryReference: int byref,
            entryReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref,
            entriesReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref,
            previousEntryIndex: int
        ) =
        RemoveHandlers.defaultHandler(key, value, behavior, &primaryReference, &entryReference, &entriesReference, previousEntryIndex)
        true

    interface ISearchSuccess<'TStore, 'TKey, 'TValue, 'TStoreAccess, bool> with
        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.Handle(key, value, behavior, primaryReference, entryReference, entriesReference, previousEntryIndex) =
            this.Handle(key, value, behavior, &primaryReference, &entryReference, &entriesReference, previousEntryIndex)

/// <summary> Handle successful search outcome for removal operation. </summary>
type RemoveSuccessReturnValue<'TStore, 'TKey, 'TValue, 'TStoreAccess when StoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess>> =
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.Handle
        (
            key: 'TKey,
            value: 'TValue,
            behavior: InsertionBehavior,
            primaryReference: int byref,
            entryReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref,
            entriesReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref,
            previousEntryIndex: int
        ) =
        let returnValue = Unchecked.defaultof<'TStoreAccess>.GetValue entryReference
        RemoveHandlers.defaultHandler(key, value, behavior, &primaryReference, &entryReference, &entriesReference, previousEntryIndex)
        returnValue

    interface ISearchSuccess<'TStore, 'TKey, 'TValue, 'TStoreAccess, 'TValue> with
        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.Handle(key, value, behavior, primaryReference, entryReference, entriesReference, previousEntryIndex) =
            this.Handle(key, value, behavior, &primaryReference, &entryReference, &entriesReference, previousEntryIndex)

/// <summary> Handle successful search outcome within a dictionary. Performs no action. </summary>
[<Struct; NoComparison; NoEquality>]
type NoOpSuccess<'TStore, 'TKey, 'TValue, 'TStoreAccess, 'TReturn when StoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess>> =
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.Handle
        (
            key: 'TKey,
            value: 'TValue,
            behavior: InsertionBehavior,
            primaryReference: int byref,
            entryReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref,
            entriesReference: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref,
            previousEntryIndex: int
        ) : 'TReturn =
        if LanguagePrimitives.PhysicalEquality typeof<'TReturn> typeof<bool> then
            IL.Push true
        else
            IL.Push Unchecked.defaultof<'TReturn>

        IL.Emit.Ret()
        raise(IL.Unreachable())

    interface ISearchSuccess<'TStore, 'TKey, 'TValue, 'TStoreAccess, 'TReturn> with
        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.Handle(key, value, behavior, primaryReference, entryReference, entriesReference, previousEntryIndex) =
            this.Handle(key, value, behavior, &primaryReference, &entryReference, &entriesReference, previousEntryIndex)