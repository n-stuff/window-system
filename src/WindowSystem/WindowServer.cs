using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
        /// <value>By default it is initialized with <see cref="CreateNativeWindowServer"/>.</value>
        public static Func<NativeWindowServerBase> NativeWindowServerCreator { get; set; } = CreateNativeWindowServer;

        internal NativeWindowServerBase? NativeWindowServer { get; private set; }

        /// <summary>
        /// The content of the clipboard.
        /// </summary>
        /// <value>The text stored in the clipboard, or the empty string.</value>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public string ClipboardString {
            get => GetNativeWindowServer().GetClipboardString();
            set => GetNativeWindowServer().SetClipboardString(value ?? string.Empty);
        }

        /// <summary>
        /// Gets a value indicating whether the window server's <see cref="Dispose"/> method was called.
        /// </summary>
        /// <value><c>true</c> if <c>Dispose</c> was called.</value>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public bool Disposed => NativeWindowServer == null;

        /// <summary>
        /// Gets the time of the last key or mouse event, in seconds with a millisecond precision.
        /// </summary>
        /// <value>The number of seconds elapsed since the last event.</value>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public double EvenTime => GetNativeWindowServer().GetEventTime();

        /// <summary>
        /// Gets the set of active modifier keys.
        /// </summary>
        /// <value>A bitwise combination of the values that specifies keyboard modifiers.</value>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public ModifierKeys ModifierKeys => GetNativeWindowServer().GetModifierKeys();

        /// <summary>
        /// Gets the list of windows currently alive.
        /// </summary>
        /// <value>A list of <see cref="Window"/> objects.</value>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public ICollection<Window> Windows => GetNativeWindowServer().GetWindows();

        /// <summary>
        /// Initializes a new instance of the <c>WindowServer</c> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the current operating system is not supported.</exception>
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
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
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
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
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
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
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
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public void RecreateWindow(Window window) => GetNativeWindowServer().RecreateNativeWindow(window);

        /// <summary>
        /// Gets a human-readable representation of the specified printable key.
        /// </summary>
        /// <param name="keycode">A code representing a keyboard key.</param>
        /// <returns>A string representing the specified keycode, or the empty string.</returns>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
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
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
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
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public void UnblockProcessEvents() => GetNativeWindowServer().UnblockProcessEvents();

        private NativeWindowServerBase GetNativeWindowServer() => NativeWindowServer ?? throw new ObjectDisposedException(GetType().FullName);

        /// <summary>
        /// Creates a new <see cref="NativeWindowServerBase"/> instance.
        /// Windows, macOS, and Linux are supported by this method.
        /// </summary>
        /// <returns>A new native window server.</returns>
        /// <exception cref="InvalidOperationException">If the current platform is not supported.</exception>
        public static NativeWindowServerBase CreateNativeWindowServer()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new Windows.NativeWindowServer();
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new macOS.NativeWindowServer();
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new Linux.NativeWindowServer();
            }
            throw new InvalidOperationException(Resources.GetMessage(Resources.Key.OSDetectionFailed));
        }
    }
}
