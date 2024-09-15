namespace Nava.Runtime

#nowarn "1204" "42" "9"

open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Microsoft.FSharp.NativeInterop

[<Extension>]
type ObjectExtensions =
    //https://github.com/stephan-tolksdorf/fparsec/blob/fdd990ad5abe32fd65d926005b4c7bd71dd2384f/FParsec/Internals.fs#L14
    [<Extension>]
    static member inline ReferenceEquals(x: 'T, y: 'T) = (# "ceq" x y : bool #) //obj.ReferenceEquals(x, y)

    //https://github.com/stephan-tolksdorf/fparsec/blob/fdd990ad5abe32fd65d926005b4c7bd71dd2384f/FParsec/Internals.fs#L16
    [<Extension>]
    static member inline IsNull<'T when 'T: null>(x: 'T) = (# "ldnull ceq" x : bool #) //ObjectExtensions.ReferenceEquals(x, null)

    //https://github.com/stephan-tolksdorf/fparsec/blob/fdd990ad5abe32fd65d926005b4c7bd71dd2384f/FParsec/Internals.fs#L18
    [<Extension>]
    static member inline IsNotNull<'T when 'T: null>(x: 'T) = (# "ldnull cgt.un" x : bool #) //ObjectExtensions.ReferenceEquals(x, null)|> not

    [<Extension>]
    static member inline IsOfType<'T> object =
        LanguagePrimitives.IntrinsicFunctions.TypeTestFast<'T>(object)

    [<Extension>]
    static member inline ForceAs<'T, 'U>(x: 'T) =

#if INTERACTIVE
        unbox x
#else
        (# "" x: 'U #)
#endif

// [<Extension>]
// type ObjectExtensions =
//     //https://github.com/stephan-tolksdorf/fparsec/blob/fdd990ad5abe32fd65d926005b4c7bd71dd2384f/FParsec/Internals.fs#L14
//     [<Extension>]
//     static member inline ReferenceEquals(x: 'T, y: 'T) = (# "ceq" x y : bool #) //obj.ReferenceEquals(x, y)

//     //https://github.com/stephan-tolksdorf/fparsec/blob/fdd990ad5abe32fd65d926005b4c7bd71dd2384f/FParsec/Internals.fs#L16
//     [<Extension>]
//     static member inline IsNull x = (# "ldnull ceq" x : bool #) //ObjectExtensions.ReferenceEquals(x, null)

//     //https://github.com/stephan-tolksdorf/fparsec/blob/fdd990ad5abe32fd65d926005b4c7bd71dd2384f/FParsec/Internals.fs#L18
//     [<Extension>]
//     static member inline IsNotNull x = (# "ldnull cgt.un" x : bool #) //ObjectExtensions.ReferenceEquals(x, null)|> not

//     [<Extension>]
//     static member inline IsOfType<'T> object =
//         LanguagePrimitives.IntrinsicFunctions.TypeTestFast<'T>(object)

//     [<Extension>]
//     static member inline As(x: 'T) =

// #if INTERACTIVE
//         unbox x
// #else
//         (# "" x: 'U #)
// #endif
// [<Extension; MethodImpl(MethodImplOptions.AggressiveInlining)>]
// static member inline As<'T>(x: obj) =
//     LanguagePrimitives.IntrinsicFunctions.UnboxFast<'T> x //Unsafe.As<'T>(x)

// [<Extension; MethodImpl(MethodImplOptions.AggressiveInlining)>]
// static member inline As<'TIn, 'TOut>(x: 'TIn byref) = &Unsafe.As<'TIn, 'TOut> &x
open Nava.Runtime

module TypeHelpers =
    type nativeptr<'T when 'T: unmanaged> with

        member inline x.Reference index =
            &Unsafe.AsRef<'T>(index |> NativePtr.add x |> NativePtr.toVoidPtr)

        member inline x.LessThan(right: nativeptr<'T>) =
            (NativePtr.toNativeInt x) < (NativePtr.toNativeInt right)

        member inline x.LessThanOrEqual right =
            (NativePtr.toNativeInt x) <= (NativePtr.toNativeInt right)

        member inline x.GreaterThan right =
            (NativePtr.toNativeInt x) > (NativePtr.toNativeInt right)

        member inline x.GreaterThanOrEqual right =
            (NativePtr.toNativeInt x) >= (NativePtr.toNativeInt right)

        member inline x.Equal right =
            (NativePtr.toNativeInt x) = (NativePtr.toNativeInt right)

        member inline x.NotEqual right =
            (NativePtr.toNativeInt x) <> (NativePtr.toNativeInt right)

        member inline x.Item
            with get (index) = index |> NativePtr.get x
            and set index value = value |> NativePtr.set x index

        member inline x.Item
            with get (index: uint32) = index |> int32 |> NativePtr.get x
            and set (index: uint32) value = value |> NativePtr.set x (index |> int32)

        member inline x.Add(index: int32) = index |> NativePtr.add x

        member inline x.Subtract(index: _ nativeptr) =
            (NativePtr.toNativeInt x) - (NativePtr.toNativeInt index)

        member inline x.Add(index: uint32) = index |> int32 |> NativePtr.add x

        member inline x.Add(index: uint8) = index |> int32 |> NativePtr.add x

        member inline x.Value
            with get () = NativePtr.read x
            and set value = NativePtr.write x value

        member inline this.AsSpan(count) =
            MemoryHelpers.GetSpan(this |> NativePtr.toVoidPtr, count)

    let inline zeroCreateUncheckedArray<'T>(count: int) =
        System.GC.AllocateUninitializedArray<'T>(count)

    let inline GetType<'TypeForChoosingAssembly> typeName =
        typeof<'TypeForChoosingAssembly>.Assembly.GetType(typeName)

    let inline alignedAlloc size alignment =
        (size |> unativeint, alignment |> unativeint)
        |> NativeMemory.AlignedAlloc
        |> NativePtr.ofVoidPtr

    let inline alignedReAlloc (ptr: 'T nativeptr) size alignment : 'T nativeptr =
        (ptr |> NativePtr.toVoidPtr, size |> unativeint, alignment |> unativeint)
        |> NativeMemory.AlignedRealloc
        |> NativePtr.ofVoidPtr

    let inline alignedFree ptr =
        ptr |> NativePtr.toVoidPtr |> NativeMemory.AlignedFree

    let inline ptrFill (ptr: 'T nativeptr) size value = ptr.AsSpan(size).Fill(value)

    let inline alignedAllocFill size alignment value =
        let ptr = alignedAlloc size alignment
        ptrFill ptr size value
        ptr