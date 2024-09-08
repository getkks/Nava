namespace Nava.Collections.Pooled

open System
open System.Runtime.CompilerServices

// /// <summary> Entry in a dictionary. </summary>
// /// <typeparam name="TKey"> Type of the key. </typeparam>
// /// <typeparam name="TValue"> Type of the value. </typeparam>
// [<Struct; NoComparison; NoEquality>]
// type KeyValueStore1<'TKey, 'TValue> =
//     /// <summary> Hash code of the entry. </summary>
//     val mutable HashCode: int
//     /// <summary> Collision index of the entry. </summary>
//     val mutable Next: int
//     /// <summary> Key of the entry. </summary>
//     val mutable Key: 'TKey
//     /// <summary> Value of the entry. </summary>
//     val mutable Value: 'TValue

//     /// <summary> Initializes a new instance of the <see cref="T:Nava.Collections.Pooled.Entry`2"/> struct. </summary>
//     /// <param name="key"> The key. </param>
//     /// <param name="value"> The value. </param>
//     /// <param name="hashCode"> The hash code. </param>
//     /// <param name="next"> The next. </param>
//     new(key: 'TKey, value: 'TValue, hashCode: int, next: int) =
//         {
//             HashCode = hashCode
//             Next = next
//             Key = key
//             Value = value
//         }

//     /// <summary> Clears the entry. </summary>
//     member this.Clear() =
//         this.Next <- Int32.MinValue
//         this.Key <- Unchecked.defaultof<_>
//         this.Value <- Unchecked.defaultof<_>

/// <summary> Insertion behavior for dictionary operations. </summary>
type InsertionBehavior =
    /// <summary> The default insertion behavior. </summary>
    | None
    /// <summary> Specifies that an existing entry with the same key should be overwritten if encountered. </summary>
    | OverwriteExisting
    /// <summary> Specifies that if an existing entry with the same key is encountered, an exception should be thrown. </summary>
    | ThrowOnExisting

[<Struct; NoComparison; NoEquality>]
type KeyValue<'TKey, 'TValue> =
    /// <summary> Key of the entry. </summary>
    val mutable Key: 'TKey
    /// <summary> Value of the entry. </summary>
    val mutable Value: 'TValue

    /// <summary> Initializes a new instance of the <see cref="T:Nava.Collections.Pooled.KeyValue`2"/> struct. </summary>
    /// <param name="key"> The key. </param>
    /// <param name="value"> The value. </param>
    new(key: 'TKey, value: 'TValue) =
        {
            Key = key
            Value = value
        }

    /// <summary> Clears the entry. </summary>
    member this.Clear() =
        if RuntimeHelpers.IsReferenceOrContainsReferences<'TKey>() then
            this.Key <- Unchecked.defaultof<_>

        if RuntimeHelpers.IsReferenceOrContainsReferences<'TValue>() then
            this.Value <- Unchecked.defaultof<_>

[<Struct; NoComparison; NoEquality>]
type HashValue<'TValue> =
    /// <summary> Value of the entry. </summary>
    val mutable Value: 'TValue

    /// <summary> Initializes a new instance of the <see cref="T:Nava.Collections.Pooled.Value`1"/> struct. </summary>
    /// <param name="value"> The value. </param>
    new(value: 'TValue) = { Value = value }

    /// <summary> Clears the entry. </summary>
    member this.Clear() =
        if RuntimeHelpers.IsReferenceOrContainsReferences<'TValue>() then
            this.Value <- Unchecked.defaultof<_>

type StoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess
    when 'TStore: struct and Structure<'TStoreAccess> and 'TStoreAccess :> IStoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess>> =
    'TStoreAccess

/// <summary> Entry in a HashTable. </summary>
/// <typeparam name="TValue"> Type of the value. </typeparam>
and [<Struct; NoComparison; NoEquality>] Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess
    when StoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess>> =
    /// <summary> Hash code of the entry. </summary>
    val mutable HashCode: int
    /// <summary> Collision index of the entry. </summary>
    val mutable Next: int
    /// <summary> Value of the entry. </summary>
    val mutable Value: 'TStore

    /// <summary> Initializes a new instance of the <see cref="T:Nava.Collections.Pooled.Entry`2"/> struct. </summary>
    /// <param name="value"> The value. </param>
    /// <param name="hashCode"> The hash code. </param>
    /// <param name="next"> The next. </param>
    new(value: 'TStore, hashCode: int, next: int) =
        {
            HashCode = hashCode
            Next = next
            Value = value
        }

    /// <summary> Clears the entry. </summary>
    member this.Clear() =
        this.Next <- Int32.MinValue
        Unchecked.defaultof<'TStoreAccess>.Clear &this.Value

and IStoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess when StoreAccess<'TStore, 'TKey, 'TValue, 'TStoreAccess>> =
    abstract member Clear: 'TStore byref -> unit
    abstract member Create: key: 'TKey * value: 'TValue * hashCode: int * next: int -> Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess>
    abstract member GetKey: entry: 'TStore -> 'TKey
    abstract member GetKey: entry: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> -> 'TKey
    abstract member GetValue: entry: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> -> 'TValue
    abstract member SetKey: entry: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref * key: 'TKey -> unit
    abstract member SetValue: entry: Entry<'TStore, 'TKey, 'TValue, 'TStoreAccess> byref * value: 'TValue -> unit
    abstract member ThrowOnNull: entry: 'TStore -> unit

[<Struct; NoComparison; NoEquality>]
type KeyValueAccess<'TKey, 'TValue> =
    member this.Clear(entry: KeyValue<'TKey, 'TValue> byref) = entry.Clear()

    member this.Create(key: 'TKey, value: 'TValue, hashCode: int, next: int) =
        Entry<KeyValue<'TKey, 'TValue>, 'TKey, 'TValue, KeyValueAccess<'TKey, 'TValue>>(KeyValue(key, value), hashCode, next)

    member this.GetKey(entry: KeyValue<'TKey, 'TValue>) = entry.Key
    member this.GetKey(entry: Entry<KeyValue<'TKey, 'TValue>, 'TKey, 'TValue, KeyValueAccess<'TKey, 'TValue>>) = entry.Value.Key
    member this.GetValue(entry: Entry<KeyValue<'TKey, 'TValue>, 'TKey, 'TValue, KeyValueAccess<'TKey, 'TValue>>) = entry.Value.Value

    member this.SetKey(entry: Entry<KeyValue<'TKey, 'TValue>, 'TKey, 'TValue, KeyValueAccess<'TKey, 'TValue>> byref, key: 'TKey) =
        entry.Value.Key <- key

    member this.SetValue(entry: Entry<KeyValue<'TKey, 'TValue>, 'TKey, 'TValue, KeyValueAccess<'TKey, 'TValue>> byref, value: 'TValue) =
        entry.Value.Value <- value

    member this.ThrowOnNull(entry: KeyValue<'TKey, 'TValue>) =
        if not typeof<'TKey>.IsValueType then
            ArgumentNullException.ThrowIfNull(entry.Key)

    interface IStoreAccess<KeyValue<'TKey, 'TValue>, 'TKey, 'TValue, KeyValueAccess<'TKey, 'TValue>> with
        member this.Clear entry = this.Clear(&entry)
        member this.Create(key, value, hashCode, next) = this.Create(key, value, hashCode, next)
        member this.GetKey(entry: KeyValue<'TKey, 'TValue>) = this.GetKey(entry)
        member this.GetKey(entry: Entry<KeyValue<'TKey, 'TValue>, 'TKey, 'TValue, KeyValueAccess<'TKey, 'TValue>>) = this.GetKey(entry)
        member this.GetValue(entry) = this.GetValue(entry)
        member this.SetKey(entry, key) = this.SetKey(&entry, key)
        member this.SetValue(entry, value) = this.SetValue(&entry, value)
        member this.ThrowOnNull entry = this.ThrowOnNull(entry)

[<Struct; NoComparison; NoEquality>]
type HashValueAccess<'TValue> =
    member this.Clear(entry: HashValue<'TValue> byref) = entry.Clear()

    member this.Create(key: 'TKey, value: 'TValue, hashCode: int, next: int) =
        Entry<HashValue<'TValue>, 'TValue, 'TValue, HashValueAccess<'TValue>>(HashValue<'TValue>(value), hashCode, next)

    member this.GetKey(entry: HashValue<'TValue>) = entry.Value
    member this.GetKey(entry: Entry<HashValue<'TValue>, 'TValue, 'TValue, HashValueAccess<'TValue>>) = entry.Value.Value
    member this.GetValue(entry: Entry<HashValue<'TValue>, 'TValue, 'TValue, HashValueAccess<'TValue>>) = entry.Value.Value

    member this.SetKey(entry: Entry<HashValue<'TValue>, 'TValue, 'TValue, HashValueAccess<'TValue>> byref, key: 'TValue) =
        entry.Value.Value <- key

    member this.SetValue(entry: Entry<HashValue<'TValue>, 'TValue, 'TValue, HashValueAccess<'TValue>> byref, value: 'TValue) =
        entry.Value.Value <- value

    member this.ThrowOnNull(entry: HashValue<'TValue>) =
        if not typeof<'TValue>.IsValueType then
            ArgumentNullException.ThrowIfNull(entry.Value)

    interface IStoreAccess<HashValue<'TValue>, 'TValue, 'TValue, HashValueAccess<'TValue>> with
        member this.Clear entry = this.Clear(&entry)
        member this.Create(key, value, hashCode, next) = this.Create(key, value, hashCode, next)
        member this.GetKey(entry: HashValue<'TValue>) = entry.Value
        member this.GetKey(entry: Entry<HashValue<'TValue>, 'TValue, 'TValue, HashValueAccess<'TValue>>) = this.GetKey(entry)
        member this.GetValue(entry) = this.GetValue(entry)
        member this.SetKey(entry, key) = this.SetKey(&entry, key)
        member this.SetValue(entry, value) = this.SetValue(&entry, value)
        member this.ThrowOnNull entry = this.ThrowOnNull(entry)