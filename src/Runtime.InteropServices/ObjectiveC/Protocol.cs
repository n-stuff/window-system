﻿using System;

using static NStuff.Runtime.InteropServices.ObjectiveC.NativeMethods;

namespace NStuff.Runtime.InteropServices.ObjectiveC
{
    /// <summary>
    /// Represents an Objective C runtime protocol.
    /// </summary>
    public struct Protocol : IEquatable<Protocol>
    {
        /// <summary>
        /// Compares two <see cref="Protocol"/> objects. 
        /// </summary>
        /// <param name="protocol1">A protocol.</param>
        /// <param name="protocol2">A protocol.</param>
        /// <returns><c>true</c> if all the properties of both objects are identical.</returns>
        public static bool operator ==(Protocol protocol1, Protocol protocol2) => protocol1.Equals(protocol2);

        /// <summary>
        /// Compares two <see cref="Protocol"/> objects. 
        /// </summary>
        /// <param name="protocol1">A protocol.</param>
        /// <param name="protocol2">A protocol.</param>
        /// <returns><c>true</c> if all the properties of both objects are not identical.</returns>
        public static bool operator !=(Protocol protocol1, Protocol protocol2) => !protocol1.Equals(protocol2);

        /// <summary>
        /// Gets the native handle of the class.
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
        public override readonly bool Equals(object? obj) => obj is Protocol protocol && Equals(protocol);

        /// <summary>
        /// Compares this <see cref="Protocol"/> object with another one.
        /// </summary>
        /// <param name="other">A ^protocol.</param>
        /// <returns><c>true</c> if <see cref="Handle"/> has the same value as <c>other.Handle</c>.</returns>
        public readonly bool Equals(Protocol other) => Handle == other.Handle;
    }
}
