using System;

namespace NStuff.GraphicsBackend
{
    /// <summary>
    /// Represents a handle to a command buffer allocated by the underlying graphics backend.
    /// </summary>
    public struct CommandBufferHandle : IEquatable<CommandBufferHandle>
    {
        /// <summary>
        /// Compares two <see cref="CommandBufferHandle"/> objects. 
        /// </summary>
        /// <param name="buffer1">A command buffer.</param>
        /// <param name="buffer2">A command buffer.</param>
        /// <returns><c>true</c> if <see cref="Value"/> properties have the same value.</returns>
        public static bool operator ==(CommandBufferHandle buffer1, CommandBufferHandle buffer2) => buffer1.Equals(buffer2);

        /// <summary>
        /// Compares two <see cref="CommandBufferHandle"/> objects. 
        /// </summary>
        /// <param name="buffer1">A command buffer.</param>
        /// <param name="buffer2">A command buffer.</param>
        /// <returns><c>true</c> if <see cref="Value"/> properties have different values.</returns>
        public static bool operator !=(CommandBufferHandle buffer1, CommandBufferHandle buffer2) => !buffer1.Equals(buffer2);

        /// <summary>
        /// The value of this handle.
        /// </summary>
        public IntPtr Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBufferHandle"/> struct using the supplied value.
        /// </summary>
        /// <param name="value">The value of this handle.</param>
        public CommandBufferHandle(IntPtr value) => Value = value;

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override readonly int GetHashCode() => Value.GetHashCode();

        /// <summary>
        /// Compares this object with <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns><c>true</c> if the supplied object is equal to this instance.</returns>
        public override readonly bool Equals(object obj) => obj is CommandBufferHandle && Equals((CommandBufferHandle)obj);

        /// <summary>
        /// Compares this <see cref="CommandBufferHandle"/> object with another one.
        /// </summary>
        /// <param name="other">A command buffer.</param>
        /// <returns><c>true</c> if <see cref="Value"/> has the same value as <c>other.Value</c>.</returns>
        public readonly bool Equals(CommandBufferHandle other) => Value == other.Value;
    }
}
