namespace Nava.Excel.Common

#nowarn "3391"

open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.IO.Compression
open System.Xml

open Nava.Runtime.Collections
open Nava.Runtime.Xml
open FsToolkit.ErrorHandling
open InlineIL
open Zio

type ZipPackage = {
    ZipArchive: ZipArchive
    Entries: IReadOnlyDictionary<UPath, ZipArchiveEntry>
} with

    member this.Dispose disposing =
        if disposing then
            this.ZipArchive.Dispose()

    member this.Dispose() = this.Dispose true

    interface IDisposable with
        member this.Dispose() = this.Dispose()

    static member From(zipArchive: ZipArchive) = {
        ZipArchive = zipArchive
        Entries =
            zipArchive.Entries
            |> Dictionary.freezeSequence(fun entry -> UPath entry.FullName)
    }

    static member From(leaveStreamOpen, stream) =
        ZipArchive(stream, ZipArchiveMode.Read, leaveStreamOpen) |> ZipPackage.From