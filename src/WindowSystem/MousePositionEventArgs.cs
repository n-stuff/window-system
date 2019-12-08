namespace NStuff.WindowSystem
{
    /// <summary>
    /// Holds the arguments of an event involving a change in mouse cursor's position.
    /// </summary>
    public struct MousePositionEventArgs
    {
        /// <summary>
        /// The position of the mouse cursor.
        /// </summary>
        /// <value>The coordinate of the cursor relative to the top-left corner of the window.</value>
        public (double x, double y) Position { get; }

        /// <summary>
        /// Initializes a new instance of the <c>MousePositionEventArgs</c> struct using provided <paramref name="position"/>.
        /// </summary>
        /// <param name="position">The position of the mouse cursor.</param>
        public MousePositionEventArgs((double x, double y) position) => Position = position;
    }
}
