namespace NStuff.Typography.Typesetting
{
    /// <summary>
    /// Represents a glyph stored in a <see cref="GlyphAtlas"/>.
    /// </summary>
    public class Glyph
    {
        /// <summary>
        /// Gets font-related information about this glyph.
        /// </summary>
        /// <value>An object containing information about this glyph.</value>
        public GlyphInfo Info { get; }

        /// <summary>
        /// Gets a value indicating whether the image representing this glyph is empty. 
        /// </summary>
        /// <value><c>true</c> if the image is empty.</value>
        public bool HasImage => Index >= 0;

        /// <summary>
        /// Gets the index of the image of the <see cref="GlyphAtlas"/> where this glyph is stored.
        /// </summary>
        /// <value>A positive number.</value>
        public int Index { get; }

        /// <summary>
        /// Gets the x-coordinate of the glyph in the associated image of the <see cref="GlyphAtlas"/>.
        /// </summary>
        /// <value>A value in pixels.</value>
        public int X { get; }

        /// <summary>
        /// Gets the y-coordinate of the glyph in the associated image of the <see cref="GlyphAtlas"/>.
        /// </summary>
        /// <value>A value in pixels.</value>
        public int Y { get; }

        /// <summary>
        /// Gets the width of the glyph in the associated image of the <see cref="GlyphAtlas"/>.
        /// </summary>
        /// <value>A value in pixels.</value>
        public int Width { get; }

        /// <summary>
        /// Gets the height of the glyph in the associated image of the <see cref="GlyphAtlas"/>.
        /// </summary>
        /// <value>A value in pixels.</value>
        public int Height { get; }

        /// <summary>
        /// Gets the horizontal offset of the image representing the glyph relative to the current
        /// position of the text cursor.
        /// </summary>
        /// <value>A value in pixels.</value>
        public int Left { get; }

        /// <summary>
        /// Gets the vertical offset of the image representing the glyph relative to the current
        /// position of the text cursor.
        /// </summary>
        /// <value>A value in pixels.</value>
        public int Top { get; }

        internal Glyph(GlyphInfo info, int index, int x, int y, int width, int height, int left, int top)
        {
            Info = info;
            Index = index;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Left = left;
            Top = top;
        }
    }
}
