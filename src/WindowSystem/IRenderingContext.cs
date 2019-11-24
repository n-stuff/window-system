namespace NStuff.WindowSystem
{
    /// <summary>
    /// Represents an object that manages the rendering context of a window's client area.
    /// </summary>
    public interface IRenderingContext
    {
        /// <summary>
        /// Called just before the native window is created.
        /// </summary>
        /// <param name="server">The window server.</param>
        /// <param name="window">The window to manage.</param>
        void AttachRenderingData(WindowServer server, Window window);

        /// <summary>
        /// Called just after the native window was created.
        /// </summary>
        /// <param name="server">The window server.</param>
        /// <param name="window">The window to manage.</param>
        void SetupRenderingData(WindowServer server, Window window);

        /// <summary>
        /// Called just before the native window is destroyed.
        /// </summary>
        /// <param name="window">The managed window.</param>
        void DetachRenderingData(Window window);

        /// <summary>
        /// Called when the window is moved and/or resized.
        /// </summary>
        /// <param name="window">The managed window.</param>
        void UpdateRenderingData(Window window);
    }
}
