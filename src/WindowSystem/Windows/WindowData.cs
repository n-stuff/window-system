using System;

namespace NStuff.WindowSystem.Windows
{
    /// <summary>
    /// Represents the data used by the native window server to manage a window.
    /// </summary>
    public class WindowData
    {
        internal bool Visible { get; set; }
        internal bool MouseEntered { get; set; }
        internal bool TopMost { get; set; }
        internal bool SystemButtonInteraction { get; set; }
        internal ulong SizeState { get; set; }
        internal (int x, int y) LastCursorPosition { get; set; }
        internal (double width, double height) MinimumSize { get; set; }
        internal (double width, double height) MaximumSize { get; set; }
        internal (double x, double y) Scale { get; set; }

        /// <summary>
        /// Gets the native handle of the window.
        /// </summary>
        /// <value>A native win32 handle.</value>
        public IntPtr Handle { get; internal set; }
    }
}
