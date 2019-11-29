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
        /// The internal handle on the X11 Display.
        /// </summary>
        public IntPtr Display { get; internal set; }

        /// <summary>
        /// The internal handle on the X11 Visual.
        /// </summary>
        public IntPtr Visual { get; set; }

        /// <summary>
        /// The internal handle on the window.
        /// </summary>
        [CLSCompliant(false)]
        public ulong Id { get; internal set; }
    }
}
