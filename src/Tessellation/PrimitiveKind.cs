namespace NStuff.Tessellation
{
    /// <summary>
    /// Lists the kind of graphics primitives that can be produced by the tessellator.
    /// </summary>
    public enum PrimitiveKind
    {
        /// <summary>
        /// The vertices will describe a line loop.
        /// </summary>
        LineLoop,

        /// <summary>
        /// The vertices will describe triangles.
        /// </summary>
        Triangles,

        /// <summary>
        /// The vertices will describe a triangle fan.
        /// </summary>
        TriangleFan,

        /// <summary>
        /// The vertices will describe a triangle strip.
        /// </summary>
        TriangleStrip
    }
}
