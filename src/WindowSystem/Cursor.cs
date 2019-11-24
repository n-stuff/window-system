using System;

namespace NStuff.WindowSystem
{
    /// <summary>
    /// Represents a window system cursor.
    /// </summary>
    public sealed class Cursor : IDisposable
    {
        private NativeWindowServerBase? nativeWindowServer;

        /// <summary>
        /// A platform-dependent object set by the native window server.
        /// </summary>
        public object? NativeData { get; set; }

        /// <summary>
        /// A value indicating whether the cursor's <see cref="Dispose"/> method was called.
        /// </summary>
        /// <value><c>true</c> if <c>Dispose</c> was called.</value>
        public bool Disposed => NativeData == null;

        internal Cursor(WindowServer windowServer, byte[] imageData, (int width, int height) size, (double x, double y) hotSpot)
        {
            nativeWindowServer = windowServer.NativeWindowServer;
            nativeWindowServer?.CreateCursor(this, imageData, size, hotSpot);
        }

        internal Cursor(WindowServer windowServer, CursorShape shape)
        {
            nativeWindowServer = windowServer.NativeWindowServer;
            nativeWindowServer?.CreateCursor(this, shape);
        }

        /// <summary>
        /// Ensures that resources are freed and other cleanup operations are performed when the garbage collector
        /// reclaims the <see cref="Cursor"/>.
        /// </summary>
        ~Cursor() => FreeResources();

        /// <summary>
        /// Releases all resources used by the cursor.
        /// </summary>
        public void Dispose()
        {
            FreeResources();
            GC.SuppressFinalize(this);
        }

        private void FreeResources()
        {
            if (NativeData != null)
            {
                if (nativeWindowServer != null && nativeWindowServer.IsRunning())
                {
                    nativeWindowServer.DestroyCursor(this);
                }
                NativeData = null;
                nativeWindowServer = null;
            }
        }
    }
}
