using System;
using System.Runtime.InteropServices;

namespace NStuff.Runtime.InteropServices.ObjectiveC
{
    internal static class NativeMethods
    {
#pragma warning disable IDE1006 // Naming Styles
        internal const string Libobjc = "/usr/lib/libobjc.dylib";

        [StructLayout(LayoutKind.Sequential)]
        internal struct objc_super
        {
            public IntPtr receiver;
            public IntPtr @class;

            internal objc_super(IReceiver receiver, Class @class)
            {
                this.receiver = receiver.Handle;
                this.@class = @class.Handle;
            }
        }

        [DllImport(Libobjc)]
        internal extern static bool class_addMethod(IntPtr newClass, IntPtr selector, IntPtr implementation,
            [MarshalAs(UnmanagedType.LPStr)] string types);

        [DllImport(Libobjc)]
        internal extern static bool class_addProtocol(IntPtr cls, IntPtr protocol);

        [DllImport(Libobjc)]
        internal extern static IntPtr objc_allocateClassPair(IntPtr superclass, [MarshalAs(UnmanagedType.LPStr)] string name, IntPtr extraBytes);

        [DllImport(Libobjc)]
        internal extern static IntPtr objc_getProtocol([MarshalAs(UnmanagedType.LPStr)] string name);

        [DllImport(Libobjc)]
        internal extern static IntPtr objc_lookUpClass([MarshalAs(UnmanagedType.LPStr)] string name);

        [DllImport(Libobjc)]
        internal extern static void objc_registerClassPair(IntPtr @class);

        [DllImport(Libobjc)]
        internal extern static IntPtr sel_registerName([MarshalAs(UnmanagedType.LPStr)] string name);


        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static double Double_objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, double arg0);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0, IntPtr arg1);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0, IntPtr arg1, IntPtr arg2);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0, IntPtr arg1, IntPtr arg2, sbyte arg3);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0, IntPtr arg1, IntPtr arg2, IntPtr arg3,
            IntPtr arg4, sbyte arg5, sbyte arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static sbyte SByte_objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static sbyte SByte_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static ushort UShort_objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static void Void_objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static void Void_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static void Void_objc_msgSend(IntPtr receiver, IntPtr selector, sbyte arg0);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static void Void_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0, sbyte arg1);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static void Void_objc_msgSend(IntPtr receiver, IntPtr selector, double arg0);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static void Void_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0, IntPtr arg1);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static void Void_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0, IntPtr arg1, IntPtr arg2);

        [DllImport(Libobjc, EntryPoint = "objc_msgSendSuper")]
        internal extern static void Void_objc_msgSendSuper(ref objc_super super, IntPtr selector);

        [DllImport(Libobjc, EntryPoint = "objc_msgSendSuper")]
        internal extern static void Void_objc_msgSendSuper(ref objc_super super, IntPtr selector, IntPtr arg0);

        [DllImport(Libobjc, EntryPoint = "objc_msgSendSuper")]
        internal extern static IntPtr IntPtr_objc_msgSendSuper(ref objc_super super, IntPtr selector);
    }
}
