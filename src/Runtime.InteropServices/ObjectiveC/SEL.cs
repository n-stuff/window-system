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
        /// <returns><c>true</c> if <see cref="Handle"/> has the same value.</returns>
        public static bool operator ==(SEL selector1, SEL selector2) => selector1.Equals(selector2);

        /// <summary>
        /// Compares two <see cref="SEL"/> objects. 
        /// </summary>
        /// <param name="selector1">A selector.</param>
        /// <param name="selector2">A selector.</param>
        /// <returns><c>true</c> if <see cref="Handle"/> has different values.</returns>
        public static bool operator !=(SEL selector1, SEL selector2) => !selector1.Equals(selector2);

        /// <summary>
        /// The nil selector.
        /// </summary>
        public static readonly SEL Nil = new SEL();

        /// <summary>
        /// The internal handle of the selector.
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
        /// <param name="handle">The internal handle of the selector.</param>
        public SEL(IntPtr handle) => Handle = handle;

        public override readonly int GetHashCode() => Handle.GetHashCode();

        public override readonly bool Equals(object obj) => obj is SEL && Equals((SEL)obj);

        /// <summary>
        /// Compares this <see cref="SEL"/> object with another one.
        /// </summary>
        /// <param name="other">A selector.</param>
        /// <returns><c>true</c> if <see cref="Handle"/> has the same value as <c>other.Handle</c>.</returns>
        public readonly bool Equals(SEL other) => Handle == other.Handle;
    }
}
