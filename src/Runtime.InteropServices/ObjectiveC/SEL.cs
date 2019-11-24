using System;

using static NStuff.Runtime.InteropServices.ObjectiveC.NativeMethods;

namespace NStuff.Runtime.InteropServices.ObjectiveC
{
    /// <summary>
    /// Represents an Objective C runtime selector.
    /// </summary>
    public struct SEL
    {
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
    }
}
