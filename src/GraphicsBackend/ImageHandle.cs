using System;

namespace NStuff.GraphicsBackend
{
    /// <summary>
    /// Represents a handle to an image allocated by the underlying graphics backend.
    /// </summary>
    public struct ImageHandle : IEquatable<ImageHandle>
    {
        /// <summary>
        /// Compares two <see cref="ImageHandle"/> objects. 
        /// </summary>
        /// <param name="image1">An image handle.</param>
        /// <param name="image2">An image handle.</param>
        /// <returns><c>true</c> if all the properties of both objects are identical.</returns>
        public static bool operator ==(ImageHandle image1, ImageHandle image2) => image1.Equals(image2);

        /// <summary>
        /// Compares two <see cref="ImageHandle"/> objects. 
        /// </summary>
        /// <param name="image1">An image handle.</param>
        /// <param name="image2">An image handle.</param>
        /// <returns><c>true</c> if all the properties of both objects are not identical.</returns>
        public static bool operator !=(ImageHandle image1, ImageHandle image2) => !image1.Equals(image2);

        /// <summary>
        /// The value of this handle.
        /// </summary>
        public IntPtr Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageHandle"/> struct using the supplied value.
        /// </summary>
        /// <param name="value">The value of this handle.</param>
        public ImageHandle(IntPtr value) => Value = value;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override readonly int GetHashCode() => Value.GetHashCode();

        /// <summary>
        /// Compares this object with <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns><c>true</c> if the supplied object is equal to this instance.</returns>
        public override readonly bool Equals(object obj) => obj is ImageHandle && Equals((ImageHandle)obj);

        /// <summary>
        /// Compares this <see cref="ImageHandle"/> object with another one.
        /// </summary>
        /// <param name="other">An image.</param>
        /// <returns><c>true</c> if all the properties of this object are identical to the properties of <paramref name="other"/>.</returns>
        public readonly bool Equals(ImageHandle other) => Value == other.Value;
    }
}
