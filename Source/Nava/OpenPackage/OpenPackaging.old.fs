namespace Nava.Excel.Common

#nowarn "3391"

open System
open System.Collections.Generic
open System.IO
open System.IO.Compression
open System.Xml

open Nava.Runtime.Collections
open Nava.Runtime.Xml

open InlineIL

module ContentType =
    [<Literal>]
    let calcChain = "application/vnd.openxmlformats-officedocument.spreadsheetml.calcChain+xml"

    [<Literal>]
    let calcChainBinary = "application/vnd.ms-excel.calcChain"

    [<Literal>]
    let coreProperties = "application/vnd.openxmlformats-package.core-properties+xml"

    [<Literal>]
    let extendedProperties = "application/vnd.openxmlformats-officedocument.extended-properties+xml"

    [<Literal>]
    let relationships = "application/vnd.openxmlformats-package.relationships+xml"

    [<Literal>]
    let sharedStrings = "application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"

    [<Literal>]
    let sharedStringsBinary = "application/vnd.ms-excel.sharedStrings"

    [<Literal>]
    let style = "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"

    [<Literal>]
    let styleBinary = "application/vnd.ms-excel.styles"

    [<Literal>]
    let table = "application/vnd.openxmlformats-officedocument.spreadsheetml.table+xml"

    [<Literal>]
    let tableBinary = "application/vnd.ms-excel.table"

    [<Literal>]
    let theme = "application/vnd.openxmlformats-officedocument.theme+xml"

    [<Literal>]
    let workbook = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"

    [<Literal>]
    let workbookBinary = "application/vnd.ms-excel.workbook"

    [<Literal>]
    let worksheet = "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"

    [<Literal>]
    let worksheetBinary = "application/vnd.ms-excel.worksheet"

    [<Literal>]
    let xmlFile = "application/xml"

type ContentFormat =
    | Binary
    | XML

type ContentType =
    | CalcChain of ContentFormat
    | CoreProperties
    | ExtendedProperties
    | Relationships
    | SharedStrings of ContentFormat
    | Style of ContentFormat
    | Table of ContentFormat
    | Theme
    | Workbook of ContentFormat
    | Worksheet of ContentFormat
    | XMLFile

    override this.ToString() =
        match this with
        | CalcChain Binary -> ContentType.calcChainBinary
        | CalcChain XML -> ContentType.calcChain
        | CoreProperties -> ContentType.coreProperties
        | ExtendedProperties -> ContentType.extendedProperties
        | Relationships -> ContentType.relationships
        | SharedStrings Binary -> ContentType.sharedStringsBinary
        | SharedStrings XML -> ContentType.sharedStrings
        | Style Binary -> ContentType.styleBinary
        | Style XML -> ContentType.style
        | Table Binary -> ContentType.tableBinary
        | Table XML -> ContentType.table
        | Theme -> ContentType.theme
        | Workbook Binary -> ContentType.workbookBinary
        | Workbook XML -> ContentType.workbook
        | Worksheet Binary -> ContentType.worksheetBinary
        | Worksheet XML -> ContentType.worksheet
        | XMLFile -> ContentType.xmlFile

    static member op_Implicit(contentType: ContentType) = contentType.ToString()

    static member op_Explicit(contentType: string) =
        match contentType with
        | ContentType.calcChain ->
            CalcChain XML
            |> ValueSome
        | ContentType.calcChainBinary ->
            CalcChain Binary
            |> ValueSome
        | ContentType.coreProperties ->
            CoreProperties
            |> ValueSome
        | ContentType.extendedProperties ->
            ExtendedProperties
            |> ValueSome
        | ContentType.relationships ->
            Relationships
            |> ValueSome
        | ContentType.sharedStrings ->
            SharedStrings XML
            |> ValueSome
        | ContentType.sharedStringsBinary ->
            SharedStrings Binary
            |> ValueSome
        | ContentType.style ->
            Style XML
            |> ValueSome
        | ContentType.styleBinary ->
            Style Binary
            |> ValueSome
        | ContentType.table ->
            Table XML
            |> ValueSome
        | ContentType.tableBinary ->
            Table Binary
            |> ValueSome
        | ContentType.theme -> Theme |> ValueSome
        | ContentType.workbook ->
            Workbook XML
            |> ValueSome
        | ContentType.workbookBinary ->
            Workbook Binary
            |> ValueSome
        | ContentType.worksheet ->
            Worksheet XML
            |> ValueSome
        | ContentType.worksheetBinary ->
            Worksheet Binary
            |> ValueSome
        | ContentType.xmlFile -> XMLFile |> ValueSome
        | _ -> ValueNone

module OpenPackaging =
    type Parts =
        | SinglePart of Path: string
        | MultipleParts of Paths: string List

    [<Literal>]
    let contentTypesFile = "[Content_Types].xml"

    let getWorkbookParts(package: ZipArchive) =
        match package.GetEntry contentTypesFile with
        | null -> ValueNone
        | entry ->
            use stream = entry.Open()

            XmlReader.Create(stream, XmlReader.defaultSettings)
            |> XmlReader.elements "Override"
            |> Seq.map(fun reader ->
                reader.GetAttribute("ContentType")
                |> ContentType.op_Explicit,
                reader.GetAttribute("PartName"))
            |> Seq.filter(fun (contentType, _) -> contentType.IsSome)
            |> ValueOption.ofObj

    let loadWorkbookStructure(package: ZipArchive) = ""

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
    let CoreRelType =
        RelationBase
        + "core-properties"

    [<Literal>]
    let AppPath = "docProps/app.xml"

    [<Struct>]
    type RelationsType =
        | Application
        | Core
        | Workbook

        static member op_Implicit(relation: RelationsType) =
            match relation with
            | Application -> ApplicationRelType
            | Core -> CoreRelType
            | Workbook -> DocRelationType

        static member op_Explicit(relation: string) =
            let span = relation.AsSpan()

            if span.StartsWith(RelationBase) then
                let span = span.Slice RelationBase.Length

                if span.Length > 0 then
                    match span[0] with
                    | 'c' when span.SequenceEqual "extended-properties" -> ValueSome Core
                    | 'e' when span.SequenceEqual "extended-properties" -> ValueSome Application
                    | 'o' when span.SequenceEqual "officeDocument" -> ValueSome Workbook
                    | _ -> ValueNone
                else
                    ValueNone
            else
                ValueNone

    and [<Struct>] WorkbookRelationsType =
        | CalcChain
        | SharedStrings
        | Style
        | Table
        | Theme
        | Worksheet

        static member op_Implicit(relation: WorkbookRelationsType) =
            match relation with
            | CalcChain -> CalcChainRelType
            | SharedStrings -> SharedStringsRelType
            | Style -> StylesRelType
            | Table -> TableRelType
            | Theme -> ThemeRelType
            | Worksheet -> WorksheetRelType

        static member op_Explicit(relation: string) : WorkbookRelationsType voption =
            let span = relation.AsSpan()
            let mutable retValue = ValueNone

            if span.StartsWith(RelationBase) then
                let span = span.Slice RelationBase.Length

                if span.Length > 0 then
                    match span[0] with
                    | 'c' when span.SequenceEqual "calcChain" ->
                        retValue <-
                            CalcChain
                            |> ValueSome
                    | 's' when span.SequenceEqual "sharedStrings" ->
                        retValue <-
                            SharedStrings
                            |> ValueSome
                    | 's' when span.SequenceEqual "styles" -> retValue <- Style |> ValueSome
                    | 't' when span.SequenceEqual "table" -> retValue <- Table |> ValueSome
                    | 't' when span.SequenceEqual "theme" -> retValue <- Theme |> ValueSome
                    | 'w' when span.SequenceEqual "worksheet" ->
                        retValue <-
                            Worksheet
                            |> ValueSome
                    | _ -> ()

            retValue

        override this.ToString() = this

    let getWorkbookPart(package: ZipArchive) =
        match package.GetEntry(PackageRelationPart) with
        | null -> ValueNone
        | entry ->
            use stream = entry.Open()
            // XmlReader.Create(stream, settings) |> getWorkbookPartReaderLoop
            XmlReader.Create(stream, XmlReader.defaultSettings)
            |> XmlReader.elements "Relationship"
            |> Seq.tryFind(fun reader -> reader.GetAttribute("Type") = DocRelationType)
            |> ValueOption.map _.GetAttribute("Target")

    let getPartRelationsName(partName: string) =
        match Path.GetDirectoryName partName with
        | null -> String.Empty
        | dir -> dir
        + "/_rels/"
        + Path.GetFileName partName
        + ".rels"

    type ZipArchive with

        member this.FindEntry name =
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

    let inline findEntry name (package: ZipArchive) = package.FindEntry name

    let stripLeadingPathSeparator(path: string) =
        match path[0] with
        | '/'
        | '\\' -> path.Substring 1
        | _ -> path

    type WorkbookRelation = {
        CalcChain: string
        SharedStrings: string
        Style: string
        Table: string
        Theme: string
        Workbook: string
        Worksheet: string
    }

    type Relations =
        | Target of string
        | PartRelation of Collections.Frozen.FrozenDictionary<string, string>
        | NestedPartRelation of Collections.Frozen.FrozenDictionary<WorkbookRelationsType, Relations>

    let makeRelative(root: string, path: string) =
        if Path.IsPathRooted path then
            stripLeadingPathSeparator path
        else if root.Length = 0 then
            path
        else
            Path.Combine(root, path)

    open FsToolkit.ErrorHandling

    let loadWorkbookRelations workbookPartName (package: ZipArchive) =
        let sheetRelMap = Dictionary()
        let workbookPartRelsName = getPartRelationsName workbookPartName

        let root =
            match Path.GetDirectoryName workbookPartName with
            | null -> String.Empty
            | root -> root

        package
        |> findEntry workbookPartRelsName
        |> ValueOption.map(fun part ->
            use stream = part.Open()
            let reader = XmlReader.Create(stream, XmlReader.defaultSettings)

            for reader in
                reader
                |> XmlReader.elements "Relationship" do
                sheetRelMap.Add(reader.GetAttribute "Id", makeRelative(root, reader.GetAttribute "Target"))

            sheetRelMap)
        |> Result.requireValueSome(InvalidDataException() :> exn)