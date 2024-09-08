namespace Nava.Runtime.Zip

#nowarn "3391"

open System
open System.Collections.Generic
open System.IO
open System.IO.Compression

open Nava.Runtime.Collections
open FsToolkit.ErrorHandling
open InlineIL
open Zio

[<AutoOpen>]
module ZipArchiveExtensions =
    let openEntry(entry: ZipArchiveEntry) = entry.Open()

    let stripLeadingPathSeparator(path: string) =
        match path[0] with
        | '/'
        | '\\' -> path.Substring 1
        | _ -> path

    let makeRelative(root: string, path: string) =
        if Path.IsPathRooted path then
            stripLeadingPathSeparator path
        elif root.Length = 0 then
            path
        else
            Path.Combine(root, path)

    type ZipArchive with
        member this.FindEntry name =
            let getEntry name (package: ZipArchive) =
                package.Entries
                |> Seq.tryFind _.FullName.Equals(name, StringComparison.OrdinalIgnoreCase)

            match getEntry name this with
            | ValueNone ->
                if name.StartsWith '/' || name.StartsWith '\\' then
                    getEntry (name.Substring 1) this
                else
                    ValueNone
            | entry -> entry

        member this.FindEntry(root, name) =
            this.FindEntry(makeRelative(root, name))

    let inline findEntry name (package: ZipArchive) = package.FindEntry name

type ZipPackage(zipArchive: ZipArchive) =
    let entries =
        zipArchive.Entries
        |> Dictionary.freezeSequence(fun entry -> UPath entry.FullName)
        :>
        // |> stripLeadingPathSeparator)
        IReadOnlyDictionary<_, _>

    new(disposeStream, stream) = ZipPackage(ZipArchive(stream, ZipArchiveMode.Read, not disposeStream))

    member this.Entries = entries

    member this.Item
        with get name = entries[name]

    member this.TryGetValue(name: string, value: ZipArchiveEntry outref) = entries.TryGetValue(name, &value)

    member this.Dispose disposing =
        if disposing then
            zipArchive.Dispose()

    member this.Dispose() = this.Dispose true

    interface IDisposable with
        member this.Dispose() = this.Dispose()

    override this.ToString() =
        "[" + (entries |> Seq.map _.Key.FullName |> String.concat "; ") + "]"