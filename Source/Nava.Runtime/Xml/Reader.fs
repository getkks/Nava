namespace Nava.Runtime.Xml

open System.Xml

module XmlParser =
    type Parser<'T> = | Parser of (XmlReader -> Result<'T, string>)

    let rec readToNextStart(reader: XmlReader) =
        reader.EOF |> not
        && (reader.Read() && reader.NodeType = XmlNodeType.Element || readToNextStart reader)

    let emptyElement func name =
        Parser(fun reader ->
            if reader.Name = name && reader.IsEmptyElement then
                let result = func reader
                reader |> readToNextStart |> ignore
                result |> Ok
            else
                "Empty element \"" + name + "\" not found" |> Error)

    let startElement func name =
        Parser(fun reader ->
            if reader.NodeType = XmlNodeType.Element && reader.Name = name then
                let result = func reader
                reader |> readToNextStart |> ignore
                result |> Ok
            else
                "Start element \"" + name + "\" not found" |> Error)

    let either (Parser parser1) (Parser parser2) =
        Parser(fun reader ->
            match parser1 reader with
            | Ok _ as ok -> ok
            | _ -> parser2 reader)

    let whenMatch(Parser parser1) =
        Parser(fun reader ->
            let rec loop reader =
                if readToNextStart reader then
                    match parser1 reader with
                    | Ok _ as ok -> ok
                    | _ -> loop reader
                else
                    Error "Reached end of file."

            loop reader)

    let run (reader: XmlReader) (Parser parser) = parser reader
    open System.IO

    let check() =
        let file = File.Open("/var/home/karthikkselvan/drives/development/FSharp/Nava/Data/sheet1.xml", FileMode.Open)
        let reader = XmlReader.Create file

        match
            "sheetPr"
            |> startElement(fun _ -> "Found Sheet Properties.")
            |> whenMatch
            |> run reader
        with
        | Ok result -> result
        | Error err -> err
        |> printfn "%s"

        reader.Close()
        file.Dispose()

[<RequireQualifiedAccess>]
module XmlReader =
    let defaultNameTable = NameTable()

    let defaultSettings =
        XmlReaderSettings(
            Async = false,
            CheckCharacters = false,
            ConformanceLevel = ConformanceLevel.Document,
            DtdProcessing = DtdProcessing.Ignore,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true,
            MaxCharactersFromEntities = 0,
            NameTable = defaultNameTable,
            Schemas = Schema.XmlSchemaSet defaultNameTable,
            ValidationFlags = Schema.XmlSchemaValidationFlags.None,
            ValidationType = ValidationType.None
        )

    let elements elementName (reader: XmlReader) =
        seq {
            while reader.ReadToFollowing elementName do
                reader
        }

    let maintainDepth depth (reader: XmlReader) =
        while reader.Depth > depth && reader.Read() do
            ()

        reader.Depth = depth

    let moveTo element (reader: XmlReader) =
        if element |> reader.ReadToFollowing then
            Ok reader
        else
            "Move to element \"" + element + "\" failed" |> Error

    let isStart element (reader: XmlReader) =
        reader.NodeType = XmlNodeType.Element && reader.Name = element

    let siblings element (reader: XmlReader) =
        if isStart element reader || reader.ReadToFollowing element then
            let depth = reader.Depth

            seq {
                yield reader

                while maintainDepth depth reader do
                    while reader.Read() && depth = reader.Depth do
                        if isStart element reader then
                            yield reader
            }
        else
            System.Linq.Enumerable.Empty()

    let attributes(reader: XmlReader) =
        seq {
            while reader.MoveToNextAttribute() do
                yield reader
        }

    let foldAttributes map (state) (reader: XmlReader) =
        let mutable state = state

        if reader.MoveToFirstAttribute() then
            while (state <- map state reader
                   reader.MoveToNextAttribute()) do
                ()

        state