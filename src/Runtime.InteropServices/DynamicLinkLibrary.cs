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
        /// Gets of sets the delegate called by the instances of <c>DynamicLinkLibrary</c> to create <see cref="NativeLibraryBase"/> instances.
        /// </summary>
        /// <value>By default it is initialized with <see cref="CreateNativeLibrary(string)"/>.</value>
        public static Func<string, NativeLibraryBase> NativeLibraryCreator { get; set; } = CreateNativeLibrary;

        private NativeLibraryBase? nativeLibrary;

        /// <summary>
        /// Gets a value indicating whether the cursor's <see cref="Dispose()"/> method was called.
        /// </summary>
        /// <value><c>true</c> if <c>Dispose</c> was called.</value>
        public bool Disposed => nativeLibrary == null;

        /// <summary>
        /// Initializes a new instance of the <b>DynamicLinkLibrary</b> class using the supplied <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the native library, for example "OpenGL32".</param>
        public DynamicLinkLibrary(string name) => nativeLibrary = NativeLibraryCreator(name);

        /// <summary>
        /// Ensures that resources are freed and other cleanup operations are performed when the garbage collector
        /// reclaims the <see cref="DynamicLinkLibrary"/>.
        /// </summary>
        ~DynamicLinkLibrary() => FreeResources();

        /// <summary>
        /// Releases the resources associated with this object. After calling this method, calling the other methods of this object
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
        /// <param name="result">The address of the exported symbol.</param>
        /// <returns><c>true</c> if the symbol was found.</returns>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public bool TryGetSymbolAddress(string symbol, out IntPtr result)
        {
            result = GetNativeLibrary().GetAddress(symbol);
            return result != IntPtr.Zero;
        }

        /// <summary>
        /// Gets the address of an exported symbol.
        /// </summary>
        /// <param name="symbol">The name of the exported symbol.</param>
        /// <returns>The address of the exported symbol.</returns>
        /// <exception cref="ArgumentException">If the supplied symbol was not found.</exception>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public IntPtr GetSymbolAddress(string symbol)
        {
            var result = GetNativeLibrary().GetAddress(symbol);
            if (result == IntPtr.Zero)
            {
                throw new ArgumentException(Resources.FormatMessage(Resources.Key.UndefinedLibrarySymbol, symbol));
            }
            return result;
        }

        private NativeLibraryBase GetNativeLibrary() => nativeLibrary ?? throw new ObjectDisposedException(GetType().FullName);

        /// <summary>
        /// Creates a new <see cref="NativeLibraryBase"/> using the supplied name.
        /// Windows, macOS, and Linux are supported by this method.
        /// </summary>
        /// <param name="name">The name of the library.</param>
        /// <returns>A new native library.</returns>
        /// <exception cref="InvalidOperationException">If the current platform is not supported.</exception>
        public static NativeLibraryBase CreateNativeLibrary(string name)
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
