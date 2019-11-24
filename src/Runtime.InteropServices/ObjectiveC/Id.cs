using System;

namespace NStuff.Runtime.InteropServices.ObjectiveC
{
    /// <summary>
    /// Represents an Objective C instance.
    /// </summary>
    public struct Id : IReceiver
    {
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
    }
}
