namespace NStuff.GraphicsBackend
{
    /// <summary>
    /// Lists the image formats.
    /// </summary>
    public enum ImageFormat
    {
        /// <summary>
        /// A pixel has 1 component representing the transparency of the texture.
        /// </summary>
        GreyscaleAlpha,

        /// <summary>
        /// A pixel has 4 components representing red, green, blue, and alpha.
        /// </summary>
        TrueColorAlpha
    }
}
