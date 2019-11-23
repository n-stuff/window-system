using System;
using System.Runtime.InteropServices;

namespace NStuff.Runtime.InteropServices.Unix
{
    internal static class NativeMethods
    {
#pragma warning disable IDE1006 // Naming Styles
        internal const int RTLD_LAZY = 0x1;

        [DllImport("libdl")]
        internal static extern int dlclose(IntPtr handle);

        [DllImport("libdl")]
        internal static extern IntPtr dlerror();

        [DllImport("libdl")]
        internal static extern IntPtr dlopen([MarshalAs(UnmanagedType.LPStr)] string path, int mode);

        [DllImport("libdl")]
        internal static extern IntPtr dlsym(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string symbol);
    }
}
