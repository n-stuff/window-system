using System.Runtime.InteropServices;

using BOOL = System.Int32;
using FARPROC = System.IntPtr;
using HMODULE = System.IntPtr;

namespace NStuff.Runtime.InteropServices.Windows
{
    internal static class NativeMethods
    {
        [DllImport("Kernel32", SetLastError = true)]
        internal static extern BOOL FreeLibrary(HMODULE hModule);

        [DllImport("Kernel32", SetLastError = true)]
        internal static extern FARPROC GetProcAddress(HMODULE hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

        [DllImport("Kernel32", SetLastError = true)]
        internal static extern HMODULE LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string fileName);
    }
}
