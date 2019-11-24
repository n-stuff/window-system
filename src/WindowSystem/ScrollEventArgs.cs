namespace NStuff.WindowSystem
{
    /// <summary>
    /// Holds the arguments of an event involving a scroll gesture.
    /// </summary>
    public struct ScrollEventArgs
    {
        /// <summary>
        /// The x-coordinates change represented by the scroll operation.
        /// </summary>
        public double DeltaX { get; }

        /// <summary>
        /// The y-coordinates change represented by the scroll operation.
        /// </summary>
        public double DeltaY { get; }

        /// <summary>
        /// Initializes a new instance of the <c>ScrollEventArgs</c> usin the supplied <paramref name="deltaX"/> and <paramref name="deltaY"/>.
        /// </summary>
        /// <param name="deltaX">The x-coordinates change.</param>
        /// <param name="deltaY">The y-coordinates change.</param>
        public ScrollEventArgs(double deltaX, double deltaY)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
        }
    }
}
