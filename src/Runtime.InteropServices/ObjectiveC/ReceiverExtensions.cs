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
        /// <returns>An object.</returns>
        public static Id Get(this IReceiver receiver, SEL selector) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0"></param>
        /// <returns>An object.</returns>
        public static Id Get(this IReceiver receiver, SEL selector, double arg0) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0"></param>
        /// <returns>An object.</returns>
        public static Id Get(this IReceiver receiver, SEL selector, Id arg0) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <returns>An object.</returns>
        public static Id Get(this IReceiver receiver, SEL selector, Id arg0, Id arg1) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle, arg1.Handle));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>A bool.</returns>
        public static bool GetBool(this IReceiver receiver, SEL selector) =>
            SByte_objc_msgSend(receiver.Handle, selector.Handle) != 0;

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0"></param>
        /// <returns>A bool.</returns>
        public static bool GetBool(this IReceiver receiver, SEL selector, Id arg0) =>
            SByte_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle) != 0;

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>A double.</returns>
        public static double GetDouble(this IReceiver receiver, SEL selector) =>
            Double_objc_msgSend(receiver.Handle, selector.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>A long.</returns>
        public static long GetInteger(this IReceiver receiver, SEL selector) =>
            IntPtr_objc_msgSend(receiver.Handle, selector.Handle).ToInt64();

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <returns>A long.</returns>
        public static long GetInteger(this IReceiver receiver, SEL selector, Id arg0, Id arg1) =>
            IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle, arg1.Handle).ToInt64();

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>A ulong.</returns>
        [CLSCompliant(false)]
        public static ulong GetUInteger(this IReceiver receiver, SEL selector) =>
            (ulong)IntPtr_objc_msgSend(receiver.Handle, selector.Handle).ToInt64();

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>An IntPtr.</returns>
        public static IntPtr GetIntPtr(this IReceiver receiver, SEL selector) =>
            IntPtr_objc_msgSend(receiver.Handle, selector.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>A short.</returns>
        public static short GetShort(this IReceiver receiver, SEL selector) =>
            (short)UShort_objc_msgSend(receiver.Handle, selector.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <returns>A ushort.</returns>
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
        /// <param name="arg0"></param>
        public static void Send(this IReceiver receiver, SEL selector, Id arg0) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public static void Send(this IReceiver receiver, SEL selector, Id arg0, bool arg1) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle, (sbyte)(arg1 ? 1 : 0));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        public static void Send(this IReceiver receiver, SEL selector, Id arg0, Id arg1) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle, arg1.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public static void Send(this IReceiver receiver, SEL selector, SEL arg0, Id arg1, Id arg2) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle, arg1.Handle, arg2.Handle);

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0"></param>
        public static void Send(this IReceiver receiver, SEL selector, bool arg0) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, (sbyte)(arg0 ? 1 : 0));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0"></param>
        public static void Send(this IReceiver receiver, SEL selector, long arg0) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, new IntPtr(arg0));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0"></param>
        [CLSCompliant(false)]
        public static void Send(this IReceiver receiver, SEL selector, ulong arg0) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, new IntPtr((long)arg0));

        /// <summary>
        /// Sends a message to <paramref name="receiver"/>.
        /// </summary>
        /// <param name="receiver">The target of the message.</param>
        /// <param name="selector">The selector identifying the message.</param>
        /// <param name="arg0"></param>
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
        /// <param name="arg0"></param>
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
        /// <returns>An object.</returns>
        public static Id SuperGet(this IReceiver receiver, Class superClass, SEL selector)
        {
            var super = new objc_super(receiver, superClass);
            return new Id(IntPtr_objc_msgSendSuper(ref super, selector.Handle));
        }
    }
}
