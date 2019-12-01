namespace NStuff.Tessellation
{
    /// <summary>
    /// Provides methods to handle tessellation events.
    /// </summary>
    /// <typeparam name="TPolygonData">The data associated with a polygon.</typeparam>
    /// <typeparam name="TVertexData">The data associated with a vertex.</typeparam>
    public interface ITessellateHandler<TPolygonData, TVertexData>
    {
        /// <summary>
        /// Called when a polygon will be specified.
        /// </summary>
        /// <param name="primitiveKind">The kind of graphics primitives that will be specified.</param>
        /// <param name="data">The data associated with the current polygon.</param>
        void Begin(PrimitiveKind primitiveKind, TPolygonData data);

        /// <summary>
        /// Called when a polygon was fully specified.
        /// </summary>
        /// <param name="data">The data associated with the polygon.</param>
        void End(TPolygonData data);

        /// <summary>
        /// Called to specifiy whether the next vertex is on the polygon boundary.
        /// </summary>
        /// <param name="onPolygonBoundary"><c>true</c> if the next vertex is part of the polygon boundary.</param>
        void FlagEdges(bool onPolygonBoundary);

        /// <summary>
        /// Called to specify that a vertex is part of the current polygon.
        /// </summary>
        /// <param name="x">The x-coordinate of the vertex.</param>
        /// <param name="y">The y-coordinate of the vertex.</param>
        /// <param name="z">The z-coordinate of the vertex.</param>
        /// <param name="data">The data associated with this vertex.</param>
        void AddVertex(double x, double y, double z, TVertexData data);

        /// <summary>
        /// Called when a new vertex, the intersection of two edges, is added to the polygon.
        /// </summary>
        /// <param name="x">The x-coordinate of the vertex.</param>
        /// <param name="y">The y-coordinate of the vertex.</param>
        /// <param name="z">The z-coordinate of the vertex.</param>
        /// <param name="origin1">The data and weight associated with one of the vertex that forms the two edges.</param>
        /// <param name="destination1">The data and weight associated with one of the vertex that forms the two edges.</param>
        /// <param name="origin2">The data and weight associated with one of the vertex that forms the two edges.</param>
        /// <param name="destination2">The data and weight associated with one of the vertex that forms the two edges.</param>
        /// <param name="polygonData">The data associated with the current polygon.</param>
        /// <returns>The data to associate with the new vertex.</returns>
        TVertexData CombineEdges(double x, double y, double z,
            (TVertexData data, double weight) origin1, (TVertexData data, double weight) destination1,
            (TVertexData data, double weight) origin2, (TVertexData data, double weight) destination2,
            TPolygonData polygonData);
    }
}
