using System;

namespace NStuff.GraphicsBackend
{
    /// <summary>
    /// Represents a textured polygon point.
    /// </summary>
    public struct PointAndImageCoordinates
    {
        /// <summary>
        /// Compares two <see cref="PointAndImageCoordinates"/> objects. 
        /// </summary>
        /// <param name="coordinates1">Coordinates.</param>
        /// <param name="coordinates2">Coordinates.</param>
        /// <returns><c>true</c> if all the properties of both objects are identical.</returns>
        public static bool operator ==(PointAndImageCoordinates coordinates1, PointAndImageCoordinates coordinates2) =>
            coordinates1.Equals(coordinates2);

        /// <summary>
        /// Compares two <see cref="PointAndImageCoordinates"/> objects. 
        /// </summary>
        /// <param name="coordinates1">Coordinates.</param>
        /// <param name="coordinates2">Coordinates.</param>
        /// <returns><c>true</c> if all the properties of both objects are not identical.</returns>
        public static bool operator !=(PointAndImageCoordinates coordinates1, PointAndImageCoordinates coordinates2) =>
            !coordinates1.Equals(coordinates2);

        /// <summary>
        /// Gets the x-coordinate of the point.
        /// </summary>
        /// <value>A x-coordinate in pixels.</value>
        public double X { get; }

        /// <summary>
        /// Gets the y-coordinate of the point.
        /// </summary>
        /// <value>A y-coordinate in pixels.</value>
        public double Y { get; }

        /// <summary>
        /// Gets the x ratio of the point in an image.
        /// </summary>
        /// <value>A value between <c>0</c> and <c>1</c>.</value>
        public double XImage { get; }

        /// <summary>
        /// Gets the y ratio of the point in an image.
        /// </summary>
        /// <value>A value between <c>0</c> and <c>1</c>.</value>
        public double YImage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointAndImageCoordinates"/> struct using the supplied coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        /// <param name="xImage">The x-coordinate of the point in an image.</param>
        /// <param name="yImage">The y-coordinate of the point in an image.</param>
        public PointAndImageCoordinates(double x, double y, double xImage, double yImage)
        {
            X = x;
            Y = y;
            XImage = xImage;
            YImage = yImage;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override readonly int GetHashCode() => HashCode.Combine(X, Y);

        /// <summary>
        /// Compares this object with <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns><c>true</c> if the supplied object is equal to this instance.</returns>
        public override readonly bool Equals(object obj) => obj is PointAndImageCoordinates && Equals((PointAndImageCoordinates)obj);

        /// <summary>
        /// Compares this <see cref="PointAndImageCoordinates"/> object with another one.
        /// </summary>
        /// <param name="other">Coordinates.</param>
        /// <returns><c>true</c> if all the properties of this object are identical to the properties of <paramref name="other"/>.</returns>
        public readonly bool Equals(PointAndImageCoordinates other) =>
            X == other.X && Y == other.Y && XImage == other.XImage && YImage == other.YImage;
    }
}
