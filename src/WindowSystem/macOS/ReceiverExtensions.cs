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
        public static Id Get(this IReceiver receiver, SEL selector, bool arg0) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, (sbyte)(arg0 ? 1 : 0)));

        [CLSCompliant(false)]
        public static Id Get(this IReceiver receiver, SEL selector, IntPtr arg0, ulong arg1) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0, new IntPtr((long)arg1)));

        [CLSCompliant(false)]
        public static Id Get(this IReceiver receiver, SEL selector, IntPtr arg0, IntPtr arg1, ulong arg2) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0, arg1, new IntPtr((long)arg2)));

        public static Id Get(this IReceiver receiver, SEL selector, Id arg0, SEL arg1, Id arg2) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle, arg1.Handle, arg2.Handle));

        public static Id Get(this IReceiver receiver, SEL selector, Id arg0, NSPoint arg1) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0.Handle, arg1));

        public static Id Get(this IReceiver receiver, SEL selector, NSSize arg0) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0));

        [CLSCompliant(false)]
        public static Id Get(this IReceiver receiver, SEL selector, IntPtr arg0, long arg1, long arg2, long arg3, long arg4,
            bool arg5, bool arg6, Id arg7, ulong arg8, long arg9, long arg10) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0, arg1, arg2, arg3, arg4,
                (sbyte)(arg5 ? 1 : 0), (sbyte)(arg6 ? 1 : 0), arg7.Handle, arg8, arg9, arg10));

        [CLSCompliant(false)]
        public static Id Get(this IReceiver receiver, SEL selector, ulong arg0, Id arg1, Id arg2, bool arg3) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, new IntPtr((long)arg0),
                arg1.Handle, arg2.Handle, (sbyte)(arg3 ? 1 : 0)));

        internal static Id Get(this IReceiver receiver, SEL selector, NSRect arg0, ulong arg1, Id arg2, Id arg3) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0, new IntPtr((long)arg1), arg2.Handle, arg3.Handle));

        internal static Id Get(this IReceiver receiver, SEL selector, NSRect arg0, ulong arg1, ulong arg2, bool arg3) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0, new IntPtr((long)arg1),
                new IntPtr((long)arg2), (sbyte)(arg3 ? 1 : 0)));

        [CLSCompliant(false)]
        public static Id Get(this IReceiver receiver, SEL selector, ulong arg0, NSPoint arg1, ulong arg2, double arg3, long arg4,
            Id arg5, short arg6, long arg7, long arg8) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0, arg1, arg2, arg3, arg4, arg5.Handle, arg6, arg7, arg8));

        public static bool GetBool(this IReceiver receiver, SEL selector, NSPoint arg0, NSRect arg1) =>
            SByte_objc_msgSend(receiver.Handle, selector.Handle, arg0, arg1) != 0;

        public static NSPoint GetPoint(this IReceiver receiver, SEL selector) =>
            Point_objc_msgSend(receiver.Handle, selector.Handle);

        public static NSPoint GetPoint(this IReceiver receiver, SEL selector, NSPoint arg0) =>
            Point_objc_msgSend(receiver.Handle, selector.Handle, arg0);

        public static NSRect GetRect(this IReceiver receiver, SEL selector) =>
            Rect_objc_msgSend(receiver.Handle, selector.Handle);

        public static NSRect GetRect(this IReceiver receiver, SEL selector, NSRect arg0) =>
            Rect_objc_msgSend(receiver.Handle, selector.Handle, arg0);

        public static NSSize GetSize(this IReceiver receiver, SEL selector) =>
            Size_objc_msgSend(receiver.Handle, selector.Handle);

        public static void Send(this IReceiver receiver, SEL selector, NSPoint arg0) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0);

        public static void Send(this IReceiver receiver, SEL selector, NSSize arg0) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0);
    }
}
