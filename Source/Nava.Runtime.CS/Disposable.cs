namespace Nava.Runtime;
/// <summary> Abstract base class for disposable types. </summary>
public abstract class Disposable : IDisposable {
	/// <summary> Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. </summary>
	/// <param name="disposing"><c>true</c> if called from <see cref="Disposable.Dispose()"/>.</param>
	protected abstract void Dispose(bool disposing);

	/// <summary> Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. </summary>
	public void Dispose() {
		GC.SuppressFinalize(this);
		Dispose(true);
	}
	/// <summary> Finalizes an instance of the <see cref="Disposable"/> class. </summary>
	~Disposable() => Dispose(false);
}
