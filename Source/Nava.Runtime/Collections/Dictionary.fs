namespace Nava.Runtime.Collections

open System.Collections.Generic
open System.Collections.Frozen
open FsToolkit.ErrorHandling

[<RequireQualifiedAccess>]
module Dictionary =
    let fromArray comparer (selector: 'T -> 'TResult) (sequence: _[]) =
        let dictionary = Dictionary(sequence.Length, comparer)

        for item in sequence do
            dictionary.Add(selector item, item)

        dictionary

    let freeze(dictionary: Dictionary<_, _>) = dictionary.ToFrozenDictionary()

    let freezeArray comparer (selector: 'T -> 'TResult) (sequence: _[]) =
        (fromArray comparer selector sequence).ToFrozenDictionary()

    let freezeSequence (selector: 'T -> 'TResult) (sequence: _ seq) = sequence.ToFrozenDictionary(selector)

    let freezeSequenceWithComparer comparer (selector: 'T -> 'TResult) (sequence: _ seq) =
        sequence.ToFrozenDictionary(selector, comparer)

    let freezeWithComparer comparer (dictionary: Dictionary<_, _>) = dictionary.ToFrozenDictionary comparer

    let inline fromCollection
        ([<InlineIfLambda>] keySelector: 'Item -> 'Key)
        ([<InlineIfLambda>] valueSelector: 'Item -> 'Value)
        (collection: 'T :> ICollection<'Item>)
        =
        let dictionary = Dictionary collection.Count
        use enumerator = collection.GetEnumerator()

        while enumerator.MoveNext() do
            let item = enumerator.Current
            dictionary.Add(keySelector item, valueSelector item)

        dictionary

    let getOrDefault key defaultValue (dictionary: 'T :> IReadOnlyDictionary<'Key, 'Value>) =
        match dictionary.TryGetValue key with
        | true, value -> value
        | false, _ -> defaultValue

    let inline fromSequence
        ([<InlineIfLambda>] keySelector: 'Item -> 'Key)
        ([<InlineIfLambda>] valueSelector: 'Item -> 'Value)
        (enumerable: 'T :> IEnumerable<'Item>)
        =
        let dictionary = Dictionary()

        for item in enumerable do
            dictionary.Add(keySelector item, valueSelector item)

        dictionary

    let inline tryGet key (dictionary: 'T :> IReadOnlyDictionary<'Key, 'Value>) =
        key |> dictionary.TryGetValue |> ValueOption.ofPair