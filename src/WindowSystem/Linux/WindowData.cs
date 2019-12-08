using System;

namespace NStuff.WindowSystem.Linux
{
    /// <summary>
    /// Represents the data used by the native window server to manage a window.
    /// </summary>
    public class WindowData
    {
        internal ulong Colormap { get; set; }
        internal WindowBorderStyle BorderStyle { get; set; }
        internal WindowSizeState SizeState { get; set; }
        internal (double x, double y) LastDragPosition { get; set; }
        internal IntPtr InputContext { get; set; }
        internal ulong LastKeyTime { get; set; }
        internal (double x, double y) Location { get; set; }
        internal (double width, double height) Size { get; set; }
        internal (double width, double height) MinimumSize { get; set; }
        internal (double width, double height) MaximumSize { get; set; }
        internal (int x, int y) WrapCursorPosition { get; set; }
        internal (int x, int y) LastCursorPosition { get; set; }
        internal bool MouseEntered { get; set; }

        /// <summary>
        /// Gets the native handle on the X11 Display.
        /// </summary>
        /// <value>The native handle on the X11 display where the window was created.</value>
        public IntPtr Display { get; internal set; }

        /// <summary>
        /// Gets or sets the native handle on the X11 Visual.
        /// </summary>
        /// <value>The native handle on the X11 visual of the window.</value>
        public IntPtr Visual { get; set; }

        /// <summary>
        /// Gets the native handle on the window.
        /// </summary>
        /// <value>The native X11 handle on the window.</value>
        [CLSCompliant(false)]
        public ulong Id { get; internal set; }
    }
}
