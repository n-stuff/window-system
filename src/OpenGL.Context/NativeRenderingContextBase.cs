using NStuff.WindowSystem;
using System;

namespace NStuff.OpenGL.Context
{
    /// <summary>
    /// Provides a base class to implement a platform-specific rendering context.
    /// </summary>
    public abstract class NativeRenderingContextBase
    {
        /// <summary>
        /// Called just before the native window is created.
        /// </summary>
        /// <param name="context">The rendering context.</param>
        /// <param name="server">The window server.</param>
        /// <param name="window">The window to manage.</param>
        public abstract void AttachRenderingData(RenderingContext context, WindowServer windowServer, Window window);

        /// <summary>
        /// Called just after the native window was created.
        /// </summary>
        /// <param name="context">The rendering context.</param>
        /// <param name="server">The window server.</param>
        /// <param name="window">The window to manage.</param>
        public abstract void SetupRenderingData(RenderingContext context, WindowServer windowServer, Window window);

        /// <summary>
        /// Called just before the native window is destroyed.
        /// </summary>
        /// <param name="window">The managed window.</param>
        public abstract void DetachRenderingData(Window window);

        /// <summary>
        /// Called when the window is moved and/or resized.
        /// </summary>
        /// <param name="window">The managed window.</param>
        public abstract void UpdateRenderingData(Window window);

        /// <summary>
        /// Sets the target of the drawing orders in the calling thread.
        /// </summary>
        /// <param name="window">A window, or null.</param>
        public abstract void MakeWindowCurrent(Window? window);

        /// <summary>
        /// Exchanges the front and back buffers.
        /// </summary>
        /// <param name="window">The window associated with the context to update.</param>
        public abstract void SwapBuffers(Window window);

        /// <summary>
        /// Configures the rendering context to repaint on vertical blanks, or not.
        /// </summary>
        /// <param name="window">The window associated with the context to configure.</param>
        /// <param name="sync"><c>true</c> to synchronize the rendering with the vertical blank signal.</param>
        public abstract void SyncWithVerticalBlank(Window window, bool sync);

        /// <summary>
        /// Gets the address of an OpenGL entry point.
        /// </summary>
        /// <param name="commandName">The name of an OpenGL command.</param>
        /// <returns>The address of the entry point, or <c>IntPtr.Zero</c>.</returns>
        public abstract IntPtr GetCommandAddress(string commandName);

        /// <summary>
        /// Releases the resources allocated by this object.
        /// </summary>
        public abstract void FreeResources();

        /// <summary>
        /// The target of the drawing orders in the calling thread, or null.
        /// </summary>
        protected static Window? CurrentWindow => RenderingContext.currentWindow;
    }
}
