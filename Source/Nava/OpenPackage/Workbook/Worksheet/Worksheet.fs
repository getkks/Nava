namespace Nava.Excel.Common

#nowarn "3391"

open System
open System.Collections.Frozen
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.IO.Compression
open System.Xml

open Nava.Runtime.Collections
open Nava.Runtime.Xml
open FsToolkit.ErrorHandling
open FsToolkit.ErrorHandling.Operator.Result
open InlineIL

[<RequireQualifiedAccess>]
type CellType =
    | Boolean
    | Date
    | Error
    | InlineString
    | Numeric
    | SharedString
    | String
    | Unkownn

    override this.ToString() =
        match this with
        | Boolean -> nameof Boolean
        | Date -> nameof Date
        | Error -> nameof Error
        | InlineString -> nameof InlineString
        | Numeric -> nameof Numeric
        | SharedString -> nameof SharedString
        | String -> nameof String
        | Unkownn -> nameof Unkownn

    static member TryParse(span: char ReadOnlySpan) =
        match span.Length with
        | 0 -> Unkownn |> ValueSome
        | _ ->
            match span[0] with
            | 'b' -> Boolean |> ValueSome
            | 'd' -> Date |> ValueSome
            | 'e' -> Error |> ValueSome
            | 'n' -> Numeric |> ValueSome
            | 's' ->
                if span.Length = 1 then
                    SharedString
                    |> ValueSome
                else
                    String |> ValueSome
            | _ -> ValueNone

type Worksheet = {
    Name: string
    PartName: string
    Relations: IReadOnlyDictionary<string, Relation> voption
    ZipArchiveEntry: ZipArchiveEntry
}

module Worksheet =

    let loadWorksheet name (zipArchiveEntry: ZipArchiveEntry) =
        result {
            let partName = zipArchiveEntry.FullName

            let relations =
                zipArchiveEntry.Archive
                |> OpenPackage.loadRelations partName
                |> Result.map(fun dictionary ->
                    dictionary
                    |> Dictionary.freeze
                    :> IReadOnlyDictionary<_, _>)
                |> ValueOption.ofResult

            return {
                Name = name
                PartName = partName
                Relations = relations
                ZipArchiveEntry = zipArchiveEntry
            }
        }

    let enumerateRows(worksheet: Worksheet) =

        seq {
            use stream = worksheet.ZipArchiveEntry.Open()
            use reader = XmlReader.Create(stream, XmlReader.defaultSettings)

            for reader in
                reader
                |> XmlReader.elements "c" do
                let cell = CellPosition.ParseReadOnly(reader.GetAttribute "r")

                let cellType =
                    "t"
                    |> reader.GetAttribute
                    |> CellType.TryParse

                let value =
                    match cellType with
                    | ValueSome CellType.InlineString when reader.ReadToFollowing "t" -> reader.ReadElementContentAsString()
                    | ValueSome CellType.SharedString when reader.ReadToFollowing "v" ->
                        reader
                            .ReadElementContentAsInt()
                            .ToString()
                    | ValueSome _ when reader.ReadToFollowing "v" -> reader.ReadElementContentAsString()
                    | _ -> String.Empty

                yield cell, value
        }
        |> Seq.groupBy(fun (cell, _) -> cell.Column)
        |> Seq.map snd