namespace Nava.Excel

#nowarn "3391"

open System
open Zio

[<RequireQualifiedAccess>]
module Relation =
    [<Literal>]
    let PackageRelationPart = "_rels/.rels"

    [<Literal>]
    let RelationNS = "http://schemas.openxmlformats.org/package/2006/relationships"

    [<Literal>]
    let RelationBase = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/"

    [<Literal>]
    let DocRelationType = RelationBase + "officeDocument"

    [<Literal>]
    let WorksheetRelType = RelationBase + "worksheet"

    [<Literal>]
    let StylesRelType = RelationBase + "styles"

    [<Literal>]
    let TableRelType = RelationBase + "table"

    [<Literal>]
    let SharedStringsRelType = RelationBase + "sharedStrings"

    [<Literal>]
    let CalcChainRelType = RelationBase + "calcChain"

    [<Literal>]
    let ThemeRelType = RelationBase + "theme"

    [<Literal>]
    let ApplicationRelType = RelationBase + "extended-properties"

    [<Literal>]
    let CoreRelType = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"

    [<Literal>]
    let AppPath = "docProps/app.xml"

type ContentFormat =
    | Binary = 1
    | XML = 2

type RelationType =
    | Application = 1
    | CalcChain = 2
    | CoreProperties = 3
    | SharedStrings = 4
    | Style = 5
    | Table = 6
    | Theme = 7
    | Workbook = 8
    | Worksheet = 9

module RelationType =
    let toString(relation: RelationType) =
        match relation with
        | RelationType.Application -> Relation.ApplicationRelType
        | RelationType.CoreProperties -> Relation.CoreRelType
        | RelationType.Workbook -> Relation.DocRelationType
        | RelationType.CalcChain -> Relation.CalcChainRelType
        | RelationType.SharedStrings -> Relation.SharedStringsRelType
        | RelationType.Style -> Relation.StylesRelType
        | RelationType.Table -> Relation.TableRelType
        | RelationType.Theme -> Relation.ThemeRelType
        | RelationType.Worksheet -> Relation.WorksheetRelType
        | _ -> raise(NotImplementedException())

    let fromString(relation: string) : RelationType voption =
        let span = relation.AsSpan()
        let mutable retValue = ValueNone

        if span.StartsWith(Relation.RelationBase) then
            let span = span.Slice Relation.RelationBase.Length

            if span.Length > 0 then
                match span[0] with
                | 'c' when span.SequenceEqual "calcChain" -> retValue <- ValueSome RelationType.CalcChain
                | 'e' when span.SequenceEqual "extended-properties" -> retValue <- ValueSome RelationType.Application
                | 'o' when span.SequenceEqual "officeDocument" -> retValue <- ValueSome RelationType.Workbook
                | 's' when span.SequenceEqual "sharedStrings" -> retValue <- ValueSome RelationType.SharedStrings
                | 's' when span.SequenceEqual "styles" -> retValue <- ValueSome RelationType.Style
                | 't' when span.SequenceEqual "table" -> retValue <- ValueSome RelationType.Table
                | 't' when span.SequenceEqual "theme" -> retValue <- ValueSome RelationType.Theme
                | 'w' when span.SequenceEqual "worksheet" -> retValue <- ValueSome RelationType.Worksheet
                | _ -> ()
        elif span.SequenceEqual Relation.CoreRelType then
            retValue <- ValueSome RelationType.CoreProperties

        retValue

type Relation =
    val mutable Id: string
    val mutable Target: UPath
    val mutable Type: RelationType

    new() =
        {
            Id = Unchecked.defaultof<_>
            Target = Unchecked.defaultof<_>
            Type = Unchecked.defaultof<_>
        }

    member this.Equals(other: Relation) =
        this.Id = other.Id && this.Type = other.Type && this.Target = other.Target

    override this.Equals(object: obj) =
        match object with
        | :? Relation as other -> this.Equals(other)
        | _ -> false

    override this.GetHashCode() =
        HashCode.Combine(this.Id.GetHashCode(), this.Type.GetHashCode(), this.Target.GetHashCode())

    override this.ToString() =
        stringBuffer {
            "{"

            indent {
                "Id: " + this.Id
                "Type: " + string(this.Type)
                "Target: " + this.Target.FullName
            }

            "}"
        }