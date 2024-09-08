namespace Nava.Runtime

open System

[<AbstractClass>]
type DisposableFS =
    new() = { }

    /// <summary> Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. </summary>
    /// <param name="disposing"><c>true</c> if called from <see cref="M:Nava.Runtime.Disposable.Dispose" />.</param>
    abstract member Dispose: disposing: bool -> unit

    /// <summary> Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. </summary>
    member this.Dispose() =
        GC.SuppressFinalize this
        this.Dispose true

    override this.Finalize() = this.Dispose false

    interface IDisposable with
        member this.Dispose() = this.Dispose()