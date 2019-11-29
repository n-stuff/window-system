using System;

using NStuff.Runtime.InteropServices.ObjectiveC;
using static NStuff.WindowSystem.macOS.NativeMethods;

namespace NStuff.WindowSystem.macOS
{
    /// <summary>
    /// Provides methods to send messages to an Objective C object.
    /// </summary>
    public static class ReceiverExtensions
    {
        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="Id"/>.</param>
        /// <param name="arg1">An <see cref="NSPoint"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        public static Id Get(this IReceiver receiver, SEL selector, Id arg0, NSPoint arg1) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle, arg1));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0"></param>
        /// <returns>An <see cref="Id"/>.</returns>
        public static Id Get(this IReceiver receiver, SEL selector, NSSize arg0) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="NSRect"/>.</param>
        /// <param name="arg1">A <see cref="ulong"/>.</param>
        /// <param name="arg2">An <see cref="Id"/>.</param>
        /// <param name="arg3">An <see cref="Id"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        internal static Id Get(this IReceiver receiver, SEL selector, NSRect arg0, ulong arg1, Id arg2, Id arg3) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0, new IntPtr((long)arg1), arg2.Handle, arg3.Handle));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="NSRect"/>.</param>
        /// <param name="arg1">A <see cref="ulong"/>.</param>
        /// <param name="arg2">A <see cref="ulong"/>.</param>
        /// <param name="arg3">A <see cref="bool"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        internal static Id Get(this IReceiver receiver, SEL selector, NSRect arg0, ulong arg1, ulong arg2, bool arg3) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0, new IntPtr((long)arg1),
                new IntPtr((long)arg2), (sbyte)(arg3 ? 1 : 0)));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">A <see cref="ulong"/>.</param>
        /// <param name="arg1">An <see cref="NSPoint"/>.</param>
        /// <param name="arg2">A <see cref="ulong"/>.</param>
        /// <param name="arg3">A <see cref="double"/>.</param>
        /// <param name="arg4">A <see cref="long"/>.</param>
        /// <param name="arg5">An <see cref="Id"/>.</param>
        /// <param name="arg6">A <see cref="short"/>.</param>
        /// <param name="arg7">A <see cref="long"/>.</param>
        /// <param name="arg8">A <see cref="long"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        [CLSCompliant(false)]
        public static Id Get(this IReceiver receiver, SEL selector, ulong arg0, NSPoint arg1, ulong arg2, double arg3, long arg4,
            Id arg5, short arg6, long arg7, long arg8) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0, arg1, arg2, arg3, arg4, arg5.Handle, arg6, arg7, arg8));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="NSPoint"/>.</param>
        /// <param name="arg1">An <see cref="NSRect"/>.</param>
        /// <returns>A <see cref="bool"/>.</returns>
        public static bool GetBool(this IReceiver receiver, SEL selector, NSPoint arg0, NSRect arg1) =>
            SByte_objc_msgSend(receiver.Handle, selector.Handle, arg0, arg1) != 0;

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>An <see cref="NSPoint"/>.</returns>
        public static NSPoint GetPoint(this IReceiver receiver, SEL selector) =>
            Point_objc_msgSend(receiver.Handle, selector.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="NSPoint"/>.</param>
        /// <returns>An <see cref="NSPoint"/>.</returns>
        public static NSPoint GetPoint(this IReceiver receiver, SEL selector, NSPoint arg0) =>
            Point_objc_msgSend(receiver.Handle, selector.Handle, arg0);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>An <see cref="NSRect"/>.</returns>
        public static NSRect GetRect(this IReceiver receiver, SEL selector) =>
            Rect_objc_msgSend(receiver.Handle, selector.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="NSRect"/>.</param>
        /// <returns>An <see cref="NSRect"/>.</returns>
        public static NSRect GetRect(this IReceiver receiver, SEL selector, NSRect arg0) =>
            Rect_objc_msgSend(receiver.Handle, selector.Handle, arg0);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>An <see cref="NSSize"/>.</returns>
        public static NSSize GetSize(this IReceiver receiver, SEL selector) =>
            Size_objc_msgSend(receiver.Handle, selector.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="NSPoint"/>.</param>
        public static void Send(this IReceiver receiver, SEL selector, NSPoint arg0) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="NSSize"/>.</param>
        public static void Send(this IReceiver receiver, SEL selector, NSSize arg0) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0);
    }
}
