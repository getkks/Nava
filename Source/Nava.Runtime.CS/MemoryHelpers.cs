using System.Buffers;
using System.Numerics;
namespace Nava.Runtime;
/// <summary>
/// Helper methods for working with memory types like <see cref="Array"/>, <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/>.
/// </summary>
public static class MemoryHelpers {
	/// <summary>
	/// Casts a value of type <typeparamref name="T"/> to <typeparamref name="U"/>.
	/// </summary>
	/// <typeparam name="T"> Type of the value. </typeparam>
	/// <typeparam name="U"> Type of the casted value. </typeparam>
	/// <param name="value"> Value to cast. </param>
	/// <returns> Casted value. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static U? Cast<T, U>(this T? value) where T : class where U : class => Unsafe.As<U>(value);
	/// <summary> Get the first element of the <see cref="Array"/>. </summary>
	/// <typeparam name="T"> Type of the <see cref="Array"/>. </typeparam>
	/// <param name="arr"> <see cref="Array"/> containing the elements. </param>
	/// <returns> First element. </returns>
	/// <remarks>
	/// <list type="bullet">
	/// <item><description> No checks are done to validate <see cref="Array"/>. </description></item>
	/// <item><description> The <see cref="Array"/> must not be empty. </description></item>
	/// </list>
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetElement<T>(this T[] arr) => GetReference(arr);
	/// <summary> Get the first element of the <see ref="Span{T}"/>. </summary>
	/// <typeparam name="T"> Type of the <see ref="Span{T}"/>. </typeparam>
	/// <param name="span"> <see ref="Span{T}"/> containing the elements. </param>
	/// <returns> First element. </returns>
	/// <remarks>
	/// <list type="bullet">
	/// <item><description> No checks are done to validate <see ref="Span{T}"/>. </description></item>
	/// <item><description> The <see ref="Span{T}"/> must not be empty. </description></item>
	/// </list>
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetElement<T>(this Span<T> span) => span.GetReference();
	/// <summary> Get the first element of the <see cref="ReadOnlySpan{T}"/>. </summary>
	/// <typeparam name="T"> Type of the <see cref="ReadOnlySpan{T}"/>. </typeparam>
	/// <param name="span"> <see cref="ReadOnlySpan{T}"/> containing the elements. </param>
	/// <returns> First element. </returns>
	/// <remarks>
	/// <list type="bullet">
	/// <item><description> No checks are done to validate <see cref="ReadOnlySpan{T}"/>. </description></item>
	/// <item><description> The <see cref="ReadOnlySpan{T}"/> must not be empty. </description></item>
	/// </list>
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetElement<T>(this ReadOnlySpan<T> span) => span.GetReference();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetElement<T>(this T[] arr, int index) => arr.GetReference(index);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetElement<T>(this T[] arr, uint index) => arr.GetReference(index);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetElement<T>(this Span<T> span, int index) => span.GetReference(index);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetElement<T>(this ReadOnlySpan<T> span, int index) => Unsafe.Add(ref span.GetReference(), index);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe T* GetPointer<T>(ref T value) where T : unmanaged => (T*)Unsafe.AsPointer(ref value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe T* GetPointer<T>(this T[] array) where T : unmanaged => GetPointer(ref GetReference(array));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe T* GetPointer<T>(this Span<T> span) where T : unmanaged => GetPointer(ref GetReference(span));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe T* GetPointer<T>(this ReadOnlySpan<T> span) where T : unmanaged => GetPointer(ref GetReference(span));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<T> GetReadOnlySpan<T>(this T[] arr, int index, int count) => GetReadOnlySpan(ref arr.GetReference(), index, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<T> GetReadOnlySpan<T>(this T[] arr, int count) => arr.GetReadOnlySpan(0, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<T> GetReadOnlySpan<T>(this T[] arr) => GetReadOnlySpan(arr, 0, arr.Length);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<T> GetReadOnlySpan<T>(this Span<T> span, int index, int count) => GetReadOnlySpan(ref span.GetReference(), index, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<T> GetReadOnlySpan<T>(this Span<T> span, int count) => span.GetReadOnlySpan(0, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<T> GetReadOnlySpan<T>(this Span<T> span) => span.GetReadOnlySpan(0, span.Length);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<T> GetReadOnlySpan<T>(this ReadOnlySpan<T> span, int index, int count) => GetReadOnlySpan(ref span.GetReference(), index, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<T> GetReadOnlySpan<T>(this ReadOnlySpan<T> span, int count) => span.GetReadOnlySpan(0, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ReadOnlySpan<T> GetReadOnlySpan<T>(void* span, int count) => new(span, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<T> GetReadOnlySpan<T>(ref T reference, int count) => GetReadOnlySpan(ref reference, 0, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<T> GetReadOnlySpan<T>(ref T reference, int index, int count) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref reference, index), count);

	public static ReadOnlySpan<TTo> GetReadOnlySpanAs<TTo>(string str) where TTo : struct => MemoryMarshal.Cast<char, TTo>(str.AsSpan());
	/// <summary>
	/// Get a reference to the first element of the <see cref="Array"/>.
	/// </summary>
	/// <typeparam name="T"> Type of the <see cref="Array"/> elements. </typeparam>
	/// <param name="arr"> <see cref="Array"/> containing the elements. </param>
	/// <returns> Reference to the first element. </returns>
	/// <remarks>
	/// <list type="bullet">
	/// <item><description> No checks are done to validate <see cref="Array"/>. </description></item>
	/// <item><description> The <see cref="Array"/> must not be empty. </description></item>
	/// </list>
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T GetReference<T>(this T[] arr) => ref MemoryMarshal.GetArrayDataReference(arr);
	/// <summary>
	/// Get a reference to the first element of the <see ref="Span{T}"/>.
	/// </summary>
	/// <typeparam name="T"> Type of the <see ref="Span{T}"/> elements. </typeparam>
	/// <param name="span"> <see ref="Span{T}"/> containing the elements. </param>
	/// <returns> Reference to the first element. </returns>
	/// <remarks>
	/// <list type="bullet">
	/// <item><description> No checks are done to validate <see ref="Span{T}"/>. </description></item>
	/// <item><description> The <see ref="Span{T}"/> must not be empty. </description></item>
	/// </list>
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T GetReference<T>(this Span<T> span) => ref MemoryMarshal.GetReference(span);
	/// <summary>
	/// Get a reference to the first element of the <see cref="ReadOnlySpan{T}"/>.
	/// </summary>
	/// <typeparam name="T"> Type of the <see cref="ReadOnlySpan{T}"/> elements. </typeparam>
	/// <param name="span"> <see cref="ReadOnlySpan{T}"/> containing the elements. </param>
	/// <returns> Reference to the first element. </returns>
	/// <remarks>
	/// <list type="bullet">
	/// <item><description> No checks are done to validate <see cref="ReadOnlySpan{T}"/>. </description></item>
	/// <item><description> The <see cref="ReadOnlySpan{T}"/> must not be empty. </description></item>
	/// </list>
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T GetReference<T>(this ReadOnlySpan<T> span) => ref MemoryMarshal.GetReference(span);
	/// <summary>
	/// Get a reference to the element at the specified index of the <see cref="Array"/>.
	/// </summary>
	/// <typeparam name="T"> Type of the <see cref="Array"/> elements. </typeparam>
	/// <param name="arr"> <see cref="Array"/> containing the elements. </param>
	/// <param name="index"> Index of the element. </param>
	/// <returns> Reference to the element. </returns>
	/// <remarks>
	/// <list type="bullet">
	/// <item><description> The index must be within the bounds of the <see cref="Array"/>. </description></item>
	/// <item><description> The <see cref="Array"/> must not be empty. </description></item>
	/// <item><description> No checks are done to validate <see cref="Array"/>. </description></item>
	/// </list>
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T GetReference<T>(this T[] arr, int index) => ref Unsafe.Add(ref arr.GetReference(), index);
	/// <summary>
	/// Get a reference to the element at the specified index of the <see cref="Array"/>.
	/// </summary>
	/// <typeparam name="T"> Type of the <see cref="Array"/> elements. </typeparam>
	/// <param name="arr"> <see cref="Array"/> containing the elements. </param>
	/// <param name="index"> Index of the element. </param>
	/// <returns> Reference to the element. </returns>
	/// <remarks>
	/// <list type="bullet">
	/// <item><description> The index must be within the bounds of the <see cref="Array"/>. </description></item>
	/// <item><description> The <see cref="Array"/> must not be empty. </description></item>
	/// <item><description> No checks are done to validate <see cref="Array"/>. </description></item>
	/// </list>
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T GetReference<T>(this T[] arr, uint index) => ref GetReference(arr, (int)index);
	/// <summary>
	/// Get a reference to the element at the specified index of the <see ref="Span{T}"/>.
	/// </summary>
	/// <typeparam name="T"> Type of the <see ref="Span{T}"/> elements. </typeparam>
	/// <param name="span"> <see ref="Span{T}"/> containing the elements. </param>
	/// <param name="index"> Index of the element. </param>
	/// <returns> Reference to the element. </returns>
	/// <remarks>
	/// <list type="bullet">
	/// <item><description> The index must be within the bounds of the <see ref="Span{T}"/>. </description></item>
	/// <item><description> The <see ref="Span{T}"/> must not be empty. </description></item>
	/// <item><description> No checks are done to validate <see ref="Span{T}"/>. </description></item>
	/// </list>
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T GetReference<T>(this Span<T> span, int index) => ref Unsafe.Add(ref span.GetReference(), index);
	/// <summary>
	/// Get a reference to the element at the specified index of the <see cref="ReadOnlySpan{T}"/>
	/// </summary>
	/// <typeparam name="T"> Type of the <see cref="ReadOnlySpan{T}"/> elements. </typeparam>
	/// <param name="span"> <see cref="ReadOnlySpan{T}"/> containing the elements. </param>
	/// <param name="index"> Index of the element. </param>
	/// <returns> Reference to the element. </returns>
	/// <remarks>
	/// <list type="bullet">
	/// <item><description> The index must be within the bounds of the <see cref="ReadOnlySpan{T}"/>. </description></item>
	/// <item><description> The <see cref="ReadOnlySpan{T}"/> must not be empty. </description></item>
	/// <item><description> No checks are done to validate <see cref="ReadOnlySpan{T}"/>. </description></item>
	/// </list>
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T GetReference<T>(this ReadOnlySpan<T> span, int index) => ref Unsafe.Add(ref span.GetReference(), index);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> GetSpan<T>(this T[] arr, int index, int count) => GetSpan(ref arr.GetReference(index), count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> GetSpan<T>(this T[] arr, int count) => arr.GetSpan(0, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> GetSpan<T>(this T[] arr) => arr.GetSpan(0, arr.Length);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> GetSpan<T>(this Span<T> span, int index, int count) => GetSpan(ref span.GetReference(index), count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> GetSpan<T>(this Span<T> span, int count) => span.GetSpan(0, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> GetSpan<T>(this ReadOnlySpan<T> span, int index, int count) => GetSpan(ref span.GetReference(index), count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> GetSpan<T>(this ReadOnlySpan<T> span, int count) => span.GetSpan(0, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> GetSpan<T>(this ReadOnlySpan<T> span) => span.GetSpan(0, span.Length);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static Span<T> GetSpan<T>(void* ptr, int count) => new(ptr, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static Span<T> GetSpan<T>(T* ptr, int count) where T : unmanaged => new(ptr, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static Span<T> GetSpan<T>(IntPtr ptr, int count) where T : unmanaged => new((void*)ptr, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> GetSpan<T>(ref T reference, int count) => GetSpan(ref reference, 0, count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> GetSpan<T>(ref T reference, int index, int count) => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref reference, index), count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexToSize(int index) => 16 << index;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Return<T>(this T[] arr) => arr.Return(RuntimeHelpers.IsReferenceOrContainsReferences<T>());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Return<T>(this T[] arr, bool clear) => ArrayPool<T>.Shared.Return(arr, clear);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int SizeToIndex(int size) => size >= 0 ? BitOperations.Log2((uint)(size - 1) | 0xFu) - 3 : 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ReadOnlySpan<byte> ValueAsByteReadOnlySpan<T>(ref T value) => GetReadOnlySpan(ref Unsafe.As<T, byte>(ref value), sizeof(T));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static Span<byte> ValueAsByteSpan<T>(ref T value) => GetSpan(ref Unsafe.As<T, byte>(ref value), sizeof(T));

	/// <summary>Create a <see cref="T:System.ReadOnlySpan`1" /> from single value reference.</summary>
	/// <param name="value">Reference to value from which <see cref="T:System.ReadOnlySpan`1" /> is created.</param>
	/// <typeparam name="TFrom">Type of the value.</typeparam>
	/// <typeparam name="TTo">Type of <see cref="T:System.ReadOnlySpan`1" /> created.</typeparam>
	/// <returns><see cref="T:System.ReadOnlySpan`1" /> refering to <paramref name="value" />.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<TTo> ValueAsReadOnlySpan<TFrom, TTo>(ref TFrom value) where TFrom : struct where TTo : struct => MemoryMarshal.Cast<TFrom, TTo>(GetReadOnlySpan(ref value, 1));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<TTo> ValueAsSpan<TFrom, TTo>(ref TFrom value) where TFrom : struct where TTo : struct => MemoryMarshal.Cast<TFrom, TTo>(GetSpan(ref value, 1));
}
