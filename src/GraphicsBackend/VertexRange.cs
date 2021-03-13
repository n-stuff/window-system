using System;

namespace NStuff.GraphicsBackend
{
    /// <summary>
    /// Represents a range of vertex used by the indirect drawing command.
    /// </summary>
    public struct VertexRange
    {
        /// <summary>
        /// Compares two <see cref="VertexRange"/> objects. 
        /// </summary>
        /// <param name="range1">A range.</param>
        /// <param name="range2">A range.</param>
        /// <returns><c>true</c> if all the properties of both objects are identical.</returns>
        public static bool operator ==(VertexRange range1, VertexRange range2) => range1.Equals(range2);

        /// <summary>
        /// Compares two <see cref="VertexRange"/> objects. 
        /// </summary>
        /// <param name="range1">A range.</param>
        /// <param name="range2">A range.</param>
        /// <returns><c>true</c> if all the properties of both objects are not identical.</returns>
        public static bool operator !=(VertexRange range1, VertexRange range2) => !range1.Equals(range2);

        /// <summary>
        /// Gets the offset of the first vertex in a vertex buffer.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Gets the number of vertices in a vertex buffer.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexRange"/> struct using the supplied offset and count.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public VertexRange(int offset, int count)
        {
            Offset = offset;
            Count = count;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override readonly int GetHashCode() => HashCode.Combine(Offset, Count);

        /// <summary>
        /// Compares this object with <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns><c>true</c> if the supplied object is equal to this instance.</returns>
        public override readonly bool Equals(object? obj) => obj is VertexRange && Equals((VertexRange)obj);

        /// <summary>
        /// Compares this <see cref="VertexRange"/> object with another one.
        /// </summary>
        /// <param name="other">A range.</param>
        /// <returns><c>true</c> if all the properties of this object are identical to the properties of <paramref name="other"/>.</returns>
        public readonly bool Equals(VertexRange other) => Offset == other.Offset && Count == other.Count;
    }
}
