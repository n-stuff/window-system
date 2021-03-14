using System;

namespace NStuff.GraphicsBackend
{
    /// <summary>
    /// Represents a handle to a <see cref="VertexRange"/> buffer allocated by the underlying graphics backend.
    /// </summary>
    public struct VertexRangeBufferHandle : IEquatable<VertexRangeBufferHandle>
    {
        /// <summary>
        /// Compares two <see cref="VertexRangeBufferHandle"/> objects. 
        /// </summary>
        /// <param name="buffer1">A vertex range buffer.</param>
        /// <param name="buffer2">A vertex range buffer.</param>
        /// <returns><c>true</c> if all the properties of both objects are identical.</returns>
        public static bool operator ==(VertexRangeBufferHandle buffer1, VertexRangeBufferHandle buffer2) => buffer1.Equals(buffer2);

        /// <summary>
        /// Compares two <see cref="VertexRangeBufferHandle"/> objects. 
        /// </summary>
        /// <param name="buffer1">A vertex range buffer.</param>
        /// <param name="buffer2">A vertex range buffer.</param>
        /// <returns><c>true</c> if all the properties of both objects are not identical.</returns>
        public static bool operator !=(VertexRangeBufferHandle buffer1, VertexRangeBufferHandle buffer2) => !buffer1.Equals(buffer2);

        /// <summary>
        /// Gets the value of this handle.
        /// </summary>
        /// <value>An internal ID.</value>
        public IntPtr Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexRangeBufferHandle"/> struct using the supplied value.
        /// </summary>
        /// <param name="value">The value of this handle.</param>
        public VertexRangeBufferHandle(IntPtr value) => Value = value;

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
        public override readonly bool Equals(object? obj) => obj is VertexRangeBufferHandle handle && Equals(handle);

        /// <summary>
        /// Compares this <see cref="VertexRangeBufferHandle"/> object with another one.
        /// </summary>
        /// <param name="other">A vertex range buffer.</param>
        /// <returns><c>true</c> if all the properties of this object are identical to the properties of <paramref name="other"/>.</returns>
        public readonly bool Equals(VertexRangeBufferHandle other) => Value == other.Value;
    }
}
