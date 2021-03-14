using System;

namespace NStuff.GraphicsBackend
{
    /// <summary>
    /// Represents a 2D transformation matrix.
    /// </summary>
    public struct AffineTransform
    {
        /// <summary>
        /// Compares two <see cref="AffineTransform"/> objects. 
        /// </summary>
        /// <param name="transform1">An affine transform.</param>
        /// <param name="transform2">An affine transform.</param>
        /// <returns><c>true</c> if all the properties of both objects are identical.</returns>
        public static bool operator ==(AffineTransform transform1, AffineTransform transform2) => transform1.Equals(transform2);

        /// <summary>
        /// Compares two <see cref="AffineTransform"/> objects. 
        /// </summary>
        /// <param name="transform1">An affine transform.</param>
        /// <param name="transform2">An affine transform.</param>
        /// <returns><c>true</c> if all the properties of both objects are not identical.</returns>
        public static bool operator !=(AffineTransform transform1, AffineTransform transform2) => !transform1.Equals(transform2);

        /// <summary>
        /// Gets the first element of the first row.
        /// </summary>
        /// <value>The value of row 1, column 1.</value>
        public double M11 { get; }

        /// <summary>
        /// Gets the second element of the first row.
        /// </summary>
        /// <value>The value of row 1, column 2.</value>
        public double M12 { get; }

        /// <summary>
        /// Gets the first element of the second row.
        /// </summary>
        /// <value>The value of row 2, column 1.</value>
        public double M21 { get; }

        /// <summary>
        /// Gets the second element of the second row.
        /// </summary>
        /// <value>The value of row 2, column 2.</value>
        public double M22 { get; }

        /// <summary>
        /// Gets the first element of the third row.
        /// </summary>
        /// <value>The value of row 3, column 1.</value>
        public double M31 { get; }

        /// <summary>
        /// Gets the second element of the third row.
        /// </summary>
        /// <value>The value of row 3, column 2.</value>
        public double M32 { get; }

        /// <summary>
        /// Initializes a new intance of the <see cref="AffineTransform"/> struct using the supplied elements.
        /// </summary>
        /// <param name="m11">The first element of the first row.</param>
        /// <param name="m12">The second element of the first row.</param>
        /// <param name="m21">The second element of the second row.</param>
        /// <param name="m22">The second element of the second row.</param>
        /// <param name="m31">The first element of the third row.</param>
        /// <param name="m32">The second element of the third row.</param>
        public AffineTransform(double m11 = 0, double m12 = 0, double m21 = 0, double m22 = 0, double m31 = 0, double m32 = 0)
        {
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
            M31 = m31;
            M32 = m32;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override readonly int GetHashCode() => HashCode.Combine(M11, M12, M21, M22, M31, M32);

        /// <summary>
        /// Compares this object with <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns><c>true</c> if the supplied object is equal to this instance.</returns>
        public override readonly bool Equals(object? obj) => obj is AffineTransform transform && Equals(transform);

        /// <summary>
        /// Compares this <see cref="AffineTransform"/> object with another one.
        /// </summary>
        /// <param name="other">An affine transform.</param>
        /// <returns><c>true</c> if all the properties of this object are identical to the properties of <paramref name="other"/>.</returns>
        public readonly bool Equals(AffineTransform other) =>
            M11 == other.M11 && M12 == other.M12 && M21 == other.M21 && M22 == other.M22 && M31 == other.M31 && M32 == other.M32;
    }
}
