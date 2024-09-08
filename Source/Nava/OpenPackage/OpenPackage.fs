namespace Nava.Excel.Common

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
        else if root.Length = 0 then
            path
        else
            Path.Combine(root, path)

    type ZipArchive with

        member this.TryFindEntry name =
            let getEntry name (package: ZipArchive) =
                package.Entries
                |> Seq.tryFind _.FullName.Equals(name, StringComparison.OrdinalIgnoreCase)

            match getEntry name this with
            | ValueNone ->
                if
                    name.StartsWith '/'
                    || name.StartsWith '\\'
                then
                    getEntry (name.Substring 1) this
                else
                    ValueNone
            | entry -> entry

        member this.FindEntry(name: string) =
            this
                .TryFindEntry(name)
                .Value

        member this.TryFindEntry(root, name) =
            this.TryFindEntry(makeRelative(root, name))

        member this.FindEntry(root, name) =
            this
                .TryFindEntry(root, name)
                .Value

    let inline tryFindEntry name (package: ZipArchive) = package.TryFindEntry name

[<NoComparison; NoEquality>]
type OpenPackage = {
    ZipArchive: ZipArchive
    Relation: Relation
} with

    member this.Dispose disposing =
        if disposing then
            this.ZipArchive.Dispose()

    member this.Dispose() = this.Dispose true
    override this.Finalize() = this.Dispose false

    interface IDisposable with
        member this.Dispose() = this.Dispose()

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module OpenPackage =

    let getPartRelationship(partName: string) =
        match Path.GetDirectoryName partName with
        | null -> String.Empty
        | dir -> dir
        + "/_rels/"
        + Path.GetFileName partName
        + ".rels"

    // let inline loadRelationsWithFilter partName ([<InlineIfLambda>] filter) (zipArchive: ZipArchive) =
    let loadRelationsWithFilter partName filter (zipArchive: ZipArchive) =
        let relationPath = getPartRelationship partName
        let dictionary = Dictionary()

        result {
            use! relations =
                zipArchive
                |> tryFindEntry relationPath
                |> Result.requireValueSome(RelationsNotFound(partName, relationPath))
                |> Result.map openEntry

            use reader = XmlReader.Create(relations, XmlReader.defaultSettings)

            for reader in
                reader
                |> XmlReader.elements "Relationship" do
                let id = reader.GetAttribute "Id"
                let target = reader.GetAttribute "Target"

                match
                    ("Type"
                     |> reader.GetAttribute
                     |> Relation.Type.TryParse)
                with
                | true, value ->
                    value
                    |> ValueOption.iter(fun relationType ->
                        if filter id target relationType then
                            dictionary.Add(
                                id,
                                {
                                    Id = id
                                    Target = target
                                    Type = relationType
                                }
                            ))
                | _ -> ()

            return dictionary
        }

    let loadRelations partName zipArchive =
        loadRelationsWithFilter partName (fun _ _ _ -> true) zipArchive

    [<NoComparison; NoEquality>]
    type OpenOptions = { DisposeStream: bool }

    let defaultOpenOptions = { DisposeStream = true }

    let openWithOptions (options: OpenOptions) (stream: Stream) =
        let zipArchive = ZipArchive(stream, ZipArchiveMode.Read, not options.DisposeStream)
        let mutable workbookPart = ValueNone

        zipArchive
        |> loadRelationsWithFilter null (fun id target relationType ->
            if Relation.Type.Workbook = relationType then
                workbookPart <-
                    {
                        Id = id
                        Target = target
                        Type = relationType
                    }
                    |> ValueSome

            false)
        |> ignore

        result {
            let! workbookPart =
                workbookPart
                |> Result.requireValueSome WorkbookPartPathNotFound

            return {
                ZipArchive = zipArchive
                Relation = workbookPart
            }
        }

    let loadPackageRelations(zipArchive: ZipArchive) =
        use relations =
            zipArchive
                .GetEntry(Relation.PackageRelationPart)
                .Open()

        use reader = XmlReader.Create(relations, XmlReader.defaultSettings)
        let relations = Dictionary()

        for reader in
            reader
            |> XmlReader.elements "Relationship" do
            match
                ("Type"
                 |> reader.GetAttribute
                 |> Relation.Type.TryParse)
            with
            | true, value -> relations.Add(value, reader.GetAttribute "Target")
            | _ -> ()

        relations