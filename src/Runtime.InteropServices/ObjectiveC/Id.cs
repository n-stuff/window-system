using System;

namespace NStuff.Runtime.InteropServices.ObjectiveC
{
    /// <summary>
    /// Represents an Objective C object.
    /// </summary>
    public struct Id : IReceiver, IEquatable<Id>
    {
        /// <summary>
        /// Compares two <see cref="Id"/> objects. 
        /// </summary>
        /// <param name="id1">An id.</param>
        /// <param name="id2">An id.</param>
        /// <returns><c>true</c> if <see cref="Handle"/> has the same value.</returns>
        public static bool operator ==(Id id1, Id id2) => id1.Equals(id2);

        /// <summary>
        /// Compares two <see cref="Id"/> objects. 
        /// </summary>
        /// <param name="id1">An id.</param>
        /// <param name="id2">An id.</param>
        /// <returns><c>true</c> if <see cref="Handle"/> has different values.</returns>
        public static bool operator !=(Id id1, Id id2) => !id1.Equals(id2);

        /// <summary>
        /// The nil id.
        /// </summary>
        public static readonly Id Nil = new Id();

        /// <summary>
        /// The internal handle of this id.
        /// </summary>
        public IntPtr Handle { get; }

        /// <summary>
        /// Initializes a new <c>Id</c> struct using the supplied <paramref name="handle"/>.
        /// </summary>
        /// <param name="handle">The internal handle of the id.</param>
        public Id(IntPtr handle) => Handle = handle;

        /// <summary>
        /// Whether the id is nil.
        /// </summary>
        public bool IsNil => Handle == IntPtr.Zero;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override readonly int GetHashCode() => Handle.GetHashCode();

        /// <summary>
        /// Compares this object with <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns><c>true</c> if the supplied object is equal to this instance.</returns>
        public override readonly bool Equals(object obj) => obj is Id && Equals((Id)obj);

        /// <summary>
        /// Compares this <see cref="Id"/> object with another one.
        /// </summary>
        /// <param name="other">An id.</param>
        /// <returns><c>true</c> if <see cref="Handle"/> has the same value as <c>other.Handle</c>.</returns>
        public readonly bool Equals(Id other) => Handle == other.Handle;
    }
}
