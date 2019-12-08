using NStuff.Runtime.InteropServices.ObjectiveC;

namespace NStuff.WindowSystem.macOS
{
    /// <summary>
    /// Represents the data used by the native window server to manage a window.
    /// </summary>
    public class WindowData
    {
        internal Id TrackingArea { get; set; }
        internal Id MarkedText { get; set; }
        internal ulong ModifierFlags { get; set; }
        internal (double x, double y) CursorWrapDelta { get; set; }
        internal bool MouseInside { get; set; }

        /// <summary>
        /// Gets the native handle on the window.
        /// </summary>
        /// <value>The handle on the Objective C <c>NSWindow</c> object.</value>
        public Id Id { get; internal set; }
    }
}
