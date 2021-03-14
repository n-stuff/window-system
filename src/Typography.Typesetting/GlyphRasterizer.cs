using NStuff.Geometry;
using NStuff.Typography.Font;
using System;

namespace NStuff.Typography.Typesetting
{
    /// <summary>
    /// Provides methods to rasterize font glyphs.
    /// </summary>
    public class GlyphRasterizer
    {
        private OpenType openType = default!;
        private double pixelSize;
        private readonly GlyphReader glyphReader = new();
        private readonly PolygonRasterizer polygonRasterizer = new();
        private readonly BezierApproximator bezierApproximator = new();
        private double xs;
        private double ys;

        /// <summary>
        /// Gets the left x-coordinate of the bounding box of the last rasterized glyph.
        /// </summary>
        /// <value>A value on the horizontal axis.</value>
        public int BoundingBoxLeft { get; private set; }

        /// <summary>
        /// Gets the top y-coordinate of the bounding box of the last rasterized glyph.
        /// </summary>
        /// <value>A value on the vertical axis.</value>
        public int BoundingBoxTop { get; private set; }

        /// <summary>
        /// Gets the right x-coordinate of the bounding box of the last rasterized glyph.
        /// </summary>
        /// <value>A value on the horizontal axis.</value>
        public int BoundingBoxRight { get; private set; }

        /// <summary>
        /// Gets the bottom y-coordinate of the bounding box of the last rasterized glyph.
        /// </summary>
        /// <value>A value on the vertical axis.</value>
        public int BoundingBoxBottom { get; private set; }

        /// <summary>
        /// Gets the horizontal size of the last rasterized glyph.
        /// </summary>
        /// <value>A horizontal size.</value>
        public int Width => BoundingBoxRight - BoundingBoxLeft + 1;

        /// <summary>
        /// Gets the vertical size of the last rasterized glyph.
        /// </summary>
        /// <value>A vertical size.</value>
        public int Height => BoundingBoxBottom - BoundingBoxTop + 1;

        /// <summary>
        /// Gets the value used to scale the coordinates of the glyph.
        /// </summary>
        /// <value>A ratio applied to coordinates.</value>
        public double Scale => pixelSize / openType.GetUnitsPerEm();

        /// <summary>
        /// Initializes this rasterizer to work with a font at the specified scaling.
        /// </summary>
        /// <param name="openType">The font containing the glyphs to rasterize.</param>
        /// <param name="pixelSize">The pixel size of the font to render.</param>
        public void Setup(OpenType openType, double pixelSize)
        {
            this.openType = openType;
            this.pixelSize = pixelSize;
            bezierApproximator.PointComputed = PointComputed;
        }

        /// <summary>
        /// Rasterizes the glyph at the specified index in the current font.
        /// </summary>
        /// <param name="glyphIndex">The index of the glyph to render in the current font.</param>
        /// <returns><c>true</c> if the glyph was successfully rasterized.</returns>
        public bool Rasterize(int glyphIndex)
        {
            if (!glyphReader.Setup(openType, pixelSize, glyphIndex))
            {
                return false;
            }
            BoundingBoxLeft = (int)Math.Floor(glyphReader.XMin);
            BoundingBoxTop = (int)Math.Floor(-glyphReader.YMax);
            BoundingBoxRight = (int)Math.Ceiling(glyphReader.XMax);
            BoundingBoxBottom = (int)Math.Ceiling(-glyphReader.YMin);
            polygonRasterizer.Setup((uint)Width, (uint)Height);
            xs = 0;
            ys = 0;
            while (glyphReader.Move())
            {
                double x = glyphReader.X - BoundingBoxLeft;
                double y = -glyphReader.Y - BoundingBoxTop;
                switch (glyphReader.PathCommand)
                {
                    case PathCommand.MoveTo:
                        xs = x;
                        ys = y;
                        break;

                    case PathCommand.LineTo:
                        PointComputed(x, y);
                        break;

                    case PathCommand.QuadraticBezierTo:
                        bezierApproximator.ApproximateQuadratic(xs, ys, glyphReader.Cx - BoundingBoxLeft, -glyphReader.Cy - BoundingBoxTop, x, y);
                        break;

                    case PathCommand.CubicBezierTo:
                        bezierApproximator.ApproximateCubic(xs, ys, glyphReader.Cx - BoundingBoxLeft, -glyphReader.Cy - BoundingBoxTop,
                            glyphReader.Cx1 - BoundingBoxLeft, -glyphReader.Cy1 - BoundingBoxTop, x, y);
                        break;
                }
            }
            return true;
        }

        /// <summary>
        /// Draws the last rasterized glyph into the specified bitmap.
        /// </summary>
        /// <param name="bitmap">A 8bit single channel bitmap.</param>
        /// <param name="bitmapWidth">The width of the bitmap in pixels.</param>
        /// <param name="x">The x coordinate where the glyph should be drawn in the bitmap.</param>
        /// <param name="y">The y coordinate where the glyph should be drawn in the bitmap.</param>
        public void DrawGlyph(byte[] bitmap, int bitmapWidth, int x, int y) =>
            polygonRasterizer.DrawPolygon(bitmap, (uint)bitmapWidth, x, y);

        private void PointComputed(double x, double y)
        {
            polygonRasterizer.RasterizeEdge(xs, ys, x, y);
            xs = x;
            ys = y;
        }
    }
}
