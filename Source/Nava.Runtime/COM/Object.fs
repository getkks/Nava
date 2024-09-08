namespace Nava.Runtime.COM

open System
open type Vanara.PInvoke.Ole32
open type Vanara.PInvoke.OleAut32

/// <summary>Provides access to COM objects.</summary>
module Object =

    /// <summary>Gets the currently active COM object with the specified ProgID.</summary>
    /// <param name="progID">The ProgID of the COM object to retrieve.</param>
    /// <returns>The currently active COM object with the specified ProgID, or null if no such object is found.</returns>
    let GetActiveObject<'T> progID =
        let mutable classId = Unchecked.defaultof<_>

        try
            CLSIDFromProgIDEx(progID, &classId) |> ignore
        with _ ->
            CLSIDFromProgID(progID, &classId) |> ignore

        let mutable obj = Unchecked.defaultof<_>

        try
            match GetActiveObject(&classId, IntPtr.Zero) with
            | _, null -> None |> Ok
            | _, obj -> obj :?> 'T |> Some |> Ok
        with exn ->
            Error exn

    /// <summary>Retrieves all running COM objects for the given type.</summary>
    /// <returns>A sequence of COM objects for the given type.</returns>
    let GetRunningObjects<'T>() =
        match GetRunningObjectTable 0u with
        | _, null -> "Unable to get Running Object Table." |> Error
        | _, runningObjectTable ->
            match runningObjectTable.EnumRunning() with
            | null -> "Running Object Table did not return IEnumMoniker." |> Error
            | monikerEnumerator ->
                monikerEnumerator.Reset()
                let result = GC.AllocateUninitializedArray(1, true) //Array.create 1 null

                seq {
                    while monikerEnumerator.Next(1, result, IntPtr.Zero) = 0 do
                        let object = runningObjectTable.GetObject result[0]

                        match object with
                        | :? 'T as object -> object
                        | _ -> ()
                }
                |> Ok