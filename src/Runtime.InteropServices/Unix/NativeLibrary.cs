using System;

using static NStuff.Runtime.InteropServices.Unix.NativeMethods;

namespace NStuff.Runtime.InteropServices.Unix
{
    internal class NativeLibrary : NativeLibraryBase
    {
        private IntPtr handle;

        internal NativeLibrary(string name) : base(name) {}

        protected override bool LoadLibrary(string name) => (handle = dlopen(name, RTLD_LAZY)) != IntPtr.Zero;

        protected internal override IntPtr GetAddress(string symbol) => dlsym(handle, symbol);

        protected internal override void CloseLibrary() => dlclose(handle);
    }
}
