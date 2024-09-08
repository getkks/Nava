using Microsoft.FSharp.Core;

namespace Nava.Runtime;
/// <summary> Helper methods for working with pointers. </summary>
public static unsafe class PointerHelpers {

	/// <summary> Add the specified <paramref name="offset"/> to the pointer. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the element. </param>
	/// <param name="offset"> Offset to add. </param>
	/// <returns> The resulting pointer after addition. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T* Add<T>(T* ptr, int offset) where T : unmanaged => ptr + offset;

	/// <summary> Add the specified <paramref name="offset"/> to the pointer. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the element. </param>
	/// <param name="offset"> Offset to add. </param>
	/// <returns> The resulting pointer after addition. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T* Add<T>(T* ptr, uint offset) where T : unmanaged => ptr + offset;
	/// <summary> Allocates an aligned block of memory of the specified <paramref name="count"/> and <paramref name="aligment"/>. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="count"> Number of elements to allocate. </param>
	/// <param name="aligment"> Aligment of the memory to allocate as number of elements. </param>
	/// <returns> Pointer to the allocated aligned block of memory. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T* AlignedAlloc<T>(int count, int aligment) where T : unmanaged => (T*)NativeMemory.AlignedAlloc((nuint)(count * sizeof(T)), (nuint)(aligment * sizeof(T)));

	/// <summary> Allocates an aligned block of memory of the specified <paramref name="count"/> and ailgned by sizeof(<typeparamref name="T"/>). </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="count"> Number of elements to allocate. </param>
	/// <returns> Pointer to the allocated aligned block of memory. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T* AlignedAlloc<T>(int count) where T : unmanaged => AlignedAlloc<T>(count, 1);

	/// <summary> Allocates an aligned block of memory of the specified <paramref name="count"/> and <paramref name="aligment"/>. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="count"> Number of elements to allocate. </param>
	/// <param name="aligment"> Aligment of the memory to allocate as number of elements. </param>
	/// <param name="value"> Value to fill the memory with. </param>
	/// <returns> Pointer to the allocated aligned block of memory. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T* AlignedAllocFill<T>(int count, int aligment, T value) where T : unmanaged {
		var ptr = (T*)NativeMemory.AlignedAlloc((nuint)(count * sizeof(T)), (nuint)(aligment * sizeof(T)));
		Fill(ptr, count, value);
		return ptr;
	}

	/// <summary> Allocates an aligned block of memory of the specified <paramref name="count"/> and ailgned by sizeof(<typeparamref name="T"/>). </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="count"> Number of elements to allocate. </param>
	/// <param name="value"> Value to fill the memory with. </param>
	/// <returns> Pointer to the allocated aligned block of memory. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T* AlignedAllocFill<T>(int count, T value) where T : unmanaged => AlignedAllocFill(count, 1, value);

	/// <summary> Checks if <paramref name="left"/> is equal to <paramref name="right"/>. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointers. </typeparam>
	/// <param name="left"> Pointer on the left side. </param> <param name="right"> Pointer on the right side. </param>
	/// <returns> <see langword="true"/> if <paramref name="left"/> is equal to <paramref name="right"/> else <see langword="false"/>. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Equal<T>(T* left, T* right) where T : unmanaged => left == right;

	/// <summary> Fills <paramref name="ptr"/> with <paramref name="value"/>. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the memory to fill. </param>
	/// <param name="count"> Number of elements to fill. </param>
	/// <param name="value"> Value to fill with. </param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Fill<T>(T* ptr, int count, T value) where T : unmanaged => MemoryHelpers.GetSpan(ptr, count).Fill(value);

	/// <summary> Fills <paramref name="ptr"/> with <paramref name="value"/>. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the memory to fill. </param>
	/// <param name="offset"> Offset of the memory to fill. </param>
	/// <param name="count"> Number of elements to fill. </param>
	/// <param name="value"> Value to fill with. </param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Fill<T>(T* ptr, int offset, int count, T value) where T : unmanaged => Fill(ptr + offset, count, value);

	/// <summary> Gets the element at the specified <paramref name="index"/>. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the element. </param>
	/// <param name="index"> Index of the element to get. </param>
	/// <returns> The element at the specified <paramref name="index"/>. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetItem<T>(T* ptr, int index) where T : unmanaged => ptr[index];

	/// <summary> Gets the element at the specified <paramref name="index"/>. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the element. </param>
	/// <param name="index"> Index of the element to get. </param>
	/// <returns> The element at the specified <paramref name="index"/>. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetItem<T>(T* ptr, uint index) where T : unmanaged => ptr[index];

	/// <summary> Gets the element at the specified pointer. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the element. </param>
	/// <returns> The element at the specified pointer. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T GetItem<T>(T* ptr) where T : unmanaged => *ptr;

	/// <summary> Get a managed reference from a pointer. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the element. </param>
	/// <returns> Managed reference to the element. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T GetReference<T>(T* ptr) where T : unmanaged => ref Unsafe.AsRef<T>(ptr);

	/// <summary> Get a managed reference from a pointer. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the element. </param>
	/// <param name="index"> Index of the element. </param>
	/// <returns> Managed reference to the element. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T GetReference<T>(T* ptr, int index) where T : unmanaged => ref GetReference(ptr + index);

	/// <summary> Get a managed reference from a pointer. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the element. </param>
	/// <param name="index"> Index of the element. </param>
	/// <returns> Managed reference to the element. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T GetReference<T>(T* ptr, uint index) where T : unmanaged => ref GetReference(ptr + index);

	/// <summary> Checks if <paramref name="left"/> is greater than <paramref name="right"/>. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointers. </typeparam>
	/// <param name="left"> Pointer on the left side. </param> <param name="right"> Pointer on the right side. </param>
	/// <returns> <see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/> else <see langword="false"/>. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool GreaterThan<T>(T* left, T* right) where T : unmanaged => left > right;

	/// <summary> Checks if <paramref name="left"/> is greater than or equal to <paramref name="right"/>. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointers. </typeparam>
	/// <param name="left"> Pointer on the left side. </param> <param name="right"> Pointer on the right side. </param>
	/// <returns> <see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/> else <see langword="false"/>. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool GreaterThanOrEqual<T>(T* left, T* right) where T : unmanaged => left >= right;
	/// <summary> Checks if <paramref name="left"/> is less than <paramref name="right"/>. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointers. </typeparam>
	/// <param name="left"> Pointer on the left side. </param> <param name="right"> Pointer on the right side. </param>
	/// <returns> <see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/> else <see langword="false"/>. </returns>

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool LessThan<T>(T* left, T* right) where T : unmanaged => left < right;

	/// <summary> Checks if <paramref name="left"/> is less than or equal to <paramref name="right"/>. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointers. </typeparam>
	/// <param name="left"> Pointer on the left side. </param> <param name="right"> Pointer on the right side. </param>
	/// <returns> <see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/> else <see langword="false"/>. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool LessThanEqual<T>(T* left, T* right) where T : unmanaged => left <= right;

	/// <summary> Checks if <paramref name="left"/> is not equal to <paramref name="right"/>. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointers. </typeparam>
	/// <param name="left"> Pointer on the left side. </param> <param name="right"> Pointer on the right side. </param>
	/// <returns> <see langword="true"/> if <paramref name="left"/> is not equal to <paramref name="right"/> else <see langword="false"/>. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool NotEqual<T>(T* left, T* right) where T : unmanaged => left == right;

	/// <summary> Sets the element at the specified <paramref name="index"/>. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the element. </param>
	/// <param name="index"> Index of the element to set. </param>
	/// <param name="value"> Value to set. </param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetItem<T>(T* ptr, int index, T value) where T : unmanaged => ptr[index] = value;

	/// <summary> Sets the element at the specified <paramref name="index"/>. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the element. </param>
	/// <param name="index"> Index of the element to set. </param>
	/// <param name="value"> Value to set. </param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetItem<T>(T* ptr, uint index, T value) where T : unmanaged => ptr[index] = value;

	/// <summary> Sets the element at the specified pointer. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the element. </param>
	/// <param name="value"> Value to set. </param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetItem<T>(T* ptr, T value) where T : unmanaged => *ptr = value;

	/// <summary> Subtract the specified <paramref name="offset"/> to the pointer. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the element. </param>
	/// <param name="offset"> Offset to add. </param>
	/// <returns> The resulting pointer after subtraction. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T* Subtract<T>(T* ptr, int offset) where T : unmanaged => ptr + offset;

	/// <summary> Subtract the specified <paramref name="offset"/> to the pointer. </summary>
	/// <typeparam name="T"> Type of the element pointed by the pointer. </typeparam>
	/// <param name="ptr"> Pointer to the element. </param>
	/// <param name="offset"> Offset to add. </param>
	/// <returns> The resulting pointer after subtraction. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T* Subtract<T>(T* ptr, uint offset) where T : unmanaged => ptr + offset;
}
/// <summary> Helpers for type. </summary>
public static class TypeHelpers {

	/// <summary> Creates an array of <typeparamref name="T"/>. </summary>
	/// <typeparam name="T"> Type of the array. </typeparam>
	/// <param name="length"> Length of the array. </param>
	/// <returns> The created array. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] CreateArray<T>(int length) => GC.AllocateArray<T>(length);

	/// <summary> Creates an uninitialized array of <typeparamref name="T"/>. </summary>
	/// <typeparam name="T"> Type of the array. </typeparam>
	/// <param name="length"> Length of the array. </param>
	/// <returns> The created array. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T[] CreateUninitializedArray<T>(int length) => GC.AllocateUninitializedArray<T>(length);

	/// <summary> Gets the type from assembly. </summary>
	/// <typeparam name="TypeFromAssembly"> Type to identify the assembly. </typeparam>
	/// <param name="typeName"> Name of the type. </param>
	/// <returns> The type from the assembly. </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static FSharpValueOption<Type> GetType<TypeFromAssembly>(string typeName) {
		var type = typeof(TypeFromAssembly).Assembly.GetType(typeName);
		return type is null ? FSharpValueOption<Type>.None : FSharpValueOption<Type>.NewValueSome(type);
	}
}