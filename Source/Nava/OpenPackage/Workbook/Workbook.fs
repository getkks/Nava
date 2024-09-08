namespace Nava.Excel.Common

#nowarn "3391"

open System
open System.Collections.Frozen
open System.Collections.Generic
open System.IO
open System.IO.Compression
open System.Xml

open Nava.Runtime.Collections
open Nava.Runtime.Xml
open FsToolkit.ErrorHandling
open InlineIL

type Workbook = {
    Package: OpenPackage
    Path: string
    Relations: IReadOnlyDictionary<string, Relation>
    SharedString: string[] voption
    Worksheets: IReadOnlyDictionary<string, Worksheet>
} with

    member this.Dispose disposing =
        if disposing then
            this.Package.Dispose()

    member this.Dispose() = this.Dispose true
    override this.Finalize() = this.Dispose false

    interface IDisposable with
        member this.Dispose() = this.Dispose()

module Workbook =

    let loadSheetsNameIdMapping(zipEntry: ZipArchiveEntry) =
        seq {
            use workbook = zipEntry.Open()
            use reader = XmlReader.Create(workbook, XmlReader.defaultSettings)

            for reader in
                reader
                |> XmlReader.elements "sheet" do
                yield struct (reader.GetAttribute "name", reader.GetAttribute "r:id")
        }

    let openWithOptions options stream =
        result {
            let! package = OpenPackage.openWithOptions options stream

            let {
                    Relation = { Target = target }
                    ZipArchive = zipArchive
                } =
                package

            let path = Path.GetDirectoryName target
            let! relations = OpenPackage.loadRelations target zipArchive

            let workbookEntry =
                target
                |> zipArchive.FindEntry

            let worksheets =
                seq {
                    for (name, id) in
                        workbookEntry
                        |> loadSheetsNameIdMapping do
                        match
                            zipArchive.FindEntry(path, relations[id].Target)
                            |> Worksheet.loadWorksheet name
                        with
                        | Ok worksheet -> yield worksheet
                        | _ -> ()
                }
                |> Dictionary.freezeSequenceWithComparer StringComparer.OrdinalIgnoreCase _.Name

            return {
                Package = package
                Path = path
                Relations = relations
                SharedString = SharedString.loadStrings relations package
                Worksheets = worksheets
            }
        }