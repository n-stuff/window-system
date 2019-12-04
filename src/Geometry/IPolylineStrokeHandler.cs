namespace NStuff.Geometry
{
    /// <summary>
    /// Used by <see cref="PolylineStroker"/> to notify of new events when computing a stroke.
    /// </summary>
    public interface IPolylineStrokeHandler
    {
        /// <summary>
        /// Called before a new stroke is computed.
        /// </summary>
        void BeginPolygon();

        /// <summary>
        /// Called after a stroke was computed.
        /// </summary>
        void EndPolygon();

        /// <summary>
        /// Called before a new contour of the current polygon is be computed.
        /// </summary>
        void BeginContour();

        /// <summary>
        /// Called after a contour of the current polygon was computed.
        /// </summary>
        void EndContour();

        /// <summary>
        /// Called when a point of a contour was computed.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        void AddPoint(double x, double y);
    }
}
