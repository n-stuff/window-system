namespace NStuff.Typography.Font
{
    /// <summary>
    /// Lists the platforms supported by the OpenType specification.
    /// </summary>
    public enum PlatformID
    {
        /// <summary>
        /// The unicode platform.
        /// </summary>
        Unicode = 0,

        /// <summary>
        /// The Macintosh platform.
        /// </summary>
        Macintosh = 1,

        /// <summary>
        /// The Windows platform.
        /// </summary>
        Windows = 3,

        /// <summary>
        /// A custom platform.
        /// </summary>
        Custom = 4
    }
}
