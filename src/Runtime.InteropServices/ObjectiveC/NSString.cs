using System;
using System.Runtime.InteropServices;

using static NStuff.Runtime.InteropServices.ObjectiveC.NativeMethods;
using static NStuff.Runtime.InteropServices.ObjectiveC.Selectors;

namespace NStuff.Runtime.InteropServices.ObjectiveC
{
    /// <summary>
    /// Represents an Objective C object of class <c>NSString</c>.
    /// </summary>
    public struct NSString : IReceiver
    {
        /// <summary>
        /// Converts a <c>NSString</c> struct to an <see cref="Id"/> struct.
        /// </summary>
        /// <param name="s">The <c>NSString</c> to convert.</param>
        public static implicit operator Id(NSString s) => new Id(s.Handle);

        private static Class @class;
        private static NSString empty;

        /// <summary>
        /// The class of this object.
        /// </summary>
        public static Class Class => (@class.Handle == IntPtr.Zero) ? @class = Class.Lookup("NSString") : @class;

        /// <summary>
        /// The empty string.
        /// </summary>
        public static NSString Empty => (empty.Handle == IntPtr.Zero) ? empty = new NSString(Class.Get(@string).Handle) : empty;

        /// <summary>
        /// Gets the native handle of the class.
        /// </summary>
        public IntPtr Handle { get; }

        /// <summary>
        /// Initializes a <c>NSString</c> struct using the supplied <paramref name="handle"/>.
        /// </summary>
        /// <param name="handle">The native handle of the string.</param>
        public NSString(IntPtr handle) => Handle = handle;

        /// <summary>
        /// Initializes a <c>NSString</c> struct using the supplied <paramref name="value"/>.
        /// </summary>
        /// <param name="value">A managed string.</param>
        public unsafe NSString(string value)
        {
            fixed (char* p = value)
            {
                Handle = IntPtr_objc_msgSend(Class.Handle, stringWithCharacters_length_.Handle, new IntPtr(p), new IntPtr(value.Length));
            }
        }

        /// <summary>
        /// Converts the Objective C string to a managed string.
        /// </summary>
        /// <returns>A managed string.</returns>
        public override string ToString() => Marshal.PtrToStringAnsi(IntPtr_objc_msgSend(Handle, UTF8String.Handle));
    }
}
