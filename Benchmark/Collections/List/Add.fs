namespace Nava.Benchmark.Collections.List

open System
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Diagnosers
open Nava.Benchmark.Configuration
open Nava.Runtime.Pooled
open Nava.Runtime

module ListAddOperation =
    [<Literal>]
    let InitialCapacity = Pooled.Collections.PooledList<int>.InitialCapacity

[<CoreAffinity>]
type ListAddOperation() =

    [<Params(10, 50, 10000)>]
    member val NumberOfElement = 0 with get, set
    // [<Params(4, 10000)>]
    // member val InitialCapacity = 16 with get, set
    [<Benchmark(Baseline = true)>]
    member this.SystemList() =
        let n = this.NumberOfElement
        let list = Collections.Generic.List ListAddOperation.InitialCapacity

        for _ in 0 .. n - 1 do
            // list.Add 123555
            list.Add "123555"

    [<Benchmark>]
    member this.Array() =
        let n = this.NumberOfElement
        let mutable array = GC.AllocateUninitializedArray ListAddOperation.InitialCapacity
        let mutable count = 0

        for i in 0 .. n - 1 do
            if array.Length = i then
                Array.Resize(&array, Math.Max(array.Length <<< 1, i))

            // array.GetReference(i) <- 123555
            array.GetReference(i) <- "123555"
            count <- count + 1

    [<Benchmark>]
    member this.PooledList() =
        let n = this.NumberOfElement
        let list = Collections.PooledList ListAddOperation.InitialCapacity

        for _ in 0 .. n - 1 do
            // list.Add 123555
            list.Add "123555"

        list.Dispose()

    [<Benchmark>]
    member this.ListFS() =
        let n = this.NumberOfElement
        let list = Nava.Collections.Pooled.List<_, _ SharedBuffer> ListAddOperation.InitialCapacity

        for _ in 0 .. n - 1 do
            // list.Add 123555
            list.Add "123555"

        list.Dispose()