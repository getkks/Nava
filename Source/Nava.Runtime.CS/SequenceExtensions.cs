using Microsoft.FSharp.Core;
namespace Nava.Runtime;
/// <summary>
/// Extensions for <see cref="IEnumerable{T}"/>.
/// </summary>
public static class SequenceExtensions {
	/// <summary>
	/// Tries to find the first element in the sequence that satisfies the predicate.
	/// </summary>
	/// <typeparam name="T"> The type of the elements in the sequence. </typeparam>
	/// <param name="seq"> The sequence to search. </param>
	/// <param name="predicate"> The predicate to search for. </param>
	/// <returns> The first element in the sequence that satisfies the predicate as a <see cref="FSharpValueOption{T}"/> else <see cref="FSharpValueOption{T}.None"/>. </returns>
	public static FSharpValueOption<T> TryFind<T>(this IEnumerable<T> seq, Func<T, bool> predicate) {
		if(seq is not null) {
			using var enumerator = seq.GetEnumerator();
			while(enumerator.MoveNext()) {
				if(predicate(enumerator.Current)) {
					return FSharpValueOption<T>.Some(enumerator.Current);
				}
			}
		}
		return FSharpValueOption<T>.None;
	}
}
