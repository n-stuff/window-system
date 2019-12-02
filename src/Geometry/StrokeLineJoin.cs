namespace NStuff.Geometry
{
    /// <summary>
    /// Lists the possible shape of the corners of a polyline.
    /// </summary>
    public enum StrokeLineJoin
    {
        /// <summary>
        /// A sharp corner is to be used to join path segments.
        /// </summary>
        Miter,
        /// <summary>
        /// A round corner is to be used to join path segments.
        /// </summary>
        Round,
        /// <summary>
        /// A triangle that fills the area between the two stroked segments is to be used to join path segments.
        /// </summary>
        Bevel
    }
}
