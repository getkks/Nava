namespace Nava.Office

open System
open System.IO
open System.IO.Compression
open Spectre.Console
open FsToolkit.ErrorHandling

open Nava
open Nava.Runtime.FileSystem

module CsvPackage =
    type ProgressMessage =
        | CreateTask of int * string * AsyncReplyChannel<ProgressTask>
        | IncrementProgressBy of int * ProgressTask
        | IncrementMaxValueBy of int * ProgressTask

    type Progress(context: ProgressContext) =
        let mailbox =
            MailboxProcessor.Start(fun inbox ->
                async {
                    while true do
                        match! inbox.Receive() with
                        | CreateTask(max, description, channel) -> channel.Reply(context.AddTask(description, true, max))
                        | IncrementProgressBy(value, progress) -> progress.Increment value
                        | IncrementMaxValueBy(value, progress) -> progress.MaxValue <- progress.MaxValue + float value
                })

        member _.CreateTask(value, description) =
            mailbox.PostAndReply(fun channel -> CreateTask(value, description, channel))

        member _.IncrementValue(value, progress) =
            mailbox.Post(IncrementProgressBy(value, progress))

        member _.IncrementMax(value, progress) =
            mailbox.Post(IncrementMaxValueBy(value, progress))

        member this.IncrementValue(progress) = this.IncrementValue(1, progress)
        member this.IncrementMax(progress) = this.IncrementMax(1, progress)

    open Microsoft.Office.Interop.Excel
    open Workbook
    open Worksheet

    type Options = {
        Compression: CompressionLevel
        Input: FileInfo
        OpenOptions: OpenOptions
        Output: FileInfo
        Overwrite: bool
        ProgressContext: ProgressContext voption
    }

    let createOptions(file: string) =
        ArgumentNullException.ThrowIfNullOrWhiteSpace file

        {
            Compression = CompressionLevel.Optimal
            Input = file |> FileInfo
            OpenOptions = OpenOptions.create() |> OpenOptions.withReadOnly true
            Output = Path.ChangeExtension(file, ".zip") |> FileInfo
            Overwrite = true
            ProgressContext = ValueNone
        }
    // JsonSerializer.Serialize(createOptions (FileInfo "output.zip") (FileInfo "input.xlsx"),defaultJsonOptions)
    let withOutputFile output options = { options with Output = output }

    let withInputFile input options = { options with Input = input }

    let withInputOutput input output options = {
        options with
            Input = input
            Output = output
    }

    let withCompressionLevel level options = { options with Compression = level }
    let withOverwrite overwrite options = { options with Overwrite = overwrite }
    let withProgressContext context options = { options with ProgressContext = ValueSome context }

    let validateOptions options =
        let input = options.Input

        if input.Exists then
            if input.IsExcelFile() then
                Ok options
            else
                "Not a supported Excel file type." |> NotSupportedException :> Exception
                |> Error
        else
            FileNotFoundException("Input file does not exist.", input.FullName) :> Exception
            |> Error

    let countSheetsAndTables(workbook: Workbook) =
        let mutable sum = 0

        for sheet in workbook.SheetCollection do
            sum <-
                sum
                + match sheet.ListObjects.Count with
                  | 0 -> 1
                  | tables -> tables

        sum

    let createProgressTask description maxValue (ctx: ProgressContext voption) =
        ctx |> ValueOption.map _.AddTask(description, true, maxValue)

    let setDescription description (progress: ProgressTask voption) =
        progress
        |> ValueOption.iter(fun progress -> progress.Description <- description)

    let incrementProgressBy value (progress: ProgressTask voption) =
        progress |> ValueOption.iter(fun progress -> progress.Increment value)

    let incrementProgress(progress: ProgressTask voption) = incrementProgressBy 1 progress

    let incrementMaxValueBy value (progress: ProgressTask voption) =
        progress
        |> ValueOption.iter(fun progress -> progress.MaxValue <- progress.MaxValue + value)

    /// <summary> Converts an Excel file to a zip file with csv files for sheets / tables.
    /// If a sheet has one or more tables, it will create a csv file for each table.
    /// Any data in the sheet not part of a table will not be copied to the csv file. </summary>
    /// <param name="options"> Options for the conversion including input and output. </param>
    /// <returns>Output file after successful conversion.</returns>
    let convert options =
        result {
            let! options = validateOptions options
            use application = ExcelApplication false
            use tempDirectory = TemporaryDirectory()
            let outputPath = tempDirectory.FullName
            let progress = createProgressTask $"Opening {options.Input.Name}" 2 options.ProgressContext
            let! workbook = application.Open(options.Input, options.OpenOptions)
            progress |> incrementMaxValueBy(workbook |> countSheetsAndTables |> float)
            progress |> incrementProgress

            seq {
                for sheet in workbook.SheetCollection do
                    if sheet.ListObjects.Count > 0 then
                        for table in sheet.ListObjects do
                            table.Name, table.Range
                    else
                        sheet.Name, sheet.UsedRange
            }
            |> Seq.iter(fun (name, dataRange) ->
                progress |> setDescription $"Converting {name}"
                let book = application.Workbooks.Add()
                dataRange.Copy(book |> activeSheet |> range "A1") |> ignore
                book.SaveAs($"""{Path.Combine(outputPath, $"{name}.csv")}""", XlFileFormat.xlCSV)
                book.Close()
                progress |> incrementProgress)

            let outputFile = options.Output

            if outputFile.Exists && options.Overwrite then
                outputFile.Delete()

            progress |> setDescription $"Creating {outputFile.Name}"
            ZipFile.CreateFromDirectory(outputPath, outputFile.FullName, CompressionLevel.Optimal, false)
            progress |> incrementProgress
            progress |> setDescription $"Created {outputFile.Name}"
            return outputFile
        }

type CsvPackage(options: CsvPackage.Options) =
    // inherit Navika.Extension<CsvPackage.Options>(options)
    class end