using System;

using static NStuff.Runtime.InteropServices.ObjectiveC.NativeMethods;

namespace NStuff.Runtime.InteropServices.ObjectiveC
{
    /// <summary>
    /// Represents an Objective C runtime selector.
    /// </summary>
    public struct SEL : IEquatable<SEL>
    {
        /// <summary>
        /// Compares two <see cref="SEL"/> objects. 
        /// </summary>
        /// <param name="selector1">A selector.</param>
        /// <param name="selector2">A selector.</param>
        /// <returns><c>true</c> if all the properties of both objects are identical.</returns>
        public static bool operator ==(SEL selector1, SEL selector2) => selector1.Equals(selector2);

        /// <summary>
        /// Compares two <see cref="SEL"/> objects. 
        /// </summary>
        /// <param name="selector1">A selector.</param>
        /// <param name="selector2">A selector.</param>
        /// <returns><c>true</c> if all the properties of both objects are not identical.</returns>
        public static bool operator !=(SEL selector1, SEL selector2) => !selector1.Equals(selector2);

        /// <summary>
        /// Gets the native handle of the class.
        /// </summary>
        public IntPtr Handle { get; }

        /// <summary>
        /// Registers a selector using the provided name.
        /// </summary>
        /// <param name="name">The name of the selector.</param>
        /// <returns>A <c>SEL</c> struct representing a selector.</returns>
        public static SEL Register(string name) => new SEL(sel_registerName(name));

        /// <summary>
        /// Initializes a new instance of the <c>SEL</c> struct using the provided <paramref name="handle"/>.
        /// </summary>
        /// <param name="handle">The native handle of the selector.</param>
        public SEL(IntPtr handle) => Handle = handle;

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
        public override readonly bool Equals(object obj) => obj is SEL && Equals((SEL)obj);

        /// <summary>
        /// Compares this <see cref="SEL"/> object with another one.
        /// </summary>
        /// <param name="other">A selector.</param>
        /// <returns><c>true</c> if <see cref="Handle"/> has the same value as <c>other.Handle</c>.</returns>
        public readonly bool Equals(SEL other) => Handle == other.Handle;
    }
}
