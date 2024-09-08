namespace Nava.Runtime

#nowarn "9" "42"

open System
open System.Numerics
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open System.Buffers

[<AbstractClass; Sealed>]
type MemoryHelpersFS =
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member inline Cast<'T, 'U>(value: 'T) : 'U = (# "" value: 'U #)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReference arr =
        &MemoryMarshal.GetArrayDataReference arr

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReference(span: 'T Span) = &MemoryMarshal.GetReference span

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReference(span: 'T ReadOnlySpan) = &MemoryMarshal.GetReference span

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReference(arr: 'T[], index: int) =
        &Unsafe.Add(&MemoryHelpers.GetReference arr, index)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member inline GetReference(arr: 'T[], index: uint) =
        &MemoryHelpers.GetReference(arr, int index)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReference(span: 'T Span, index: int) =
        &Unsafe.Add(&MemoryHelpers.GetReference span, index)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReference(span: 'T ReadOnlySpan, index: int) =
        &Unsafe.Add(&MemoryHelpers.GetReference span, index)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetElement(arr: 'T[]) = MemoryHelpers.GetReference arr

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetElement(span: 'T Span) = MemoryHelpers.GetReference span

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetElement(span: 'T ReadOnlySpan) = MemoryHelpers.GetReference span

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetElement(arr: 'T[], index: int) = MemoryHelpers.GetReference(arr, index)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetElement(arr: 'T[], index: uint) = MemoryHelpers.GetReference(arr, index)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetElement(span: 'T Span, index) = MemoryHelpers.GetReference(span, index)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetElement(span: 'T ReadOnlySpan, index) = MemoryHelpers.GetReference(span, index)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReadOnlySpan(arr: 'T[], index: int, count) =
        MemoryMarshal.CreateReadOnlySpan(&MemoryHelpers.GetReference(arr, index), count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReadOnlySpan(arr: 'T[], count) =
        MemoryHelpers.GetReadOnlySpan(&MemoryHelpers.GetReference arr, count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReadOnlySpan(arr: 'T[]) =
        MemoryHelpers.GetReadOnlySpan(arr, 0, arr.Length)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReadOnlySpan(span: 'T Span, index, count) =
        MemoryHelpers.GetReadOnlySpan(&MemoryHelpers.GetReference(span, index), count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReadOnlySpan(span: 'T Span, count) =
        MemoryHelpers.GetReadOnlySpan(&MemoryHelpers.GetReference span, count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReadOnlySpan(span: 'T Span) =
        MemoryHelpers.GetReadOnlySpan(span, span.Length)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReadOnlySpan(span: 'T ReadOnlySpan, index, count) =
        MemoryHelpers.GetReadOnlySpan(&MemoryHelpers.GetReference(span, index), count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReadOnlySpan(span: 'T ReadOnlySpan, count) =
        MemoryHelpers.GetReadOnlySpan(span, 0, count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReadOnlySpan<'T>(span: voidptr, count) = ReadOnlySpan<'T>(span, count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReadOnlySpan<'T>(reference: 'T byref, count) =
        MemoryMarshal.CreateReadOnlySpan(&reference, count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetReadOnlySpan<'T>(reference: 'T byref, index: int, count) =
        MemoryMarshal.CreateReadOnlySpan(&Unsafe.Add(&reference, index), count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetSpan(arr: 'T[], index: int, count) =
        MemoryHelpers.GetSpan(&MemoryHelpers.GetReference(arr, index), count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetSpan(arr: 'T[], count) =
        MemoryHelpers.GetSpan(&MemoryHelpers.GetReference arr, count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetSpan(arr: 'T[]) = MemoryHelpers.GetSpan(arr, arr.Length)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetSpan(span: 'T Span, index, count) =
        MemoryHelpers.GetSpan(&MemoryHelpers.GetReference(span, index), count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetSpan(span: 'T Span, count) =
        MemoryHelpers.GetSpan(&MemoryHelpers.GetReference span, count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetSpan(span: 'T ReadOnlySpan, index, count) =
        MemoryHelpers.GetSpan(&MemoryHelpers.GetReference(span, index), count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetSpan(span: 'T ReadOnlySpan, count) =
        MemoryHelpers.GetSpan(&MemoryHelpers.GetReference span, count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetSpan(span: 'T ReadOnlySpan) =
        MemoryHelpers.GetSpan(span, span.Length)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetSpan<'T>(ptr: voidptr, count) = Span<'T>(ptr, count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetSpan(ptr: 'T nativeptr, count) =
        Span<'T>(NativeInterop.NativePtr.toVoidPtr ptr, count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetSpan(ptr: unativeint, count) =
        Span<'T>(
            ptr
            |> nativeint
            |> NativeInterop.NativePtr.ofNativeInt<'T>
            |> NativeInterop.NativePtr.toVoidPtr,
            count
        )

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetSpan<'T>(reference: 'T byref, count) =
        MemoryMarshal.CreateSpan(&reference, count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member GetSpan<'T>(reference: 'T byref, index: int, count) =
        MemoryMarshal.CreateSpan(&Unsafe.Add(&reference, index), count)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member IndexToSize index = 16 <<< index

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member SizeToIndex size =
        if size >= 0 then
            BitOperations.Log2(uint(size - 1) ||| 0xFu) - 3
        else
            0

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member ReturnArray arr = ArrayPool.Shared.Return arr

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member inline ValueAsByteSpan(value: 'T byref) =
        MemoryHelpers.GetSpan(&Unsafe.As<_, byte> &value, sizeof<'T>)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member inline ValueAsByteReadOnlySpan(value: 'T byref) =
        MemoryHelpers.GetReadOnlySpan(&Unsafe.As<_, byte> &value, sizeof<'T>)

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member inline ValueAsSpan(value: 'TFrom byref) =
        MemoryMarshal.Cast<_, 'TTo>(MemoryHelpers.GetSpan(&value, 1))

    static member inline GetReadOnlySpanAs(str: string) =
        MemoryMarshal.Cast<_, 'TTo>(str.AsSpan())

    /// <summary>Create a <see cref="T:System.ReadOnlySpan`1"/> from single value reference.</summary>
    /// <param name="value">Reference to value from which <see cref="T:System.ReadOnlySpan`1"/> is created.</param>
    /// <typeparam name="'TFrom">Type of the value.</typeparam>
    /// <typeparam name="'TTo">Type of <see cref="T:System.ReadOnlySpan`1"/> created.</typeparam>
    /// <returns><see cref="T:System.ReadOnlySpan`1"/> refering to <paramref name="value"/>.</returns>
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member inline ValueAsReadOnlySpan(value: 'TFrom byref) =
        MemoryMarshal.Cast<_, 'TTo>(MemoryHelpers.GetReadOnlySpan(&value, 1))

    static member inline GetPointer(value: 'T byref) =
        Unsafe.AsPointer &value |> NativeInterop.NativePtr.ofVoidPtr<'T>

    static member inline GetPointer(array: 'T[]) =
        MemoryHelpersFS.GetPointer(&MemoryHelpers.GetReference array)