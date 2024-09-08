namespace Nava.Benchmark.Collections.Dictionary

open System
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Diagnosers
open Nava.Benchmark.Configuration
open Nava.Runtime.Pooled.Collections
open Nava.Runtime

module DictionaryAddOperation =
    [<Literal>]
    let InitialCapacity = 16

[<CoreAffinity>]
type DictionaryAddOperation() =

    [<Params(10, 50, 10000)>]
    member val NumberOfElement = 0 with get, set
    // [<Params(4, 10000)>]
    // member val InitialCapacity = 16 with get, set
    [<Benchmark(Baseline = true)>]
    member this.SystemDictionary() =
        let n = this.NumberOfElement
        let dictionary = Collections.Generic.Dictionary() // DictionaryAddOperation.InitialCapacity

        for i in 0 .. n - 1 do
            // let i = i.ToString()
            dictionary.Add(i, i)

    [<Benchmark>]
    member this.SystemHashSet() =
        let n = this.NumberOfElement
        let dictionary = Collections.Generic.HashSet() // DictionaryAddOperation.InitialCapacity

        for i in 0 .. n - 1 do
            // let i = i.ToString()
            i |> dictionary.Add |> ignore

    [<Benchmark>]
    member this.Dictionary() =
        let n = this.NumberOfElement

        let dictionary =
            Nava.Collections.Pooled.DictionaryBase<
                _,
                _,
                _ DefaultEqualityComparer,
                Nava.Collections.Pooled.Entry<Nava.Collections.Pooled.KeyValue<_, _>, _, _, Nava.Collections.Pooled.KeyValueAccess<_, _>> Pooled.SharedBuffer,
                int Pooled.SharedBuffer,
                Nava.Collections.Pooled.KeyValue<_, _>,
                Nava.Collections.Pooled.KeyValueAccess<_, _>
             >
                1
        // let dictionary = Nava.Collections.Pooled.Dictionary()
        // DictionaryAddOperation.InitialCapacity

        for i in 0 .. n - 1 do
            // let i = i.ToString()
            dictionary.Add(i, i) |> ignore

        dictionary.Dispose()

    [<Benchmark>]
    member this.HashTable() =
        let n = this.NumberOfElement

        let dictionary =
            Nava.Collections.Pooled.DictionaryBase<
                _,
                _,
                _ DefaultEqualityComparer,
                Nava.Collections.Pooled.Entry<Nava.Collections.Pooled.HashValue<_>, _, _, Nava.Collections.Pooled.HashValueAccess<_>> Pooled.SharedBuffer,
                int Pooled.SharedBuffer,
                Nava.Collections.Pooled.HashValue<_>,
                Nava.Collections.Pooled.HashValueAccess<_>
             >
                1
        // let dictionary = Nava.Collections.Pooled.Dictionary()
        // DictionaryAddOperation.InitialCapacity

        for i in 0 .. n - 1 do
            // let i = i.ToString()
            dictionary.Add(i, i) |> ignore

        dictionary.Dispose()

// [<Benchmark>]
// member this.PooledDictionary() =
//     let n = this.NumberOfElement
//     let dictionary = PooledDictionary<_, _, DefaultEqualityComparer<_>>() // DictionaryAddOperation.InitialCapacity

//     for i in 0 .. n - 1 do
//         // let i = i.ToString()
//         dictionary.Add(i, i)

//     dictionary.Dispose()