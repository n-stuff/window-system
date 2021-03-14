using System;

namespace NStuff.GraphicsBackend
{
    /// <summary>
    /// Represents a 8-bit components true color.
    /// </summary>
    public struct RgbaColor : IEquatable<RgbaColor>
    {
        /// <summary>
        /// Compares two <see cref="RgbaColor"/> objects. 
        /// </summary>
        /// <param name="color1">A color.</param>
        /// <param name="color2">A color.</param>
        /// <returns><c>true</c> if all the properties of both objects are identical.</returns>
        public static bool operator ==(RgbaColor color1, RgbaColor color2) => color1.Equals(color2);

        /// <summary>
        /// Compares two <see cref="RgbaColor"/> objects. 
        /// </summary>
        /// <param name="color1">A color.</param>
        /// <param name="color2">A color.</param>
        /// <returns><c>true</c> if all the properties of both objects are not identical.</returns>
        public static bool operator !=(RgbaColor color1, RgbaColor color2) => !color1.Equals(color2);

        /// <summary>
        /// Gets the red component of this color.
        /// </summary>
        /// <value>A value indicating the level of red in the color.</value>
        public byte Red { get; }

        /// <summary>
        /// Gets the green component of this color.
        /// </summary>
        /// <value>A value indicating the level of green in the color.</value>
        public byte Green { get; }

        /// <summary>
        /// Gets the blue component of this color.
        /// </summary>
        /// <value>A value indicating the level of blue in the color.</value>
        public byte Blue { get; }

        /// <summary>
        /// Gets the alpha component of this color.
        /// </summary>
        /// <value>A value indicating the level of opacity of the color.</value>
        public byte Alpha { get; }

        /// <summary>
        /// Initializes a new intance of the <see cref="RgbaColor"/> struct using the supplied components.
        /// </summary>
        /// <param name="red">The red component of the color.</param>
        /// <param name="green">The green component of the color.</param>
        /// <param name="blue">The blue component of the color.</param>
        /// <param name="alpha">The alpha component of the color.</param>
        public RgbaColor(byte red, byte green, byte blue, byte alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override readonly int GetHashCode() => HashCode.Combine(Red, Green, Blue, Alpha);

        /// <summary>
        /// Compares this object with <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns><c>true</c> if the supplied object is equal to this instance.</returns>
        public override readonly bool Equals(object? obj) => obj is RgbaColor color && Equals(color);

        /// <summary>
        /// Compares this <see cref="RgbaColor"/> object with another one.
        /// </summary>
        /// <param name="other">A color.</param>
        /// <returns><c>true</c> if all the properties of this object are identical to the properties of <paramref name="other"/>.</returns>
        public readonly bool Equals(RgbaColor other) => Red == other.Red && Green == other.Green && Blue == other.Blue && Alpha == other.Alpha;
    }
}
