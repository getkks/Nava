namespace Nava.Excel.Common

#nowarn "3391"

open System

[<RequireQualifiedAccess>]
module Relation =
    [<Literal>]
    let PackageRelationPart = "_rels/.rels"

    [<Literal>]
    let RelationNS = "http://schemas.openxmlformats.org/package/2006/relationships"

    [<Literal>]
    let RelationBase = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/"

    [<Literal>]
    let DocRelationType =
        RelationBase
        + "officeDocument"

    [<Literal>]
    let WorksheetRelType =
        RelationBase
        + "worksheet"

    [<Literal>]
    let StylesRelType =
        RelationBase
        + "styles"

    [<Literal>]
    let TableRelType =
        RelationBase
        + "table"

    [<Literal>]
    let SharedStringsRelType =
        RelationBase
        + "sharedStrings"

    [<Literal>]
    let CalcChainRelType =
        RelationBase
        + "calcChain"

    [<Literal>]
    let ThemeRelType =
        RelationBase
        + "theme"

    [<Literal>]
    let ApplicationRelType =
        RelationBase
        + "extended-properties"

    [<Literal>]
    let CoreRelType = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"

    [<Literal>]
    let AppPath = "docProps/app.xml"

    type ContentFormat =
        | Binary = 1
        | XML = 2

    type Type =
        | Application = 1
        | CalcChain = 2
        | CoreProperties = 3
        | SharedStrings = 4
        | Style = 5
        | Table = 6
        | Theme = 7
        | Workbook = 8
        | Worksheet = 9

    module Type =
        let toString(relation: Type) =
            match relation with
            | Type.Application -> ApplicationRelType
            | Type.CoreProperties -> CoreRelType
            | Type.Workbook -> DocRelationType
            | Type.CalcChain -> CalcChainRelType
            | Type.SharedStrings -> SharedStringsRelType
            | Type.Style -> StylesRelType
            | Type.Table -> TableRelType
            | Type.Theme -> ThemeRelType
            | Type.Worksheet -> WorksheetRelType
            | _ -> raise(NotImplementedException())

        let from(relation: string) : Type voption =
            let span = relation.AsSpan()
            let mutable retValue = ValueNone

            if span.StartsWith(RelationBase) then
                let span = span.Slice RelationBase.Length

                if span.Length > 0 then
                    match span[0] with
                    | 'c' when span.SequenceEqual "calcChain" -> retValue <- ValueSome Type.CalcChain
                    | 'e' when span.SequenceEqual "extended-properties" -> retValue <- ValueSome Type.Application
                    | 'o' when span.SequenceEqual "officeDocument" -> retValue <- ValueSome Type.Workbook
                    | 's' when span.SequenceEqual "sharedStrings" -> retValue <- ValueSome Type.SharedStrings
                    | 's' when span.SequenceEqual "styles" -> retValue <- ValueSome Type.Style
                    | 't' when span.SequenceEqual "table" -> retValue <- ValueSome Type.Table
                    | 't' when span.SequenceEqual "theme" -> retValue <- ValueSome Type.Theme
                    | 'w' when span.SequenceEqual "worksheet" -> retValue <- ValueSome Type.Worksheet
                    | _ -> ()
            elif span.SequenceEqual CoreRelType then
                retValue <- ValueSome Type.CoreProperties

            retValue

[<NoComparison; CustomEquality>]
type Relation = {
    Id: string
    Target: string
    Type: Relation.Type
} with

    member this.Equals(other: Relation) =
        this.Id = other.Id
        && this.Type = other.Type
        && this.Target = other.Target

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
                "Id\t\t: " + this.Id

                "Type\t: "
                + string(this.Type)

                "Target\t: "
                + this.Target
            }

            "}"
        }