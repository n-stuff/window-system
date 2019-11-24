using System;

using static NStuff.Runtime.InteropServices.ObjectiveC.NativeMethods;

namespace NStuff.Runtime.InteropServices.ObjectiveC
{
    /// <summary>
    /// Represents an Object C class.
    /// </summary>
    public struct Class : IReceiver
    {
        /// <summary>
        /// The internal handle of the class.
        /// </summary>
        public IntPtr Handle { get; }

        /// <summary>
        /// Loads an Objective C class.
        /// </summary>
        /// <param name="name">The name of the class.</param>
        /// <returns>A <c>Class</c> struct representing the existing class.</returns>
        public static Class Lookup(string name)
        {
            var result = new Class(objc_lookUpClass(name));
            if (result.Handle == IntPtr.Zero)
            {
                throw new ArgumentException(Resources.FormatMessage(Resources.Key.UndefinedClass, name));
            }
            return result;
        }

        /// <summary>
        /// Adds a new class to the Objective C runtime.
        /// </summary>
        /// <param name="name">The name of the class.</param>
        /// <param name="superClass">The superclass of the new class.</param>
        /// <param name="setup">An action invoked between the allocation of the class and its registration.</param>
        /// <returns>A <c>Class</c> struct representing the new class.</returns>
        public static Class Create(string name, Class superClass, Action<ClassBuilder> setup)
        {
            var result = new Class(objc_allocateClassPair(superClass.Handle, name, IntPtr.Zero));
            if (result.Handle == IntPtr.Zero)
            {
                throw new ArgumentException(Resources.FormatMessage(Resources.Key.ClassAllocationFailed, name));
            }
            setup(new ClassBuilder(result, superClass));
            objc_registerClassPair(result.Handle);
            return result;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Class</c> struct using the supplied <paramref name="handle"/>.
        /// </summary>
        /// <param name="handle">The internal handle of an Objective C class.</param>
        public Class(IntPtr handle) => Handle = handle;
    }
}
