using System;

namespace NStuff.Typography.Font
{
    /// <summary>
    /// Indicates information concerning the nature of the font patterns.
    /// </summary>
    [Flags]
    public enum MacStyles
    {
        /// <summary>
        /// Glyphs are emboldened.
        /// </summary>
        Bold = 0x01,

        /// <summary>
        /// Font contains italic or oblique glyphs.
        /// </summary>
        Italic = 0x02,

        /// <summary>
        /// Glyphs are underscored.
        /// </summary>
        Underline = 0x04,

        /// <summary>
        /// Outline (hollow) glyphs, otherwise they are solid.
        /// </summary>
        Outline = 0x08,

        /// <summary>
        /// Glyphs are shadowed.
        /// </summary>
        Shadow = 0x10,

        /// <summary>
        /// Glyphs are condensed.
        /// </summary>
        Condensed = 0x20,

        /// <summary>
        /// Glyphs are extended.
        /// </summary>
        Extended = 0x40,
    }
}
