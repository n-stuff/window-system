using System;
using System.Runtime.InteropServices;

using ATOM = System.UInt64;
using BOOL = System.Int32;
using COLORMAP = System.UInt64;
using CURSOR = System.UInt64;
using LONG = System.Int64;
using PIXMAP = System.UInt64;
using PDISPLAY = System.IntPtr;
using PSCREEN = System.IntPtr;
using PVISUAL = System.IntPtr;
using TIME = System.UInt64;
using ULONG = System.UInt64;
using WINDOW = System.UInt64;
using XIC = System.IntPtr;
using XIM = System.IntPtr;

using XcursorDim = System.UInt32;
using XcursorPixel = System.UInt32;
using XcursorUInt = System.UInt32;

namespace NStuff.WindowSystem.Linux
{
    internal static class NativeMethods
    {
#pragma warning disable IDE1006 // Naming Styles

        private const string X11Library = "libX11.so.6";
        private const string XcursorLibrary = "libXcursor.so.1";

        internal const int AllocNone = 0;
        internal const int None = 0;

        internal const ULONG AnyPropertyType = 0;

        internal const ULONG CurrentTime = 0;

        internal const ULONG CWColormap = 1 << 13;
        internal const ULONG CWEventMask = 1 << 11;

        internal const int GrabModeAsync = 1;

        internal const int NormalState = 1;
        internal const int IconicState = 3;

        internal const uint InputOutput = 1;
        internal const uint InputOnly = 2;

        internal const int IsUnmapped = 0;

        internal const int PropertyNewValue = 0;
        internal const int PropertyDelete = 1;

        internal const int PropModeReplace = 0;
        internal const int PropModeAppend = 2;

        internal const LONG PPosition = 1 << 2;
        internal const LONG PMinSize = 1 << 4;
        internal const LONG PMaxSize = 1 << 5;
        internal const LONG PWinGravity = 1 << 9;

        internal const int RevertToParent = 2;

        internal const int StaticGravity = 10;

        internal const LONG NoEventMask = 0;
        internal const LONG KeyPressMask = 1 << 0;
        internal const LONG KeyReleaseMask = 1 << 1;
        internal const LONG ButtonPressMask = 1 << 2;
        internal const LONG ButtonReleaseMask = 1 << 3;
        internal const LONG EnterWindowMask = 1 << 4;
        internal const LONG LeaveWindowMask = 1 << 5;
        internal const LONG PointerMotionMask = 1 << 6;
        internal const LONG ExposureMask = 1 << 15;
        internal const LONG VisibilityChangeMask = 1 << 16;
        internal const LONG StructureNotifyMask = 1 << 17;
        internal const LONG SubstructureNotifyMask = 1 << 19;
        internal const LONG SubstructureRedirectMask = 1 << 20;
        internal const LONG FocusChangeMask = 1 << 21;
        internal const LONG PropertyChangeMask = 1 << 22;

        internal const int KeyPress = 2;
        internal const int KeyRelease = 3;
        internal const int ButtonPress = 4;
        internal const int ButtonRelease = 5;
        internal const int MotionNotify = 6;
        internal const int EnterNotify = 7;
        internal const int LeaveNotify = 8;
        internal const int FocusIn = 9;
        internal const int FocusOut = 10;
        internal const int Expose = 12;
        internal const int VisibilityNotify = 15;
        internal const int UnmapNotify = 18;
        internal const int MapNotify = 19;
        internal const int ConfigureNotify = 22;
        internal const int PropertyNotify = 28;
        internal const int SelectionClear = 29;
        internal const int SelectionRequest = 30;
        internal const int SelectionNotify = 31;
        internal const int ClientMessage = 33;
        internal const int GenericEvent = 35;

        internal const uint ShiftMask = 1;
        internal const uint LockMask = 1 << 1;
        internal const uint ControlMask = 1 << 2;
        internal const uint Mod1Mask = 1 << 3;
        internal const uint Mod2Mask = 1 << 4;
        internal const uint Mod4Mask = 1 << 6;
        internal const uint Mod5Mask = 1 << 7;
        internal const uint Button1Mask = 1 << 8;
        internal const uint Button2Mask = 1 << 9;
        internal const uint Button3Mask = 1 << 10;
        internal const uint Button4Mask = 1 << 11;
        internal const uint Button5Mask = 1 << 12;

        internal const ULONG MWM_HINTS_DECORATIONS = 2;

        internal static readonly int SizeOfLong = Marshal.SizeOf<long>();

        internal const ULONG XA_PRIMARY = 1;
        internal const ULONG XA_ATOM = 4;
        internal const ULONG XA_CARDINAL = 6;
        internal const ULONG XA_STRING = 31;
        internal const ULONG XA_WINDOW = 33;

        internal const uint XC_crosshair = 34;
        internal const uint XC_hand2 = 60;
        internal const uint XC_left_ptr = 68;
        internal const uint XC_sb_h_double_arrow = 108;
        internal const uint XC_sb_v_double_arrow = 116;
        internal const uint XC_xterm = 152;

        internal const ULONG XIMPreeditNothing = 0x0008;
        internal const ULONG XIMStatusNothing = 0x0400;

        internal const int XBufferOverflow = -1;
        internal const int XLookupChars = 2;
        internal const int XLookupBoth = 4;


        [StructLayout(LayoutKind.Explicit)]
        internal unsafe struct XEvent
        {
            [FieldOffset(0)]
            internal int type;
            [FieldOffset(0)]
            internal XAnyEvent xany;
            [FieldOffset(0)]
            internal XKeyEvent xkey;
            [FieldOffset(0)]
            internal XButtonEvent xbutton;
            [FieldOffset(0)]
            internal XMotionEvent xmotion;
            [FieldOffset(0)]
            internal XCrossingEvent xcrossing;
            [FieldOffset(0)]
            internal XConfigureEvent xconfigure;
            [FieldOffset(0)]
            internal XPropertyEvent xproperty;
            [FieldOffset(0)]
            internal XSelectionClearEvent xselectionclear;
            [FieldOffset(0)]
            internal XSelectionRequestEvent xselectionrequest;
            [FieldOffset(0)]
            internal XSelectionEvent xselection;
            [FieldOffset(0)]
            internal XClientMessageEvent xclient;
            [FieldOffset(0)]
            internal fixed long pad[24];
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XAnyEvent
        {
            internal int type;
            internal ULONG serial;
            internal BOOL send_event;
            internal PDISPLAY display;
            internal WINDOW window;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XKeyEvent
        {
            internal int type;
            internal ULONG serial;
            internal BOOL send_event;
            internal PDISPLAY display;
            internal WINDOW window;
            internal WINDOW root;
            internal WINDOW subwindow;
            internal TIME time;
            internal int x, y;
            internal int x_root, y_root;
            internal uint state;
            internal uint keycode;
            internal BOOL same_screen;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XButtonEvent
        {
            internal int type;
            internal ULONG serial;
            internal BOOL send_event;
            internal PDISPLAY display;
            internal WINDOW window;
            internal WINDOW root;
            internal WINDOW subwindow;
            internal TIME time;
            internal int x, y;
            internal int x_root, y_root;
            internal uint state;
            internal uint button;
            internal BOOL same_screen;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XMotionEvent
        {
            internal int type;
            internal ULONG serial;
            internal BOOL send_event;
            internal PDISPLAY display;
            internal WINDOW window;
            internal WINDOW root;
            internal WINDOW subwindow;
            internal TIME time;
            internal int x, y;
            internal int x_root, y_root;
            internal uint state;
            internal sbyte is_hint;
            internal BOOL same_screen;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XCrossingEvent
        {
            internal int type;
            internal ULONG serial;
            internal BOOL send_event;
            internal PDISPLAY display;
            internal WINDOW window;
            internal WINDOW root;
            internal WINDOW subwindow;
            internal TIME time;
            internal int x, y;
            internal int x_root, y_root;
            internal int mode;
            internal int detail;
            internal BOOL same_screen;
            internal BOOL focus;
            internal uint state;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XConfigureEvent
        {
            internal int type;
            internal ULONG serial;
            internal BOOL send_event;
            internal PDISPLAY display;
            internal WINDOW @event;
            internal WINDOW window;
            internal int x, y;
            internal int width, height;
            internal int border_width;
            internal WINDOW above;
            internal BOOL override_redirect;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XPropertyEvent
        {
            internal int type;
            internal ULONG serial;
            internal BOOL send_event;
            internal PDISPLAY display;
            internal WINDOW window;
            internal ATOM atom;
            internal ULONG time;
            internal int state;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XSelectionClearEvent
        {
            internal int type;
            internal ULONG serial;
            internal BOOL send_event;
            internal PDISPLAY display;
            internal WINDOW window;
            internal ATOM selection;
            internal TIME time;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XSelectionRequestEvent
        {
            internal int type;
            internal ULONG serial;
            internal BOOL send_event;
            internal PDISPLAY display;
            internal WINDOW owner;
            internal WINDOW requestor;
            internal ATOM selection;
            internal ATOM target;
            internal ATOM property;
            internal TIME time;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XSelectionEvent
        {
            internal int type;
            internal ULONG serial;
            internal BOOL send_event;
            internal PDISPLAY display;
            internal WINDOW requestor;
            internal ATOM selection;
            internal ATOM target;
            internal ATOM property;
            internal TIME time;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct XClientMessageEvent
        {
            internal int type;
            internal ULONG serial;
            internal BOOL send_event;
            internal PDISPLAY display;
            internal WINDOW window;
            internal ATOM message_type;
            internal int format;
            internal fixed LONG data[5];
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XSetWindowAttributes
        {
            internal PIXMAP background_pixmap;
            internal ULONG background_pixel;
            internal PIXMAP border_pixmap;
            internal ULONG border_pixel;
            internal int bit_gravity;
            internal int win_gravity;
            internal int backing_store;
            internal ULONG backing_planes;
            internal ULONG backing_pixel;
            internal BOOL save_under;
            internal LONG event_mask;
            internal LONG do_not_propagate_mask;
            internal BOOL override_redirect;
            internal COLORMAP colormap;
            internal CURSOR cursor;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XWindowAttributes
        {
            internal int x, y;
            internal int width, height;
            internal int border_width;
            internal int depth;
            internal PVISUAL visual;
            internal WINDOW root;
            internal int @class;
            internal int bit_gravity;
            internal int win_gravity;
            internal int backing_store;
            internal ULONG backing_planes;
            internal ULONG backing_pixel;
            internal BOOL save_under;
            internal COLORMAP colormap;
            internal int map_installed;
            internal int map_state;
            internal LONG all_event_masks;
            internal LONG your_event_mask;
            internal LONG do_not_propagate_mask;
            internal BOOL override_redirect;
            internal PSCREEN screen;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XSizeHints
        {
            internal LONG flags;
            internal int x, y;
            internal int width, height;
            internal int min_width, min_height;
            internal int max_width, max_height;
            internal int width_inc, height_inc;
            internal int min_aspect_x, min_aspect_y;
            internal int max_aspect_x, max_aspect_y;
            internal int base_width, base_height;
            internal int win_gravity;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MotifWmHints
        {
            internal ULONG flags;
            internal ULONG functions;
            internal ULONG decorations;
            internal LONG input_mode;
            internal ULONG status;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct XIMStyles
        {
            internal ushort count_styles;
            internal ULONG* supported_styles;
        }

        internal delegate int XCheckIfPredicate(PDISPLAY display, ref XEvent @event, IntPtr pointer);

        [DllImport(X11Library)]
        internal static extern unsafe int XChangeProperty(PDISPLAY display, WINDOW w, ATOM property, ATOM type, int format,
            int mode, void* data, int nelements);

        [DllImport(X11Library)]
        internal static extern int XCheckIfEvent(PDISPLAY display, out XEvent event_return, IntPtr predicate, IntPtr arg);

        [DllImport(X11Library)]
        internal static extern int XCheckTypedWindowEvent(PDISPLAY display, WINDOW w, int event_type, out XEvent event_return);

        [DllImport(X11Library)]
        internal static extern int XCloseDisplay(PDISPLAY display);

        [DllImport(X11Library)]
        internal static extern int XCloseIM(XIM im);

        [DllImport(X11Library)]
        internal static extern int XConnectionNumber(PDISPLAY display);

        [DllImport(X11Library)]
        internal static extern int XConvertSelection(PDISPLAY display, ATOM selection, ATOM target, ATOM property, WINDOW requestor, TIME time);

        [DllImport(X11Library)]
        internal static extern COLORMAP XCreateColormap(PDISPLAY display, WINDOW window, PVISUAL visual, int alloc);

        [DllImport(X11Library)]
        internal static extern CURSOR XCreateFontCursor(PDISPLAY display, uint shape);

        [DllImport(X11Library)]
        internal static extern XIC XCreateIC(XIM im, IntPtr key0, IntPtr value0, IntPtr key1,
            IntPtr value1, IntPtr key2, IntPtr value2, IntPtr zero);

        [DllImport(X11Library)]
        internal static extern WINDOW XCreateWindow(PDISPLAY display, WINDOW parent, int x, int y, uint width, uint height, uint borderWidth,
            int depth, uint @class, PVISUAL visual, ULONG valuemask, ref XSetWindowAttributes attributes);

        [DllImport(X11Library)]
        internal static extern int XDefaultScreen(PDISPLAY display);

        [DllImport(X11Library)]
        internal static extern PVISUAL XDefaultVisual(PDISPLAY display, int screenNumber);

        [DllImport(X11Library)]
        internal static extern int XDefineCursor(PDISPLAY display, WINDOW w, CURSOR cursor);

        [DllImport(X11Library)]
        internal static extern int XDeleteProperty(PDISPLAY display, WINDOW w, ATOM property);

        [DllImport(X11Library)]
        internal static extern void XDestroyIC(XIC ic);

        [DllImport(X11Library)]
        internal static extern int XDestroyWindow(PDISPLAY display, WINDOW window);

        [DllImport(X11Library)]
        internal static extern int XFilterEvent(ref XEvent @event, WINDOW window);

        [DllImport(X11Library)]
        internal static extern int XFlush(PDISPLAY display);

        [DllImport(X11Library)]
        internal static extern int XFree(IntPtr data);

        [DllImport(X11Library)]
        internal static extern int XFreeColormap(PDISPLAY display, COLORMAP colormap);

        [DllImport(X11Library)]
        internal static extern int XFreeCursor(PDISPLAY display, CURSOR cursor);

        [DllImport(X11Library)]
        internal static extern IntPtr XGetIMValues(XIM im, [MarshalAs(UnmanagedType.LPStr)] string key, out IntPtr value, IntPtr zero);

        [DllImport(X11Library)]
        internal static extern int XGetInputFocus(PDISPLAY display, out WINDOW focus_return, out int revert_to_return);

        [DllImport(X11Library)]
        internal static extern WINDOW XGetSelectionOwner(PDISPLAY display, ATOM selection);

        [DllImport(X11Library)]
        internal static extern int XGetWindowAttributes(PDISPLAY display, WINDOW window, out XWindowAttributes window_attributes_return);

        [DllImport(X11Library)]
        internal static extern int XGetWindowProperty(PDISPLAY display, WINDOW window, ATOM @property, LONG long_offset, LONG long_length,
            BOOL delete, ATOM req_type, out ATOM actual_type_return, out int actual_format_return,
            out ULONG nitems_return, out ULONG bytes_after_return, out IntPtr prop_return);

        [DllImport(X11Library)]
        internal static extern int XGetWMNormalHints(PDISPLAY display, WINDOW window, out XSizeHints hints, out LONG supplied_return);

        [DllImport(X11Library)]
        internal static extern int XGrabPointer(PDISPLAY display, WINDOW grab_window, BOOL owner_events, uint event_mask,
            int pointer_mode, int keyboard_mode, WINDOW confine_to, CURSOR cursor, TIME time);

        [DllImport(X11Library)]
        internal static extern int XIconifyWindow(PDISPLAY display, WINDOW window, int screen_number);

        [DllImport(X11Library)]
        internal static extern int XInitThreads();

        [DllImport(X11Library)]
        internal static extern ATOM XInternAtom(PDISPLAY display, [MarshalAs(UnmanagedType.LPStr)] string atomName, BOOL onlyIfExists);

        [DllImport(X11Library)]
        internal static extern int XLookupString(ref XKeyEvent @event, IntPtr buffer_return, int bytes_buffer,
            out ULONG keysym_return, IntPtr status_in_out);

        [DllImport(X11Library)]
        internal static extern int XMapRaised(PDISPLAY display, WINDOW window);

        [DllImport(X11Library)]
        internal static extern LONG XMaxRequestSize(PDISPLAY display);

        [DllImport(X11Library)]
        internal static extern int XMoveWindow(PDISPLAY display, WINDOW window, int x, int y);

        [DllImport(X11Library)]
        internal static extern int XNextEvent(PDISPLAY display, out XEvent eventReturn);

        [DllImport(X11Library)]
        internal static extern PDISPLAY XOpenDisplay([MarshalAs(UnmanagedType.LPStr)] string? displayName);

        [DllImport(X11Library)]
        internal static extern XIM XOpenIM(PDISPLAY display, IntPtr rdb, IntPtr res_name, IntPtr res_class);

        [DllImport(X11Library)]
        internal static extern int XPending(PDISPLAY display);

        [DllImport(X11Library)]
        internal static extern BOOL XQueryPointer(PDISPLAY display, WINDOW window, out WINDOW root_return, out WINDOW child_return,
                                                  out int root_x_return, out int root_y_return, out int win_x_return, out int win_y_return,
                                                  out uint mask_return);

        [DllImport(X11Library)]
        internal static extern int XRaiseWindow(PDISPLAY display, WINDOW window);

        [DllImport(X11Library)]
        internal static extern int XResizeWindow(PDISPLAY display, WINDOW window, uint width, uint height);

        [DllImport(X11Library)]
        internal static extern WINDOW XRootWindow(PDISPLAY display, int screenNumber);

        [DllImport(X11Library)]
        internal static extern int XSelectInput(PDISPLAY display, WINDOW w, LONG event_mask);

        [DllImport(X11Library)]
        internal static extern int XSendEvent(PDISPLAY display, WINDOW window, BOOL propagate, LONG event_mask, ref XEvent event_send);

        [DllImport(X11Library)]
        internal static extern void XSetICFocus(XIC ic);

        [DllImport(X11Library)]
        internal static extern int XSetInputFocus(PDISPLAY display, WINDOW focus, int revert_to, TIME time);

        [DllImport(X11Library)]
        internal static extern IntPtr XSetLocaleModifiers([MarshalAs(UnmanagedType.LPStr)] string modifier_list);

        [DllImport(X11Library)]
        internal static extern int XSetSelectionOwner(PDISPLAY display, ATOM selection, WINDOW window, TIME time);

        [DllImport(X11Library)]
        internal static extern void XSetWMNormalHints(PDISPLAY display, WINDOW window, ref XSizeHints hints);

        [DllImport(X11Library)]
        internal unsafe static extern int XSetWMProtocols(PDISPLAY display, WINDOW window, ATOM* protocols, int count);

        [DllImport(X11Library)]
        internal static extern int XSync(PDISPLAY display, BOOL discard);

        [DllImport(X11Library)]
        internal static extern int XSupportsLocale();

        [DllImport(X11Library)]
        internal static extern int XTranslateCoordinates(PDISPLAY display, WINDOW src_w, WINDOW dest_w, int src_x, int src_y,
            out int dest_x_return, out int dest_y_return, out WINDOW child_return);

        [DllImport(X11Library)]
        internal static extern int XUndefineCursor(PDISPLAY display, WINDOW w);

        [DllImport(X11Library)]
        internal static extern int XUngrabPointer(PDISPLAY display, TIME time);

        [DllImport(X11Library)]
        internal static extern int XUnmapWindow(PDISPLAY display, WINDOW window);

        [DllImport(X11Library)]
        internal static extern void XUnsetICFocus(XIC ic);

        [DllImport(X11Library)]
        internal static extern int Xutf8LookupString(XIC ic, ref XKeyEvent @event, IntPtr buffer_return, int bytes_buffere,
            IntPtr keysym_return, out int status_return);

        [DllImport(X11Library)]
        internal static extern IntPtr Xutf8ResetIC(XIC ic);

        [DllImport(X11Library)]
        internal static extern int XWarpPointer(PDISPLAY display, WINDOW src_w, WINDOW dest_w, int src_x, int src_y,
            uint src_width, uint src_height, int dest_x, int dest_y);

        // Xcursor --------------------------------------------------------------------------------

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct XcursorImage
        {
            internal XcursorUInt version;
            internal XcursorDim size;
            internal XcursorDim width;
            internal XcursorDim height;
            internal XcursorDim xhot;
            internal XcursorDim yhot;
            internal XcursorUInt delay;
            internal XcursorPixel* pixels;
        }

        [DllImport(XcursorLibrary)]
        internal static extern unsafe XcursorImage* XcursorImageCreate(int width, int height);

        [DllImport(XcursorLibrary)]
        internal static extern unsafe void XcursorImageDestroy(XcursorImage* image);

        [DllImport(XcursorLibrary)]
        internal static extern unsafe CURSOR XcursorImageLoadCursor(IntPtr display, XcursorImage* image);

        // Xkb ------------------------------------------------------------------------------------

        internal const uint XkbKeyNamesMask = 1 << 9;

        internal const uint XkbUseCoreKbd = 0x0100;

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct XkbDescRec
        {
            internal PDISPLAY dpy;
            internal ushort flags;
            internal ushort device_spec;
            internal byte min_key_code;
            internal byte max_key_code;
            internal IntPtr ctrls;
            internal IntPtr server;
            internal IntPtr map;
            internal IntPtr indicators;
            internal XkbNamesRec* names;
            internal IntPtr compat;
            internal IntPtr geom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct XkbNamesRec
        {
            internal ULONG keycodes;
            internal ULONG geometry;
            internal ULONG symbols;
            internal ULONG types;
            internal ULONG compat;
            internal fixed ULONG vmods[16];
            internal fixed ULONG indicators[32];
            internal fixed ULONG groups[4];
            internal XkbKeyNameRec* keys;
            internal IntPtr key_aliases;
            internal IntPtr radio_groups;
            internal ULONG phys_symbols;
            internal byte num_keys;
            internal byte num_key_aliases;
            internal ushort num_rg;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct XkbKeyNameRec
        {
            internal fixed sbyte name[4];
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XkbStateRec
        {
            internal byte group;
            internal byte locked_group;
            internal ushort base_group;
            internal ushort latched_group;
            internal byte mods;
            internal byte base_mods;
            internal byte latched_mods;
            internal byte locked_mods;
            internal byte compat_state;
            internal byte grab_mods;
            internal byte compat_grab_mods;
            internal byte lookup_mods;
            internal byte compat_lookup_mods;
            internal ushort ptr_buttons;
        }

        [DllImport(X11Library)]
        internal static extern unsafe int XkbFreeClientMap(XkbDescRec* xkb, uint what, int freeMap);

        [DllImport(X11Library)]
        internal static extern unsafe int XkbFreeNames(XkbDescRec* xkb, uint which, int freeMap);

        [DllImport(X11Library)]
        internal static extern unsafe XkbDescRec* XkbGetMap(PDISPLAY display, uint which, uint deviceSpec);

        [DllImport(X11Library)]
        internal static extern unsafe int XkbGetNames(PDISPLAY display, uint which, XkbDescRec* xkb);

        [DllImport(X11Library)]
        internal static extern unsafe int XkbGetState(PDISPLAY display, uint device_spec, XkbStateRec* state_return);

        [DllImport(X11Library)]
        internal static extern ULONG XkbKeycodeToKeysym(PDISPLAY display, byte kc, int group, int level);

        [DllImport(X11Library)]
        internal static extern int XkbQueryExtension(PDISPLAY display, out int opcodeReturn, out int eventBaseReturn,
                                                     out int errorBaseReturn, ref int majorRtrn, ref int minorRtrn);

        [DllImport(X11Library)]
        internal static extern int XkbSetDetectableAutoRepeat(PDISPLAY display, int detectable, out int supported);

        // EPOLL ----------------------------------------------------------------------------------

        internal const uint EPOLLIN = 0x001;
        internal const uint EPOLLERR = 0x008;
        internal const uint EPOLLHUP = 0x010;
        internal const uint EPOLLET = 1u << 31;

        internal const int EPOLL_CTL_ADD = 1;

        [StructLayout(LayoutKind.Explicit)]
        internal unsafe struct epoll_data_t
        {
            [FieldOffset(0)]
            internal IntPtr ptr;
            [FieldOffset(0)]
            internal int fd;
            [FieldOffset(0)]
            internal uint u32;
            [FieldOffset(0)]
            internal ulong u64;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct epoll_event
        {
            internal uint events;
            internal epoll_data_t data;
        }

        [DllImport("libc")]
        internal static extern int close(int fd);

        [DllImport("libc")]
        internal static extern int epoll_create(int size);

        [DllImport("libc")]
        internal static extern int epoll_ctl(int epfd, int op, int fd, ref epoll_event @event);

        [DllImport("libc")]
        internal static extern unsafe int epoll_wait(int epfd, epoll_event* events, int maxevents, int timeout);
    }
}
