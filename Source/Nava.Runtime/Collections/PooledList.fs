namespace Nava.Collections

open System
open System.Collections

open CommunityToolkit.HighPerformance.Buffers

/// <summary>
/// List implementation using <see cref="ArrayPool{T}"/> for storing elements. This implementation is based on <see cref="List{T}"/> and <see href="https://github.com/jtmueller/Collections.Pooled"/>.
/// </summary>
/// <typeparam name="T">Type of element.</typeparam>
type 'T PooledList =
    val mutable buffer :'T MemoryOwner