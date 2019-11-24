namespace NStuff.WindowSystem
{
    /// <summary>
    /// Lists the border styles of a <see cref="Window"/>.
    /// </summary>
    public enum WindowBorderStyle
    {
        /// <summary>
        /// Indicates that the window must not have borders.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that the window must not have resize grips.
        /// </summary>
        Fixed,

        /// <summary>
        /// Indicates that the window must have resize grips.
        /// </summary>
        Sizable
    }
}
