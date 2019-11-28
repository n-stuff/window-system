using System;
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
        public static Func<string, NativeLibraryBase> NativeLibraryCreator { get; set; } = CreateNativeLibrary;

        private NativeLibraryBase? nativeLibrary;

        /// <summary>
        /// A value indicating whether the cursor's <see cref="Dispose"/> method was called.
        /// </summary>
        /// <value><c>true</c> if <c>Dispose</c> was called.</value>
        public bool Disposed => nativeLibrary == null;

        /// <summary>
        /// Initializes a new instance of the <b>DynamicLinkLibrary</b> class using the supplied <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the native library.</param>
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
        public bool TryGetSymbolAddress(string symbol, out IntPtr result)
        {
            result = GetNativeLibrary().GetSymbolAddress(symbol, false);
            return result != IntPtr.Zero;
        }

        /// <summary>
        /// Gets the address of an exported symbol.
        /// </summary>
        /// <param name="symbol">The name of the exported symbol.</param>
        /// <returns>The address of the exported symbol.</returns>
        public IntPtr GetSymbolAddress(string symbol) => GetNativeLibrary().GetSymbolAddress(symbol, true);

        private NativeLibraryBase GetNativeLibrary()
            => nativeLibrary ?? throw new ObjectDisposedException(GetType().FullName);

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
