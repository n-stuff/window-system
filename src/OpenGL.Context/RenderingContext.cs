using NStuff.WindowSystem;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NStuff.OpenGL.Context
{
    /// <summary>
    /// Provides methods to manage OpenGL contexts.
    /// </summary>
    public sealed class RenderingContext : IRenderingContext, IDisposable
    {
        /// <summary>
        /// The delegate called by the instances of <c>RenderingContext</c> to create <see cref="NativeRenderingContextBase"/> instances.
        /// </summary>
        /// <value>By default it is initialized with a delegate that supports Windows, macOS, and Linux.</value>
        public static Func<NativeRenderingContextBase> NativeRenderingContextCreator { get; set; } = CreateNativeRenderingContext;

        [ThreadStatic]
        internal static Window? currentWindow;

        private NativeRenderingContextBase? nativeRenderingContext;

        /// <summary>
        /// The target of drawing orders in the calling thread, or null.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public Window? CurrentWindow {
            get {
                if (nativeRenderingContext == null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                if (currentWindow != null && currentWindow.Disposed)
                {
                    nativeRenderingContext.MakeWindowCurrent(null);
                    currentWindow = null;
                }
                return currentWindow;
            }
            set {
                if (nativeRenderingContext == null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                if (currentWindow != null)
                {
                    nativeRenderingContext.MakeWindowCurrent(null);
                    currentWindow = null;
                }
                nativeRenderingContext.MakeWindowCurrent(value);
                currentWindow = value;
            }
        }

        /// <summary>
        /// A value indicating whether the rendering context's <see cref="Dispose"/> method was called.
        /// </summary>
        /// <value><c>true</c> if <c>Dispose</c> was called.</value>
        public bool Disposed => nativeRenderingContext == null;

        /// <summary>
        /// The settings used for window creation.
        /// </summary>
        public RenderingSettings? Settings { get; set; }

        /// <summary>
        /// Initializes a new instance of the <c>RenderingContext</c> class.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the current operating system is not supported.</exception>
        public RenderingContext() => nativeRenderingContext = NativeRenderingContextCreator();

        /// <summary>
        /// Ensures that resources are freed and other cleanup operations are performed when the garbage collector
        /// reclaims the <see cref="RenderingContext"/>.
        /// </summary>
        ~RenderingContext() => FreeResources();

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
            if (nativeRenderingContext != null)
            {
                nativeRenderingContext.FreeResources();
                nativeRenderingContext = null;
            }
        }

        /// <summary>
        /// Exchanges the front and back buffers.
        /// </summary>
        /// <param name="window">The window associated with the context to update.</param>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public void SwapBuffers(Window window) => GetNativeRenderingContext().SwapBuffers(window);

        /// <summary>
        /// Configures the rendering context to repaint on vertical blanks, or not.
        /// </summary>
        /// <param name="window">The window associated with the context to configure.</param>
        /// <param name="sync"><c>true</c> to synchronize the rendering with the vertical blank signal.</param>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public void SyncWithVerticalBlank(Window window, bool sync) => GetNativeRenderingContext().SyncWithVerticalBlank(window, sync);

        /// <summary>
        /// Gets an OpenGL entry point, or null.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type of the entry point.</typeparam>
        /// <param name="commandName">The name of command associated with the entry point.</param>
        /// <param name="result">A delegate that can be used to invoke an OpenGL command.</param>
        /// <returns><c>true</c> if the entry point was found.</returns>
        /// <exception cref="InvalidOperationException">If the <see cref="CurrentWindow"/> was not set.</exception>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public bool TryGetOpenGLEntryPoint<TDelegate>(string commandName, [NotNullWhen(returnValue: true)] out TDelegate? result)
            where TDelegate : class
        {
            if (nativeRenderingContext == null)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (currentWindow == null)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.RenderingContextNotSet));
            }
            var address = nativeRenderingContext.GetCommandAddress(commandName);
            if (address == IntPtr.Zero)
            {
                result = default;
                return false;
            }
            result = Marshal.GetDelegateForFunctionPointer<TDelegate>(address);
            return true;
        }

        /// <summary>
        /// Gets an OpenGL entry point, or throws an exception if not found.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type of the entry point.</typeparam>
        /// <param name="commandName">The name of command associated with the entry point.</param>
        /// <returns>A delegate that can be used to invoke an OpenGL command.</returns>
        /// <exception cref="InvalidOperationException">If the entry point was not found or if <see cref="CurrentWindow"/> was not set.</exception>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        public TDelegate GetOpenGLEntryPoint<TDelegate>(string commandName) where TDelegate : class
        {
            if (TryGetOpenGLEntryPoint<TDelegate>(commandName, out var result))
            {
                return result;
            }
            throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.OpenGLEntryPointNotPresent, commandName));
        }

        /// <summary>
        /// Called just before the native window is created.
        /// </summary>
        /// <param name="server">The window server.</param>
        /// <param name="window">The window to manage.</param>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        void IRenderingContext.AttachRenderingData(WindowServer server, Window window)
        {
            if (nativeRenderingContext == null)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (Settings == null)
            {
                Settings = new RenderingSettings();
            }
            nativeRenderingContext.AttachRenderingData(this, server, window);
        }

        /// <summary>
        /// Called just before the native window is destroyed.
        /// </summary>
        /// <param name="window">The managed window.</param>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        void IRenderingContext.DetachRenderingData(Window window)
        {
            if (nativeRenderingContext == null)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (window == currentWindow)
            {
                nativeRenderingContext.MakeWindowCurrent(null);
                currentWindow = null;
            }
            nativeRenderingContext.DetachRenderingData(window);
        }

        /// <summary>
        /// Called just after the native window was created.
        /// </summary>
        /// <param name="server">The window server.</param>
        /// <param name="window">The window to manage.</param>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        void IRenderingContext.SetupRenderingData(WindowServer server, Window window) =>
            GetNativeRenderingContext().SetupRenderingData(this, server, window);

        /// <summary>
        /// Called when the window is moved and/or resized.
        /// </summary>
        /// <param name="window">The managed window.</param>
        /// <exception cref="ObjectDisposedException">If <see cref="Dispose()"/> was called.</exception>
        void IRenderingContext.UpdateRenderingData(Window window) =>
            GetNativeRenderingContext().UpdateRenderingData(window);

        private NativeRenderingContextBase GetNativeRenderingContext() =>
            nativeRenderingContext ?? throw new ObjectDisposedException(GetType().FullName);

        private static NativeRenderingContextBase CreateNativeRenderingContext()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new Windows.NativeRenderingContext();
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new macOS.NativeRenderingContext();
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new Linux.NativeRenderingContext();
            }
            throw new InvalidOperationException(Resources.GetMessage(Resources.Key.OSDetectionFailed));
        }
    }
}
