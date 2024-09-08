using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Versioning;
using System.Security;

namespace Nava.Office.Runtime;

/// <summary>
/// Provides access to imported functions from DLLs.
/// </summary>
public static partial class DllImports {

	/// <summary>
	/// Contains functions for working with ole32.dll APIs.
	/// </summary>
	public static partial class OLE32 {

		/// <summary>Name of OLE32.dll</summary>
		public const string DllName = "ole32.dll";

		/// <summary>Creates a new bind context object.</summary>
		/// <param name="reserved">Reserved parameter.</param>
		/// <param name="bindContext">The created bind context object.</param>
		/// <returns>An integer representing the result of the method call.</returns>
		[DllImport(DllName, CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern int CreateBindCtx(uint reserved, out IBindCtx bindContext);

		/// <summary>Retrieves a pointer to the Running Object Table (ROT) that manages the running objects for the current desktop.</summary>
		/// <param name="reserved">Reserved parameter.</param>
		/// <param name="prot">A pointer to the retrieved IRunningObjectTable interface pointer.</param>
		[DllImport(DllName, CharSet = CharSet.Ansi, SetLastError = true)]
		public static extern void GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

		/// <summary>Retrieves the classId of the specified ProgID.</summary>
		/// <param name="progId">The ProgID of the COM object to retrieve.</param>
		/// <param name="classId">A reference to the variable that receives the classId.</param>
		[LibraryImport(DllName)]
		[ResourceExposure(ResourceScope.None)]
		public static partial int CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid classId);

		/// <summary>Retrieves the classId of the specified ProgID.</summary>
		/// <param name="progId">The ProgID of the COM object to retrieve.</param>
		/// <param name="classId">A reference to the variable that receives the classId.</param>
		[LibraryImport(DllName)]
		[ResourceExposure(ResourceScope.None)]
		public static partial int CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid classId);
	}

	/// <summary>Contains functions for working with oleaut32.dll APIs.</summary>
	public static partial class OLEAUT32 {

		/// <summary>Name of OLEAut32.dll</summary>
		public const string DllName = "oleaut32.dll";

		/// <summary>Retrieves the currently active COM object with the specified classId.</summary>
		/// <param name="classId">The classId of the COM object to retrieve.</param>
		/// <param name="reserved">Reserved parameter.</param>
		/// <param name="obj">A reference to the variable that receives the currently active COM object.</param>
		[LibraryImport(DllName)]
		[ResourceExposure(ResourceScope.None)]
		public static partial int GetActiveObject(ref Guid classId, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out object obj);
	}

	/// <summary>
	/// Contains functions for working with user32.dll APIs.
	/// </summary>

	public static partial class User32 {

		/// <summary>Name of user32.dll</summary>
		public const string DllName = "user32.dll";

		/// <summary>Retrieves the identifier of the process that owns the specified window.</summary>
		/// <param name="windowHandle">A handle to the window.</param>
		/// <param name="processId">A reference to the variable that receives the process identifier.</param>
		/// <returns>The identifier of the process that owns the window.</returns>
		[LibraryImport("user32.dll", SetLastError = true)]
		public static partial int GetWindowThreadProcessId(IntPtr windowHandle, out int processId);
	}
}