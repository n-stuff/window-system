namespace NStuff.Typography.Typesetting
{
    /// <summary>
    /// Provides information about a glyph in a font family / subfamily.
    /// </summary>
    public class GlyphInfo
    {
        /// <summary>
        /// Gets information about the font of this glyph.
        /// </summary>
        /// <value>The font where this glyph comes from.</value>
        public FontInfo FontInfo { get; }

        /// <summary>
        /// Gets the index of this glyph in its font.
        /// </summary>
        /// <value>A non-negative number.</value>
        public int Index { get; }
        
        /// <summary>
        /// Gets the horizontal distance increment between this glyph and the next glyph to render.
        /// </summary>
        /// <value>A number in design units.</value>
        public int AdvanceWidth { get; }

        internal GlyphInfo(FontInfo fontInfo, int index, int advanceWidth)
        {
            FontInfo = fontInfo;
            Index = index;
            AdvanceWidth = advanceWidth;
        }
    }
}
