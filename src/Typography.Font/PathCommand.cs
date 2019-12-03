namespace NStuff.Typography.Font
{
    /// <summary>
    /// Lists the commands used to describe the shape of a glyph.
    /// </summary>
    public enum PathCommand
    {
        /// <summary>
        /// The supplied point becomes the current point.
        MoveTo,

        /// <summary>
        /// Draw a line from the current point to the supplied end point.
        /// The end point becomes the current point.
        /// </summary>
        LineTo,

        /// <summary>
        /// Draw a quadratic Bézier curve from the current point, using the supplied control point and end point.
        /// The end point becomes the current point.
        /// </summary>
        QuadraticBezierTo,

        /// <summary>
        /// Draw a cubic Bézier curve from the current point, using the supplied control points and end point.
        /// The end point becomes the current point.
        /// </summary>
        CubicBezierTo,
    }
}
