namespace Nava.Excel.Packaging.Xml

type Reader = System.Xml.XmlReader
namespace Nava.Excel.Packaging.Biff

open System
open System.Diagnostics
open System.IO
open System.Text

open CommunityToolkit.HighPerformance.Buffers
open FsToolkit.ErrorHandling

type Reader = {
    Stream: Stream
} with

    member this.Dispose disposing =
        if disposing then
            this.Stream.Dispose()
            GC.SuppressFinalize this

    member this.Dispose() = this.Dispose true
    override this.Finalize() = this.Dispose false

    interface IDisposable with
        member this.Dispose() = this.Dispose()

module Reader =
    open System.Runtime.InteropServices
    open System.Runtime.CompilerServices

    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    let complete(span: byte Span) =
        let b = &span[0]
        b <- b &&& 0x7Fuy
        b &&& 0x80uy = 0uy

    let readVariableValue(stream: Stream, value: int outref) =
        let span = MemoryMarshal.CreateSpan(&Unsafe.As<int, byte>(&value), 4)
        let mutable buffer = span.Slice(0, 1)
        let mutable ret = false

        if stream.Read buffer <> 0 then
            ret <- complete buffer

            if not ret then
                buffer <- span.Slice(1, 1)

                if stream.Read buffer <> 0 then
                    ret <- complete buffer

                    if not ret then
                        buffer <- span.Slice(2, 1)

                        if stream.Read buffer <> 0 then
                            ret <- complete buffer

                            if not ret then
                                buffer <- span.Slice(3, 1)

                                if stream.Read buffer <> 0 then
                                    ret <- complete buffer

        ret

// let readRecordHeader({ Stream = stream }) =
//     result {
//         let! recordType = readVariableValue stream
//         let! recordLength = readVariableValue stream
//         return struct (recordType, recordLength)
//     }

// let readRecord(reader) =
//     let { Stream = stream } = reader

//     match readRecordHeader reader with
//     | Ok struct (recordType, recordLength) ->
//         let buffer = SpanOwner.Allocate recordLength
//         let span = buffer.Span
//         if stream.Read span <> 0 then span else Span.Empty
//     | _ -> Span.Empty

[<Sealed>]
type Reader1(stream: Stream) =
    do ArgumentNullException.ThrowIfNull stream
    let mutable buffer = Unchecked.defaultof<byte MemoryOwner>

    member this.ReadContentAsInt() =
        let result = BitConverter.ToInt32(buffer.Span.Slice(0, 4))
        buffer <- buffer.Slice(0, 4)
        result

    member this.ReadContentAsUInt() =
        let buf = buffer
        let result = BitConverter.ToUInt32(buf.Span.Slice(0, 4))
        buffer <- buf.Slice(0, 4)
        result

    abstract OnDispose: unit -> unit
    override this.OnDispose() = ()

    member this.Dispose disposing =
        if disposing then
            this.OnDispose()
            stream.Dispose()
            GC.SuppressFinalize this

    member this.Dispose() = this.Dispose true
    override this.Finalize() = this.Dispose false

    interface IDisposable with
        member this.Dispose() = this.Dispose()

namespace Nava.Excel.Packaging

type Reader =
    | Binary of Biff.Reader
    | Xml of Xml.Reader

module Reader =
    let readWorkbook(reader: Reader) =
        match reader with
        | Binary biff -> biff |> ignore
        | Xml xml -> xml |> ignore