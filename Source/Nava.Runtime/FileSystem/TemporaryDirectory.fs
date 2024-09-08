namespace Nava.Runtime.FileSystem

open System
open System.IO

/// <summary>Represents temporary directory. The directory will be deleted on dispose if it exists.</summary>
type TemporaryDirectory() =
    let directory = Directory.CreateTempSubdirectory()

    let dispose(disposing) =
        if directory.Exists then
            directory.Delete true

    member _.FullName: string = directory.FullName

    /// <summary>Get the disposable directory.</summary>
    member _.Directory = directory

    member this.Dispose() =
        GC.SuppressFinalize this
        dispose true

    /// <inherit/>
    override _.Finalize() =
        try
            dispose false
        with _ ->
            ()

    interface IDisposable with
        /// <inherit/>
        member this.Dispose() = this.Dispose()