using NStuff.Typography.Font;
using System;
using System.Collections.Generic;

namespace NStuff.Typography.Typesetting
{
    /// <summary>
    /// Provides information about the rendering of the fonts of an <see cref="OpenTypeCollection"/>.
    /// </summary>
    public class FontMetrics
    {
        private struct FontInfoKey : IEquatable<FontInfoKey>
        {
            private readonly string family;
            private readonly FontSubfamily subfamily;

            internal FontInfoKey(string family, FontSubfamily subfamily)
            {
                this.family = family;
                this.subfamily = subfamily;
            }

            public bool Equals(FontInfoKey other) => family == other.family && subfamily == other.subfamily;

            public override int GetHashCode() => HashCode.Combine(family, subfamily);
        }

        private struct GlyphInfoKey : IEquatable<GlyphInfoKey>
        {
            private readonly FontInfoKey font;
            private readonly int codePoint;

            internal GlyphInfoKey(string family, FontSubfamily subfamily, int codePoint)
            {
                font = new FontInfoKey(family, subfamily);
                this.codePoint = codePoint;
            }

            public bool Equals(GlyphInfoKey other) => font.Equals(other.font) && codePoint == other.codePoint;

            public override int GetHashCode() => HashCode.Combine(font, codePoint);
        }

        private readonly Dictionary<GlyphInfoKey, GlyphInfo> glyphInfos = new();
        private readonly Dictionary<FontInfoKey, FontInfo> fontInfos = new();

        /// <summary>
        /// Gets the font collection this object is providing information about.
        /// </summary>
        /// <value>A set of fonts.</value>
        public OpenTypeCollection OpenTypeCollection { get; }

        /// <summary>
        /// Gets or sets a ratio to apply to any number provided by this object.
        /// </summary>
        /// <value>The factor to apply. Default is <c>1</c>.</value>
        public double PixelScaling { get; set; } = 1;

        /// <summary>
        /// Initializes a new instance of the <c>FontMetrics</c> class using the supplied set of fonts.
        /// </summary>
        /// <param name="openTypeCollection">The fonts managed by this object.</param>
        public FontMetrics(OpenTypeCollection openTypeCollection) => OpenTypeCollection = openTypeCollection;

        /// <summary>
        /// Gets the <see cref="FontInfo"/> object associated with the supplied <paramref name="fontFamily"/>
        /// and <paramref name="fontSubfamily"/>.
        /// </summary>
        /// <param name="fontFamily">The font family of the requested information.</param>
        /// <param name="fontSubfamily">The font subfamily of the requested information.</param>
        /// <returns>An object encapsulating information about a font.</returns>
        public FontInfo GetFontInfo(string fontFamily, FontSubfamily fontSubfamily) => GetFontInfo(fontFamily, fontSubfamily, null);

        /// <summary>
        /// Gets the <see cref="GlyphInfo"/> object associated with the supplied <paramref name="fontFamily"/>,
        /// <paramref name="fontSubfamily"/> and unicode code point.
        /// </summary>
        /// <param name="fontFamily">The font family of the requested information.</param>
        /// <param name="fontSubfamily">The font subfamily of the requested information.</param>
        /// <param name="codePoint">The unicode code point of the requested information.</param>
        /// <returns>An object encapsulating information about a glyph.</returns>
        public GlyphInfo GetGlyphInfo(string fontFamily, FontSubfamily fontSubfamily, int codePoint)
        {
            var key = new GlyphInfoKey(fontFamily, fontSubfamily, codePoint);
            if (!glyphInfos.TryGetValue(key, out var glyphInfo))
            {
                var openType = OpenTypeCollection[fontFamily, fontSubfamily];
                var fontInfo = GetFontInfo(fontFamily, fontSubfamily, openType);
                var index = openType.GetGlyphIndex(codePoint);
                glyphInfo = new GlyphInfo(fontInfo, index, openType.GetAdvanceWidth(index));
                glyphInfos.Add(key, glyphInfo);
            }
            return glyphInfo;
        }

        /// <summary>
        /// Gets the cell ascent of the supplied font. 
        /// </summary>
        /// <param name="fontInfo">The font to query.</param>
        /// <param name="fontPoints">The size of the font in points.</param>
        /// <returns>A number of pixels.</returns>
        public double GetAscent(FontInfo fontInfo, double fontPoints) => GetFontScaling(fontInfo, fontPoints) * fontInfo.Ascent;

        /// <summary>
        /// Gets the cell descent of the supplied font. 
        /// </summary>
        /// <param name="fontInfo">The font to query.</param>
        /// <param name="fontPoints">The size of the font in points.</param>
        /// <returns>A number of pixels.</returns>
        public double GetDescent(FontInfo fontInfo, double fontPoints) => GetFontScaling(fontInfo, fontPoints) * fontInfo.Descent;

        /// <summary>
        /// Gets the recommended whitespace between two lines of text displayed using the supplied font.
        /// </summary>
        /// <param name="fontInfo">The font to query.</param>
        /// <param name="fontPoints">The size of the font in points.</param>
        /// <returns>A number of pixels.</returns>
        public double GetLineGap(FontInfo fontInfo, double fontPoints) => GetFontScaling(fontInfo, fontPoints) * fontInfo.LineGap;

        /// <summary>
        /// Gets the horizontal distance increment between the supplied glyph and the next glyph to render.
        /// </summary>
        /// <param name="glyphInfo">The glyph to query.</param>
        /// <param name="fontPoints">The size of the font in points.</param>
        /// <returns>A number in pixels.</returns>
        public double GetAdvanceWidth(GlyphInfo glyphInfo, double fontPoints) =>
            GetFontScaling(glyphInfo.FontInfo, fontPoints) * glyphInfo.AdvanceWidth;

        /// <summary>
        /// Gets the value by which the default distance between the supplied glyph should be modified during glyph layout.
        /// </summary>
        /// <param name="glyphInfo1">A glyph.</param>
        /// <param name="glyphInfo2">A glyph.</param>
        /// <param name="fontPoints">The size of the font in points.</param>
        /// <returns>A number in pixels.</returns>
        public double GetKerningAdvance(GlyphInfo glyphInfo1, GlyphInfo glyphInfo2, double fontPoints)
        {
            var fontInfo = glyphInfo1.FontInfo;
            if (fontInfo != glyphInfo2.FontInfo)
            {
                return 0;
            }
            var kerningAdvances = fontInfo.KerningAdvances;
            var key = (uint)glyphInfo1.Index << 16 | (uint)glyphInfo2.Index;
            if (!kerningAdvances.TryGetValue(key, out var kerningAdvance))
            {
                var openType = OpenTypeCollection[fontInfo.Family, fontInfo.Subfamily];
                kerningAdvance = openType.GetKerningAdvance(glyphInfo1.Index, glyphInfo2.Index);
                kerningAdvances.Add(key, kerningAdvance);
            }
            return GetFontScaling(fontInfo, fontPoints) * kerningAdvance;
        }

        private FontInfo GetFontInfo(string fontFamily, FontSubfamily fontSubfamily, OpenType? openType)
        {
            var key = new FontInfoKey(fontFamily, fontSubfamily);
            if (!fontInfos.TryGetValue(key, out var fontInfo))
            {
                openType ??= OpenTypeCollection[fontFamily, fontSubfamily];
                fontInfo = new FontInfo(fontFamily, fontSubfamily, openType.GetUnitsPerEm(),
                    openType.GetAscent(), openType.GetDescent(), openType.GetLineGap());
                fontInfos.Add(key, fontInfo);
            }
            return fontInfo;
        }

        private double GetFontScaling(FontInfo fontInfo, double fontPoints) => fontPoints * PixelScaling * 96 / 72 / fontInfo.UnitsPerEm;
    }
}
