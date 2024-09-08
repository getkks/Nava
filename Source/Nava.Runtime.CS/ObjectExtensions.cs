namespace Nava.Runtime;
/// <summary>
/// Extensions for <see cref="object"/>
/// </summary>
public static class ObjectExtensions {
	/// <summary> Cast an object to <typeparamref name="T"/>. </summary>
	/// <typeparam name="T"> Type of the object. </typeparam>
	/// <param name="obj"> Object to cast. </param>
	/// <returns> Casted object. </returns>
	public static T As<T>(this object obj) where T : class => Unsafe.As<T>(obj);
	/// <summary> Cast a struct to <typeparamref name="T"/>. </summary>
	/// <typeparam name="T"> Type of the struct. </typeparam>
	/// <typeparam name="U"> Type of the casted struct. </typeparam>
	/// <param name="obj"> Object to cast. </param>
	/// <returns> Casted object. </returns>
	public static U As<T, U>(this T obj) where T : struct where U : struct => Unsafe.As<T, U>(ref obj);
	/// <summary> Check if <paramref name="obj"/> is <see langword="null"/> </summary>
	/// <typeparam name="T"> Type of <paramref name="obj"/>. </typeparam>
	/// <param name="obj"> Object to check. </param>
	/// <returns> <see langword="true"/> if <paramref name="obj"/> is <see langword="null"/> else <see langword="false"/>. </returns>
	public static bool IsNull<T>(this T obj) where T : class => obj is null;
	/// <summary> Check if <paramref name="obj"/> is <see langword="null"/> </summary>
	/// <typeparam name="T"> Type of <paramref name="obj"/>. </typeparam>
	/// <param name="obj"> Object to check. </param>
	/// <returns> <see langword="false"/> if <paramref name="obj"/> is <see langword="null"/> else <see langword="true"/>. </returns>
	public static bool IsNotNull<T>(this T obj) where T : class => obj is null;
	/// <summary> Check if <paramref name="obj"/> is of type <typeparamref name="T"/>. </summary>
	/// <typeparam name="T"> Type of <paramref name="obj"/>. </typeparam>
	/// <param name="obj"> Object to check. </param>
	/// <returns> <see langword="true"/> if <paramref name="obj"/> is of type <typeparamref name="T"/> else <see langword="false"/>. </returns>
	public static bool Is<T>(this object obj) => obj is T;
}