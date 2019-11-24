using System;
using System.Collections.Generic;

namespace NStuff.WindowSystem
{
    /// <summary>
    /// Provides windowing system management methods.
    /// </summary>
    public sealed class WindowServer : IDisposable
    {
        /// <summary>
        /// The delegate called by the instances of <c>WindowServer</c> to create <see cref="NativeWindowServerBase"/> instances.
        /// </summary>
        /// <value>By default it is initialized with a delegate that supports Windows, macOS, and Linux.</value>
        public static Func<NativeWindowServerBase> NativeWindowServerCreator { get; set; } = CreateNativeWindowServer;

        internal NativeWindowServerBase? NativeWindowServer { get; private set; }

        /// <summary>
        /// The content of the clipboard.
        /// </summary>
        public string ClipboardString {
            get => GetNativeWindowServer().GetClipboardString() ?? string.Empty;
            set => GetNativeWindowServer().SetClipboardString(value ?? string.Empty);
        }

        /// <summary>
        /// A value indicating whether the cursor's <see cref="Dispose"/> method was called.
        /// </summary>
        /// <value><c>true</c> if <c>Dispose</c> was called.</value>
        public bool Disposed => NativeWindowServer == null;

        /// <summary>
        /// The time of the last key or mouse event, in seconds with a millisecond precision.
        /// </summary>
        public double EvenTime => GetNativeWindowServer().GetEventTime();

        /// <summary>
        /// The set of active modifier keys.
        /// </summary>
        public ModifierKeys ModifierKeys => GetNativeWindowServer().GetModifierKeys();

        /// <summary>
        /// The windows currently alive.
        /// </summary>
        public ICollection<Window> Windows => GetNativeWindowServer().GetWindows();

        /// <summary>
        /// Initializes a new instance of the <c>WindowServer</c> class.
        /// </summary>
        public WindowServer() => NativeWindowServer = NativeWindowServerCreator();

        /// <summary>
        /// Ensures that resources are freed and other cleanup operations are performed when the garbage collector
        /// reclaims the <see cref="WindowServer"/>.
        /// </summary>
        ~WindowServer() => FreeResources();

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
            if (NativeWindowServer != null)
            {
                foreach (Window window in new List<Window>(NativeWindowServer.GetWindows()))
                {
                    window.Dispose();
                }
                NativeWindowServer.Shutdown();
                NativeWindowServer = null;
            }
        }

        /// <summary>
        /// Creates a predefined cursor using a supplied cursor shape.
        /// </summary>
        /// <param name="cursorShape">The shape of the cursor.</param>
        /// <returns>A new cursor object.</returns>
        public Cursor CreateCursor(CursorShape cursorShape)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            return new Cursor(this, cursorShape);
        }

        /// <summary>
        /// Creates a cursor using the supplied image.
        /// </summary>
        /// <param name="imageData">The data of the image representing the cursor.</param>
        /// <param name="size">The size of the image representing the cursor.</param>
        /// <param name="hotSpot">The hot spot of the cursor inside the image representing the cursor.</param>
        /// <returns>A new cursor object.</returns>
        public Cursor CreateCursor(byte[] imageData, (int width, int height) size, (double x, double y) hotSpot)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (imageData.Length < size.width * size.height * 4)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.WrongCursorImageFormat));
            }
            return new Cursor(this, imageData, size, hotSpot);
        }

        /// <summary>
        /// Creates a window using the specified rendering context.
        /// </summary>
        /// <param name="renderingContext">The object used to initialize the rendering of the client area of the window.</param>
        /// <returns>A new window object.</returns>
        public Window CreateWindow(IRenderingContext renderingContext)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            return new Window(this, renderingContext);
        }

        /// <summary>
        /// Recreates if needed the supplied window in response to a graphic mode modification.
        /// </summary>
        /// <param name="window">The window to recreate.</param>
        public void RecreateWindow(Window window) => GetNativeWindowServer().RecreateNativeWindow(window);

        /// <summary>
        /// Gets a human-readable representation of the specified printable key.
        /// </summary>
        /// <param name="keycode">A code representing a keyboard key.</param>
        /// <returns>A string representing the specified keycode, or the empty string.</returns>
        public string GetKeyName(Keycode keycode)
        {
            if (NativeWindowServer == null)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (keycode <= Keycode.Space || keycode > Keycode.Backquote)
            {
                return string.Empty;
            }
            return NativeWindowServer.ConvertKeycodeToString(keycode);
        }

        /// <summary>
        /// Processes the incoming events.
        /// </summary>
        /// <param name="timeoutInSeconds">Specify the maximum amount of time, in seconds, the method should wait for an event to occur.
        ///     A negative value means that the method should wait indefinitely.</param>
        /// <returns><c>true</c> if at least one event was handled.</returns>
        public bool ProcessEvents(double timeoutInSeconds)
        {
            if (timeoutInSeconds < 0)
            {
                return GetNativeWindowServer().WaitAndProcessEvents();
            }
            else if (timeoutInSeconds < 1e-4)
            {
                return GetNativeWindowServer().ProcessEvents();
            }
            else
            {
                return GetNativeWindowServer().WaitAndProcessEvents(timeoutInSeconds);
            }
        }

        /// <summary>
        /// If <see cref="ProcessEvents(double)"/> is waiting for events, this method unblocks it.
        /// </summary>
        public void UnblockProcessEvents() => GetNativeWindowServer().UnblockProcessEvents();

        private NativeWindowServerBase GetNativeWindowServer() => NativeWindowServer ?? throw new ObjectDisposedException(GetType().FullName);

        private static NativeWindowServerBase CreateNativeWindowServer()
        {
            throw new NotImplementedException();
        }
    }
}
