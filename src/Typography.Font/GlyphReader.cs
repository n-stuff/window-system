using System;

namespace NStuff.Typography.Font
{
    /// <summary>
    /// Reads the data of a glyph as a stream of path commands.
    /// </summary>
    public class GlyphReader
    {
        private GlyphDecoder? currentDecoder;
        private readonly TtfGlyphDecoder trueTypeGlyphDecoder = new TtfGlyphDecoder();
        private readonly CffGlyphDecoder cffGlyphDecoder = new CffGlyphDecoder();

        /// <summary>
        /// Gets the current path command.
        /// </summary>
        public PathCommand PathCommand => GetCurrentDecoder().PathCommand;

        /// <summary>
        /// Gets the x-coordinate of the end point of the current command.
        /// </summary>
        /// <value>A value representing an x-coordinate.</value>
        public double X => GetCurrentDecoder().X;

        /// <summary>
        /// Gets the y-coordinate of the end point of the current command.
        /// </summary>
        /// <value>A value representing a y-coordinate.</value>
        public double Y => GetCurrentDecoder().Y;

        /// <summary>
        /// Gets the x-coordinate of the first control point of the current command.
        /// </summary>
        /// <value>A value representing an x-coordinate.</value>
        public double Cx => GetCurrentDecoder().Cx;

        /// <summary>
        /// Gets the y-coordinate of the first control point of the current command.
        /// </summary>
        /// <value>A value representing a y-coordinate.</value>
        public double Cy => GetCurrentDecoder().Cy;

        /// <summary>
        /// Gets the x-coordinate of the second control point of the current command.
        /// </summary>
        /// <value>A value representing an x-coordinate.</value>
        public double Cx1 => GetCurrentDecoder().Cx1;

        /// <summary>
        /// Gets the y-coordinate of the second control point of the current command.
        /// </summary>
        /// <value>A value representing a y-coordinate.</value>
        public double Cy1 => GetCurrentDecoder().Cy1;

        /// <summary>
        /// Gets the minimal value <see cref="X"/>, <see cref="Cx"/>, or <see cref="Cx1"/> can have for the current glyph.
        /// </summary>
        /// <value>A minimal horizontal value.</value>
        public double XMin => GetCurrentDecoder().XMin;

        /// <summary>
        /// Gets the maximal value <see cref="X"/>, <see cref="Cx"/>, or <see cref="Cx1"/> can have for the current glyph.
        /// </summary>
        /// <value>A maximal horizontal value.</value>
        public double XMax => GetCurrentDecoder().XMax;

        /// <summary>
        /// Gets the minimal value <see cref="Y"/>, <see cref="Cy"/>, or <see cref="Cy1"/> can have for the current glyph.
        /// </summary>
        /// <value>A minimal vertical value.</value>
        public double YMin => GetCurrentDecoder().YMin;

        /// <summary>
        /// Gets the maximal value <see cref="Y"/>, <see cref="Cy"/>, or <see cref="Cy1"/> can have for the current glyph.
        /// </summary>
        /// <value>A maximal vertical value.</value>
        public double YMax => GetCurrentDecoder().YMax;

        /// <summary>
        /// Gets the scale used to transform values.
        /// </summary>
        /// <value>A value representing the coordinates scale.</value>
        public double Scale => GetCurrentDecoder().Scale;

        /// <summary>
        /// Initializes this reader to iterate over the supplied glyph.
        /// </summary>
        /// <param name="openType">The font containing the glyph.</param>
        /// <param name="pixelSize">The size in pixels of the font.</param>
        /// <param name="glyphIndex">The index of the glyph in <paramref name="openType"/>.</param>
        /// <returns><c>true</c> if the glyph can be read.</returns>
        public bool Setup(OpenType openType, double pixelSize, int glyphIndex)
        {
            currentDecoder = (openType.GetGlyphOutlineKind()) switch
            {
                GlyphOutlineKind.Ttf => trueTypeGlyphDecoder,
                GlyphOutlineKind.Cff => cffGlyphDecoder,
                _ => throw new InvalidOperationException("Glyph outline kind: " + openType.GetGlyphOutlineKind()),
            };
            return currentDecoder.Setup(openType, pixelSize, (uint)glyphIndex);
        }

        /// <summary>
        /// Moves to the next path command in the glyph.
        /// </summary>
        /// <returns><c>true</c> if a command was available.</returns>
        public bool Move() => GetCurrentDecoder().Move();

        private GlyphDecoder GetCurrentDecoder() => currentDecoder ?? throw new InvalidOperationException();
    }
}
