using System;

namespace NStuff.Runtime.InteropServices
{
    public abstract class NativeLibraryBase
    {
        protected NativeLibraryBase(string name)
        {
            if (!LoadLibrary(name))
            {
                throw new ArgumentException(Resources.FormatMessage(Resources.Key.OpenLibraryFailed, name));
            }
        }

        internal IntPtr GetSymbolAddress(string symbol, bool throwIfNotFound)
        {
            var address = GetAddress(symbol);
            if (address == IntPtr.Zero && throwIfNotFound)
            {
                throw new ArgumentException(Resources.FormatMessage(Resources.Key.UndefinedLibrarySymbol, symbol));
            }
            return address;
        }

        protected abstract bool LoadLibrary(string name);

        protected abstract IntPtr GetAddress(string symbol);

        internal abstract void CloseLibrary();
    }
}
