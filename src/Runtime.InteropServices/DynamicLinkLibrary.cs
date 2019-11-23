using System;
using System.Runtime.InteropServices;

namespace NStuff.Runtime.InteropServices
{
    public sealed class DynamicLinkLibrary : IDisposable
    {
        public static NativeLibraryCreator NativeLibraryCreator { get; set; } = CreateNativeLibrary;

        private NativeLibraryBase? nativeLibrary;

        public DynamicLinkLibrary(string name) => nativeLibrary = NativeLibraryCreator(name);

        ~DynamicLinkLibrary() => FreeResources();

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
