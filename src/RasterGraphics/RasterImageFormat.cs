namespace NStuff.RasterGraphics
{
    /// <summary>
    /// Lists the image formats.
    /// </summary>
    public enum RasterImageFormat
    {
        /// <summary>
        /// Pixels are in the red format.
        /// </summary>
        GreyscaleAlpha,

        /// <summary>
        /// Pixels are in the red-alpha format.
        /// </summary>
        Greyscale,

        /// <summary>
        /// Pixels are in red-green-blue format.
        /// </summary>
        TrueColor,

        /// <summary>
        /// Pixels are in red-green-blue-alpha format.
        /// </summary>
        TrueColorAlpha
    }
}
