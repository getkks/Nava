open System
open System.IO
open Spectre.Console
open Nava.Office
open EasyBuild.FileSystemProvider

type Workspace = RelativeFileSystem<".">

AnsiConsole
    .Progress()
    // .HideCompleted(true)
    // .AutoClear(true)
    .Columns(TaskDescriptionColumn(), ProgressBarColumn(), PercentageColumn(), SpinnerColumn(), ElapsedTimeColumn())
    .Start(
        (fun ctx ->
            [|
                Workspace.``..``.``..``.Data.``Large File.xlsx``
                Workspace.``..``.``..``.Data.``Large File - Copy.xlsx``
                Workspace.``..``.``..``.Data.``Large File - Copy (2).xlsx``
                Workspace.``..``.``..``.Data.``Large File - Copy (3).xlsx``
                Workspace.``..``.``..``.Data.``Large File - Copy (4).xlsx``
                Workspace.``..``.``..``.Data.``Large File - Copy (5).xlsx``
                Workspace.``..``.``..``.Data.``Large File - Copy (6).xlsx``
                Workspace.``..``.``..``.Data.``Large File - Copy (7).xlsx``
                Workspace.``..``.``..``.Data.``Large File - Copy (8).xlsx``
                Workspace.``..``.``..``.Data.``Large File - Copy (10).xlsx``
                Workspace.``..``.``..``.Data.``Large File - Copy (11).xlsx``
                Workspace.``..``.``..``.Data.``Large File - Copy (12).xlsx``
                Workspace.``..``.``..``.Data.``Test.xlsx``
            |]
            |> Array.map(fun path ->
                async {
                    return
                        path
                        |> CsvPackage.createOptions
                        // |> CsvPackage.withProgressContext ctx
                        |> CsvPackage.convert
                })
            |> Async.Parallel
            |> Async.RunSynchronously)
    )
|> ignore