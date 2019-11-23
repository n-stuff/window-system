using System;

using static NStuff.Runtime.InteropServices.Windows.NativeMethods;

namespace NStuff.Runtime.InteropServices.Windows
{
    internal class NativeLibrary : NativeLibraryBase
    {
        private IntPtr handle;

        internal NativeLibrary(string name) : base(name) {}

        protected override bool LoadLibrary(string name) => (handle = LoadLibraryW(name)) != IntPtr.Zero;

        protected override IntPtr GetAddress(string symbol) => GetProcAddress(handle, symbol);

        internal override void CloseLibrary() => FreeLibrary(handle);
    }
}
