using System;

namespace NStuff.Typography.Font
{
    /// <summary>
    /// Indicates information concerning the nature of the font patterns.
    /// </summary>
    [Flags]
    public enum FsSelection
    {
        /// <summary>
        /// Font contains Italic glyphs, otherwise they are upright.
        /// </summary>
        Italic = 0x01,

        /// <summary>
        /// Glyphs are underscored.
        /// </summary>
        Underscore = 0x02,

        /// <summary>
        /// Glyphs have their foreground and background reversed.
        /// </summary>
        Negative = 0x04,

        /// <summary>
        /// Outline (hollow) glyphs, otherwise they are solid.
        /// </summary>
        Outlined = 0x08,

        /// <summary>
        /// Glyphs are overstruck.
        /// </summary>
        Strikeout = 0x10,

        /// <summary>
        /// Glyphs are emboldened.
        /// </summary>
        Bold = 0x20,

        /// <summary>
        /// Glyphs are in the standard weight/style for the font.
        /// </summary>
        Regular = 0x40
    }
}
