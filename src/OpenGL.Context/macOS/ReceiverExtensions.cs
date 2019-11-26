using NStuff.Runtime.InteropServices.ObjectiveC;
using System;

using static NStuff.OpenGL.Context.macOS.NativeMethods;

namespace NStuff.OpenGL.Context.macOS
{
    /// <summary>
    /// Provides methods to send messages to an Objective C object.
    /// </summary>
    public static class ReceiverExtensions
    {
        internal static Id Get(this IReceiver receiver, SEL selector, IntPtr arg0) =>
            new Id(IntPtr_objc_msgSend(receiver.Handle, selector.Handle, arg0));

        [CLSCompliant(false)]
        public static void Send(this IReceiver receiver, SEL selector, IntPtr arg0, ulong arg1) =>
            Void_objc_msgSend(receiver.Handle, selector.Handle, arg0, new IntPtr((long)arg1));
    }
}
