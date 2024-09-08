namespace Nava.Runtime.Collections

open System.Collections.Generic

[<RequireQualifiedAccess>]
module Seq =
    [<TailCall>]
    let rec loop predicate (enumerator: 'T IEnumerator) =
        if enumerator.MoveNext() then
            let current = enumerator.Current

            if predicate current then
                ValueSome current
            else
                loop predicate enumerator
        else
            ValueNone

    let tryFind predicate (source: seq<'T>) =
        source
        |> ValueOption.ofObj
        |> ValueOption.bind(fun source ->
            use enumerator = source.GetEnumerator()
            loop predicate enumerator)