using System.Runtime.InteropServices;

namespace NStuff.WindowSystem.macOS
{
    /// <summary>
    /// A rectangle.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NSRect
    {
        private readonly NSPoint location;
        private readonly NSSize size;

        /// <summary>
        /// The location of the top left corner of the rectangle.
        /// </summary>
        public NSPoint Location => location;

        /// <summary>
        /// The size of the rectangle.
        /// </summary>
        public NSSize Size => size;

        /// <summary>
        /// The x coordinate of the top left corner of the rectangle.
        /// </summary>
        public double X => location.X;

        /// <summary>
        /// The y coordinate of the top left corner of the rectangle.
        /// </summary>
        public double Y => location.Y;

        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        public double Width => size.Width;

        /// <summary>
        /// The height of the rectangle.
        /// </summary>
        public double Height => size.Height;

        /// <summary>
        /// Initializes a new instance of the <c>NSRect</c> struct using the supplied location and size.
        /// </summary>
        /// <param name="location">The location of the top left corner of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        public NSRect(NSPoint location, NSSize size)
        {
            this.location = location;
            this.size = size;
        }

        /// <summary>
        /// Initializes a new instance of the <c>NSRect</c> struct using the supplied coordinates, width, and height.
        /// </summary>
        /// <param name="x">The x coordinate of the top left corner of the rectangle.</param>
        /// <param name="y">The y coordinate of the top left corner of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public NSRect(double x, double y, double width, double height)
        {
            location = new NSPoint(x, y);
            size = new NSSize(width, height);
        }
    }
}
