using NStuff.Typography.Font;
using System;

namespace NStuff.Typography.Typesetting
{
    /// <summary>
    /// Provides methods to arrange the glyphs corresponding to a sequence of unicode code points.
    /// </summary>
    public class GlyphLayout
    {
        private GlyphInfo? previousGlyph;
        private string? fontFamily;
        private FontSubfamily? fontSubfamily;
        private double fontPoints;
        private double x;

        /// <summary>
        /// Gets the object used to query metrics of fonts.
        /// </summary>
        /// <value>The object supplied to the constructor.</value>
        public FontMetrics FontMetrics { get; }

        /// <summary>
        /// Gets or sets the font used to layout glyphs.
        /// </summary>
        /// <value>The name of a font in <see cref="FontMetrics.OpenTypeCollection"/>.</value>
        public string? FontFamily {
            get => fontFamily;
            set {
                if (fontFamily != value)
                {
                    fontFamily = value;
                    previousGlyph = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the font subfamily to use to layout glyphs.
        /// </summary>
        /// <value>A font subfamily valid for <see cref="FontFamily"/>.</value>
        public FontSubfamily? FontSubFamily {
            get => fontSubfamily;
            set {
                if (fontSubfamily.HasValue != value.HasValue || (fontSubfamily.HasValue && fontSubfamily.Value != value!.Value))
                {
                    fontSubfamily = value;
                    previousGlyph = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the font to use to layout glyphs.
        /// </summary>
        /// <value>A font size in points.</value>
        public double FontPoints {
            get => fontPoints;
            set {
                if (fontPoints != value)
                {
                    fontPoints = value;
                    previousGlyph = null;
                }
            }
        }

        /// <summary>
        /// Gets the current line number. It is incremented by a call to <see cref="StartLine()"/>, or automatically
        /// when <see cref="X"/> becomes greater than <see cref="MaxLineWidth"/>.
        /// </summary>
        /// <value>A positive number.</value>
        public int Line { get; private set; }

        /// <summary>
        /// Gets or sets the x-coordinate of the insertion point of the next glyph.
        /// </summary>
        /// <value>An offset in pixel.</value>
        public double X {
            get => x;
            set {
                if (x != value)
                {
                    x = value;
                    previousGlyph = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum line width. A value less of equals to zero means that there is no limit.
        /// </summary>
        /// <value>A number of pixels.</value>
        public double MaxLineWidth { get; set; }

        /// <summary>
        /// Gets the ascent of the largest glyph on the current line.
        /// </summary>
        /// <value>A number of pixels.</value>
        public double Ascent { get; private set; }

        /// <summary>
        /// Gets the descent of the largest glyph on the current line.
        /// </summary>
        /// <value>A number of pixels.</value>
        public double Descent { get; private set; }

        /// <summary>
        /// Gets the line gap of the largest glyph on the current line.
        /// </summary>
        /// <value>A number of pixels.</value>
        public double LineGap { get; private set; }

        /// <summary>
        /// Initializes a new object of the <c>GlyphLayout</c> class using the provided <paramref name="fontMetrics"/>.
        /// </summary>
        /// <param name="fontMetrics"></param>
        public GlyphLayout(FontMetrics fontMetrics) => FontMetrics = fontMetrics;

        /// <summary>
        /// Puts this <c>GlyphLayout</c> instance back to its initial state.
        /// </summary>
        public void Reset()
        {
            Line = -1;
            fontFamily = null;
            fontSubfamily = null;
            StartLine();
        }

        /// <summary>
        /// Starts a new line using the current font properties. <see cref="Line"/> is incremented.
        /// <see cref="X"/>, <see cref="Ascent"/>, <see cref="Descent"/>, and <see cref="LineGap"/> are reset to <c>0</c>.
        /// </summary>
        public void StartLine()
        {
            Line++;
            X = 0;
            Ascent = 0;
            Descent = 0;
            LineGap = 0;
        }

        /// <summary>
        /// Inserts the glyph associated with the supplied unicode code point at the current insertion point.
        /// It updates <see cref="X"/>, <see cref="Line"/>, <see cref="Ascent"/>, <see cref="Descent"/>, and <see cref="LineGap"/> if needed.
        /// </summary>
        /// <param name="codePoint">A unicode code point.</param>
        /// <exception cref="InvalidOperationException">If <see cref="FontFamily"/> or <see cref="FontSubFamily"/> are <c>null</c>.</exception>
        public void Insert(int codePoint)
        {
            if (fontFamily == null || fontSubfamily == null)
            {
                throw new InvalidOperationException();
            }
            var glyph = FontMetrics.GetGlyphInfo(fontFamily, fontSubfamily.Value, codePoint);
            var kerningAdvance = (previousGlyph == null) ? 0 : FontMetrics.GetKerningAdvance(previousGlyph, glyph, FontPoints);
            var advanceWidth = FontMetrics.GetAdvanceWidth(glyph, FontPoints);
            if (MaxLineWidth > 0 && kerningAdvance + advanceWidth > MaxLineWidth)
            {
                StartLine();
                x = advanceWidth;
            }
            else
            {
                x += kerningAdvance + advanceWidth;
            }
            if (previousGlyph == null)
            {
                Ascent = Math.Max(Ascent, FontMetrics.GetAscent(glyph.FontInfo, FontPoints));
                Descent = Math.Max(Descent, FontMetrics.GetDescent(glyph.FontInfo, FontPoints));
                LineGap = Math.Max(LineGap, FontMetrics.GetLineGap(glyph.FontInfo, FontPoints));
            }
        }
    }
}
