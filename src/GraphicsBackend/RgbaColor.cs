﻿using System;

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
        /// <returns><c>true</c> if both color have the same components.</returns>
        public static bool operator ==(RgbaColor color1, RgbaColor color2) => color1.Equals(color2);

        /// <summary>
        /// Compares two <see cref="RgbaColor"/> objects. 
        /// </summary>
        /// <param name="color1">A color.</param>
        /// <param name="color2">A color.</param>
        /// <returns><c>true</c> if colors have different components.</returns>
        public static bool operator !=(RgbaColor color1, RgbaColor color2) => !color1.Equals(color2);

        /// <summary>
        /// The red component of this color.
        /// </summary>
        public byte Red { get; }

        /// <summary>
        /// The green component of this color.
        /// </summary>
        public byte Green { get; }

        /// <summary>
        /// The blue component of this color.
        /// </summary>
        public byte Blue { get; }

        /// <summary>
        /// The alpha component of this color.
        /// </summary>
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
        public override readonly bool Equals(object obj) => obj is RgbaColor && Equals((RgbaColor)obj);

        /// <summary>
        /// Compares this <see cref="RgbaColor"/> object with another one.
        /// </summary>
        /// <param name="other">A color.</param>
        /// <returns><c>true</c> if all components have the same values.</returns>
        public readonly bool Equals(RgbaColor other) => Red == other.Red && Green == other.Green && Blue == other.Blue && Alpha == other.Alpha;
    }
}