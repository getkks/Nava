namespace Nava.Office

open System
open System.Diagnostics
open System.IO
open System.Runtime.CompilerServices
open System.Text.Json.Serialization
open Microsoft.Office.Interop.Excel

open Nava.Runtime
open Nava.Runtime.Process

module Scheduler =
    type ScheduleMessage =
        | StartAfter of (unit -> unit) * int
        | Start of (unit -> unit)

    let mailbox =
        MailboxProcessor.Start(fun inbox ->
            async {
                while true do
                    match! inbox.Receive() with
                    | StartAfter(action, ticks) ->
                        do! Async.Sleep ticks
                        action()
                    | Start action -> action()
            })

    let schedule action = action |> Start |> mailbox.Post

    let scheduleAfter ticks action =
        StartAfter(action, ticks) |> mailbox.Post

[<DebuggerDisplay("{ToString(),nq}")>]
type ExcelFileType =
    | Csv
    | Xls
    | Xlsb
    | Xlsm
    | Xlsx
    | Xltm
    | Xltx

    member this.Extension =
        match this with
        | Csv -> ".csv"
        | Xls -> ".xls"
        | Xlsb -> ".xlsb"
        | Xlsm -> ".xlsm"
        | Xlsx -> ".xlsx"
        | Xltm -> ".xltm"
        | Xltx -> ".xltx"

    override this.ToString() =
        match this with
        | Csv -> "CSV (comma delimited)"
        | Xls -> "Excel 97-2003 Workbook (BIFF8)"
        | Xlsb -> "Excel Binary Workbook (BIFF12)"
        | Xlsm -> "Excel Macro-Enabled Workbook"
        | Xlsx -> "Excel 2007+ Workbook"
        | Xltm -> "Excel Macro-Enabled Template"
        | Xltx -> "Excel 2007+ Template"

    static member TryParse(extension: char ReadOnlySpan) =
        let span = extension.TrimStart '.'

        match "csvxlsxlsbxlsmxltmxltx".AsSpan().IndexOf(span), span.Length with
        | 0, 3 -> Some Csv
        | 3, 3 -> Some Xls
        | 3, 4 -> Some Xlsx
        | 6, 4 -> Some Xlsb
        | 10, 4 -> Some Xlsm
        | 14, 4 -> Some Xltm
        | 18, 4 -> Some Xltx
        | _ -> None

    static member TryParse(extension: string) =
        ExcelFileType.TryParse(extension.AsSpan())

    static member TryParse(extension: FileInfo) =
        ExcelFileType.TryParse(extension.Extension.AsSpan())

[<Extension>]
type FileInfoExtensions =
    [<Extension>]
    static member IsExcelFile(file: FileInfo) =
        file |> ExcelFileType.TryParse |> Option.isSome

[<JsonFSharpConverter(BaseUnionEncoding = JsonUnionEncoding.InternalTag)>]
type HandleCorrupted =
    | Extract
    | Repair

type OpenOptions = {
    AddToRecentFiles: bool
    Corrupted: HandleCorrupted voption
    Delimiter: string voption
    // [<Password>]
    Password: string voption
    ReadOnly: bool
    UpdateLinks: bool
    // [<Password>]
    WriteReservedPassword: string voption
}

module OpenOptions =
    let create() = {
        AddToRecentFiles = true
        Corrupted = ValueNone
        Delimiter = ValueNone
        Password = ValueNone
        ReadOnly = false
        UpdateLinks = true
        WriteReservedPassword = ValueNone
    }

    let withAddToRecentFiles (value: bool) (options: OpenOptions) = { options with AddToRecentFiles = value }
    let withCorruptedFile (value: HandleCorrupted) (options: OpenOptions) = { options with Corrupted = ValueSome value }
    let withReadOnly (value: bool) (options: OpenOptions) = { options with ReadOnly = value }
    let withDelimiter (value: string) (options: OpenOptions) = { options with Delimiter = value |> ValueSome }
    let withPassword (value: string) (options: OpenOptions) = { options with Password = ValueSome value }
    let withWriteReservedPassword (value: string) (options: OpenOptions) = { options with WriteReservedPassword = ValueSome value }
    let withUpdateLinks (value: bool) (options: OpenOptions) = { options with UpdateLinks = value }

/// <summary>Encapsulates an Excel application as a disposable object.Application and the workbooks are closed when the object is disposed or garbage collected.</summary>
type ExcelApplication(saveOnClose: bool) =
    inherit ApplicationClass()

    static member DefaultValue(value: 'T voption) =
        match value with
        | ValueNone -> Type.Missing
        | ValueSome v -> v :> _

    static member DefaultValue(condition, value: 'T voption) =
        match condition, value with
        | false, _
        | true, ValueNone -> Type.Missing
        | true, ValueSome v -> v :> _

    member this.Close() =
        let workbooks = this.Workbooks

        for workbook in workbooks do
            workbook.Close(SaveChanges = saveOnClose)

        workbooks.Close()
        this.Quit()

        Scheduler.schedule(fun () ->
            GC.WaitForPendingFinalizers()
            GC.Collect()
            let windowHandle = this.Hwnd |> Vanara.PInvoke.HWND

            windowHandle.GetWindowThreadProcessId()
            |> ValueOption.iter(fun proc ->
                if not proc.HasExited then
                    proc.Kill()))

    member this.Open(inputFile, options) =
        if String.IsNullOrWhiteSpace inputFile then
            ArgumentNullException(nameof inputFile) :> Exception |> Error
        else
            let file = inputFile |> FileInfo
            this.Open(file, options)

    member this.Open(inputFile: FileInfo, options) =
        let noDelimiter = options.Delimiter.IsNone

        try
            this.Workbooks.Open(
                inputFile.FullName,
                AddToMru = options.AddToRecentFiles,
                CorruptLoad =
                    (match options.Corrupted with
                     | ValueSome Extract -> XlCorruptLoad.xlExtractData |> box
                     | ValueSome Repair -> XlCorruptLoad.xlRepairFile |> box
                     | ValueNone -> Type.Missing),
                Delimiter = ExcelApplication.DefaultValue options.Delimiter,
                Format = (if noDelimiter then Type.Missing else 6),
                IgnoreReadOnlyRecommended = false,
                Password = ExcelApplication.DefaultValue options.Password,
                ReadOnly = options.ReadOnly,
                UpdateLinks = (if options.UpdateLinks then 3 else 0),
                WriteResPassword = ExcelApplication.DefaultValue options.WriteReservedPassword
            )
            |> Ok
        with exn ->
            Error exn

    member this.Dispose disposing =
        if disposing then
            GC.SuppressFinalize this

        this.Close()

    member this.Dispose() = this.Dispose true
    /// <inherit/>
    override this.Finalize() = this.Dispose false

    interface IDisposable with
        /// <inherit/>
        member this.Dispose() = this.Dispose()

module ExcelApplication =
    let getExcel() =
        COM.Object.GetActiveObject<Application> "Excel.Application"

    let close(excel: ExcelApplication) = excel.Close()
    let openWorkbook (inputFile: FileInfo) options (application: ExcelApplication) = application.Open(inputFile, options)

    let getRunningWorkbooks() =
        COM.Object.GetRunningObjects<Workbook>()

    let forceCloseAll() = "EXCEL".ForceClose()