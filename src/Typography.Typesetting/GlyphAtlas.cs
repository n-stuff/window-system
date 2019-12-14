using NStuff.Geometry;
using NStuff.Typography.Font;
using System;
using System.Collections.Generic;

namespace NStuff.Typography.Typesetting
{
    /// <summary>
    /// Provides methods to store rasterized glyphs.
    /// </summary>
    public class GlyphAtlas
    {
        private struct GlyphKey : IEquatable<GlyphKey>
        {
            private readonly string fontFamily;
            private readonly FontSubfamily fontSubfamily;
            private readonly double fontPixels;
            private readonly int codePoint;

            internal GlyphKey(string fontFamily, FontSubfamily fontSubfamily, double fontPixels, int codePoint)
            {
                this.fontFamily = fontFamily;
                this.fontSubfamily = fontSubfamily;
                this.fontPixels = fontPixels;
                this.codePoint = codePoint;
            }

            public bool Equals(GlyphKey other) =>
                fontFamily == other.fontFamily &&
                fontSubfamily == other.fontSubfamily &&
                fontPixels == other.fontPixels &&
                codePoint == other.codePoint;

            public override int GetHashCode() => HashCode.Combine(fontFamily, fontSubfamily, fontPixels, codePoint);
        }

        private readonly Dictionary<GlyphKey, Glyph> glyphs = new Dictionary<GlyphKey, Glyph>();
        private readonly List<byte[]> images = new List<byte[]>();
        private readonly GlyphRasterizer rasterizer = new GlyphRasterizer();
        private readonly List<RectanglePacker> rectanglePackers = new List<RectanglePacker>();

        /// <summary>
        /// Gets the <see cref="FontMetrics"/> object used to manage glyph information.
        /// </summary>
        /// <value>An object used to get information about glyphs.</value>
        public FontMetrics FontMetrics { get; }

        /// <summary>
        /// The width and height of the images used to store the rasterized glyphs.
        /// </summary>
        public int ImageDimension { get; }

        /// <summary>
        /// Gets the images used to store the rasterized glyphs.
        /// </summary>
        public IList<byte[]> Images { get; }

        /// <summary>
        /// Initializes a new instance of the <c>GlyphAtlas</c> class.
        /// </summary>
        /// <param name="fontMetrics">The object used to get information about glyphs.</param>
        /// <param name="imageDimension">The width and height of the images used to store the rasterized glyphs.</param>
        public GlyphAtlas(FontMetrics fontMetrics, int imageDimension)
        {
            FontMetrics = fontMetrics;
            ImageDimension = imageDimension;
            Images = images.AsReadOnly();
            images.Add(new byte[imageDimension * imageDimension]);
            rectanglePackers.Add(new RectanglePacker(imageDimension, imageDimension));
        }

        /// <summary>
        /// Gets the glyph corresponding to the supplied <paramref name="fontFamily"/>, <paramref name="fontSubfamily"/>,
        /// <paramref name="fontPoints"/>, and <paramref name="codePoint"/>.
        /// </summary>
        /// <param name="fontFamily">The name of a font.</param>
        /// <param name="fontSubfamily">The subfamily of a font.</param>
        /// <param name="fontPoints">The size of the font in points.</param>
        /// <param name="codePoint">The unicode code point of the character corresponding to the glyph.</param>
        /// <param name="newGlyph">A value indicating whether the glyph did exist before this method was called.</param>
        /// <returns>The requested glyph.</returns>
        public Glyph GetGlyph(string fontFamily, FontSubfamily fontSubfamily, double fontPoints, int codePoint, out bool newGlyph)
        {
            var fontPixels = fontPoints * FontMetrics.PixelScaling * 96 / 72;
            var key = new GlyphKey(fontFamily, fontSubfamily, fontPixels, codePoint);
            if (!glyphs.TryGetValue(key, out var glyph))
            {
                newGlyph = true;
                var openType = FontMetrics.OpenTypeCollection[fontFamily, fontSubfamily];
                rasterizer.Setup(openType, fontPixels);
                var glyphInfo = FontMetrics.GetGlyphInfo(fontFamily, fontSubfamily, codePoint);
                if (!rasterizer.Rasterize(openType.GetGlyphIndex(codePoint)))
                {
                    // White space
                    glyph = new Glyph(glyphInfo, -1, 0, 0, 0, 0, 0, 0);
                }
                else
                {
                    RectanglePacker? rectanglePacker = null;
                    (int left, int top) location = (0, 0);
                    int index = 0;
                    for (int i = 0; i < rectanglePackers.Count; i++)
                    {
                        rectanglePacker = rectanglePackers[i];
                        if (rectanglePacker.TryPackRectangle(rasterizer.Width, rasterizer.Height, out location))
                        {
                            index = i;
                            break;
                        }
                        rectanglePacker = null;
                    }
                    if (rectanglePacker == null)
                    {
                        rectanglePacker = new RectanglePacker(ImageDimension, ImageDimension);
                        if (!rectanglePacker.TryPackRectangle(rasterizer.Width, rasterizer.Height, out location))
                        {
                            throw new InvalidOperationException("Glyph is bigger than the maximum image size");
                        }
                        index = rectanglePackers.Count;
                        rectanglePackers.Add(rectanglePacker);
                        images.Add(new byte[ImageDimension * ImageDimension]);
                    }
                    rasterizer.DrawGlyph(images[index], ImageDimension, location.left, location.top);
                    glyph = new Glyph(glyphInfo, index, location.left, location.top, rasterizer.Width, rasterizer.Height,
                        rasterizer.BoundingBoxLeft, rasterizer.BoundingBoxTop);
                }
            }
            else
            {
                newGlyph = false;
            }
            return glyph;
        }
    }
}
