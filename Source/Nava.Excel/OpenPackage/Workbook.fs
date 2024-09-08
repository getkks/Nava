namespace Nava.Excel

#nowarn "3391"

open System
open System.Collections.Generic
open System.IO
open System.IO.Compression
open System.Xml

open Nava.Runtime.Collections
open Nava.Runtime.Xml
open Nava.Runtime.Zip
open FsToolkit.ErrorHandling
open InlineIL
open StringBuffer

type Workbook internal (id, target, openPart: OpenPart) =
    inherit OpenPart(id, target, openPart)

    let worksheets =
        let stream = base.ZipPackage[target].Open()
        let reader = XmlReader.Create(stream, XmlReader.defaultSettings)
        let dictionary = Dictionary()
        let this = base.OpenPart

        try
            for _ in XmlReader.elements "sheet" reader do
                let mutable id = null
                let mutable name = null

                for _ in XmlReader.attributes reader do
                    match reader.LocalName with
                    | "id" -> id <- reader.Value
                    | "name" -> name <- reader.Value
                    | _ -> ()

                dictionary.Add(name, Worksheet(name, id, this.Relations[id].Target, this))
        finally
            reader.Dispose()
            stream.Dispose()

        dictionary |> Dictionary.freeze :> IReadOnlyDictionary<_, _>

    override this.ToString() =
        base.ToString(
            stringBuffer {
                "Worksheets: ["
                worksheets.Values |> Seq.map _.ToString() |> String.concat ", "
                "]"
            }
        )