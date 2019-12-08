using System;

namespace NStuff.Typography.Font
{
    /// <summary>
    /// A font subfamily is a combination of style, weight, and width.
    /// </summary>
    public struct FontSubfamily : IEquatable<FontSubfamily>
    {
        /// <summary>
        /// Compares two <see cref="FontSubfamily"/> objects. 
        /// </summary>
        /// <param name="subfamily1">A font subfamily.</param>
        /// <param name="subfamily2">A font subfamily.</param>
        /// <returns><c>true</c> if all the properties of both objects are identical.</returns>
        public static bool operator ==(FontSubfamily subfamily1, FontSubfamily subfamily2) => subfamily1.Equals(subfamily2);

        /// <summary>
        /// Compares two <see cref="FontSubfamily"/> objects. 
        /// </summary>
        /// <param name="subfamily1">A font subfamily.</param>
        /// <param name="subfamily2">A font subfamily.</param>
        /// <returns><c>true</c> if all the properties of both objects are not identical.</returns>
        public static bool operator !=(FontSubfamily subfamily1, FontSubfamily subfamily2) => !subfamily1.Equals(subfamily2);

        /// <summary>
        /// The normal font subfamily.
        /// </summary>
        /// <value>A subfamily with properties set to represent a normal one.</value>
        public static FontSubfamily Normal => new FontSubfamily(StyleClass.Normal, WeightClass.Normal, WidthClass.Normal);

        /// <summary>
        /// The bold font subfamily.
        /// </summary>
        /// <value>A subfamily with properties set to represent a bold one.</value>
        public static FontSubfamily Bold => new FontSubfamily(StyleClass.Normal, WeightClass.Bold, WidthClass.Normal);

        /// <summary>
        /// The bold italic subfont family.
        /// </summary>
        /// <value>A subfamily with properties set to represent a bold italic one.</value>
        public static FontSubfamily BoldItalic => new FontSubfamily(StyleClass.Italic, WeightClass.Bold, WidthClass.Normal);

        /// <summary>
        /// The italic font subfamily.
        /// </summary>
        /// <value>A subfamily with properties set to represent an italic one.</value>
        public static FontSubfamily Italic => new FontSubfamily(StyleClass.Italic, WeightClass.Normal, WidthClass.Normal);

        /// <summary>
        /// The style of this subfamily.
        /// </summary>
        /// <value>One of the values that indicates a style.</value>
        public StyleClass Style { get; }

        /// <summary>
        /// The weight of this subfamily.
        /// </summary>
        /// <value>One of the values that indicates a weight.</value>
        public WeightClass Weight { get; }

        /// <summary>
        /// The width of this subfamily.
        /// </summary>
        /// <value>One of the values that indicates a width.</value>
        public WidthClass Width { get; }

        internal FontSubfamily(StyleClass style, WeightClass weight, WidthClass width)
        {
            Style = style;
            Weight = weight;
            Width = width;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override readonly int GetHashCode() => HashCode.Combine(Style, Weight, Width);

        /// <summary>
        /// Compares this object with <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns><c>true</c> if the supplied object is equal to this instance.</returns>
        public override readonly bool Equals(object obj) => obj is FontSubfamily && Equals((FontSubfamily)obj);

        /// <summary>
        /// Compares this <see cref="FontSubfamily"/> object with another one.
        /// </summary>
        /// <param name="other">An font subfamily.</param>
        /// <returns><c>true</c> if all properties of this object are the same as the properties of <paramref name="other"/>.</returns>
        public readonly bool Equals(FontSubfamily other) => Style == other.Style && Weight == other.Weight && Width == other.Width;
    }
}
