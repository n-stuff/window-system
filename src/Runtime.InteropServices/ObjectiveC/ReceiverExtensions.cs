using System;

using static NStuff.Runtime.InteropServices.ObjectiveC.NativeMethods;

namespace NStuff.Runtime.InteropServices.ObjectiveC
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
        /// <returns>An <see cref="Id"/>.</returns>
        public static Id Get(this IReceiver receiver, SEL selector) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">A <see cref="bool"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        public static Id Get(this IReceiver receiver, SEL selector, bool arg0) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, (sbyte)(arg0 ? 1 : 0)));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="IntPtr"/>.</param>
        /// <param name="arg1">An <see cref="IntPtr"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        public static Id Get(this IReceiver receiver, SEL selector, IntPtr arg0, IntPtr arg1) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0, arg1));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="IntPtr"/>.</param>
        /// <param name="arg1">A <see cref="ulong"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        [CLSCompliant(false)]
        public static Id Get(this IReceiver receiver, SEL selector, IntPtr arg0, ulong arg1) =>
            Get(receiver, selector, arg0, new IntPtr((long)arg1));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="IntPtr"/>.</param>
        /// <param name="arg1">An <see cref="IntPtr"/>.</param>
        /// <param name="arg2">An <see cref="IntPtr"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        public static Id Get(this IReceiver receiver, SEL selector, IntPtr arg0, IntPtr arg1, IntPtr arg2) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0, arg1, arg2));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="IntPtr"/>.</param>
        /// <param name="arg1">An <see cref="IntPtr"/>.</param>
        /// <param name="arg2">An <see cref="IntPtr"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        [CLSCompliant(false)]
        public static Id Get(this IReceiver receiver, SEL selector, IntPtr arg0, IntPtr arg1, ulong arg2) =>
            Get(receiver, selector, arg0, arg1, new IntPtr((long)arg2));


        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">A <see cref="double"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        public static Id Get(this IReceiver receiver, SEL selector, double arg0) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="Id"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        public static Id Get(this IReceiver receiver, SEL selector, Id arg0) => Get(receiver, selector, arg0.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="Id"/>.</param>
        /// <param name="arg1">An <see cref="Id"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        public static Id Get(this IReceiver receiver, SEL selector, Id arg0, Id arg1) => Get(receiver, selector, arg0.Handle, arg1.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="Id"/>.</param>
        /// <param name="arg1">A <see cref="SEL"/>.</param>
        /// <param name="arg2">An <see cref="Id"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        public static Id Get(this IReceiver receiver, SEL selector, Id arg0, SEL arg1, Id arg2) =>
            Get(receiver, selector, arg0.Handle, arg1.Handle, arg2.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="IntPtr"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        public static Id Get(this IReceiver receiver, SEL selector, IntPtr arg0) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="IntPtr"/>.</param>
        /// <param name="arg1">An <see cref="IntPtr"/>.</param>
        /// <param name="arg2">An <see cref="IntPtr"/>.</param>
        /// <param name="arg3">An <see cref="IntPtr"/>.</param>
        /// <param name="arg4">An <see cref="IntPtr"/>.</param>
        /// <param name="arg5">A <see cref="bool"/>.</param>
        /// <param name="arg6">A <see cref="bool"/>.</param>
        /// <param name="arg7">An <see cref="IntPtr"/>.</param>
        /// <param name="arg8">An <see cref="IntPtr"/>.</param>
        /// <param name="arg9">An <see cref="IntPtr"/>.</param>
        /// <param name="arg10">An <see cref="IntPtr"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        public static Id Get(this IReceiver receiver, SEL selector, IntPtr arg0, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4,
            bool arg5, bool arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0, arg1, arg2, arg3, arg4, (sbyte)(arg5 ? 1 : 0),
                (sbyte)(arg6 ? 1 : 0), arg7, arg8, arg9, arg10));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="IntPtr"/>.</param>
        /// <param name="arg1">A <see cref="long"/>.</param>
        /// <param name="arg2">A <see cref="long"/>.</param>
        /// <param name="arg3">A <see cref="long"/>.</param>
        /// <param name="arg4">A <see cref="long"/>.</param>
        /// <param name="arg5">A <see cref="bool"/>.</param>
        /// <param name="arg6">A <see cref="bool"/>.</param>
        /// <param name="arg7">An <see cref="Id"/>.</param>
        /// <param name="arg8">A <see cref="ulong"/>.</param>
        /// <param name="arg9">A <see cref="long"/>.</param>
        /// <param name="arg10">A <see cref="long"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        [CLSCompliant(false)]
        public static Id Get(this IReceiver receiver, SEL selector, IntPtr arg0, long arg1, long arg2, long arg3, long arg4,
            bool arg5, bool arg6, Id arg7, ulong arg8, long arg9, long arg10) =>
            Get(receiver, selector, arg0, new IntPtr(arg1), new IntPtr(arg2), new IntPtr(arg3), new IntPtr(arg4),
                arg5, arg6, arg7.Handle, new IntPtr((long)arg8), new IntPtr(arg9), new IntPtr(arg10));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">A <see cref="long"/>.</param>
        /// <param name="arg1">An <see cref="Id"/>.</param>
        /// <param name="arg2">An <see cref="Id"/>.</param>
        /// <param name="arg3">A <see cref="bool"/>.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        [CLSCompliant(false)]
        public static Id Get(this IReceiver receiver, SEL selector, ulong arg0, Id arg1, Id arg2, bool arg3) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, new IntPtr((long)arg0),
                arg1.Handle, arg2.Handle, (sbyte)(arg3 ? 1 : 0)));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>A <see cref="bool"/>.</returns>
        public static bool GetBool(this IReceiver receiver, SEL selector) =>
            SByte_objc_msgSend(receiver.Handle, selector.Handle) != 0;

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="Id"/>.</param>
        /// <returns>A <see cref="bool"/>.</returns>
        public static bool GetBool(this IReceiver receiver, SEL selector, Id arg0) =>
            SByte_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle) != 0;

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>A <see cref="double"/>.</returns>
        public static double GetDouble(this IReceiver receiver, SEL selector) =>
            Double_objc_msgSend(receiver.Handle, selector.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>A <see cref="long"/>.</returns>
        public static long GetInteger(this IReceiver receiver, SEL selector) =>
            IntPtr_objc_msgSend(receiver.Handle, selector.Handle).ToInt64();

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="Id"/>.</param>
        /// <param name="arg1">An <see cref="Id"/>.</param>
        /// <returns>A <see cref="long"/>.</returns>
        public static long GetInteger(this IReceiver receiver, SEL selector, Id arg0, Id arg1) =>
            IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle, arg1.Handle).ToInt64();

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>A <see cref="ulong"/>.</returns>
        [CLSCompliant(false)]
        public static ulong GetUInteger(this IReceiver receiver, SEL selector) =>
            (ulong)IntPtr_objc_msgSend(receiver.Handle, selector.Handle).ToInt64();

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>An <see cref="IntPtr"/>.</returns>
        public static IntPtr GetIntPtr(this IReceiver receiver, SEL selector) =>
            IntPtr_objc_msgSend(receiver.Handle, selector.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>A <see cref="short"/>.</returns>
        public static short GetShort(this IReceiver receiver, SEL selector) =>
            (short)UShort_objc_msgSend(receiver.Handle, selector.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>A <see cref="ushort"/>.</returns>
        [CLSCompliant(false)]
        public static ushort GetUShort(this IReceiver receiver, SEL selector) =>
            UShort_objc_msgSend(receiver.Handle, selector.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        public static void Send(this IReceiver receiver, SEL selector) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="Id"/>.</param>
        public static void Send(this IReceiver receiver, SEL selector, Id arg0) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="Id"/>.</param>
        /// <param name="arg1">A bool.</param>
        public static void Send(this IReceiver receiver, SEL selector, Id arg0, bool arg1) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle, (sbyte)(arg1 ? 1 : 0));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="Id"/>.</param>
        /// <param name="arg1">An <see cref="Id"/>.</param>
        public static void Send(this IReceiver receiver, SEL selector, Id arg0, Id arg1) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle, arg1.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="Id"/>.</param>
        /// <param name="arg1">A <see cref="ulong"/>.</param>
        [CLSCompliant(false)]
        public static void Send(this IReceiver receiver, SEL selector, IntPtr arg0, ulong arg1) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0, new IntPtr((long)arg1));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">A <see cref="SEL"/>.</param>
        /// <param name="arg1">An <see cref="Id"/>.</param>
        /// <param name="arg2">An <see cref="Id"/>.</param>
        public static void Send(this IReceiver receiver, SEL selector, SEL arg0, Id arg1, Id arg2) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle, arg1.Handle, arg2.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">A <see cref="bool"/>.</param>
        public static void Send(this IReceiver receiver, SEL selector, bool arg0) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, (sbyte)(arg0 ? 1 : 0));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">A <see cref="long"/>.</param>
        public static void Send(this IReceiver receiver, SEL selector, long arg0) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, new IntPtr(arg0));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">A <see cref="ulong"/>.</param>
        [CLSCompliant(false)]
        public static void Send(this IReceiver receiver, SEL selector, ulong arg0) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, new IntPtr((long)arg0));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">A <see cref="double"/>.</param>
        public static void Send(this IReceiver receiver, SEL selector, double arg0) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="superClass">The super class holding the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        public static void SuperSend(this IReceiver receiver, Class superClass, SEL selector)
        {
            var super = new objc_super(receiver, superClass);
            Void_objc_msgSendSuper(ref super, selector.Handle);
        }

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="superClass">The super class holding the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0">An <see cref="Id"/>.</param>
        public static void SuperSend(this IReceiver receiver, Class superClass, SEL selector, Id arg0)
        {
            var super = new objc_super(receiver, superClass);
            Void_objc_msgSendSuper(ref super, selector.Handle, arg0.Handle);
        }

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="superClass">The super class holding the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        public static Id SuperGet(this IReceiver receiver, Class superClass, SEL selector)
        {
            var super = new objc_super(receiver, superClass);
            return new Id(IntPtr_objc_msgSendSuper(ref super, selector.Handle));
        }
    }
}
