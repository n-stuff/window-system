using NStuff.Typography.Font;
using System.Collections.Generic;

namespace NStuff.Typography.Typesetting
{
    /// <summary>
    /// Provides information about a font family / subfamily pair.
    /// </summary>
    public class FontInfo
    {
        /// <summary>
        /// Gets the family of the font.
        /// </summary>
        /// <value>The name of the font family.</value>
        public string Family { get; }

        /// <summary>
        /// Gets the subfamily of the font.
        /// </summary>
        /// <value>A <see cref="FontSubfamily"/> object.</value>
        public FontSubfamily Subfamily { get; }

        /// <summary>
        /// Gets the size of the font.
        /// </summary>
        /// <value>A number in design units.</value>
        public int UnitsPerEm { get; }

        /// <summary>
        /// Gets the cell ascent of the font.
        /// </summary>
        /// <value>A number in design units.</value>
        public int Ascent { get; }

        /// <summary>
        /// Gets the cell descent of the font.
        /// </summary>
        /// <value>A number in design units.</value>
        public int Descent { get; }

        /// <summary>
        /// Gets the recommended whitespece between two lines of text displayed using this font.
        /// </summary>
        /// <value>A number in design units.</value>
        public int LineGap { get; }

        internal Dictionary<uint, int> KerningAdvances { get; } = new();

        internal FontInfo(string family, FontSubfamily subfamily, int unitsPerEm, int ascent, int descent, int lineGap)
        {
            Family = family;
            Subfamily = subfamily;
            UnitsPerEm = unitsPerEm;
            Ascent = ascent;
            Descent = descent;
            LineGap = lineGap;
        }
    }
}
