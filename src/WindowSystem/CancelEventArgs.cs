namespace NStuff.WindowSystem
{
    /// <summary>
    /// Holds the arguments of an event that can be cancelled.
    /// </summary>
    public struct CancelEventArgs
    {
        /// <summary>
        /// Whether the event should be cancelled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <c>CancelEventArgs</c> struct using supplied <paramref name="cancel"/> value.
        /// </summary>
        /// <param name="cancel">Whether the event should be cancelled.</param>
        public CancelEventArgs(bool cancel) => Cancel = cancel;
    }
}
