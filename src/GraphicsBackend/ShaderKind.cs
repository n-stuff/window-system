namespace NStuff.GraphicsBackend
{
    /// <summary>
    /// Lists the shaders that can be used while drawing polygons.
    /// </summary>
    public enum ShaderKind
    {
        /// <summary>
        /// Uses a plain color uniform to render pixels.
        /// </summary>
        PlainColor,

        /// <summary>
        /// Uses a plain color uniform and a bound image to render pixels.
        /// The image must be in greyscale format.
        /// </summary>
        GreyscaleImage,

        /// <summary>
        /// Uses a plain color uniform and a bound image to render pixels.
        /// The image must be in true-color format.
        /// </summary>
        TrueColorImage
    }
}
