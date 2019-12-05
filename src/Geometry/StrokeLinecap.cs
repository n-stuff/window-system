namespace NStuff.Geometry
{
    /// <summary>
    /// Lists the possible shape of the ends of an open polyline.
    /// </summary>
    public enum StrokeLinecap
    {
        /// <summary>
        /// The stroke for each subpath does not extend beyond its two endpoints.
        /// </summary>
        Butt,

        /// <summary>
        /// The stroke will be extended by a half circle with a radius equal to half of the stroke width.
        /// </summary>
        Round,

        /// <summary>
        /// The stroke will be extended by a rectangle with the same width as the stroke width and whose length is half of the stroke width.
        /// </summary>
        Square
    }
}
