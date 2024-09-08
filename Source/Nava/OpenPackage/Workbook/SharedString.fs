namespace Nava.Excel.Common

#nowarn "3391"

open System.Collections.Generic
open System.IO
open System.Xml
open System.Text
open Nava.Runtime.Xml
open FsToolkit.ErrorHandling

module SharedString =

    let loadStrings (dictionary: IReadOnlyDictionary<string, Relation>) (package: OpenPackage) =
        voption {
            let! relation =
                dictionary.Values
                |> Seq.tryFind(fun relation -> relation.Type = Relation.Type.SharedStrings)

            let! entry = package.ZipArchive.TryFindEntry relation.Target
            use stream = entry.Open()
            use reader = XmlReader.Create(stream, XmlReader.defaultSettings)

            return [|
                for reader in
                    reader
                    |> XmlReader.elements "t" do
                    yield reader.ReadElementContentAsString()
            |]
        }

    let saveStrings (strings: string[]) (builder: StringBuilder) = //(stream: Stream) =
        use writer = XmlWriter.Create(builder)
        writer.WriteStartDocument true
        writer.WriteStartElement("sst", "http://schemas.openxmlformats.org/spreadsheetml/2006/main")
        writer.WriteAttributeString("uniqueCount", strings.Length.ToString())

        strings
        |> Array.iter(fun str ->
            writer.WriteStartElement("si")
            writer.WriteElementString("t", str)
            writer.WriteEndElement())

        writer.WriteEndDocument()