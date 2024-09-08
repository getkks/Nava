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
open Zio

module OpenPart =
    let relsDirectory = UPath("_rels")

[<AbstractClass>]
type OpenPart(id: string, target: UPath, zipPackage: ZipPackage) =
    let relationsPath =
        UPath.Combine(
            (match target.GetDirectory() with
             | parent -> if parent.IsNull then UPath.Empty else parent),
            OpenPart.relsDirectory,
            UPath(target.GetName() + ".rels")
        )

    let fixPath lead follow =
        Path.GetFullPath("/" + lead + "/" + follow).TrimStart('/')

    let relations =
        let directory =
            match target.GetDirectory() with
            | parent -> if parent.IsNull then UPath.Empty else parent

        let dictionary = Dictionary()

        match zipPackage.TryGetValue relationsPath.FullName with
        | true, entry ->
            use relations = entry.Open()
            use reader = XmlReader.Create(relations, XmlReader.defaultSettings)

            for _ in reader |> XmlReader.elements "Relationship" do
                let relation = Relation()

                for _ in reader |> XmlReader.attributes do
                    match reader.LocalName with
                    | "Id" -> relation.Id <- reader.Value
                    | "Target" -> relation.Target <- directory / UPath reader.Value //fixPath directory reader.Value
                    | "Type" ->
                        match reader.Value |> RelationType.fromString with
                        | ValueSome value -> relation.Type <- value
                        | _ -> ()
                    | _ -> ()

                dictionary.Add(relation.Id, relation)
        | false, _ -> ()

        dictionary |> Dictionary.freeze

    member this.ID = id
    member this.RelationPath = relationsPath
    member this.Relations = relations
    member this.Target = target
    member this.ZipPackage = zipPackage
    member internal this.OpenPart = this
    new(id, target, parentPart: OpenPart) = OpenPart(id, target, parentPart.ZipPackage)

    member this.ToString(derived: string) =
        stringBuffer {
            "{"

            indent {
                "Id: " + this.ID
                "Target: " + this.Target.FullName
                "Relations: ["
                this.Relations |> Seq.map _.Value.ToString() |> String.concat ", "
                derived
                "]"
            }

            "}"
        }

    override this.ToString() = this.ToString ""
