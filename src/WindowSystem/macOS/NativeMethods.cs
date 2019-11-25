using System;
using System.Runtime.InteropServices;

using boolean_t = System.Int32;
using CGError = System.Int32;
using CGPoint = NStuff.WindowSystem.macOS.NSPoint;
using CGRect = NStuff.WindowSystem.macOS.NSRect;
using CGDirectDisplayID = System.UInt32;
using CGWindowLevel = System.Int32;
using CGWindowLevelKey = System.Int32;

namespace NStuff.WindowSystem.macOS
{
    internal static class NativeMethods
    {
        internal const string CarbonFramework = "/System/Library/Frameworks/Carbon.framework/Carbon";
        internal const string CoreFoundationFramework = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
        internal const string CoreGraphicsFramework = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";
        internal const string CoreServicesFramework = "/System/Library/Frameworks/CoreServices.framework/CoreServices";
        internal const string Libobjc = "/usr/lib/libobjc.dylib";

        internal const ulong NSAlphaNonpremultipliedBitmapFormat = 1 << 1;

        internal const ulong NSBackingStoreBuffered = 2;

        internal const ulong NSDragOperationNone = 0;
        internal const ulong NSDragOperationGeneric = 4;

        internal const ulong NSEventMaskAny = 0xFFFFFFFFFFFFFFFF;
        internal const ulong NSEventTypeApplicationDefined = 15;

        internal const ulong NSAlphaShiftKeyMask = 1 << 16;
        internal const ulong NSShiftKeyMask = 1 << 17;
        internal const ulong NSControlKeyMask = 1 << 18;
        internal const ulong NSAlternateKeyMask = 1 << 19;
        internal const ulong NSCommandKeyMask = 1 << 20;
        internal const ulong NSDeviceIndependentModifierFlags = 0xFFFF0000;

        internal const ulong NSKeyUp = 11;

        internal const long NSApplicationActivationPolicyRegular = 0;
        internal const ulong NSTerminateCancel = 0;

        internal const ulong NSBorderlessWindowMask = 0;
        internal const ulong NSTitledWindowMask = 1 << 0;
        internal const ulong NSClosableWindowMask = 1 << 1;
        internal const ulong NSMiniaturizableWindowMask = 1 << 2;
        internal const ulong NSResizableWindowMask = 1 << 3;
        internal const ulong NSTexturedBackgroundWindowMask = 1 << 8;
        internal const ulong NSUnifiedTitleAndToolbarWindowMask = 1 << 12;
        internal const ulong NSFullScreenWindowMask = 1 << 14;
        internal const ulong NSFullSizeContentViewWindowMask = 1 << 15;

        internal const ulong NSInformationalRequest = 10;

        internal const ulong NSTrackingMouseEnteredAndExited = 0x01;
        internal const ulong NSTrackingMouseMoved = 0x02;
        internal const ulong NSTrackingCursorUpdate = 0x04;
        internal const ulong NSTrackingActiveInActiveApp = 0x40;
        internal const ulong NSTrackingAssumeInside = 0x100;
        internal const ulong NSTrackingInVisibleRect = 0x200;


        internal const long NSWindowTabbingModeDisallowed = 2;

        internal const ushort kUCKeyActionDisplay = 3;
        internal const uint kUCKeyTranslateNoDeadKeysBit = 0;

        private const CGWindowLevelKey kCGNormalWindowLevelKey = 4;
        private const CGWindowLevelKey kCGFloatingWindowLevelKey = 5;

        internal static CGWindowLevel NSNormalWindowLevel => CGWindowLevelForKey(kCGNormalWindowLevelKey);
        internal static CGWindowLevel NSFloatingWindowLevel => CGWindowLevelForKey(kCGFloatingWindowLevelKey);

        [DllImport(CoreFoundationFramework)]
        internal static extern void CFRelease(IntPtr cf);

        [DllImport(CoreGraphicsFramework)]
        internal static extern CGError CGAssociateMouseAndMouseCursorPosition(boolean_t connected);

        [DllImport(CoreGraphicsFramework)]
        internal static extern CGRect CGDisplayBounds(CGDirectDisplayID display);

        [DllImport(CoreGraphicsFramework)]
        internal static extern CGDirectDisplayID CGMainDisplayID();

        [DllImport(CoreGraphicsFramework)]
        private static extern CGWindowLevel CGWindowLevelForKey(CGWindowLevelKey key);

        [DllImport(CoreGraphicsFramework)]
        internal static extern CGError CGWarpMouseCursorPosition(CGPoint newCursorPosition);

        [DllImport(CoreServicesFramework)]
        internal static extern unsafe int UCKeyTranslate(IntPtr keyLayoutPtr, UInt16 virtualKeyCode, UInt16 keyAction, UInt32 modifierKeyState,
            UInt32 keyboardType, uint keyTranslateOptions, ref UInt32 deadKeyState, ulong maxStringLength,
            out ulong actualStringLength, char* unicodeString);

        [DllImport(CarbonFramework)]
        internal static extern byte LMGetKbdType();

        [DllImport(CarbonFramework)]
        internal static extern IntPtr TISGetInputSourceProperty(IntPtr inputSource, IntPtr propertyKey);

        [DllImport(CarbonFramework)]
        internal static extern IntPtr TISCopyCurrentKeyboardInputSource();

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, sbyte arg0);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0, IntPtr arg1);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0, IntPtr arg1, IntPtr arg2);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0, IntPtr arg1, IntPtr arg2, sbyte arg3);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, NSRect arg0, IntPtr arg1, IntPtr arg2, IntPtr arg3);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, NSRect arg0, IntPtr arg1, IntPtr arg2, sbyte arg3);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0, NSPoint arg1);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, NSSize arg0);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0, long arg1, long arg2, long arg3, long arg4,
            sbyte arg5, sbyte arg6, IntPtr arg7, ulong arg8, long arg9, long arg10);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector, ulong arg0, NSPoint arg1, ulong arg2, double arg3,
            long arg4, IntPtr arg5, short arg6, long arg7, long arg8);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static NSPoint Point_objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static NSPoint Point_objc_msgSend(IntPtr receiver, IntPtr selector, NSPoint arg0);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend_stret")]
        internal extern static NSRect Rect_objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend_stret")]
        internal extern static NSRect Rect_objc_msgSend(IntPtr receiver, IntPtr selector, NSRect arg0);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend_stret")]
        internal extern static NSSize Size_objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static sbyte SByte_objc_msgSend(IntPtr receiver, IntPtr selector, NSPoint arg0, NSRect arg1);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static void Void_objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg0, IntPtr arg1);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static void Void_objc_msgSend(IntPtr receiver, IntPtr selector, NSPoint arg0);

        [DllImport(Libobjc, EntryPoint = "objc_msgSend")]
        internal extern static void Void_objc_msgSend(IntPtr receiver, IntPtr selector, NSSize arg0);
    }
}
