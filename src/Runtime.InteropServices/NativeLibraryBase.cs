using System;

namespace NStuff.Runtime.InteropServices
{
    /// <summary>
    /// Provides a base class to implement a platform-specific native library. 
    /// </summary>
    public abstract class NativeLibraryBase
    {
        /// <summary>
        /// Initializes a new instance of the <b>NativeLibraryBase</b> class using the supplied <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the native library.</param>
        protected NativeLibraryBase(string name)
        {
            if (!LoadLibrary(name))
            {
                throw new ArgumentException(Resources.FormatMessage(Resources.Key.OpenLibraryFailed, name));
            }
        }

        /// <summary>
        /// Loads a native library.
        /// </summary>
        /// <param name="name">The name of the native library.</param>
        /// <returns><c>true</c> if the library was actually loaded.</returns>
        protected abstract bool LoadLibrary(string name);

        /// <summary>
        /// Gets the address of an exported symbol.
        /// </summary>
        /// <param name="symbol">The name of the exported symbol.</param>
        /// <returns>The address of the exported symbol.</returns>
        protected internal abstract IntPtr GetAddress(string symbol);

        /// <summary>
        /// Closes the native library.
        /// </summary>
        protected internal abstract void CloseLibrary();
    }
}
