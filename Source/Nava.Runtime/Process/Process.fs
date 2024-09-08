namespace Nava.Runtime.Process

open System.Diagnostics
open System.Runtime.CompilerServices

[<Extension>]
type ProcessExtentions =
    [<Extension>]
    static member GetWindowThreadProcessId hwnd =
        match Vanara.PInvoke.User32.GetWindowThreadProcessId hwnd with
        | _, processId when processId <> 0u -> processId |> int |> Process.GetProcessById |> ValueOption.ofObj
        | _ -> ValueNone

    /// <summary> Force close a process if it is still running after main window is closed. </summary>
    /// <param name="proc">The process to close.</param>
    [<Extension>]
    static member ForceClose(proc: Process) =
        match proc.CloseMainWindow() with
        | false -> proc.Kill()
        | _ -> ()

        proc.Close()

    /// <summary> Force close all processes with given name. </summary>
    /// <param name="processName">The name of the process.</param>
    [<Extension>]
    static member ForceClose processName =
        for proc in Process.GetProcessesByName processName do
            proc.ForceClose()