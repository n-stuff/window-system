using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

using static NStuff.WindowSystem.Linux.NativeMethods;

using XID = System.UInt64;

namespace NStuff.WindowSystem.Linux
{
    internal class NativeWindowServer : NativeWindowServerBase
    {
        private const string FileProtocol = "file:/";
        private const XID XDND_VERSION = 5;
        private const XID _NET_WM_STATE_REMOVE = 0;
        private const XID _NET_WM_STATE_ADD = 1;

        private readonly XID NULL;
        private readonly XID _MOTIF_WM_HINTS;
        private readonly XID SAVE_TARGETS;
        private readonly XID TARGETS;
        private readonly XID MULTIPLE;
        private readonly XID CLIPBOARD;
        private readonly XID CLIPBOARD_MANAGER;
        private readonly XID WM_DELETE_WINDOW;
        private readonly XID WM_PROTOCOLS;
        private readonly XID WM_STATE;
        private readonly XID _NET_ACTIVE_WINDOW;
        private readonly XID _NET_FRAME_EXTENTS;
        private readonly XID _NET_REQUEST_FRAME_EXTENTS;
        private readonly XID _NET_WM_STATE;
        private readonly XID _NET_WM_STATE_ABOVE;
        private readonly XID _NET_WM_STATE_DEMANDS_ATTENTION;
        private readonly XID _NET_WM_STATE_MAXIMIZED_HORZ;
        private readonly XID _NET_WM_STATE_MAXIMIZED_VERT;
        private readonly XID _NET_WM_WINDOW_OPACITY;
        private readonly XID _NET_WM_WINDOW_TYPE;
        private readonly XID _NET_WM_WINDOW_TYPE_NORMAL;
        private readonly XID _NET_WM_NAME;
        private readonly XID _NET_WM_PING;
        private readonly XID ATOM_PAIR;
        private readonly XID UTF8_STRING;
        private readonly XID INCR;
        private readonly XID NStuff_SELECTION;
        private readonly XID XdndActionCopy;
        private readonly XID XdndAware;
        private readonly XID XdndEnter;
        private readonly XID XdndDrop;
        private readonly XID XdndFinished;
        private readonly XID XdndPosition;
        private readonly XID XdndSelection;
        private readonly XID XdndStatus;
        private readonly XID XdndTypeList;
        private readonly XID text_uri_list;

        private static IntPtr display;
        private readonly static byte[] failedConversionData = new byte[4];

        private readonly XID rootWindow;
        private readonly XID helperWindow;
        private readonly XID hiddenCursor;
        private XID XdndSource;
        private XID XdndVersion;
        private XID XdndFormat;
        private readonly IntPtr inputMethod;
        private readonly IntPtr XNInputStyle;
        private readonly IntPtr XNClientWindow;
        private readonly IntPtr XNFocusWindow;
        private readonly Keycode[] keycodeMappings = new Keycode[256];
        private readonly int[] scancodes = new int[(int)Keycode.Invalid];
        private string? clipboardString;
        private ulong eventTime;
        private readonly byte[] targetsConversionData;
        private readonly int epoll;
        private Window? freeLookMouseWindow;
        private (double x, double y) cursorPositionBackup;
        private readonly Dictionary<XID, Window> windows = new Dictionary<XID, Window>();

        internal NativeWindowServer()
        {
            if (display != IntPtr.Zero)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.ServerAlreadyInitialized));
            }
            XInitThreads();
            display = XOpenDisplay(null);
            if (display == IntPtr.Zero)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.XServerConnectionFailed));
            }
            rootWindow = XRootWindow(display, XDefaultScreen(display));

            epoll = epoll_create(1);
            if (epoll == -1)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            var epollEvent = new epoll_event();
            epollEvent.data.fd = XConnectionNumber(display);
            epollEvent.events = EPOLLIN | EPOLLET;
            if (epoll_ctl(epoll, EPOLL_CTL_ADD, epollEvent.data.fd, ref epollEvent) == -1)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }


            int major = 1;
            int minor = 0;
            if (XkbQueryExtension(display, out var _, out var _, out var _, ref major, ref minor) == 0)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.MissingKeyboardExtension));
            }

            if (XkbSetDetectableAutoRepeat(display, 1, out var supported) == 0 || supported == 0)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.DetectableKeyboardRepeatFailure));
            }

            var _NET_SUPPORTING_WM_CHECK = XInternAtom(display, "_NET_SUPPORTING_WM_CHECK", 1);
            if (_NET_SUPPORTING_WM_CHECK == 0)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.EWMHNotSupported));
            }

            if (GetWindowProperty(display, rootWindow, _NET_SUPPORTING_WM_CHECK, XA_WINDOW, out var rootWindowPtr) != 1)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.EWMHNotSupported));
            }
            try
            {
                if (GetWindowProperty(display, ReadXID(rootWindowPtr, 0), _NET_SUPPORTING_WM_CHECK, XA_WINDOW, out var childWindowPtr) != 1)
                {
                    throw new InvalidOperationException(Resources.GetMessage(Resources.Key.EWMHNotSupported));
                }
                try
                {
                    if (ReadXID(rootWindowPtr, 0) != ReadXID(childWindowPtr, 0))
                    {
                        throw new InvalidOperationException(Resources.GetMessage(Resources.Key.EWMHNotSupported));
                    }
                }
                finally
                {
                    XFree(childWindowPtr);
                }
            }
            finally
            {
                XFree(rootWindowPtr);
            }
            var _NET_SUPPORTED = XInternAtom(display, "_NET_SUPPORTED", 1);
            if (_NET_SUPPORTED == 0)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.EWMHNotSupported));
            }
            {
                var atomCount = GetWindowProperty(display, rootWindow, _NET_SUPPORTED, XA_ATOM, out var atomsPtr);

                var _NET_ACTIVE_WINDOW = XInternAtom(display, "_NET_ACTIVE_WINDOW", 1);
                var _NET_FRAME_EXTENTS = XInternAtom(display, "_NET_FRAME_EXTENTS", 1);
                var _NET_REQUEST_FRAME_EXTENTS = XInternAtom(display, "_NET_REQUEST_FRAME_EXTENTS", 1);
                var _NET_WM_NAME = XInternAtom(display, "_NET_WM_NAME", 1);
                var _NET_WM_PING = XInternAtom(display, "_NET_WM_PING", 1);
                var _NET_WM_STATE = XInternAtom(display, "_NET_WM_STATE", 1);
                var _NET_WM_STATE_ABOVE = XInternAtom(display, "_NET_WM_STATE_ABOVE", 1);
                var _NET_WM_STATE_DEMANDS_ATTENTION = XInternAtom(display, "_NET_WM_STATE_DEMANDS_ATTENTION", 1);
                var _NET_WM_STATE_MAXIMIZED_HORZ = XInternAtom(display, "_NET_WM_STATE_MAXIMIZED_HORZ", 1);
                var _NET_WM_STATE_MAXIMIZED_VERT = XInternAtom(display, "_NET_WM_STATE_MAXIMIZED_VERT", 1);
                var _NET_WM_WINDOW_OPACITY = XInternAtom(display, "_NET_WM_WINDOW_OPACITY", 1);
                var _NET_WM_WINDOW_TYPE = XInternAtom(display, "_NET_WINDOW_TYPE", 1);
                var _NET_WM_WINDOW_TYPE_NORMAL = XInternAtom(display, "_NET_WINDOW_TYPE_NORMAL", 1);

                for (ulong i = 0; i < atomCount; i++)
                {
                    var atom = ReadXID(atomsPtr, (int)(i * (ulong)SizeOfLong));
                    if (atom == _NET_WM_STATE)
                    {
                        this._NET_WM_STATE = _NET_WM_STATE;
                    }
                    else if (atom == _NET_WM_STATE_ABOVE)
                    {
                        this._NET_WM_STATE_ABOVE = _NET_WM_STATE_ABOVE;
                    }
                    else if (atom == _NET_WM_STATE_DEMANDS_ATTENTION)
                    {
                        this._NET_WM_STATE_DEMANDS_ATTENTION = _NET_WM_STATE_DEMANDS_ATTENTION;
                    }
                    else if (atom == _NET_WM_STATE_MAXIMIZED_HORZ)
                    {
                        this._NET_WM_STATE_MAXIMIZED_HORZ = _NET_WM_STATE_MAXIMIZED_HORZ;
                    }
                    else if (atom == _NET_WM_STATE_MAXIMIZED_VERT)
                    {
                        this._NET_WM_STATE_MAXIMIZED_VERT = _NET_WM_STATE_MAXIMIZED_VERT;
                    }
                    else if (atom == _NET_WM_WINDOW_OPACITY)
                    {
                        this._NET_WM_WINDOW_OPACITY = _NET_WM_WINDOW_OPACITY;
                    }
                    else if (atom == _NET_WM_WINDOW_TYPE)
                    {
                        this._NET_WM_WINDOW_TYPE = _NET_WM_WINDOW_TYPE;
                    }
                    else if (atom == _NET_WM_WINDOW_TYPE_NORMAL)
                    {
                        this._NET_WM_WINDOW_TYPE_NORMAL = _NET_WM_WINDOW_TYPE_NORMAL;
                    }
                    else if (atom == _NET_WM_NAME)
                    {
                        this._NET_WM_NAME = _NET_WM_NAME;
                    }
                    else if (atom == _NET_WM_PING)
                    {
                        this._NET_WM_PING = _NET_WM_PING;
                    }
                    else if (atom == _NET_ACTIVE_WINDOW)
                    {
                        this._NET_ACTIVE_WINDOW = _NET_ACTIVE_WINDOW;
                    }
                    else if (atom == _NET_FRAME_EXTENTS)
                    {
                        this._NET_FRAME_EXTENTS = _NET_FRAME_EXTENTS;
                    }
                    else if (atom == _NET_REQUEST_FRAME_EXTENTS)
                    {
                        this._NET_REQUEST_FRAME_EXTENTS = _NET_REQUEST_FRAME_EXTENTS;
                    }
                }
                XFree(atomsPtr);
            }

            _MOTIF_WM_HINTS = XInternAtom(display, "_MOTIF_WM_HINTS", 0);
            NULL = XInternAtom(display, "NULL", 0);
            SAVE_TARGETS = XInternAtom(display, "SAVE_TARGETS", 0);
            TARGETS = XInternAtom(display, "TARGETS", 0);
            MULTIPLE = XInternAtom(display, "MULTIPLE", 0);
            CLIPBOARD = XInternAtom(display, "CLIPBOARD", 0);
            CLIPBOARD_MANAGER = XInternAtom(display, "CLIPBOARD_MANAGER", 0);
            WM_DELETE_WINDOW = XInternAtom(display, "WM_DELETE_WINDOW", 0);
            WM_PROTOCOLS = XInternAtom(display, "WM_PROTOCOLS", 0);
            WM_STATE = XInternAtom(display, "WM_STATE", 0);
            ATOM_PAIR = XInternAtom(display, "ATOM_PAIR", 0);
            UTF8_STRING = XInternAtom(display, "UTF8_STRING", 0);
            INCR = XInternAtom(display, "INCR", 0);
            NStuff_SELECTION = XInternAtom(display, "NStuff_SELECTION", 0);
            XdndActionCopy = XInternAtom(display, "XdndActionCopy", 0);
            XdndAware = XInternAtom(display, "XdndAware", 0);
            XdndDrop = XInternAtom(display, "XdndDrop", 0);
            XdndEnter = XInternAtom(display, "XdndEnter", 0);
            XdndFinished = XInternAtom(display, "XdndFinished", 0);
            XdndPosition = XInternAtom(display, "XdndPosition", 0);
            XdndSelection = XInternAtom(display, "XdndSelection", 0);
            XdndStatus = XInternAtom(display, "XdndStatus", 0);
            XdndTypeList = XInternAtom(display, "XdndTypeList", 0);
            text_uri_list = XInternAtom(display, "text/uri-list", 0);

            unsafe
            {
                var d = new byte[4 * SizeOfLong];
                fixed (byte* p = d)
                {
                    var targets = (XID*)p;
                    targets[0] = TARGETS;
                    targets[1] = MULTIPLE;
                    targets[2] = UTF8_STRING;
                    targets[3] = XA_STRING;
                }
                targetsConversionData = d;
            }

            InitializeKeycodeMappings();

            var setWindowAttributes = new XSetWindowAttributes { event_mask = PropertyChangeMask };
            helperWindow = XCreateWindow(display, rootWindow, 0, 0, 1, 1, 0, 0, InputOnly,
                XDefaultVisual(display, XDefaultScreen(display)), CWEventMask, ref setWindowAttributes);

            unsafe
            {
                var ximage = XcursorImageCreate(16, 16);
                if (ximage == null)
                {
                    throw new InvalidOperationException(Resources.GetMessage(Resources.Key.ImageCreationFailed));
                }
                ximage->xhot = 0;
                ximage->yhot = 0;
                var p = ximage->pixels;
                for (int i = 0; i < 16 * 16; i++, p++)
                {
                    *p = 0;
                }
                var id = XcursorImageLoadCursor(display, ximage);
                if (id == 0)
                {
                    throw new InvalidOperationException(Resources.GetMessage(Resources.Key.CursorCreationFailed));
                }
                hiddenCursor = id;
            }

            if (XSupportsLocale() != 0)
            {
                unsafe
                {
                    // Should be empty string, but the value of XMODIFIERS doesn't work on ubuntu.
                    XSetLocaleModifiers("@im=");
                }
                inputMethod = XOpenIM(display, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                if (inputMethod != IntPtr.Zero)
                {
                    if (!HasUsableInputMethodStyle())
                    {
                        XCloseIM(inputMethod);
                        inputMethod = IntPtr.Zero;
                    }
                    else
                    {
                        XNInputStyle = CreateXNProperty("inputStyle");
                        XNClientWindow = CreateXNProperty("clientWindow");
                        XNFocusWindow = CreateXNProperty("focusWindow");
                    }
                }
            }
        }

        protected internal override void Shutdown()
        {
            if (inputMethod != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(XNInputStyle);
                Marshal.FreeHGlobal(XNClientWindow);
                Marshal.FreeHGlobal(XNFocusWindow);
                XCloseIM(inputMethod);
            }
            PushSelectionToClipboardManager();
            XFreeCursor(display, hiddenCursor);
            XDestroyWindow(display, helperWindow);
            close(epoll);
            XCloseDisplay(display);
            display = IntPtr.Zero;
        }

        protected internal override bool IsRunning() => display != IntPtr.Zero;

        protected internal override void CreateWindowData(Window window)
        {
            var data = new WindowData();
            window.NativeData = data;
            data.Visual = XDefaultVisual(display, XDefaultScreen(display));
            data.Display = display;
        }

        protected internal override unsafe void CreateWindow(Window window)
        {
            var data = GetData(window);
            var colormap = XCreateColormap(display, rootWindow, data.Visual, AllocNone);
            var setWindowAttributes = new XSetWindowAttributes
            {
                colormap = colormap,
                event_mask = KeyPressMask | KeyReleaseMask | ButtonPressMask | ButtonReleaseMask |
                EnterWindowMask | LeaveWindowMask | PointerMotionMask | ExposureMask | VisibilityChangeMask |
                StructureNotifyMask | FocusChangeMask | PropertyChangeMask

            };

            var id = XCreateWindow(display, rootWindow, 0, 0, DefaultSurfaceWidth, DefaultSurfaceHeight,
                0, 0, InputOutput, data.Visual, CWEventMask | CWColormap, ref setWindowAttributes);

            var protocolCount = 1;
            XID* protocols = stackalloc XID[2];
            protocols[0] = WM_DELETE_WINDOW;
            if (_NET_WM_PING != 0)
            {
                protocols[protocolCount++] = _NET_WM_PING;
            }
            XSetWMProtocols(display, id, protocols, protocolCount);

            if (_NET_WM_WINDOW_TYPE != 0 && _NET_WM_WINDOW_TYPE_NORMAL != 0)
            {
                XID type = _NET_WM_WINDOW_TYPE_NORMAL;
                XChangeProperty(display, id, _NET_WM_WINDOW_TYPE, XA_ATOM, 32, PropModeReplace, &type, 1);
            }

            data.Id = id;
            data.Colormap = colormap;
            data.Location = GetWindowLocation(window);
            data.Size = GetWindowSize(window);

            SetWindowBorderStyle(window, WindowBorderStyle.Fixed);

            var version = XDND_VERSION;
            XChangeProperty(display, id, XdndAware, XA_ATOM, 32, PropModeReplace, &version, 1);

            if (inputMethod != IntPtr.Zero)
            {
                data.InputContext = XCreateIC(inputMethod, XNInputStyle, new IntPtr((long)(XIMPreeditNothing | XIMStatusNothing)),
                    XNClientWindow, (IntPtr)data.Id, XNFocusWindow, (IntPtr)data.Id, IntPtr.Zero);
            }
            windows.Add(id, window);
        }

        protected internal override void DestroyWindow(Window window)
        {
            if (freeLookMouseWindow == window)
            {
                freeLookMouseWindow = null;
            }
            var data = GetData(window);
            if (data.InputContext != IntPtr.Zero)
            {
                XDestroyIC(data.InputContext);
            }
            XUnmapWindow(display, data.Id);
            XDestroyWindow(display, data.Id);
            XFreeColormap(display, data.Colormap);
            windows.Remove(data.Id);
            window.NativeData = null;
        }

        protected internal override void RecreateNativeWindow(Window window)
        {
        }

        protected internal override ICollection<Window> GetWindows() => windows.Values;

        protected internal override bool IsWindowFocused(Window window)
        {
            XGetInputFocus(display, out var focused, out var _);
            return GetId(window) == focused;
        }

        protected internal override bool IsWindowVisible(Window window)
        {
            XGetWindowAttributes(display, GetId(window), out var windowAttributes);
            return windowAttributes.map_state != IsUnmapped;
        }

        protected internal override void SetWindowVisible(Window window, bool visible)
        {
            if (visible == IsWindowVisible(window))
            {
                return;
            }
            if (visible)
            {
                XMapRaised(display, GetId(window));
                WaitForVisibilityNotify(window);
            }
            else
            {
                XUnmapWindow(display, GetId(window));
                XFlush(display);
            }
        }

        protected internal override (double width, double height) GetWindowSize(Window window)
        {
            XGetWindowAttributes(display, GetId(window), out var windowAttributes);
            return (windowAttributes.width, windowAttributes.height);
        }

        protected internal override void SetWindowSize(Window window, (double width, double height) size)
        {
            UpdateWMNormalHints(window, size);
            XResizeWindow(display, GetId(window), (uint)size.width, (uint)size.height);
            XFlush(display);
        }

        protected internal override (double x, double y) GetWindowViewportSize(Window window) => GetWindowSize(window);

        protected internal override (double x, double y) GetWindowLocation(Window window)
        {
            XTranslateCoordinates(display, GetId(window), rootWindow, 0, 0, out var x, out var y, out var _);
            return (x, y);
        }

        protected internal override void SetWindowLocation(Window window, (double x, double y) location)
        {
            XID id = GetId(window);
            if (!IsWindowVisible(window))
            {
                if (XGetWMNormalHints(display, id, out var sizeHints, out var _) != 0)
                {
                    sizeHints.flags |= PPosition;
                    sizeHints.x = sizeHints.y = 0;
                    XSetWMNormalHints(display, id, ref sizeHints);
                }
            }
            XMoveWindow(display, id, (int)location.x, (int)location.y);
            XFlush(display);
        }

        protected internal override (double width, double height) GetWindowMaximumSize(Window window) => GetData(window).MaximumSize;

        protected internal override void SetWindowMaximumSize(Window window, (double width, double height) size)
        {
            var data = GetData(window);
            data.MaximumSize = size;
            UpdateWMNormalHints(window, data.Size);
            XFlush(display);
        }

        protected internal override (double width, double height) GetWindowMinimumSize(Window window) => GetData(window).MinimumSize;

        protected internal override void SetWindowMinimumSize(Window window, (double width, double height) size)
        {
            var data = GetData(window);
            data.MinimumSize = size;
            UpdateWMNormalHints(window, data.Size);
            XFlush(display);
        }

        protected internal override WindowBorderStyle GetWindowBorderStyle(Window window) => GetData(window).BorderStyle;

        protected internal override unsafe void SetWindowBorderStyle(Window window, WindowBorderStyle borderStyle)
        {
            var data = GetData(window);
            data.BorderStyle = borderStyle;
            var motifWmHints = new MotifWmHints { flags = MWM_HINTS_DECORATIONS };

            switch (borderStyle)
            {
                case WindowBorderStyle.None:
                    motifWmHints.decorations = 0;
                    break;
                case WindowBorderStyle.Sizable:
                case WindowBorderStyle.Fixed:
                    motifWmHints.decorations = 1;
                    break;
            }
            XChangeProperty(display, data.Id, _MOTIF_WM_HINTS, _MOTIF_WM_HINTS, 32, PropModeReplace,
                            &motifWmHints, sizeof(MotifWmHints) / SizeOfLong);
            UpdateWMNormalHints(window, data.Size);
            XFlush(display);
        }

        protected internal override double GetWindowOpacity(Window window)
        {
            if (GetWindowProperty(display, GetId(window), _NET_WM_WINDOW_OPACITY, XA_CARDINAL, out var dataPtr) != 1)
            {
                return 1.0;
            }
            var result = (uint)Marshal.ReadInt64(dataPtr);
            XFree(dataPtr);
            return (double)result / 0xFFFFFFFF;
        }

        protected internal override unsafe void SetWindowOpacity(Window window, double opacity)
        {
            if (opacity == 1.0)
            {
                XDeleteProperty(display, GetId(window), _NET_WM_WINDOW_OPACITY);
            }
            else
            {
                long alpha = (long)(opacity * 0xFFFFFFFF);
                XChangeProperty(display, GetId(window), _NET_WM_WINDOW_OPACITY, XA_CARDINAL, 32, PropModeReplace, &alpha, 1);
            }
        }

        protected internal override WindowSizeState GetWindowSizeState(Window window) => GetData(window).SizeState;

        protected internal override void SetWindowSizeState(Window window, WindowSizeState sizeState)
        {
            if (_NET_WM_STATE == 0 || _NET_WM_STATE_MAXIMIZED_HORZ == 0 || _NET_WM_STATE_MAXIMIZED_VERT == 0)
            {
                return;
            }
            switch (sizeState)
            {
                case WindowSizeState.Normal:
                    UpdateNetWmState(window, false, _NET_WM_STATE_MAXIMIZED_HORZ, _NET_WM_STATE_MAXIMIZED_VERT);
                    SetWindowVisible(window, true);
                    break;

                case WindowSizeState.Minimized:
                    XIconifyWindow(display, GetId(window), XDefaultScreen(display));
                    XFlush(display);
                    break;

                case WindowSizeState.Maximized:
                    UpdateNetWmState(window, true, _NET_WM_STATE_MAXIMIZED_HORZ, _NET_WM_STATE_MAXIMIZED_VERT);
                    break;
            }
        }

        protected internal override unsafe string GetWindowTitle(Window window)
        {
            var length = GetWindowProperty(display, GetId(window), _NET_WM_NAME, UTF8_STRING, out var data);
            if (length > 0)
            {
                var charCount = Encoding.UTF8.GetMaxCharCount((int)length);
                var buffer = stackalloc char[charCount];
                charCount = Encoding.UTF8.GetChars((byte*)data, (int)length, buffer, charCount);
                var result = new string(buffer, 0, charCount);
                XFree(data);
                return result;
            }
            return string.Empty;
        }

        protected internal override unsafe void SetWindowTitle(Window window, string title)
        {
            var byteCount = Encoding.UTF8.GetMaxByteCount(title.Length);
            var buffer = stackalloc byte[byteCount];
            fixed (char* p = title)
            {
                byteCount = Encoding.UTF8.GetBytes(p, title.Length, buffer, byteCount);
            }
            XChangeProperty(display, GetId(window), _NET_WM_NAME, UTF8_STRING, 8, PropModeReplace, buffer, byteCount);
            XFlush(display);
        }

        protected internal override void ActivateWindow(Window window)
        {
            if (IsWindowVisible(window))
            {
                var id = GetId(window);
                if (_NET_ACTIVE_WINDOW == 0)
                {
                    XRaiseWindow(display, id);
                    XSetInputFocus(display, id, RevertToParent, CurrentTime);
                }
                else
                {
                    SendEventToWM(id, _NET_ACTIVE_WINDOW, 1, 0, 0, 0, 0);
                }
                XFlush(display);
            }
        }

        protected internal override bool IsWindowTopMost(Window window)
        {
            if (_NET_WM_STATE == 0 || _NET_WM_STATE_ABOVE == 0)
            {
                return false;
            }
            var count = GetWindowProperty(display, GetId(window), _NET_WM_STATE, XA_ATOM, out var statesPtr);
            if (statesPtr == IntPtr.Zero)
            {
                return false;
            }
            unsafe
            {
                var states = (XID*)statesPtr;
                for (uint i = 0; i < count; i++)
                {
                    if (states[i] == _NET_WM_STATE_ABOVE)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        protected internal override unsafe void SetWindowTopMost(Window window, bool topMost)
        {
            if (_NET_WM_STATE != 0 && _NET_WM_STATE_ABOVE != 0)
            {
                UpdateNetWmState(window, topMost, _NET_WM_STATE_ABOVE, 0);
            }
        }

        protected internal override (double top, double left, double bottom, double right) GetWindowBorderSize(Window window)
        {
            var result = (top: 0.0, left: 0.0, bottom: 0.0, right: 0.0);
            var data = GetData(window);
            if (data.BorderStyle == WindowBorderStyle.None)
            {
                return result;
            }
            if (_NET_REQUEST_FRAME_EXTENTS != 0 && !IsWindowVisible(window))
            {
                SendEventToWM(data.Id, _NET_REQUEST_FRAME_EXTENTS, 0, 0, 0, 0, 0);
                var predicatePtr = Marshal.GetFunctionPointerForDelegate<XCheckIfPredicate>(IsFrameExtentsEvent);
                while (XCheckIfEvent(display, out var xevent, predicatePtr, (IntPtr)data.Id) == 0)
                {
                    if (WaitForEvents(500) != WaitStatus.Success)
                    {
                        return result;
                    }
                }
            }
            if (GetWindowProperty(display, data.Id, _NET_FRAME_EXTENTS, XA_CARDINAL, out var pointer) == 4)
            {
                unsafe
                {
                    var p = (long*)pointer;
                    result.left = p[0];
                    result.right = p[1];
                    result.top = p[2];
                    result.bottom = p[3];
                }
            }
            Free(pointer);
            return result;
        }

        protected internal override (double x, double y) ConvertFromScreen(Window window, (double x, double y) point)
        {
            XTranslateCoordinates(display, rootWindow, GetId(window), (int)point.x, (int)point.y, out var x, out var y, out var child);
            if (child != None)
            {
                XTranslateCoordinates(display, rootWindow, child, 0, 0, out var xc, out var yc, out _);
                x -= xc;
                y -= yc;
            }
            return (x, y);
        }

        protected internal override (double x, double y) ConvertToScreen(Window window, (double x, double y) point)
        {
            XTranslateCoordinates(display, GetId(window), rootWindow, (int)point.x, (int)point.y, out var x, out var y, out var _);
            return (x, y);
        }

        protected internal override void SetFreeLookMouseWindow(Window window, bool enable)
        {
            if (enable)
            {
                freeLookMouseWindow = window;
                cursorPositionBackup = GetCursorPosition(window);
                var id = GetId(window);
                var (width, height) = GetWindowSize(window);
                SetCursorPosition(window, (Math.Truncate(width / 2), Math.Truncate(height / 2)));
                XGrabPointer(display, id, 1, (uint)(ButtonPressMask | ButtonReleaseMask | PointerMotionMask),
                    GrabModeAsync, GrabModeAsync, id, hiddenCursor, CurrentTime);
            }
            else if (freeLookMouseWindow == window)
            {
                freeLookMouseWindow = null;
                XUngrabPointer(display, CurrentTime);
                SetCursorPosition(window, cursorPositionBackup);
            }
            UpdateWindowCursor(window);
            XFlush(display);
        }

        protected internal override void RequestWindowAttention(Window window)
        {
            if (_NET_WM_STATE != 0 && _NET_WM_STATE_DEMANDS_ATTENTION != 0)
            {
                SendEventToWM(GetId(window), _NET_WM_STATE, (long)_NET_WM_STATE_ADD, (long)_NET_WM_STATE_DEMANDS_ATTENTION, 0, 1, 0);
            }
        }

        protected internal override (double x, double y) GetCursorPosition(Window window)
        {
            XQueryPointer(display, GetId(window), out var _, out var _, out var _, out var _, out var childX, out var childY, out var _);
            return (childX, childY);
        }

        protected internal override void SetCursorPosition(Window window, (double x, double y) position)
        {
            var data = GetData(window);
            data.WrapCursorPosition = ((int)position.x, (int)position.y);
            XWarpPointer(display, None, data.Id, 0, 0, 0, 0, (int)position.x, (int)position.y);
            XFlush(display);
        }

        protected internal override void SetWindowCursor(Window window)
        {
            if (!window.Disposed && !window.FreeLookMouse)
            {
                UpdateWindowCursor(window);
                XFlush(display);
            }
        }

        protected internal override void CreateCursor(Cursor cursor, CursorShape shape)
        {
            uint s = shape switch
            {
                CursorShape.Arrow => XC_left_ptr,
                CursorShape.Crosshair => XC_crosshair,
                CursorShape.Hand => XC_hand2,
                CursorShape.HorizontalResize => XC_sb_h_double_arrow,
                CursorShape.IBeam => XC_xterm,
                CursorShape.VerticalResize => XC_sb_v_double_arrow,
                _ => throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.PredefinedCursorCreationFailed, shape)),
            };
            var id = XCreateFontCursor(display, s);
            if (id == 0)
            {
                throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.PredefinedCursorCreationFailed, shape));
            }
            cursor.NativeData = new CursorData(id);
        }

        protected internal override unsafe void CreateCursor(Cursor cursor, byte[] imageData, (int width, int height) size, (double x, double y) hotSpot)
        {
            var ximage = XcursorImageCreate(size.width, size.height);
            if (ximage == null)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.ImageCreationFailed));
            }

            ximage->xhot = (uint)hotSpot.x;
            ximage->yhot = (uint)hotSpot.y;

            var source = imageData;
            var target = ximage->pixels;
            var pixelCount = size.width * size.height;
            for (int i = 0, si = 0; i < pixelCount; i++, target++, si += 4)
            {
                uint alpha = source[si + 3];
                *target = (alpha << 24) |
                    (((source[si] * alpha) / 255u) << 16) |
                    (((source[si + 1] * alpha) / 255u) << 8) |
                    (((source[si + 2] * alpha) / 255u));
            }

            var id = XcursorImageLoadCursor(display, ximage);
            if (id == 0)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.CursorCreationFailed));
            }
            cursor.NativeData = new CursorData(id);
        }

        protected internal override void DestroyCursor(Cursor cursor)
        {
            XFreeCursor(display, GetId(cursor));
            cursor.NativeData = null;
        }

        protected internal override unsafe ModifierKeys GetModifierKeys()
        {
            var state = new XkbStateRec();
            XkbGetState(display, XkbUseCoreKbd, &state);
            return TranslateModifierKeys(state.mods);
        }

        protected internal override string ConvertKeycodeToString(Keycode keycode)
        {
            var scancode = (byte)scancodes[(int)keycode];
            var codePoint = UnicodeHelper.KeySymToCodePoint(XkbKeycodeToKeysym(display, scancode, 0, 0));
            if (codePoint > 0)
            {
                return char.ConvertFromUtf32(codePoint);
            }
            return string.Empty;
        }

        protected internal override string GetClipboardString()
        {
            if (XGetSelectionOwner(display, CLIPBOARD) == helperWindow)
            {
                return clipboardString ?? string.Empty;
            }

            clipboardString = null;
            XID format;
            if (CanConvertSelection(CLIPBOARD, UTF8_STRING, out var xevent))
            {
                format = UTF8_STRING;
            }
            else if (CanConvertSelection(CLIPBOARD, XA_STRING, out xevent))
            {
                format = XA_STRING;
            }
            else
            {
                return string.Empty;
            }
            var encoding = (format == UTF8_STRING) ? Encoding.UTF8 : Encoding.GetEncoding("ISO-8859-1");

            var selectionDisplay = xevent.xselection.display;
            XGetWindowProperty(selectionDisplay, xevent.xselection.requestor, xevent.xselection.property,
                0, long.MaxValue, 0, AnyPropertyType, out var actualType, out var _, out var itemCount, out var _, out var data);

            if (actualType == INCR)
            {
                var arguments = new IsPropertyNotifyEventArguments
                {
                    window = xevent.xselection.requestor,
                    atom = xevent.xselection.property,
                    state = PropertyNewValue
                };
                var builder = new StringBuilder();
                var decoder = encoding.GetDecoder();
                var charCount = encoding.GetMaxCharCount(1024);
                var charBuffer = new char[charCount];
                for (;;)
                {
                    Free(data);
                    if (WaitForPropertyNotifyEvent(selectionDisplay, arguments, 500) != WaitStatus.Success)
                    {
                        break;
                    }
                    XGetWindowProperty(selectionDisplay, xevent.xselection.requestor, xevent.xselection.property,
                        0, long.MaxValue, 1, AnyPropertyType, out actualType, out var _, out itemCount, out var _, out data);
                    if (itemCount > 0)
                    {
                        if (actualType != INCR)
                        {
                            unsafe
                            {
                                var p = (byte*)data;
                                fixed (char* c = charBuffer)
                                {
                                    while (itemCount > 0)
                                    {
                                        var bytes = Math.Min((int)itemCount, 1024);
                                        try
                                        {
                                            charCount = decoder.GetChars(p, bytes, c, charBuffer.Length, false);
                                            builder.Append(charBuffer, 0, charCount);
                                        }
                                        catch
                                        {
                                        }
                                        itemCount -= (ulong)bytes;
                                        p += bytes;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        clipboardString = builder.ToString();
                        break;
                    }
                }
            }
            else if (actualType == format)
            {
                if (itemCount > 0)
                {
                    try
                    {
                        unsafe
                        {
                            clipboardString = encoding.GetString((byte*)data, (int)itemCount);
                        }
                    }
                    catch
                    {
                    }
                }
                XDeleteProperty(selectionDisplay, xevent.xselection.requestor, xevent.xselection.property);
            }
            Free(data);
            return clipboardString ?? string.Empty;
        }

        protected internal override void SetClipboardString(string text)
        {
            clipboardString = text;
            XSetSelectionOwner(display, CLIPBOARD, helperWindow, CurrentTime);
            if (XGetSelectionOwner(display, CLIPBOARD) != helperWindow)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.SettingSelectionOwnerFailed));
            }
        }

        protected internal override double GetEventTime() => eventTime / 1000d;

        protected internal override bool WaitAndProcessEvents()
        {
            while (XPending(display) == 0)
            {
                if (WaitForEvents(-1) != WaitStatus.Success)
                {
                    return false;
                }
            }
            return ProcessEvents();
        }

        protected internal override bool WaitAndProcessEvents(double timeout)
        {
            while (XPending(display) == 0)
            {
                if (WaitForEvents((int)(timeout * 1000)) != WaitStatus.Success)
                {
                    return false;
                }
            }
            return ProcessEvents();
        }

        protected internal override unsafe bool ProcessEvents()
        {
            var result = false;
            var eventCount = XPending(display);
            while (eventCount-- > 0)
            {
                result = true;
                XNextEvent(display, out var xevent);
                var eventType = xevent.type;
                switch (eventType)
                {
                    case GenericEvent:
                        continue;

                    case SelectionClear:
                        HandleSelectionClear(ref xevent);
                        continue;

                    case SelectionRequest:
                        HandleSelectionRequest(ref xevent);
                        continue;

                    default:
                        break;
                }

                if (!windows.TryGetValue(xevent.xany.window, out var window))
                {
                    continue;
                }
                var data = GetData(window);
                switch (eventType)
                {
                    case KeyPress:
                        eventTime = xevent.xkey.time;
                        var keycode = TranslateKeycode(xevent.xkey.keycode);
                        var modifiers = GetModifierKeys();
                        if (data.InputContext != IntPtr.Zero)
                        {
                            if (data.LastKeyTime < xevent.xkey.time)
                            {
                                if (keycode != Keycode.Unknown)
                                {
                                    KeyDownEventOccurred(window, keycode, modifiers);
                                }
                                data.LastKeyTime = xevent.xkey.time;
                            }
                            if (XFilterEvent(ref xevent, None) == 0)
                            {
                                var byteBuffer = stackalloc byte[128];
                                var count = Xutf8LookupString(data.InputContext, ref xevent.xkey, new IntPtr(byteBuffer), 127, IntPtr.Zero, out var status);
                                switch (status)
                                {
                                    case XBufferOverflow:
                                        Xutf8ResetIC(data.InputContext);
                                        break;

                                    case XLookupChars:
                                    case XLookupBoth:
                                        var charCount = Encoding.UTF8.GetMaxCharCount(count);
                                        var charBuffer = stackalloc char[charCount];
                                        charCount = Encoding.UTF8.GetChars(byteBuffer, count, charBuffer, charCount);
                                        for (int i = 0; i < charCount; i++)
                                        {
                                            var c = charBuffer[i];
                                            if (char.IsHighSurrogate(c))
                                            {
                                                if (i + 1 < charCount)
                                                {
                                                    HandleTextInputEvent(window, char.ConvertToUtf32(c, charBuffer[++i]), modifiers);
                                                }
                                            }
                                            else
                                            {
                                                HandleTextInputEvent(window, c, modifiers);
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                        else
                        {
                            if (keycode != Keycode.Unknown)
                            {
                                KeyDownEventOccurred(window, keycode, modifiers);
                            }
                            XLookupString(ref xevent.xkey, IntPtr.Zero, 0, out var keySym, IntPtr.Zero);
                            var codePoint = UnicodeHelper.KeySymToCodePoint(keySym);
                            if (codePoint > 0)
                            {
                                HandleTextInputEvent(window, codePoint, modifiers);
                            }
                        }
                        break;

                    case KeyRelease:
                        eventTime = xevent.xkey.time;
                        keycode = TranslateKeycode(xevent.xkey.keycode);

                        if (keycode != Keycode.Unknown)
                        {
                            KeyUpEventOccurred(window, keycode, GetModifierKeys());
                        }
                        break;

                    case ButtonPress:
                        eventTime = xevent.xbutton.time;
                        var button = xevent.xbutton.button;
                        switch (button)
                        {
                            case 1:
                                MouseDownEventOccurred(window, MouseButton.Left);
                                break;
                            case 2:
                                MouseDownEventOccurred(window, MouseButton.Middle);
                                break;
                            case 3:
                                MouseDownEventOccurred(window, MouseButton.Right);
                                break;
                            case 4:
                                ScrollEventOccurred(window, 0, -1);
                                break;
                            case 5:
                                ScrollEventOccurred(window, 0, 1);
                                break;
                            case 6:
                                ScrollEventOccurred(window, -1, 0);
                                break;
                            case 7:
                                ScrollEventOccurred(window, 1, 0);
                                break;
                            default:
                                MouseDownEventOccurred(window, (int)button - 8 + MouseButton.Middle + 1);
                                break;
                        }
                        break;

                    case ButtonRelease:
                        eventTime = xevent.xbutton.time;
                        button = xevent.xbutton.button;
                        switch (button)
                        {
                            case 1:
                                MouseUpEventOccurred(window, MouseButton.Left);
                                break;
                            case 2:
                                MouseUpEventOccurred(window, MouseButton.Middle);
                                break;
                            case 3:
                                MouseUpEventOccurred(window, MouseButton.Right);
                                break;
                            default:
                                MouseUpEventOccurred(window, (int)button - 8 + MouseButton.Middle + 1);
                                break;
                        }
                        break;

                    case MotionNotify:
                        {
                            if (!data.MouseEntered)
                            {
                                data.MouseEntered = true;
                                eventTime = xevent.xmotion.time;
                                MouseEnterEventOccurred(window);
                            }
                            var (x, y) = (xevent.xmotion.x, xevent.xmotion.y);
                            var (wx, wy) = data.WrapCursorPosition;
                            if (x != wx || y != wy)
                            {
                                if (window.FreeLookMouse)
                                {
                                    if (freeLookMouseWindow != window)
                                    {
                                        if (IsCursorInClientArea(window))
                                        {
                                            SetFreeLookMouseWindow(window, true);
                                            data.LastCursorPosition = (x, y);
                                        }
                                        break;
                                    }
                                    eventTime = xevent.xmotion.time;
                                    var (cx, cy) = data.LastCursorPosition;
                                    var (dx, dy) = (x - cx, y - cy);
                                    MouseMoveEventOccurred(window, window.FreeLookPosition.x + dx, window.FreeLookPosition.y + dy);
                                }
                                else
                                {
                                    eventTime = xevent.xmotion.time;
                                    MouseMoveEventOccurred(window, x, y);
                                }
                            }
                            data.LastCursorPosition = (x, y);
                        }
                        break;

                    case EnterNotify:
                        if (!IsWindowFocused(window))
                        {
                            break;
                        }
                        if (!data.MouseEntered && (xevent.xcrossing.state & (Button1Mask | Button2Mask | Button3Mask)) == 0)
                        {
                            data.MouseEntered = true;
                            eventTime = xevent.xcrossing.time;
                            MouseEnterEventOccurred(window);
                            if (!window.FreeLookMouse)
                            {
                                MouseMoveEventOccurred(window, xevent.xcrossing.x, xevent.xcrossing.y);
                            }
                        }
                        break;

                    case LeaveNotify:
                        if (data.MouseEntered && (xevent.xcrossing.state & (Button1Mask | Button2Mask | Button3Mask)) == 0)
                        {
                            data.MouseEntered = false;
                            eventTime = xevent.xcrossing.time;
                            MouseLeaveEventOccurred(window);
                        }
                        break;

                    case FocusIn:
                        if (window.FreeLookMouse && IsCursorInClientArea(window))
                        {
                            SetFreeLookMouseWindow(window, true);
                        }
                        if (data.InputContext != IntPtr.Zero)
                        {
                            XSetICFocus(data.InputContext);
                        }
                        GotFocusEventOccurred(window);
                        break;

                    case FocusOut:
                        if (window.FreeLookMouse)
                        {
                            SetFreeLookMouseWindow(window, false);
                        }
                        if (data.InputContext != IntPtr.Zero)
                        {
                            XUnsetICFocus(data.InputContext);
                        }
                        LostFocusEventOccurred(window);
                        break;

                    case UnmapNotify:
                    case MapNotify:
                        VisibleChangedEventOccurred(window);
                        break;

                    case ConfigureNotify:
                        (double x, double y) eventLocation = (xevent.xconfigure.x, xevent.xconfigure.y);
                        (double width, double height) eventSize = (xevent.xconfigure.width, xevent.xconfigure.height);
                        if (eventSize.width != data.Size.width || eventSize.height != data.Size.height)
                        {
                            window.RenderingContext?.UpdateRenderingData(window);
                            ResizeEventOccurred(window);
                            FramebufferResizeEventOccurred(window);
                            data.Size = eventSize;
                        }
                        if ((eventLocation.x > 0 || eventLocation.y > 0) &&
                            (eventLocation.x != data.Location.x || eventLocation.y != data.Location.y))
                        {
                            window.RenderingContext?.UpdateRenderingData(window);
                            MoveEventOccurred(window);
                            data.Location = eventLocation;
                        }
                        break;

                    case PropertyNotify:
                        var atom = xevent.xproperty.atom;
                        var state = xevent.xproperty.state;
                        if (atom == WM_STATE && state == PropertyNewValue)
                        {
                            if (GetWindowProperty(display, xevent.xproperty.window, WM_STATE, WM_STATE, out var pointer) >= 2)
                            {
                                var value = Marshal.ReadInt32(pointer);
                                if (value == IconicState)
                                {
                                    data.SizeState = WindowSizeState.Minimized;
                                    SizeStateChangedEventOccurred(window);
                                }
                                else if (value == NormalState && data.SizeState != WindowSizeState.Normal)
                                {
                                    data.SizeState = WindowSizeState.Normal;
                                    SizeStateChangedEventOccurred(window);
                                }
                            }
                            Free(pointer);
                        }
                        else if (atom == _NET_WM_STATE)
                        {
                            var isMaximizedH = false;
                            var isMaximizedV = false;

                            var count = GetWindowProperty(display, data.Id, _NET_WM_STATE, XA_ATOM, out var pointer);
                            if (count > 0)
                            {
                                for (ulong i = 0; i < count; i++)
                                {
                                    var a = ReadXID(pointer, (int)i * SizeOfLong);
                                    if (a == _NET_WM_STATE_MAXIMIZED_HORZ)
                                    {
                                        isMaximizedH = true;
                                    }
                                    else if (a == _NET_WM_STATE_MAXIMIZED_VERT)
                                    {
                                        isMaximizedV = true;
                                    }
                                }
                            }
                            Free(pointer);
                            var isMaximized = isMaximizedH && isMaximizedH == isMaximizedV;

                            if (isMaximized && data.SizeState == WindowSizeState.Normal)
                            {
                                data.SizeState = WindowSizeState.Maximized;
                                SizeStateChangedEventOccurred(window);
                            }
                            else if (!isMaximized && data.SizeState != WindowSizeState.Normal)
                            {
                                data.SizeState = WindowSizeState.Normal;
                                SizeStateChangedEventOccurred(window);
                            }
                        }
                        break;

                    case ClientMessage:
                        if (xevent.xclient.message_type == WM_PROTOCOLS)
                        {
                            var eventData = (XID)xevent.xclient.data[0];
                            if (eventData == WM_DELETE_WINDOW)
                            {
                                CloseRequestOccurred(window);
                            }
                            else if (eventData == _NET_WM_PING)
                            {
                                xevent.xany.window = rootWindow;
                                XSendEvent(display, rootWindow, 0, SubstructureNotifyMask | SubstructureRedirectMask, ref xevent);
                            }
                        }
                        else if (xevent.xclient.message_type == XdndEnter)
                        {
                            var isList = (xevent.xclient.data[1] & 1) != 0;
                            XdndSource = (XID)xevent.xclient.data[0];
                            XdndVersion = (XID)xevent.xclient.data[1] >> 24;
                            XdndFormat = None;

                            if (XdndVersion > XDND_VERSION)
                            {
                                break;
                            }

                            ulong count;
                            XID* formats;
                            if (isList)
                            {
                                count = GetWindowProperty(display, XdndSource, XdndTypeList, XA_ATOM, out var pointer);
                                formats = (XID*)pointer;
                            }
                            else
                            {
                                count = 3;
                                formats = (XID*)&xevent.xclient.data[2];
                            }
                            for (int i = 0; i < (int)count; i++)
                            {
                                if (formats[i] == text_uri_list)
                                {
                                    XdndFormat = text_uri_list;
                                    break;
                                }
                            }
                            if (isList && formats != null)
                            {
                                XFree(new IntPtr(formats));
                            }
                        }
                        else if (xevent.xclient.message_type == XdndDrop)
                        {
                            if (XdndVersion > XDND_VERSION)
                            {
                                break;
                            }
                            if (XdndFormat != 0)
                            {
                                var time = (XdndVersion >= 1) ? (XID)xevent.xclient.data[2] : CurrentTime;
                                XConvertSelection(display, XdndSelection, XdndFormat, XdndSelection, data.Id, time);
                            }
                            else if (XdndVersion >= 2)
                            {
                                var reply = new XEvent { type = ClientMessage };
                                reply.xclient.window = XdndSource;
                                reply.xclient.message_type = XdndFinished;
                                reply.xclient.format = 32;
                                reply.xclient.data[0] = (long)data.Id;
                                reply.xclient.data[1] = 0;

                                XSendEvent(display, XdndSource, 0, NoEventMask, ref reply);
                                XFlush(display);
                            }
                        }
                        else if (xevent.xclient.message_type == XdndPosition)
                        {
                            if (XdndVersion > XDND_VERSION)
                            {
                                break;
                            }
                            var x = (int)((xevent.xclient.data[2] >> 16) & 0xFFFF);
                            var y = (int)(xevent.xclient.data[2] & 0xFFFF);
                            XTranslateCoordinates(display, rootWindow, data.Id, x, y, out var xw, out var yw, out var _);
                            data.LastDragPosition = (xw, yw);

                            var reply = new XEvent { type = ClientMessage };
                            reply.xclient.window = XdndSource;
                            reply.xclient.message_type = XdndStatus;
                            reply.xclient.format = 32;
                            reply.xclient.data[0] = (long)data.Id;
                            if (XdndFormat != 0)
                            {
                                reply.xclient.data[1] = 1;
                                if (XdndVersion >= 2)
                                {
                                    reply.xclient.data[4] = (long)XdndActionCopy;
                                }
                            }

                            XSendEvent(display, XdndSource, 0, NoEventMask, ref reply);
                            XFlush(display);
                        }
                        break;

                    case SelectionNotify:
                        if (xevent.xselection.property == XdndSelection)
                        {
                            var length = GetWindowProperty(display, xevent.xselection.requestor,
                                xevent.xselection.property, xevent.xselection.target, out var pointer);
                            if (length > 0)
                            {
                                var p = (byte*)pointer;
                                var i = 0;
                                var stringList = new List<string>();
                                while (i < (int)length)
                                {
                                    var start = i;
                                    while (++i < (int)length && p[i] != '\n' && p[i] != '\r') ;
                                    var end = i;
                                    while (++i < (int)length && (p[i] == '\n' || p[i] == '\r')) ;
                                    if (start == end || p[start] == '#')
                                    {
                                        continue;
                                    }
                                    var j = start;
                                    foreach (var c in FileProtocol)
                                    {
                                        if (j >= end || p[j] != c)
                                        {
                                            break;
                                        }
                                        j++;
                                    }
                                    var isUri = j - start == FileProtocol.Length;
                                    if (isUri)
                                    {
                                        while (j < end && p[j] == '/')
                                        {
                                            j++;
                                        }
                                        start = j - 1;
                                    }
                                    var byteBuffer = stackalloc byte[end - start];
                                    var byteCount = 0;
                                    while (start < end)
                                    {
                                        if (isUri && p[start] == '%' && start + 2 < end)
                                        {
                                            if (TryConvertDigitToByte(p[start + 1], out var b0) && TryConvertDigitToByte(p[start + 2], out var b1))
                                            {
                                                var c = (byte)((b0 << 4) | b1);
                                                start += 3;
                                                byteBuffer[byteCount++] = c;
                                            }
                                            else
                                            {
                                                byteBuffer[byteCount++] = p[start++];
                                                byteBuffer[byteCount++] = p[start++];
                                                byteBuffer[byteCount++] = p[start++];
                                            }
                                        }
                                        else
                                        {
                                            byteBuffer[byteCount++] = p[start++];
                                        }
                                    }
                                    var charCount = Encoding.UTF8.GetMaxCharCount(byteCount);
                                    var charBuffer = stackalloc char[charCount];
                                    var count = Encoding.UTF8.GetChars(byteBuffer, byteCount, charBuffer, charCount);
                                    var s = new string(charBuffer, 0, count);
                                    stringList.Add(s);
                                }
                                if (stringList.Count > 0)
                                {
                                    FileDropEventOccurred(window, data.LastDragPosition.x, data.LastDragPosition.y, stringList.ToArray());
                                }
                            }
                            Free(pointer);
                            if (XdndVersion >= 2)
                            {
                                var reply = new XEvent { type = ClientMessage };
                                reply.xclient.window = XdndSource;
                                reply.xclient.message_type = XdndFinished;
                                reply.xclient.format = 32;
                                reply.xclient.data[0] = (long)data.Id;
                                reply.xclient.data[1] = (long)length;
                                reply.xclient.data[2] = (long)XdndActionCopy;

                                XSendEvent(display, XdndSource, 0, NoEventMask, ref reply);
                                XFlush(display);
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
            if (freeLookMouseWindow != null)
            {
                var (width, height) = GetWindowSize(freeLookMouseWindow);
                var freeLookData = GetData(freeLookMouseWindow);
                var (x, y) = ((int)Math.Truncate(width / 2), (int)Math.Truncate(height / 2));
                var (cx, cy) = freeLookData.LastCursorPosition;
                if (cx != x || cy != y)
                {
                    SetCursorPosition(freeLookMouseWindow, (x, y));
                }
            }
            XFlush(display);
            return result;
        }

        private bool IsCursorInClientArea(Window window)
        {
            var (x, y) = GetCursorPosition(window);
            if (x < 0 || y < 0)
            {
                return false;
            }
            var (width, height) = GetWindowSize(window);
            return x < width && y < height;
        }

        private void HandleTextInputEvent(Window window, int codePoint, ModifierKeys modifiers)
        {
            if (codePoint < 32 && (modifiers & ModifierKeys.Control) != 0)
            {
                codePoint += '@';
            }
            TextInputEventOccurred(window, codePoint, modifiers);
        }

        private static bool TryConvertDigitToByte(byte d, out byte result)
        {
            if (d >= '0' && d <= '9')
            {
                d -= (byte)'0';
            }
            else if (d >= 'A' && d <= 'F')
            {
                d -= 'A' - 10;
            }
            else if (d >= 'a' && d <= 'f')
            {
                d -= 'a' - 10;
            }
            else
            {
                result = 0;
                return false;
            }
            result = d;
            return true;
        }

        private Keycode TranslateKeycode(uint keycode) => (keycode < 256) ? keycodeMappings[keycode] : Keycode.Unknown;

        protected internal override void UnblockProcessEvents()
        {
            var xevent = new XEvent { type = ClientMessage };
            xevent.xclient.window = helperWindow;
            xevent.xclient.message_type = NULL;
            xevent.xclient.format = 32;

            XSendEvent(display, helperWindow, 0, NoEventMask, ref xevent);
            XFlush(display);
        }

        private void UpdateWindowCursor(Window window)
        {
            var id = GetId(window);
            if (freeLookMouseWindow == window)
            {
                XDefineCursor(display, id, hiddenCursor);
            }
            else
            {
                var cursor = window.Cursor;
                if (cursor == null)
                {
                    XUndefineCursor(display, id);
                }
                else
                {
                    XDefineCursor(display, id, GetId(cursor));
                }
            }
        }

        private bool CanConvertSelection(XID selection, XID format, out XEvent xevent)
        {
            XConvertSelection(display, selection, format, NStuff_SELECTION, helperWindow, CurrentTime);
            while (XCheckTypedWindowEvent(display, helperWindow, SelectionNotify, out xevent) == 0)
            {
                if (WaitForEvents(-1) == WaitStatus.Error)
                {
                    return false;
                }
            }
            return xevent.xselection.property != None;
        }

        private unsafe ModifierKeys TranslateModifierKeys(uint mask)
        {
            var result = ModifierKeys.None;
            if ((mask & ShiftMask) != 0)
            {
                result |= ModifierKeys.Shift;
            }
            if ((mask & ControlMask) != 0)
            {
                result |= ModifierKeys.Control;
            }
            if ((mask & Mod1Mask) != 0)
            {
                result |= ModifierKeys.Alternate;
            }
            if ((mask & Mod2Mask) != 0)
            {
                result |= ModifierKeys.Command;
            }
            if ((mask & Mod4Mask) != 0)
            {
                result |= ModifierKeys.Command;
            }
            if ((mask & Mod5Mask) != 0)
            {
                result |= ModifierKeys.Alternate;
            }
            if ((mask & LockMask) != 0)
            {
                result |= ModifierKeys.CapsLock;
            }
            return result;
        }

        private int IsFrameExtentsEvent(IntPtr display, ref XEvent xevent, IntPtr pointer)
        {
            return (xevent.type == PropertyNotify &&
                xevent.xproperty.state == PropertyNewValue &&
                xevent.xproperty.window == (XID)pointer &&
                xevent.xproperty.atom == _NET_FRAME_EXTENTS) ? 1 : 0;

        }

        private unsafe void UpdateNetWmState(Window window, bool add, XID atom1, XID atom2)
        {
            var id = GetId(window);
            var atom1Present = false;
            var atom2Present = false;
            var atoms = stackalloc XID[16];
            var atomCount = 0;

            var count = GetWindowProperty(display, id, _NET_WM_STATE, XA_ATOM, out var state);
            for (ulong i = 0; i < count; i++)
            {
                var atom = ReadXID(state, (int)i * SizeOfLong);
                if (atom == atom1)
                {
                    atom1Present = true;
                }
                else if (atom2 != 0 && atom == atom2)
                {
                    atom2Present = true;
                }
                else
                {
                    atoms[atomCount++] = atom;
                }
            }
            Free(state);

            if (add == atom1Present && atom1Present == atom2Present)
            {
                return;
            }

            if (IsWindowVisible(window))
            {
                var action = (long)(add ? _NET_WM_STATE_ADD : _NET_WM_STATE_REMOVE);
                if (add != atom1Present && add != atom2Present)
                {
                    SendEventToWM(id, _NET_WM_STATE, action, (long)atom1, (long)atom2, 1, 0);
                }
                else if (add != atom1Present && add == atom2Present)
                {
                    SendEventToWM(id, _NET_WM_STATE, action, (long)atom1, 0, 1, 0);
                }
                else
                {
                    SendEventToWM(id, _NET_WM_STATE, action, (long)atom2, 0, 1, 0);
                }
            }
            else
            {
                if (add)
                {
                    atoms[atomCount++] = atom1;
                    if (atom2 != 0)
                    {
                        atoms[atomCount++] = atom2;
                    }
                }
                if (atomCount > 0)
                {
                    XChangeProperty(display, id, _NET_WM_STATE, XA_ATOM, 32, PropModeReplace, atoms, atomCount);
                }
                else
                {
                    XDeleteProperty(display, id, _NET_WM_STATE);
                }
            }
        }

        private unsafe void SendEventToWM(XID window, XID type, long a, long b, long c, long d, long e)
        {
            var xevent = new XEvent { type = ClientMessage };
            xevent.xclient.window = window;
            xevent.xclient.message_type = type;
            xevent.xclient.format = 32;
            xevent.xclient.data[0] = a;
            xevent.xclient.data[1] = b;
            xevent.xclient.data[2] = c;
            xevent.xclient.data[3] = d;
            xevent.xclient.data[4] = e;
            XSendEvent(display, rootWindow, 0, SubstructureNotifyMask | SubstructureRedirectMask, ref xevent);
        }

        private void UpdateWMNormalHints(Window window, (double width, double height) size)
        {
            var sizeHints = new XSizeHints();
            var data = GetData(window);
            switch (data.BorderStyle)
            {
                case WindowBorderStyle.None:
                case WindowBorderStyle.Sizable:
                    if (data.MinimumSize.width > 0 || data.MinimumSize.height > 0)
                    {
                        sizeHints.flags |= PMinSize;
                        sizeHints.min_width = (int)data.MinimumSize.width;
                        sizeHints.min_height = (int)data.MinimumSize.height;
                    }
                    if (data.MaximumSize.width > 0 || data.MaximumSize.height > 0)
                    {
                        sizeHints.flags |= PMaxSize;
                        sizeHints.max_width = (int)data.MaximumSize.width;
                        sizeHints.max_height = (int)data.MaximumSize.height;
                    }
                    break;

                case WindowBorderStyle.Fixed:
                    sizeHints.flags = PMinSize | PMaxSize;
                    sizeHints.min_width = (int)size.width;
                    sizeHints.min_height = (int)size.height;
                    sizeHints.max_width = (int)size.width;
                    sizeHints.max_height = (int)size.height;
                    break;
            }
            sizeHints.flags |= PWinGravity;
            sizeHints.win_gravity = StaticGravity;
            XSetWMNormalHints(display, data.Id, ref sizeHints);
        }

        private bool WaitForVisibilityNotify(Window window)
        {
            while (XCheckTypedWindowEvent(display, GetId(window), VisibilityNotify, out var _) == 0)
            {
                var status = WaitForEvents(500);
                if (status != WaitStatus.Success)
                {
                    return false;
                }
            }
            return true;
        }

        private void PushSelectionToClipboardManager()
        {
            if (XGetSelectionOwner(display, CLIPBOARD) == helperWindow)
            {
                XConvertSelection(display, CLIPBOARD_MANAGER, SAVE_TARGETS, None, helperWindow, CurrentTime);
                var predicatePtr = Marshal.GetFunctionPointerForDelegate<XCheckIfPredicate>(IsSelectionEvent);
                for (;;)
                {
                    while (XCheckIfEvent(display, out var xevent, predicatePtr, IntPtr.Zero) != 0)
                    {
                        switch (xevent.type)
                        {
                            case SelectionClear:
                                HandleSelectionClear(ref xevent);
                                break;

                            case SelectionRequest:
                                HandleSelectionRequest(ref xevent);
                                break;

                            case SelectionNotify:
                                if (xevent.xselection.target == SAVE_TARGETS)
                                {
                                    return;
                                }
                                break;
                        }
                    }
                    WaitForEvents(-1);
                }
            }
        }

        private int IsSelectionEvent(IntPtr display, ref XEvent xevent, IntPtr pointer)
        {
            if (xevent.xany.window != helperWindow)
            {
                return 0;
            }
            switch (xevent.type)
            {
                case SelectionClear:
                case SelectionNotify:
                case SelectionRequest:
                    return 1;

                default:
                    return 0;
            }
        }

        private void HandleSelectionClear(ref XEvent xevent)
        {
            if (xevent.xselectionclear.selection != XA_PRIMARY)
            {
                clipboardString = null;
            }
        }

        private struct ChangePropertyData
        {
            public int format;
            public byte[] data;
            public int count;
            public XID property;
            public XID type;
        }

        private void HandleSelectionRequest(ref XEvent xevent)
        {
            var reply = new XEvent();
            var requestDisplay = xevent.xselectionrequest.display;
            var requestor = xevent.xselectionrequest.requestor;
            reply.xselection.type = SelectionNotify;
            reply.xselection.display = requestDisplay;
            reply.xselection.requestor = requestor;
            reply.xselection.selection = xevent.xselectionrequest.selection;
            reply.xselection.target = xevent.xselectionrequest.target;
            reply.xselection.time = xevent.xselectionrequest.time;
            reply.xselection.property = xevent.xselectionrequest.property;
            if (reply.xselection.property == None)
            {
                reply.xselection.property = reply.xselection.target;
            }

            if (xevent.xselectionrequest.selection == XA_PRIMARY || clipboardString == null)
            {
                reply.xselection.property = None;
                XSendEvent(requestDisplay, requestor, 0, 0, ref reply);
                XFlush(requestDisplay);
                return;
            }

            ChangePropertyData[] data;
            if (xevent.xselectionrequest.target == MULTIPLE)
            {
                var count = GetWindowProperty(requestDisplay, requestor, xevent.xselectionrequest.property, ATOM_PAIR, out var pointer);
                data = new ChangePropertyData[count / 2];
                unsafe
                {
                    var targets = (XID*)pointer;
                    for (int i = 0, n = 0; i < (int)count; i += 2, n++)
                    {
                        data[n] = ConvertSelection(targets[i + 1], targets[i]);
                    }
                }
                Free(pointer);
            }
            else
            {
                data = new ChangePropertyData[1];
                data[0] = ConvertSelection(reply.xselection.property, reply.xselection.target);
                if (data[0].data == failedConversionData)
                {
                    reply.xselection.property = None;
                    XSendEvent(requestDisplay, requestor, 0, 0, ref reply);
                    XFlush(requestDisplay);
                    return;
                }
            }

            var maxDataSize = GetMaxDataSize(requestDisplay);
            for (int i = 0; i < data.Length; i++)
            {
                var d = data[i];
                var dataSize = d.count * (d.format >> 3);
                if (dataSize > maxDataSize)
                {
                    unsafe
                    {
                        var value = stackalloc long[1];
                        value[0] = dataSize;
                        XChangeProperty(requestDisplay, requestor, d.property, INCR, 32, PropModeReplace, value, 1);
                    }
                    XSelectInput(requestDisplay, requestor, PropertyChangeMask);
                }
                else
                {
                    unsafe
                    {
                        fixed (byte* p = d.data)
                        {
                            XChangeProperty(requestDisplay, requestor, d.property, d.type, d.format, PropModeReplace, p, d.count);
                        }
                    }
                    data[i] = new ChangePropertyData();
                }
            }

            XSendEvent(requestDisplay, requestor, 0, 0, ref reply);
            XFlush(requestDisplay);

            for (int i = 0; i < data.Length; i++)
            {
                var d = data[i];
                if (d.data == null)
                {
                    continue;
                }
                var arguments = new IsPropertyNotifyEventArguments { window = requestor, atom = d.property, state = PropertyDelete };
                var status = WaitForPropertyNotifyEvent(requestDisplay, arguments, 500);
                if (status != WaitStatus.Success)
                {
                    break;
                }
                var formatBytes = d.format >> 3;
                var dataSize = d.count * formatBytes;
                var offset = 0;
                unsafe
                {
                    do
                    {
                        var blockSize = Math.Min(dataSize, maxDataSize);
                        var count = blockSize / formatBytes;
                        fixed (byte* p = d.data)
                        {
                            XChangeProperty(requestDisplay, requestor, d.property, d.type, d.format, PropModeAppend, &p[offset], count);
                        }
                        offset += count * ((d.format == 32) ? SizeOfLong : formatBytes);
                        dataSize -= blockSize;

                        XFlush(requestDisplay);
                        status = WaitForPropertyNotifyEvent(requestDisplay, arguments, 500);
                        if (status != WaitStatus.Success)
                        {
                            break;
                        }
                    }
                    while (dataSize > 0);
                    if (dataSize > 0)
                    {
                        break;
                    }
                    XChangeProperty(requestDisplay, requestor, d.property, d.type, d.format, PropModeReplace, null, 0);
                }
            }
            XSelectInput(requestDisplay, requestor, 0);
            XSync(requestDisplay, 0);
        }

        private static int GetMaxDataSize(IntPtr display)
        {
            return Math.Min(0xffffff, (int)((XMaxRequestSize(display) - 25) * 4));
        }

        private ChangePropertyData ConvertSelection(XID property, XID type)
        {
            var result = new ChangePropertyData
            {
                property = property,
                type = XA_ATOM,
                format = 32,
                count = 1,
                data = failedConversionData
            };
            if (type == XA_STRING)
            {
                try
                {
                    result.data = Encoding.GetEncoding("ISO-8859-1").GetBytes(clipboardString);
                    result.type = type;
                    result.format = 8;
                    result.count = result.data.Length;
                }
                catch
                {
                }
            }
            else if (type == UTF8_STRING)
            {
                try
                {
                    result.data = Encoding.UTF8.GetBytes(clipboardString);
                    result.type = type;
                    result.format = 8;
                    result.count = result.data.Length;
                }
                catch
                {
                }
            }
            else if (type == TARGETS)
            {
                result.data = targetsConversionData;
                result.count = 4;
            }
            else if (type == SAVE_TARGETS)
            {
                result.type = NULL;
                result.count = 0;
            }
            return result;
        }

        private struct IsPropertyNotifyEventArguments
        {
            public XID window;
            public XID atom;
            public int state;
        }

        private static unsafe int IsPropertyNotifyEvent(IntPtr display, ref XEvent xevent, IntPtr pointer)
        {
            var arguments = (IsPropertyNotifyEventArguments*)pointer;
            return (xevent.type == PropertyNotify &&
                xevent.xproperty.state == arguments->state &&
                xevent.xproperty.window == arguments->window &&
                xevent.xproperty.atom == arguments->atom) ? 1 : 0;

        }

        private unsafe WaitStatus WaitForPropertyNotifyEvent(IntPtr display, IsPropertyNotifyEventArguments arguments, int timeoutInMilliseconds)
        {
            var predicatePtr = Marshal.GetFunctionPointerForDelegate<XCheckIfPredicate>(IsPropertyNotifyEvent);
            while (XCheckIfEvent(display, out var xevent, predicatePtr, new IntPtr(&arguments)) == 0)
            {
                var status = WaitForEvents(timeoutInMilliseconds);
                if (status != WaitStatus.Success)
                {
                    return status;
                }
            }
            return WaitStatus.Success;
        }

        private enum WaitStatus
        {
            Error,
            Timeout,
            Success
        }

        private unsafe WaitStatus WaitForEvents(int timeoutInMilliseconds)
        {
            var events = new epoll_event();
            var n = epoll_wait(epoll, &events, 1, timeoutInMilliseconds);
            if (n == -1)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            else if (n == 0)
            {
                return WaitStatus.Timeout;
            }
            if ((events.events & EPOLLERR) != 0 ||
                (events.events & EPOLLHUP) != 0 ||
                (events.events & EPOLLIN) == 0)
            {
                return WaitStatus.Error;
            }
            return WaitStatus.Success;
        }

        private unsafe bool HasUsableInputMethodStyle()
        {
            XIMStyles* styles;
            if (XGetIMValues(inputMethod, "queryInputStyle", out var pointer, IntPtr.Zero) != IntPtr.Zero)
            {
                return false;
            }
            styles = (XIMStyles*)pointer;
            var found = false;
            for (int i = 0; i < styles->count_styles; i++)
            {
                if (styles->supported_styles[i] == (XIMPreeditNothing | XIMStatusNothing))
                {
                    found = true;
                    break;
                }
            }
            Free(pointer);
            return found;
        }

        private static unsafe IntPtr CreateXNProperty(string value)
        {
            var result = Marshal.AllocHGlobal(value.Length + 1);
            var p = (byte*)result;
            for (int i = 0; i < value.Length; i++)
            {
                p[i] = (byte)value[i];
            }
            p[value.Length] = 0;
            return result;
        }

        private static void Free(IntPtr pointer)
        {
            if (pointer != IntPtr.Zero)
            {
                XFree(pointer);
            }
        }

        private static XID ReadXID(IntPtr ptr, int offset) => (XID)Marshal.ReadInt64(ptr, offset);

        private static XID GetWindowProperty(IntPtr display, XID windowHandle, XID @property, XID propertyType, out IntPtr result)
        {
            XGetWindowProperty(display, windowHandle, @property, 0, long.MaxValue, 0, propertyType,
                               out var actualType, out var _, out var itemCount, out var _, out result);
            return (propertyType != AnyPropertyType && actualType != propertyType) ? 0 : itemCount;
        }

        private static int NameToInt(string name)
        {
            var b0 = (byte)name[0];
            var b1 = (byte)name[1];
            var b2 = (byte)name[2];
            var b3 = (byte)name[3];
            return b0 | (b1 << 8) | (b2 << 16) | (b3 << 24);
        }

        private static unsafe int NameToInt(sbyte* name)
        {
            var b0 = (byte)name[0];
            var b1 = (byte)name[1];
            var b2 = (byte)name[2];
            var b3 = (byte)name[3];
            return b0 | (b1 << 8) | (b2 << 16) | (b3 << 24);
        }

        private static WindowData GetData(Window window) =>
            (WindowData?)window.NativeData ?? throw new InvalidOperationException();

        private static XID GetId(Window window) => GetData(window).Id;

        private static XID GetId(Cursor cursor) =>
            ((CursorData?)cursor.NativeData ?? throw new InvalidOperationException()).Id;

        private unsafe void InitializeKeycodeMappings()
        {
            var nameToKeycode = new Dictionary<int, Keycode>
            {
                [NameToInt("TLDE")] = Keycode.Backquote,
                [NameToInt("AE01")] = Keycode.One,
                [NameToInt("AE02")] = Keycode.Two,
                [NameToInt("AE03")] = Keycode.Three,
                [NameToInt("AE04")] = Keycode.Four,
                [NameToInt("AE05")] = Keycode.Five,
                [NameToInt("AE06")] = Keycode.Six,
                [NameToInt("AE07")] = Keycode.Seven,
                [NameToInt("AE08")] = Keycode.Eight,
                [NameToInt("AE09")] = Keycode.Nine,
                [NameToInt("AE10")] = Keycode.Zero,
                [NameToInt("AE11")] = Keycode.Minus,
                [NameToInt("AE12")] = Keycode.Equal,
                [NameToInt("AD01")] = Keycode.Q,
                [NameToInt("AD02")] = Keycode.W,
                [NameToInt("AD03")] = Keycode.E,
                [NameToInt("AD04")] = Keycode.R,
                [NameToInt("AD05")] = Keycode.T,
                [NameToInt("AD06")] = Keycode.Y,
                [NameToInt("AD07")] = Keycode.U,
                [NameToInt("AD08")] = Keycode.I,
                [NameToInt("AD09")] = Keycode.O,
                [NameToInt("AD10")] = Keycode.P,
                [NameToInt("AD11")] = Keycode.LeftBracket,
                [NameToInt("AD12")] = Keycode.RightBracket,
                [NameToInt("AC01")] = Keycode.A,
                [NameToInt("AC02")] = Keycode.S,
                [NameToInt("AC03")] = Keycode.D,
                [NameToInt("AC04")] = Keycode.F,
                [NameToInt("AC05")] = Keycode.G,
                [NameToInt("AC06")] = Keycode.H,
                [NameToInt("AC07")] = Keycode.J,
                [NameToInt("AC08")] = Keycode.K,
                [NameToInt("AC09")] = Keycode.L,
                [NameToInt("AC10")] = Keycode.SemiColon,
                [NameToInt("AC11")] = Keycode.Apostrophe,
                [NameToInt("AB01")] = Keycode.Z,
                [NameToInt("AB02")] = Keycode.X,
                [NameToInt("AB03")] = Keycode.C,
                [NameToInt("AB04")] = Keycode.V,
                [NameToInt("AB05")] = Keycode.B,
                [NameToInt("AB06")] = Keycode.N,
                [NameToInt("AB07")] = Keycode.M,
                [NameToInt("AB08")] = Keycode.Comma,
                [NameToInt("AB09")] = Keycode.Dot,
                [NameToInt("AB10")] = Keycode.Slash,
                [NameToInt("BKSL")] = Keycode.Backslash,
                [NameToInt("LSGT")] = Keycode.World1
            };

            var desc = XkbGetMap(display, 0, XkbUseCoreKbd);
            XkbGetNames(display, XkbKeyNamesMask, desc);
            var minKeyCode = desc->min_key_code;
            var maxKeyCode = desc->max_key_code;
            var keys = desc->names->keys;
            for (int i = minKeyCode; i <= maxKeyCode; i++)
            {
                if (nameToKeycode.TryGetValue(NameToInt(keys[i].name), out var keycode))
                {
                    keycodeMappings[i] = keycode;
                }
            }
            XkbFreeNames(desc, XkbKeyNamesMask, 1);
            XkbFreeClientMap(desc, 0, 1);

            for (int i = 0; i < 256; i++)
            {
                if (keycodeMappings[i] != 0)
                {
                    continue;
                }
                var keySym = XkbKeycodeToKeysym(display, (byte)i, 0, 1);
                switch (keySym)
                {
                    case 0xFFB0: keycodeMappings[i] = Keycode.KeypadZero; continue;
                    case 0xFFB1: keycodeMappings[i] = Keycode.KeypadOne; continue;
                    case 0xFFB2: keycodeMappings[i] = Keycode.KeypadTwo; continue;
                    case 0xFFB3: keycodeMappings[i] = Keycode.KeypadThree; continue;
                    case 0xFFB4: keycodeMappings[i] = Keycode.KeypadFour; continue;
                    case 0xFFB5: keycodeMappings[i] = Keycode.KeypadFive; continue;
                    case 0xFFB6: keycodeMappings[i] = Keycode.KeypadSix; continue;
                    case 0xFFB7: keycodeMappings[i] = Keycode.KeypadSeven; continue;
                    case 0xFFB8: keycodeMappings[i] = Keycode.KeypadEight; continue;
                    case 0xFFB9: keycodeMappings[i] = Keycode.KeypadNine; continue;
                    case 0xFFAC: keycodeMappings[i] = Keycode.KeypadDot; continue;
                    case 0xFFAE: keycodeMappings[i] = Keycode.KeypadDot; continue;
                    case 0xFFBD: keycodeMappings[i] = Keycode.KeypadEqual; continue;
                    case 0xFF8D: keycodeMappings[i] = Keycode.KeypadEnter; continue;
                }
                keySym = XkbKeycodeToKeysym(display, (byte)i, 0, 0);
                switch (keySym)
                {
                    case 0xFF1B: keycodeMappings[i] = Keycode.Escape; continue;
                    case 0xFF09: keycodeMappings[i] = Keycode.Tab; continue;
                    case 0xFFE1: keycodeMappings[i] = Keycode.LeftShift; continue;
                    case 0xFFE2: keycodeMappings[i] = Keycode.RightShift; continue;
                    case 0xFFE3: keycodeMappings[i] = Keycode.LeftControl; continue;
                    case 0xFFE4: keycodeMappings[i] = Keycode.RightControl; continue;
                    case 0xFFE7: keycodeMappings[i] = Keycode.LeftAlternate; continue;
                    case 0xFFE9: keycodeMappings[i] = Keycode.LeftAlternate; continue;
                    case 0xFFE8: keycodeMappings[i] = Keycode.RightAlternate; continue;
                    case 0xFFEA: keycodeMappings[i] = Keycode.RightAlternate; continue;
                    case 0xFF7E: keycodeMappings[i] = Keycode.RightAlternate; continue;
                    case 0xFE03: keycodeMappings[i] = Keycode.RightAlternate; continue;
                    case 0xFFEB: keycodeMappings[i] = Keycode.LeftCommand; continue;
                    case 0xFFEC: keycodeMappings[i] = Keycode.RightCommand; continue;
                    case 0xFF67: keycodeMappings[i] = Keycode.Menu; continue;
                    case 0xFF7F: keycodeMappings[i] = Keycode.NumLock; continue;
                    case 0xFFE5: keycodeMappings[i] = Keycode.CapsLock; continue;
                    case 0xFF61: keycodeMappings[i] = Keycode.PrintScreen; continue;
                    case 0xFF14: keycodeMappings[i] = Keycode.ScrollLock; continue;
                    case 0xFF13: keycodeMappings[i] = Keycode.Pause; continue;
                    case 0xFFFF: keycodeMappings[i] = Keycode.Delete; continue;
                    case 0xFF08: keycodeMappings[i] = Keycode.Backspace; continue;
                    case 0xFF0D: keycodeMappings[i] = Keycode.Enter; continue;
                    case 0xFF50: keycodeMappings[i] = Keycode.Home; continue;
                    case 0xFF57: keycodeMappings[i] = Keycode.End; continue;
                    case 0xFF55: keycodeMappings[i] = Keycode.PageUp; continue;
                    case 0xFF56: keycodeMappings[i] = Keycode.PageDown; continue;
                    case 0xFF63: keycodeMappings[i] = Keycode.Insert; continue;
                    case 0xFF51: keycodeMappings[i] = Keycode.Left; continue;
                    case 0xFF53: keycodeMappings[i] = Keycode.Right; continue;
                    case 0xFF54: keycodeMappings[i] = Keycode.Down; continue;
                    case 0xFF52: keycodeMappings[i] = Keycode.Up; continue;
                    case 0xFFBE: keycodeMappings[i] = Keycode.F1; continue;
                    case 0xFFBF: keycodeMappings[i] = Keycode.F2; continue;
                    case 0xFFC0: keycodeMappings[i] = Keycode.F3; continue;
                    case 0xFFC1: keycodeMappings[i] = Keycode.F4; continue;
                    case 0xFFC2: keycodeMappings[i] = Keycode.F5; continue;
                    case 0xFFC3: keycodeMappings[i] = Keycode.F6; continue;
                    case 0xFFC4: keycodeMappings[i] = Keycode.F7; continue;
                    case 0xFFC5: keycodeMappings[i] = Keycode.F8; continue;
                    case 0xFFC6: keycodeMappings[i] = Keycode.F9; continue;
                    case 0xFFC7: keycodeMappings[i] = Keycode.F10; continue;
                    case 0xFFC8: keycodeMappings[i] = Keycode.F11; continue;
                    case 0xFFC9: keycodeMappings[i] = Keycode.F12; continue;
                    case 0xFFCA: keycodeMappings[i] = Keycode.F13; continue;
                    case 0xFFCB: keycodeMappings[i] = Keycode.F14; continue;
                    case 0xFFCC: keycodeMappings[i] = Keycode.F15; continue;
                    case 0xFFCD: keycodeMappings[i] = Keycode.F16; continue;
                    case 0xFFCE: keycodeMappings[i] = Keycode.F17; continue;
                    case 0xFFCF: keycodeMappings[i] = Keycode.F18; continue;
                    case 0xFFD0: keycodeMappings[i] = Keycode.F19; continue;
                    case 0xFFD1: keycodeMappings[i] = Keycode.F20; continue;
                    case 0xFFD2: keycodeMappings[i] = Keycode.F21; continue;
                    case 0xFFD3: keycodeMappings[i] = Keycode.F22; continue;
                    case 0xFFD4: keycodeMappings[i] = Keycode.F23; continue;
                    case 0xFFD5: keycodeMappings[i] = Keycode.F24; continue;
                    case 0xFFD6: keycodeMappings[i] = Keycode.F25; continue;
                    case 0xFFAF: keycodeMappings[i] = Keycode.KeypadSlash; continue;
                    case 0xFFAA: keycodeMappings[i] = Keycode.KeypadAsterisk; continue;
                    case 0xFFAD: keycodeMappings[i] = Keycode.KeypadMinus; continue;
                    case 0xFFAB: keycodeMappings[i] = Keycode.KeypadPlus; continue;
                    case 0x0020: keycodeMappings[i] = Keycode.Space; continue;
                    case 0x002C: keycodeMappings[i] = Keycode.Comma; continue;
                    case 0x002E: keycodeMappings[i] = Keycode.Dot; continue;
                    case 0x002F: keycodeMappings[i] = Keycode.Slash; continue;
                    case 0x0027: keycodeMappings[i] = Keycode.Apostrophe; continue;
                    case 0x002D: keycodeMappings[i] = Keycode.Minus; continue;
                    case 0x003B: keycodeMappings[i] = Keycode.SemiColon; continue;
                    case 0x003D: keycodeMappings[i] = Keycode.Equal; continue;
                    case 0x005B: keycodeMappings[i] = Keycode.LeftBracket; continue;
                    case 0x005C: keycodeMappings[i] = Keycode.Backslash; continue;
                    case 0x005D: keycodeMappings[i] = Keycode.RightBracket; continue;
                    case 0x0060: keycodeMappings[i] = Keycode.Backquote; continue;

                    case 0x0061: keycodeMappings[i] = Keycode.A; continue;
                    case 0x0062: keycodeMappings[i] = Keycode.B; continue;
                    case 0x0063: keycodeMappings[i] = Keycode.C; continue;
                    case 0x0064: keycodeMappings[i] = Keycode.D; continue;
                    case 0x0065: keycodeMappings[i] = Keycode.E; continue;
                    case 0x0066: keycodeMappings[i] = Keycode.F; continue;
                    case 0x0067: keycodeMappings[i] = Keycode.G; continue;
                    case 0x0068: keycodeMappings[i] = Keycode.H; continue;
                    case 0x0069: keycodeMappings[i] = Keycode.I; continue;
                    case 0x006A: keycodeMappings[i] = Keycode.J; continue;
                    case 0x006B: keycodeMappings[i] = Keycode.K; continue;
                    case 0x006C: keycodeMappings[i] = Keycode.L; continue;
                    case 0x006D: keycodeMappings[i] = Keycode.M; continue;
                    case 0x006E: keycodeMappings[i] = Keycode.N; continue;
                    case 0x006F: keycodeMappings[i] = Keycode.O; continue;
                    case 0x0070: keycodeMappings[i] = Keycode.P; continue;
                    case 0x0071: keycodeMappings[i] = Keycode.Q; continue;
                    case 0x0072: keycodeMappings[i] = Keycode.R; continue;
                    case 0x0073: keycodeMappings[i] = Keycode.S; continue;
                    case 0x0074: keycodeMappings[i] = Keycode.T; continue;
                    case 0x0075: keycodeMappings[i] = Keycode.U; continue;
                    case 0x0076: keycodeMappings[i] = Keycode.V; continue;
                    case 0x0077: keycodeMappings[i] = Keycode.W; continue;
                    case 0x0078: keycodeMappings[i] = Keycode.X; continue;
                    case 0x0079: keycodeMappings[i] = Keycode.Y; continue;
                    case 0x007A: keycodeMappings[i] = Keycode.Z; continue;
                    default:
                        keycodeMappings[i] = Keycode.Unknown;
                        break;
                }
            }

            for (int i = 0; i < keycodeMappings.Length; i++)
            {
                var keycode = keycodeMappings[i];
                if (keycode != Keycode.Unknown)
                {
                    scancodes[(int)keycode] = i;
                }
            }
        }
    }
}
