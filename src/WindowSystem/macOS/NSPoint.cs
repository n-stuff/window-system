using System.Runtime.InteropServices;

namespace NStuff.WindowSystem.macOS
{
    /// <summary>
    /// A point in a Cartesian coordinate system.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NSPoint
    {
        /// <summary>
        /// Implicitely converts a <c>NSPoint</c> to a tuple.
        /// </summary>
        /// <param name="size">The point to convert.</param>
        public static implicit operator (double x, double y)(NSPoint point) => (point.x, point.y);

        private readonly double x;
        private readonly double y;

        /// <summary>
        /// The x coordinate of the point.
        /// </summary>
        public double X => x;

        /// <summary>
        /// The y coordinate of the point.
        /// </summary>
        public double Y => y;

        /// <summary>
        /// Initializes a new instance of the <c>NSPoint</c> struct.
        /// </summary>
        /// <param name="x">The x coordinate of the point.</param>
        /// <param name="y">The y coordinate of the point.</param>
        public NSPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
