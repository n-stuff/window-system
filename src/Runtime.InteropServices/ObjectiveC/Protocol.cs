using System;

using static NStuff.Runtime.InteropServices.ObjectiveC.NativeMethods;

namespace NStuff.Runtime.InteropServices.ObjectiveC
{
    /// <summary>
    /// Represents an Objective C runtime protocol.
    /// </summary>
    public struct Protocol
    {
        /// <summary>
        /// The internal handle of the protocol.
        /// </summary>
        public IntPtr Handle { get; }

        /// <summary>
        /// Gets an existing protocol using the supplied name.
        /// </summary>
        /// <param name="name">The name of the protocol.</param>
        /// <returns>A <c>Protocol</c> struct representing an existing protocol.</returns>
        public static Protocol Get(string name)
        {
            var result = new Protocol(objc_getProtocol(name));
            if (result.Handle == IntPtr.Zero)
            {
                throw new ArgumentException(Resources.FormatMessage(Resources.Key.UndefinedProtocol, name));
            }
            return result;
        }

        private Protocol(IntPtr handle) => Handle = handle;
    }
}
