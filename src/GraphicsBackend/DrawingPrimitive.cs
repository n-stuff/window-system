namespace NStuff.GraphicsBackend
{
    /// <summary>
    /// Lists the drawing primitives.
    /// </summary>
    public enum DrawingPrimitive
    {
        /// <summary>
        /// A closed polyline.
        /// </summary>
        LineLoop,

        /// <summary>
        /// An open polyline.
        /// </summary>
        LineStrip,

        /// <summary>
        /// Triangles.
        /// </summary>
        Triangles,

        /// <summary>
        /// A fan of triangles.
        /// </summary>
        TriangleFan,

        /// <summary>
        /// A strip of triangles.
        /// </summary>
        TriangleStrip
    }
}
