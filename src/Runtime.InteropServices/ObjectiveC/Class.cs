using System;

using static NStuff.Runtime.InteropServices.ObjectiveC.NativeMethods;

namespace NStuff.Runtime.InteropServices.ObjectiveC
{
    /// <summary>
    /// Represents an Objective C class.
    /// </summary>
    public struct Class : IReceiver, IEquatable<Class>
    {
        /// <summary>
        /// Compares two <see cref="Class"/> objects. 
        /// </summary>
        /// <param name="class1">A class.</param>
        /// <param name="class2">A class.</param>
        /// <returns><c>true</c> if all the properties of both objects are identical.</returns>
        public static bool operator ==(Class class1, Class class2) => class1.Equals(class2);

        /// <summary>
        /// Compares two <see cref="Class"/> objects. 
        /// </summary>
        /// <param name="class1">A class.</param>
        /// <param name="class2">A class.</param>
        /// <returns><c>true</c> if all the properties of both objects are not identical.</returns>
        public static bool operator !=(Class class1, Class class2) => !class1.Equals(class2);

        /// <summary>
        /// Gets the native handle of the class.
        /// </summary>
        public readonly IntPtr Handle { get; }

        /// <summary>
        /// Loads an Objective C class.
        /// </summary>
        /// <param name="name">The name of the class.</param>
        /// <returns>A <c>Class</c> struct representing the existing class.</returns>
        /// <exception cref="ArgumentException">If the supplied name is not the name of an existing class.</exception>
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
        /// <exception cref="ArgumentException">If the class could not be allocated.</exception>
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
        /// <param name="handle">The native handle of an Objective C class.</param>
        public Class(IntPtr handle) => Handle = handle;

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
        public override readonly bool Equals(object? obj) => obj is Class @class && Equals(@class);

        /// <summary>
        /// Compares this <see cref="Class"/> object with another one.
        /// </summary>
        /// <param name="other">A class.</param>
        /// <returns><c>true</c> if all the properties of this object are identical to the properties of <paramref name="other"/>.</returns>
        public readonly bool Equals(Class other) => Handle == other.Handle;
    }
}
