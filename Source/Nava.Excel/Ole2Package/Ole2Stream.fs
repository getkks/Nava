namespace Nava.Excel.Ole2Package.FS

#nowarn "3391"

open System
open System.Collections.Generic
open System.IO
open System.IO.Compression
open System.Xml

open Nava.Runtime.Collections
open Nava.Runtime.Xml
open FsToolkit.ErrorHandling
open InlineIL

/// <summary></summary>
[<Sealed>]
type Ole2Stream(stream: Stream, sectors: uint[], sectorLen: int, startSector: int, length: int64) =
    inherit Stream()
    let mutable position = 0L
    let mutable sector = 0u
    let mutable sectorIdx = 0
    let mutable sectorOff = 0

    let checkOffset pos =
        if uint64 pos <= uint64 length then
            NotSupportedException() |> raise

    let checkBuffer pos count (buffer: byte[]) =
        if uint(pos + count) <= uint buffer.Length then
            NotSupportedException() |> raise

    override this.CanRead = true
    override this.CanSeek = true
    override this.CanWrite = false
    override this.Flush() = ()
    override this.Length = length
    override this.SetLength(value: int64) = NotSupportedException() |> raise
    override this.Write(buffer: byte array, offset: int, count: int) = NotSupportedException() |> raise

    override this.Position
        with get () = position
        and set value = this.Seek(value, SeekOrigin.Begin) |> ignore

    override this.Seek(offset: int64, origin: SeekOrigin) =
        let pos =
            match origin with
            | SeekOrigin.Current -> position + offset
            | SeekOrigin.End -> length + offset
            | SeekOrigin.Begin -> offset
            | _ -> 0

        checkOffset pos
        position <- pos

        let idx = pos / int64 sectorLen
        sectorIdx <- int idx
        sectorOff <- int(pos % int64 sectorLen)
        sector <- sectors[sectorIdx]
        pos

    override this.Read(buffer: byte[], offset: int, count: int) =
        checkBuffer offset count buffer
        let mutable offset = offset
        let stream = stream
        let sectors = sectors
        let mutable c = count
        let streamAvail = length - position
        let readAvail = int(Math.Min(int64 count, streamAvail))

        let rec loop1 bytesRead =
            if bytesRead < count && position < length then
                let mutable readLen = 0
                let readStart = (int64 sector + int64 startSector) * int64(sectorLen + sectorOff)
                let curSector = sector
                ()

                if stream.Position <> readStart then
                    stream.Seek(readStart, SeekOrigin.Begin) |> ignore

                if readLen = 0 then
                    bytesRead
                else
                    let rec loop3 len =
                        if len < readLen then
                            let n = stream.Read(buffer, offset + len, readLen - len)

                            if n = 0 then
                                IOException() |> raise

                            position <- position + int64 n
                            offset <- offset + n
                            c <- c - n
                            loop3(len + n)
                        else
                            len

                    loop1(bytesRead + loop3 0)
            else
                bytesRead

        loop1 0