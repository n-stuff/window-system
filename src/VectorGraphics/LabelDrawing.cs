using NStuff.GraphicsBackend;
using NStuff.Text;
using NStuff.Typography.Font;
using System;
using System.Collections.Generic;

namespace NStuff.VectorGraphics
{
    /// <summary>
    /// Represents a text rendered using rasterized glyphs.
    /// Applying scaling or rotation to this drawing will result in blurry and pixelated glyphs.
    /// </summary>
    public class LabelDrawing
    {
        private readonly List<int> codePoints = new List<int>();

        /// <summary>
        /// Gets or sets the transform to apply to all coordinates of the drawing.
        /// </summary>
        /// <value>An affine transform. The initial value is the identity matrix.</value>
        public AffineTransform Transform { get; set; } = new AffineTransform(m11: 1, m22: 1);

        /// <summary>
        /// Gets or sets the name of the font family to use to render this label.
        /// </summary>
        /// <value>A string representing the name of a font family.</value>
        public string? FontFamily { get; set; }

        /// <summary>
        /// Gets or sets the font subfamily to use to render this label.
        /// </summary>
        /// <value>A valid font subfamily for the supplied <see cref="FontFamily"/>.</value>
        public FontSubfamily? FontSubfamily { get; set; }

        /// <summary>
        /// Gets or sets the size in points of the font.
        /// </summary>
        /// <value>A value in design point units.</value>
        public double FontPoints { get; set; }

        /// <summary>
        /// Gets or sets the color to use to render the text.
        /// </summary>
        /// <value>Default is fully opaque black.</value>
        public RgbaColor Color { get; set; } = new RgbaColor(0, 0, 0, 255);

        /// <summary>
        /// Clears the text of this label.
        /// </summary>
        public void Clear() => codePoints.Clear();

        /// <summary>
        /// Appends a unicode code point to the text of this label.
        /// </summary>
        /// <param name="codePoint">The unicode code point to append.</param>
        public void AppendCodePoint(int codePoint) => codePoints.Add(codePoint);

        /// <summary>
        /// Appends the supplied text to the text of this label.
        /// </summary>
        /// <param name="text">The characters to append.</param>
        public void AppendString(string text)
        {
            var index = 0;
            while (TextHelper.TryGetCodePoint(text, ref index, out var codePoint))
            {
                AppendCodePoint(codePoint);
            }
        }

        /// <summary>
        /// Draws this label.
        /// </summary>
        /// <param name="context">The context used to draw this label.</param>
        public void Draw(DrawingContext context)
        {
            if (context.Disposed)
            {
                throw new ObjectDisposedException(context.GetType().FullName);
            }

            if (FontFamily == null || FontSubfamily == null || FontPoints == 0 || codePoints.Count == 0 || Color.Alpha == 0)
            {
                return;
            }

            context.SetColor(Color);
            context.SetTransform(Transform);
            context.SetupGreyScaleImageColorRendering();

            var glyphLayout = context.GlyphLayout;
            glyphLayout.Reset();
            glyphLayout.FontFamily = FontFamily;
            glyphLayout.FontSubFamily = FontSubfamily;
            glyphLayout.FontPoints = FontPoints;

            var y = 0;
            for (int i = 0; i < codePoints.Count; i++)
            {
                var codePoint = codePoints[i];
                var x = glyphLayout.X;
                glyphLayout.Insert(codePoint);
                var glyph = context.GetGlyph(FontFamily, FontSubfamily.Value, FontPoints, codePoint);
                if (glyph.HasImage)
                {
                    context.BindGlyphImage(glyph.Index);
                    context.AppendGlyphVertices(glyph, x, y);
                }
            }
        }
    }
}
