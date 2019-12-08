using System.Runtime.InteropServices;

namespace NStuff.WindowSystem.macOS
{
    /// <summary>
    /// A two-dimensional size.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NSSize
    {
        /// <summary>
        /// Implicitely converts a <c>NSSize</c> to a tuple.
        /// </summary>
        /// <param name="size">The size to convert.</param>
        public static implicit operator (double width, double height)(NSSize size) => (size.width, size.height);

        private readonly double width;
        private readonly double height;

        /// <summary>
        /// The horizontal component of the size.
        /// </summary>
        /// <value>A horizontal dimension.</value>
        public double Width => width;

        /// <summary>
        /// The vertical component of the size.
        /// </summary>
        /// <value>A vertical dimension.</value>
        public double Height => height;

        /// <summary>
        /// Initializes a new instance of the <c>NSSize</c> struct using the supplied width and height.
        /// </summary>
        /// <param name="width">The horizontal component of the size.</param>
        /// <param name="height">The vertical component of the size.</param>
        public NSSize(double width, double height)
        {
            this.width = width;
            this.height = height;
        }
    }
}
