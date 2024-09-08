namespace Nava.Runtime.Pooled.Collections

#nowarn "3391"

open System
open System.Numerics

open Xunit
// open FsCheck
// open FsCheck.FSharp
// open FsCheck.Xunit

open Nava.Collections.Pooled

module ``Dictionary Tests`` =

    // [<Property>]
    // let ``Capactiy should always be a power of 2`` capacity =
    //     use dictionary = DictionaryTest<int, int, DefaultEqualityComparer<_>> capacity
    //     dictionary.Capacity |> BitOperations.IsPow2 |> Assert.True

    // [<Fact>]
    // let ``HashCodes should not overlap each other``() =
    //     let power2LengthArray =
    //         gen {
    //             let! arrayLength =
    //                 Gen.choose(1, Int32.MaxValue)
    //                 |> Gen.filter(fun (x: int) -> BitOperations.IsPow2 x)

    //             return! Gen.choose(Int32.MinValue, Int32.MaxValue) |> Gen.arrayOfLength arrayLength
    //         }
    //         |> Arb.fromGen

    //     Prop.forAll power2LengthArray (fun hashCodes ->
    //         use dictionary = DictionaryTest<int, int, DefaultEqualityComparer<_>> hashCodes.Length

    //         hashCodes
    //         |> Array.distinct
    //         |> Array.forall(fun hashCode ->
    //             let bucket = &dictionary.GetHead(hashCode)

    //             if bucket <> DictionaryTest.EndOfIndex then
    //                 bucket <- hashCode
    //                 true
    //             else
    //                 false))
    //     |> Check.QuickThrowOnFailure

    open CsCheck

    let ValidateCapacity value =
        value > 0 && value < (Array.MaxLength >>> 1)

    let ValidCapacity = Gen.Int.Positive.Where ValidateCapacity
    let InvalidCapacity = Gen.Int.Where(ValidateCapacity >> not)
    let ValueDicitionaryCapacity = ValidCapacity.Select(fun value -> PooledDictionary<int, int, DefaultEqualityComparer<_>> value)

    [<Fact>]
    let ``Capacity should be at least 1``() =
        InvalidCapacity.Sample(
            (fun value ->
                Xunit.Assert.Throws<ArgumentOutOfRangeException>(
                    (fun () -> PooledDictionary<int, int, DefaultEqualityComparer<_>>(value) |> ignore)
                )
                |> ignore

                true),
            seed = "0000eDxTynX6"
        )

    [<Fact>]
    let ``Capactiy should always be a power of 2``() =
        ValueDicitionaryCapacity.Sample((fun value -> value.Capacity |> BitOperations.IsPow2))