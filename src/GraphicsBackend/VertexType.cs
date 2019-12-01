namespace NStuff.GraphicsBackend
{
    /// <summary>
    /// Lists the possible layouts for the vertices.
    /// </summary>
    public enum VertexType
    {
        /// <summary>
        /// A vertex is composed of 2 doubles specifying an (x, y) point.
        /// </summary>
        PointCoordinates,
        /// <summary>
        /// A vertex is composed of 2 doubles specifying and (x, y) point, and 2 double specify image coordinates.
        /// </summary>
        PointAndImageCoordinates
    }
}
