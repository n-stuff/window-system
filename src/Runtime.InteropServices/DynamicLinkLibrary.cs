﻿using System;
using System.Runtime.InteropServices;

namespace NStuff.Runtime.InteropServices
{
    /// <summary>
    /// Provides access to the exported symbols of a native library.
    /// </summary>
    public sealed class DynamicLinkLibrary : IDisposable
    {
        /// <summary>
        /// The delegate called by the instances of <c>DynamicLinkLibrary</c> to create <see cref="NativeLibraryBase"/> instances.
        /// </summary>
        /// <value>By default it is initialized with a delegate that supports Windows, macOS, and Linux.</value>
        public static NativeLibraryCreator NativeLibraryCreator { get; set; } = CreateNativeLibrary;

        private NativeLibraryBase? nativeLibrary;

        /// <summary>
        /// Initializes a new instance of the <b>DynamicLinkLibrary</b> class.
        /// </summary>
        /// <param name="name">The name of the native library.</param>
        public DynamicLinkLibrary(string name) => nativeLibrary = NativeLibraryCreator(name);

        /// <summary>
        /// Called by the garbage collector to free the resources associated with this object.
        /// </summary>
        ~DynamicLinkLibrary() => FreeResources();

        /// <summary>
        /// Frees the resources associated with this object. After calling this method, calling the other methods of this object
        /// is throwing an <c>ObjectDisposedException</c>.
        /// </summary>
        public void Dispose()
        {
            FreeResources();
            GC.SuppressFinalize(this);
        }

        private void FreeResources()
        {
            if (nativeLibrary != null)
            {
                nativeLibrary.CloseLibrary();
                nativeLibrary = null;
            }
        }

        /// <summary>
        /// Gets the address of an exported symbol.
        /// </summary>
        /// <param name="symbol">The name of the exported symbol.</param>
        /// <param name="throwIfNotFound">Whether or not the method must throw an exception if the symbol was not found.</param>
        /// <returns>The address of the exported symbol.</returns>
        public IntPtr GetSymbolAddress(string symbol, bool throwIfNotFound)
        {
            if (nativeLibrary == null)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            return nativeLibrary.GetSymbolAddress(symbol, throwIfNotFound);
        }

        private static NativeLibraryBase CreateNativeLibrary(string name)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new Windows.NativeLibrary(name);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new Unix.NativeLibrary(name);
            }
            throw new InvalidOperationException(Resources.GetMessage(Resources.Key.OSDetectionFailed));
        }
    }
}
