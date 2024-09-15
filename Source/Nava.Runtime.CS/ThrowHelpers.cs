using System.Diagnostics;

namespace Nava.Runtime;

//
// The convention for this enum is using the argument name as the enum name
//
internal enum ExceptionArgument {
	action,
	array,
	arrayIndex,
	callBack,
	capacity,
	ch,
	collection,
	comparable,
	comparer,
	comparison,
	comparisonType,
	converter,
	count,
	creationOptions,
	culture,
	delay,
	destinationArray,
	destinationIndex,
	dictionary,
	elementType,
	endIndex,
	enumerable,
	exception,
	exceptions,
	format,
	function,
	index,
	indices,
	info,
	input,
	item,
	key,
	keys,
	len,
	length,
	lengths,
	list,
	lowerBounds,
	manager,
	match,
	millisecondsDelay,
	millisecondsTimeout,
	newSize,
	obj,
	other,
	s,
	source,
	sourceArray,
	sourceBytesToCopy,
	sourceIndex,
	start,
	startIndex,
	state,
	task,
	text,
	timeout,
	type,
	value,
	values,
}

//
// The convention for this enum is using the resource name as the enum name
//
internal enum ExceptionResource {
	Arg_ArrayPlusOffTooSmall,
	Arg_LowerBoundsMustMatch,
	Arg_MustBeType,
	Arg_NonZeroLowerBound,
	Arg_RankMultiDimNotSupported,
	Arg_TypeNotSupported,
	Argument_AddingDuplicate,
	Argument_InvalidArgumentForComparison,
	Argument_InvalidOffLen,
	ArgumentException_OtherNotArrayOfCorrectLength,
	ArgumentOutOfRange_BiggerThanCollection,
	ArgumentOutOfRange_Count,
	ArgumentOutOfRange_EndIndexStartIndex,
	ArgumentOutOfRange_HugeArrayNotSupported,
	ArgumentOutOfRange_Index,
	ArgumentOutOfRange_ListInsert,
	ArgumentOutOfRange_NeedNonNegNum,
	ArgumentOutOfRange_SmallCapacity,
	ConcurrentCollection_SyncRoot_NotSupported,
	InvalidOperation_HSCapacityOverflow,
	InvalidOperation_IComparerFailed,
	InvalidOperation_NullArray,
	NotSupported_FixedSizeCollection,
	NotSupported_KeyCollectionSet,
	NotSupported_ReadOnlyCollection,
	NotSupported_StringComparison,
	NotSupported_ValueCollectionSet,
	Rank_MultiDimNotSupported,
	Serialization_MissingKeys,
	Serialization_NullKey,
}
internal static class ThrowHelpers {

	// Allow nulls for reference types and Nullable<U>, but not for value types.
	// Aggressively inline so the jit evaluates the if in place and either drops the call altogether
	// Or just leaves null test and call to the Non-returning ThrowHelper.ThrowArgumentNullException
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void IfNullAndNullsAreIllegalThenThrow<T>(object value, ExceptionArgument argName) {
		// Note that default(T) is not equal to null for value types except when T is Nullable<U>.
		if(!(default(T) == null) && value == null)
			ThrowHelpers.ThrowArgumentNullException(argName);
	}

	internal static void ThrowAddingDuplicateWithKeyArgumentException<T>(T key) => throw GetAddingDuplicateWithKeyArgumentException(key!);

	internal static void ThrowAggregateException(List<Exception> exceptions) => throw new AggregateException(exceptions);

	internal static void ThrowArgumentException(ExceptionResource resource) => throw GetArgumentException(resource);

	internal static void ThrowArgumentException(ExceptionResource resource, ExceptionArgument argument) => throw GetArgumentException(resource, argument);

	internal static void ThrowArgumentException_Argument_InvalidArrayType() => throw new ArgumentException("Invalid array type.");

	internal static void ThrowArgumentException_DestinationTooShort() => throw new ArgumentException("Destination too short.");

	internal static void ThrowArgumentException_OverlapAlignmentMismatch() => throw new ArgumentException("Overlap alignment mismatch.");

	internal static void ThrowArgumentNullException(ExceptionArgument argument) => throw GetArgumentNullException(argument);

	internal static void ThrowArgumentNullException(ExceptionResource resource) => throw new ArgumentNullException(GetResourceString(resource));

	internal static void ThrowArgumentNullException(ExceptionArgument argument, ExceptionResource resource) => throw new ArgumentNullException(GetArgumentName(argument), GetResourceString(resource));

	internal static void ThrowArgumentOutOfRangeException() => throw new ArgumentOutOfRangeException();

	internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument) => throw new ArgumentOutOfRangeException(GetArgumentName(argument));

	internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, ExceptionResource resource) => throw GetArgumentOutOfRangeException(argument, resource);

	internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, int paramNumber, ExceptionResource resource) => throw GetArgumentOutOfRangeException(argument, paramNumber, resource);

	internal static void ThrowArgumentOutOfRange_IndexException() => throw GetArgumentOutOfRangeException(ExceptionArgument.index,
												ExceptionResource.ArgumentOutOfRange_Index);
	internal static void ThrowArrayTypeMismatchException() => throw new ArrayTypeMismatchException();

	internal static void ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count() => throw GetArgumentOutOfRangeException(ExceptionArgument.count,
												ExceptionResource.ArgumentOutOfRange_Count);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void ThrowForUnsupportedVectorBaseType<T>() where T : struct {
		if(typeof(T) != typeof(byte) && typeof(T) != typeof(sbyte) &&
			typeof(T) != typeof(short) && typeof(T) != typeof(ushort) &&
			typeof(T) != typeof(int) && typeof(T) != typeof(uint) &&
			typeof(T) != typeof(long) && typeof(T) != typeof(ulong) &&
			typeof(T) != typeof(float) && typeof(T) != typeof(double)) {
			ThrowNotSupportedException(ExceptionResource.Arg_TypeNotSupported);
		}
	}

	internal static void ThrowFormatException_BadFormatSpecifier() => throw new FormatException("Bad format specifier.");

	internal static void ThrowIndexArgumentOutOfRange_NeedNonNegNumException() => throw GetArgumentOutOfRangeException(ExceptionArgument.index,
												ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

	internal static void ThrowIndexOutOfRangeException() => throw new IndexOutOfRangeException();

	internal static void ThrowInvalidOperationException(ExceptionResource resource) => throw GetInvalidOperationException(resource);

	internal static void ThrowInvalidOperationException(ExceptionResource resource, Exception e) => throw new InvalidOperationException(GetResourceString(resource), e);

	internal static void ThrowInvalidOperationException_ConcurrentOperationsNotSupported() => throw new InvalidOperationException("Concurrent operations are not supported.");

	internal static void ThrowInvalidOperationException_EnumCurrent(int index) => throw GetInvalidOperationException_EnumCurrent(index);

	internal static void ThrowInvalidOperationException_HandleIsNotInitialized() => throw new InvalidOperationException("Handle is not initialized.");

	internal static void ThrowInvalidOperationException_InvalidOperation_EnumEnded() => throw new InvalidOperationException("Enumeration has ended.");

	internal static void ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion() => throw new InvalidOperationException("Collection was modified during enumeration.");

	internal static void ThrowInvalidOperationException_InvalidOperation_EnumNotStarted() => throw new InvalidOperationException("Enumeration has not started.");

	internal static void ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen() => throw new InvalidOperationException("Invalid enumerator state: enumeration cannot proceed.");

	internal static void ThrowInvalidOperationException_InvalidOperation_NoValue() => throw new InvalidOperationException("No value provided.");

	internal static void ThrowKeyNotFoundException<T>(T key) => throw GetKeyNotFoundException(key!);

	internal static void ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum() => throw GetArgumentOutOfRangeException(ExceptionArgument.length,
												ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

	internal static void ThrowNotSupportedException(ExceptionResource resource) => throw new NotSupportedException(GetResourceString(resource));

	internal static void ThrowNotSupportedException() => throw new NotSupportedException();

	internal static void ThrowObjectDisposedException(string objectName, ExceptionResource resource) => throw new ObjectDisposedException(objectName, GetResourceString(resource));

	internal static void ThrowObjectDisposedException(ExceptionResource resource) => throw new ObjectDisposedException(null, GetResourceString(resource));

	internal static void ThrowOutOfMemoryException() => throw new OutOfMemoryException();

	internal static void ThrowRankException(ExceptionResource resource) => throw new RankException(GetResourceString(resource));

	internal static void ThrowSecurityException(ExceptionResource resource) => throw new System.Security.SecurityException(GetResourceString(resource));

	internal static void ThrowSerializationException(ExceptionResource resource) => throw new System.Runtime.Serialization.SerializationException(GetResourceString(resource));

	internal static void ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index() => throw GetArgumentOutOfRangeException(ExceptionArgument.startIndex,
												ExceptionResource.ArgumentOutOfRange_Index);

	internal static void ThrowUnauthorizedAccessException(ExceptionResource resource) => throw new UnauthorizedAccessException(GetResourceString(resource));

	internal static void ThrowValueArgumentOutOfRange_NeedNonNegNumException() => throw GetArgumentOutOfRangeException(ExceptionArgument.value,
												ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);

	internal static void ThrowWrongKeyTypeArgumentException<T>(T key, Type targetType) => throw GetWrongKeyTypeArgumentException(key!, targetType);

	internal static void ThrowWrongValueTypeArgumentException<T>(T value, Type targetType) => throw GetWrongValueTypeArgumentException(value!, targetType);

	private static ArgumentException GetAddingDuplicateWithKeyArgumentException(object key) => new($"Error adding duplicate with key: {key}.");

	private static ArgumentException GetArgumentException(ExceptionResource resource) => new(GetResourceString(resource));

	private static ArgumentException GetArgumentException(ExceptionResource resource, ExceptionArgument argument) => new(GetResourceString(resource), GetArgumentName(argument));

	private static string GetArgumentName(ExceptionArgument argument) {
		return argument switch {
			ExceptionArgument.obj => "obj",
			ExceptionArgument.dictionary => "dictionary",
			ExceptionArgument.array => "array",
			ExceptionArgument.info => "info",
			ExceptionArgument.key => "key",
			ExceptionArgument.text => "text",
			ExceptionArgument.values => "values",
			ExceptionArgument.value => "value",
			ExceptionArgument.startIndex => "startIndex",
			ExceptionArgument.task => "task",
			ExceptionArgument.ch => "ch",
			ExceptionArgument.s => "s",
			ExceptionArgument.input => "input",
			ExceptionArgument.list => "list",
			ExceptionArgument.index => "index",
			ExceptionArgument.capacity => "capacity",
			ExceptionArgument.collection => "collection",
			ExceptionArgument.item => "item",
			ExceptionArgument.converter => "converter",
			ExceptionArgument.match => "match",
			ExceptionArgument.count => "count",
			ExceptionArgument.action => "action",
			ExceptionArgument.comparison => "comparison",
			ExceptionArgument.exceptions => "exceptions",
			ExceptionArgument.exception => "exception",
			ExceptionArgument.enumerable => "enumerable",
			ExceptionArgument.start => "start",
			ExceptionArgument.format => "format",
			ExceptionArgument.culture => "culture",
			ExceptionArgument.comparer => "comparer",
			ExceptionArgument.comparable => "comparable",
			ExceptionArgument.source => "source",
			ExceptionArgument.state => "state",
			ExceptionArgument.length => "length",
			ExceptionArgument.comparisonType => "comparisonType",
			ExceptionArgument.manager => "manager",
			ExceptionArgument.sourceBytesToCopy => "sourceBytesToCopy",
			ExceptionArgument.callBack => "callBack",
			ExceptionArgument.creationOptions => "creationOptions",
			ExceptionArgument.function => "function",
			ExceptionArgument.delay => "delay",
			ExceptionArgument.millisecondsDelay => "millisecondsDelay",
			ExceptionArgument.millisecondsTimeout => "millisecondsTimeout",
			ExceptionArgument.timeout => "timeout",
			ExceptionArgument.type => "type",
			ExceptionArgument.sourceIndex => "sourceIndex",
			ExceptionArgument.sourceArray => "sourceArray",
			ExceptionArgument.destinationIndex => "destinationIndex",
			ExceptionArgument.destinationArray => "destinationArray",
			ExceptionArgument.other => "other",
			ExceptionArgument.newSize => "newSize",
			ExceptionArgument.lowerBounds => "lowerBounds",
			ExceptionArgument.lengths => "lengths",
			ExceptionArgument.len => "len",
			ExceptionArgument.keys => "keys",
			ExceptionArgument.indices => "indices",
			ExceptionArgument.endIndex => "endIndex",
			ExceptionArgument.elementType => "elementType",
			ExceptionArgument.arrayIndex => "arrayIndex",
			_ => argument.ToString(),
		};
	}

	private static ArgumentNullException GetArgumentNullException(ExceptionArgument argument) => new(GetArgumentName(argument));

	private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(ExceptionArgument argument, ExceptionResource resource) => new(GetArgumentName(argument), GetResourceString(resource));

	private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(ExceptionArgument argument, int paramNumber, ExceptionResource resource) => new(GetArgumentName(argument) + "[" + paramNumber.ToString() + "]", GetResourceString(resource));

	private static InvalidOperationException GetInvalidOperationException(ExceptionResource resource) => new(GetResourceString(resource));

	private static InvalidOperationException GetInvalidOperationException_EnumCurrent(int index) => new(index < 0 ? "Enumeration has not started" : "Enumeration has ended");

	private static KeyNotFoundException GetKeyNotFoundException(object key) => new($"Key not found: {key}");

	private static string GetResourceString(ExceptionResource resource) {
		return resource switch {
			ExceptionResource.ArgumentOutOfRange_Index => "Argument 'index' was out of the range of valid values.",
			ExceptionResource.ArgumentOutOfRange_Count => "Argument 'count' was out of the range of valid values.",
			ExceptionResource.Arg_ArrayPlusOffTooSmall => "Array plus offset too small.",
			ExceptionResource.NotSupported_ReadOnlyCollection => "This operation is not supported on a read-only collection.",
			ExceptionResource.Arg_RankMultiDimNotSupported => "Multi-dimensional arrays are not supported.",
			ExceptionResource.Arg_NonZeroLowerBound => "Arrays with a non-zero lower bound are not supported.",
			ExceptionResource.ArgumentOutOfRange_ListInsert => "Insertion index was out of the range of valid values.",
			ExceptionResource.ArgumentOutOfRange_NeedNonNegNum => "The number must be non-negative.",
			ExceptionResource.ArgumentOutOfRange_SmallCapacity => "The capacity cannot be set below the current Count.",
			ExceptionResource.Argument_InvalidOffLen => "Invalid offset length.",
			ExceptionResource.ArgumentOutOfRange_BiggerThanCollection => "The given value was larger than the size of the collection.",
			ExceptionResource.Serialization_MissingKeys => "Serialization error: missing keys.",
			ExceptionResource.Serialization_NullKey => "Serialization error: null key.",
			ExceptionResource.NotSupported_KeyCollectionSet => "The KeyCollection does not support modification.",
			ExceptionResource.NotSupported_ValueCollectionSet => "The ValueCollection does not support modification.",
			ExceptionResource.InvalidOperation_NullArray => "Null arrays are not supported.",
			ExceptionResource.InvalidOperation_HSCapacityOverflow => "Set hash capacity overflow. Cannot increase size.",
			ExceptionResource.NotSupported_StringComparison => "String comparison not supported.",
			ExceptionResource.ConcurrentCollection_SyncRoot_NotSupported => "SyncRoot not supported.",
			ExceptionResource.ArgumentException_OtherNotArrayOfCorrectLength => "The other array is not of the correct length.",
			ExceptionResource.ArgumentOutOfRange_EndIndexStartIndex => "The end index does not come after the start index.",
			ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported => "Huge arrays are not supported.",
			ExceptionResource.Argument_AddingDuplicate => "Duplicate item added.",
			ExceptionResource.Argument_InvalidArgumentForComparison => "Invalid argument for comparison.",
			ExceptionResource.Arg_LowerBoundsMustMatch => "Array lower bounds must match.",
			ExceptionResource.Arg_MustBeType => "Argument must be of type: ",
			ExceptionResource.InvalidOperation_IComparerFailed => "IComparer failed.",
			ExceptionResource.NotSupported_FixedSizeCollection => "This operation is not suppored on a fixed-size collection.",
			ExceptionResource.Rank_MultiDimNotSupported => "Multi-dimensional arrays are not supported.",
			ExceptionResource.Arg_TypeNotSupported => "Type not supported.",
			_ => resource.ToString(),
		};
	}

	private static ArgumentException GetWrongKeyTypeArgumentException(object key, Type targetType) => new($"Wrong key type. Expected {targetType}, got: '{key}'.", nameof(key));

	private static ArgumentException GetWrongValueTypeArgumentException(object value, Type targetType) => new($"Wrong value type. Expected {targetType}, got: '{value}'.", nameof(value));
}
