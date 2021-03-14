using System;

namespace NStuff.GraphicsBackend
{
    /// <summary>
    /// Represents a non-textured polygon point.
    /// </summary>
    public struct PointCoordinates
    {
        /// <summary>
        /// Compares two <see cref="PointCoordinates"/> objects. 
        /// </summary>
        /// <param name="coordinates1">Coordinates.</param>
        /// <param name="coordinates2">Coordinates.</param>
        /// <returns><c>true</c> if all the properties of both objects are identical.</returns>
        public static bool operator ==(PointCoordinates coordinates1, PointCoordinates coordinates2) => coordinates1.Equals(coordinates2);

        /// <summary>
        /// Compares two <see cref="PointCoordinates"/> objects. 
        /// </summary>
        /// <param name="coordinates1">Coordinates.</param>
        /// <param name="coordinates2">Coordinates.</param>
        /// <returns><c>true</c> if all the properties of both objects are not identical.</returns>
        public static bool operator !=(PointCoordinates coordinates1, PointCoordinates coordinates2) => !coordinates1.Equals(coordinates2);

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
        /// Initializes a new instance of the <see cref="PointCoordinates"/> struct using the supplied coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate of the point.</param>
        /// <param name="y">The y-coordinate of the point.</param>
        public PointCoordinates(double x, double y)
        {
            X = x;
            Y = y;
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
        public override readonly bool Equals(object? obj) => obj is PointCoordinates coordinates && Equals(coordinates);

        /// <summary>
        /// Compares this <see cref="PointCoordinates"/> object with another one.
        /// </summary>
        /// <param name="other">Coordinates.</param>
        /// <returns><c>true</c> if all the properties of this object are identical to the properties of <paramref name="other"/>.</returns>
        public readonly bool Equals(PointCoordinates other) => X == other.X && Y == other.Y;
    }
}
