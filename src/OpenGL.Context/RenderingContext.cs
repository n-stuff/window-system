using NStuff.WindowSystem;
using System;
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
        /// A value indicating whether the cursor's <see cref="Dispose"/> method was called.
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
        public void SwapBuffers(Window window) => GetNativeRenderingContext().SwapBuffers(window);

        /// <summary>
        /// Configures the rendering context to repaint on vertical blanks, or not.
        /// </summary>
        /// <param name="window">The window associated with the context to configure.</param>
        /// <param name="sync"><c>true</c> to synchronize the rendering with the vertical blank signal.</param>
        public void SyncWithVerticalBlank(Window window, bool sync) => GetNativeRenderingContext().SyncWithVerticalBlank(window, sync);

        /// <summary>
        /// Gets an OpenGL entry point.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type of the entry point.</typeparam>
        /// <param name="commandName">The name of command associated with the entry point.</param>
        /// <param name="throwIfNotFound"><c>true</c> to throw an exception if the entry point is not found.</param>
        /// <returns>A delegate that can be used to invoke an OpenGL command.</returns>
        public TDelegate? GetOpenGLEntryPoint<TDelegate>(string commandName, bool throwIfNotFound) where TDelegate : class
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
                if (throwIfNotFound)
                {
                    throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.OpenGLEntryPointNotPresent, commandName));
                }
                return default;
            }
            return Marshal.GetDelegateForFunctionPointer<TDelegate>(address);
        }

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

        void IRenderingContext.SetupRenderingData(WindowServer server, Window window) =>
            GetNativeRenderingContext().SetupRenderingData(this, server, window);

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
            throw new InvalidOperationException(Resources.GetMessage(Resources.Key.OSDetectionFailed));
        }
    }
}
