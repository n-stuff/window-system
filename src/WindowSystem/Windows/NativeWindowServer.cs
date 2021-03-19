using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

using static NStuff.WindowSystem.Windows.NativeMethods;

namespace NStuff.WindowSystem.Windows
{
    internal class NativeWindowServer : NativeWindowServerBase
    {
        private static IntPtr windowClassNamePtr;
        private static readonly Keycode[] keycodeMappings = new Keycode[0x15E];
        private static readonly int[] scancodes = new int[(int)Keycode.Invalid];

        static NativeWindowServer() => InitializeKeyMappings();

        private readonly WNDPROCDelegate wndprocDelegate;
        private Exception? eventException;
        private readonly IntPtr helperWindowHandle;
        private int messageTime;
        private Window? freeLookMouseWindow;
        private (double x, double y) cursorPositionBackup;
        private IntPtr rawDataBuffer;
        private int rawDataBufferSize;
        private readonly Dictionary<IntPtr, Window> windows = new Dictionary<IntPtr, Window>();

        public NativeWindowServer()
        {
            if (IsRunning())
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.ServerAlreadyInitialized));
            }
            try
            {
                SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
            }
            catch
            {
            }

            var wndclass = new WNDCLASS
            {
                style = CS_HREDRAW | CS_VREDRAW | CS_OWNDC,
                lpszClassName = windowClassNamePtr = Marshal.StringToHGlobalUni(typeof(Window).FullName),
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndprocDelegate = WndProc),
                hCursor = LoadCursorW(IntPtr.Zero, IDC_ARROW)
            };
            if (RegisterClassW(ref wndclass) == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            helperWindowHandle = CreateWindowExW(WS_EX_OVERLAPPEDWINDOW, windowClassNamePtr, IntPtr.Zero, WS_CLIPSIBLINGS | WS_CLIPCHILDREN,
                0, 0, 1, 1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if (helperWindowHandle == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            ShowWindow(helperWindowHandle, SW_HIDE);
            while (PeekMessageW(out var msg, helperWindowHandle, 0, 0, PM_REMOVE) != 0)
            {
                TranslateMessage(ref msg);
                DispatchMessageW(ref msg);
            }
        }

        protected internal override bool IsRunning() => windowClassNamePtr != IntPtr.Zero;

        protected internal override void Shutdown()
        {
            NativeMethods.DestroyWindow(helperWindowHandle);
            UnregisterClassW(windowClassNamePtr, IntPtr.Zero);
            Marshal.FreeHGlobal(windowClassNamePtr);
            Marshal.FreeHGlobal(rawDataBuffer);
            windowClassNamePtr = IntPtr.Zero;
        }

        protected internal override ICollection<Window> GetWindows() => windows.Values;

        protected internal override void CreateWindowData(Window window) => window.NativeData = new WindowData();

        protected internal override void CreateWindow(Window window)
        {
            var data = GetData(window);
            data.Handle = CreateNativeWindow();
            GetWindowScale(data.Handle, out var x, out var y);
            data.Scale = (x, y);

            window.NativeData = data;
            windows.Add(data.Handle, window);
        }

        protected internal override void DestroyWindow(Window window)
        {
            if (freeLookMouseWindow == window)
            {
                freeLookMouseWindow = null;
            }
            var handle = GetHandle(window);
            NativeMethods.DestroyWindow(handle);
            windows.Remove(handle);
            window.NativeData = null;
        }

        protected internal override void RecreateNativeWindow(Window window)
        {
            var data = GetData(window);
            windows.Remove(data.Handle);
            NativeMethods.DestroyWindow(data.Handle);
            data.Handle = CreateNativeWindow();
            windows.Add(data.Handle, window);
        }

        protected internal override bool IsWindowVisible(Window window) => GetData(window).Visible;

        protected internal override void SetWindowVisible(Window window, bool visible)
        {
            var handle = GetHandle(window);
            if (visible)
            {
                ShowWindow(handle, SW_SHOWNORMAL);
                BringWindowToTop(handle);
                SetForegroundWindow(handle);
                SetFocus(handle);
            }
            else
            {
                ShowWindow(handle, SW_HIDE);
            }
        }

        protected internal override bool IsWindowFocused(Window window) => GetHandle(window) == GetActiveWindow();

        protected internal override void ActivateWindow(Window window)
        {
            if (IsWindowVisible(window))
            {
                SetForegroundWindow(GetHandle(window));
            }
        }

        protected internal override bool IsWindowTopMost(Window window) => GetData(window).TopMost;

        protected internal override void SetWindowTopMost(Window window, bool topMost)
        {
            var data = GetData(window);
            SetWindowPos(data.Handle, (topMost) ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            data.TopMost = topMost;
        }

        protected internal override (double top, double left, double bottom, double right) GetWindowBorderSize(Window window)
        {
            var data = GetData(window);
            GetClientRect(data.Handle, out var rect);
            var width = rect.right;
            var height = rect.bottom;
            AdjustWindowRect(data.Handle, ref rect);
            var (sx, sy) = data.Scale;
            return (-rect.top / sy, -rect.left / sx, (rect.bottom - height) / sy, (rect.right - width) / sx);
        }

        protected internal override WindowBorderStyle GetWindowBorderStyle(Window window)
        {
            var style = GetWindowLongPtrW(GetHandle(window), GWL_STYLE);
            if ((style & WS_POPUP) != 0)
            {
                return WindowBorderStyle.None;
            }
            if ((style & WS_SIZEBOX) != 0)
            {
                return WindowBorderStyle.Sizable;
            }
            return WindowBorderStyle.Fixed;
        }

        protected internal override void SetWindowBorderStyle(Window window, WindowBorderStyle borderStyle)
        {
            var handle = GetHandle(window);
            var style = GetWindowLongPtrW(handle, GWL_STYLE);
            var exStyle = GetWindowLongPtrW(handle, GWL_EXSTYLE);
            switch (borderStyle)
            {
                case WindowBorderStyle.None:
                    style |= WS_POPUP;
                    style &= ~(WS_SIZEBOX | WS_MAXIMIZEBOX | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX);
                    exStyle &= ~WS_EX_WINDOWEDGE;
                    break;

                case WindowBorderStyle.Sizable:
                    style |= WS_SIZEBOX | WS_MAXIMIZEBOX;
                    exStyle |= WS_EX_WINDOWEDGE;
                    goto case WindowBorderStyle.Fixed;

                case WindowBorderStyle.Fixed:
                    style |= WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX;
                    style &= ~WS_POPUP;
                    break;
            }
            SetWindowLongPtrW(handle, GWL_STYLE, style);
            SetWindowLongPtrW(handle, GWL_EXSTYLE, exStyle);
            SetWindowPos(handle, HWND_TOP, 0, 0, 0, 0,
                SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOCOPYBITS | SWP_NOOWNERZORDER);
        }

        protected internal override unsafe (double x, double y) GetWindowLocation(Window window)
        {
            var data = GetData(window);
            var point = new POINT { x = 0, y = 0 };
            ClientToScreen(data.Handle, &point);
            var (sx, sy) = data.Scale;
            return (point.x / sx, point.y / sy);
        }

        protected internal override void SetWindowLocation(Window window, (double x, double y) location)
        {
            var data = GetData(window);
            var (sx, sy) = data.Scale;
            var rect = new RECT
            {
                left = (int)Math.Round(location.x * sx),
                top = (int)Math.Round(location.y * sy),
            };
            AdjustWindowRect(data.Handle, ref rect);
            SetWindowPos(data.Handle, HWND_TOP, rect.left, rect.top, 0, 0, SWP_NOACTIVATE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOOWNERZORDER);
        }

        protected internal override (double width, double height) GetWindowMaximumSize(Window window) => GetData(window).MaximumSize;

        protected internal override void SetWindowMaximumSize(Window window, (double width, double height) size)
        {
            var data = GetData(window);
            data.MaximumSize = size;
            GetWindowRect(data.Handle, out var rect);
            MoveWindow(data.Handle, rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top, 1);
        }

        protected internal override (double width, double height) GetWindowMinimumSize(Window window) => GetData(window).MinimumSize;

        protected internal override void SetWindowMinimumSize(Window window, (double width, double height) size)
        {
            var data = GetData(window);
            data.MinimumSize = size;
            GetWindowRect(data.Handle, out var rect);
            MoveWindow(data.Handle, rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top, 1);
        }

        protected internal override double GetWindowOpacity(Window window)
        {
            var handle = GetHandle(window);
            var exStyle = GetWindowLongPtrW(handle, GWL_EXSTYLE);
            if ((exStyle & WS_EX_LAYERED) == 0)
            {
                return 1.0;
            }
            if (GetLayeredWindowAttributes(handle, out var _, out var alpha, out var _) == 0)
            {
                return 1.0;
            }
            return alpha / 255d;
        }

        protected internal override void SetWindowOpacity(Window window, double opacity)
        {
            var handle = GetHandle(window);
            var exStyle = GetWindowLongPtrW(handle, GWL_EXSTYLE);
            if (opacity == 1.0)
            {
                SetWindowLongPtrW(handle, GWL_EXSTYLE, exStyle & ~WS_EX_LAYERED);
            }
            else
            {
                if ((exStyle & WS_EX_LAYERED) == 0)
                {
                    if (SetWindowLongPtrW(handle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED) == 0)
                    {
                        return;
                    }
                }
                SetLayeredWindowAttributes(handle, 0, (byte)(opacity * 255), LWA_ALPHA);
            }
        }

        protected internal override (double width, double height) GetWindowSize(Window window)
        {
            var data = GetData(window);
            GetClientRect(data.Handle, out var rect);
            var (sx, sy) = data.Scale;
            return (rect.right / sx, rect.bottom / sy);
        }

        protected internal override void SetWindowSize(Window window, (double width, double height) size)
        {
            var data = GetData(window);
            var (sx, sy) = data.Scale;
            var rect = new RECT { right = (int)Math.Round(size.width * sx), bottom = (int)Math.Round(size.height * sy) };
            AdjustWindowRect(data.Handle, ref rect);
            var width = rect.right - rect.left;
            var height = rect.bottom - rect.top;
            SetWindowPos(data.Handle, HWND_TOP, 0, 0, width, height, SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOMOVE | SWP_NOZORDER);
        }

        protected internal override unsafe WindowSizeState GetWindowSizeState(Window window)
        {
            var windowplacement = new WINDOWPLACEMENT { length = (uint)sizeof(WINDOWPLACEMENT) };
            GetWindowPlacement(GetHandle(window), ref windowplacement);
            return windowplacement.showCmd switch
            {
                SW_SHOWMINIMIZED => WindowSizeState.Minimized,
                SW_SHOWMAXIMIZED => WindowSizeState.Maximized,
                _ => WindowSizeState.Normal,
            };
        }

        protected internal override void SetWindowSizeState(Window window, WindowSizeState sizeState)
        {
            var handle = GetHandle(window);
            switch (sizeState)
            {
                case WindowSizeState.Normal:
                    ShowWindow(handle, SW_RESTORE);
                    break;

                case WindowSizeState.Minimized:
                    ShowWindow(handle, SW_MINIMIZE);
                    break;

                case WindowSizeState.Maximized:
                    ShowWindow(handle, SW_MAXIMIZE);
                    break;
            }
        }

        protected internal override string GetWindowTitle(Window window)
        {
            var handle = GetHandle(window);
            var length = GetWindowTextLengthW(handle) + 1;
            if (length == 1)
            {
                return string.Empty;
            }
            var builder = new StringBuilder(length);
            GetWindowTextW(handle, builder, length);
            return builder.ToString();
        }

        protected internal override void SetWindowTitle(Window window, string title) => SetWindowTextW(GetHandle(window), title);

        protected internal override (double x, double y) GetWindowViewportSize(Window window)
        {
            GetClientRect(GetHandle(window), out var rect);
            return (rect.right, rect.bottom);
        }

        protected internal override void SetFreeLookMouseWindow(Window window, bool enable)
        {
            if (enable)
            {
                freeLookMouseWindow = window;
                cursorPositionBackup = GetCursorPosition(window);
                var (width, height) = GetWindowSize(window);
                SetCursorPosition(window, (Math.Truncate(width / 2), Math.Truncate(height / 2)));
                UpdateCursorClip(window);
                var rawInputDevice = new RAWINPUTDEVICE { usUsagePage = 1, usUsage = 2, hwndTarget = GetHandle(window) };
                RegisterRawInputDevices(ref rawInputDevice, 1, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
            }
            else if (freeLookMouseWindow == window)
            {
                freeLookMouseWindow = null;
                UpdateCursorClip(null);
                var rawInputDevice = new RAWINPUTDEVICE { usUsagePage = 1, usUsage = 2, dwFlags = RIDEV_REMOVE, hwndTarget = IntPtr.Zero };
                SetCursorPosition(window, cursorPositionBackup);
                RegisterRawInputDevices(ref rawInputDevice, 1, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
            }
            if (IsCursorInClientArea(window))
            {
                UpdateWindowCursor(window);
            }
        }

        protected internal override void RequestWindowAttention(Window window) => FlashWindow(GetHandle(window), 1);

        protected internal override void CreateCursor(Cursor cursor, CursorShape shape)
        {
            var resource = shape switch
            {
                CursorShape.Arrow => OCR_NORMAL,
                CursorShape.IBeam => OCR_IBEAM,
                CursorShape.Crosshair => OCR_CROSS,
                CursorShape.Hand => OCR_HAND,
                CursorShape.HorizontalResize => OCR_SIZEWE,
                CursorShape.VerticalResize => OCR_SIZENS,
                _ => throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.PredefinedCursorCreationFailed, shape)),
            };
            var handle = LoadImageW(IntPtr.Zero, resource, IMAGE_CURSOR, 0, 0, LR_DEFAULTSIZE | LR_SHARED);
            if (handle == IntPtr.Zero)
            {
                throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.PredefinedCursorCreationFailed, shape));
            }
            cursor.NativeData = new CursorData(handle);
        }

        protected internal override void CreateCursor(Cursor cursor, byte[] imageData, (int width, int height) size, (double x, double y) hotSpot) =>
            cursor.NativeData = new CursorData(CreateIcon(imageData, size, hotSpot, false));

        protected internal override void SetWindowCursor(Window window)
        {
            if (IsCursorInClientArea(window))
            {
                UpdateWindowCursor(window);
            }
        }

        protected internal override (double x, double y) GetCursorPosition(Window window)
        {
            var data = GetData(window);
            GetCursorPos(out var point);
            ScreenToClient(data.Handle, ref point);
            var (sx, sy) = data.Scale;
            return (point.x / sx, point.y / sy);
        }

        protected internal override unsafe void SetCursorPosition(Window window, (double x, double y) position)
        {
            var data = GetData(window);
            var (sx, sy) = data.Scale;
            var point = new POINT { x = (int)Math.Round(position.x * sx), y = (int)Math.Round(position.y * sy) };
            data.LastCursorPosition = (point.x, point.y);
            ClientToScreen(data.Handle, &point);
            SetCursorPos(point.x, point.y);
        }

        protected internal override void DestroyCursor(Cursor cursor)
        {
            var handle = GetHandle(cursor);
            if (handle != IntPtr.Zero)
            {
                DestroyIcon(handle);
                cursor.NativeData = null;
            }
        }

        protected internal override double GetEventTime() => messageTime / 1000d;

        protected internal override (double x, double y) ConvertFromScreen(Window window, (double x, double y) point)
        {
            var data = GetData(window);
            var (sx, sy) = data.Scale;
            var p = new POINT { x = (int)Math.Round(point.x * sx), y = (int)Math.Round(point.y * sy) };
            ScreenToClient(data.Handle, ref p);
            return (p.x / sx, p.y / sy);
        }

        protected internal override unsafe (double x, double y) ConvertToScreen(Window window, (double x, double y) point)
        {
            var data = GetData(window);
            var (sx, sy) = data.Scale;
            var p = new POINT { x = (int)Math.Round(point.x * sx), y = (int)Math.Round(point.y * sy) };
            ClientToScreen(data.Handle, &p);
            return (p.x / sx, p.y / sy);
        }

        protected internal override ModifierKeys GetModifierKeys()
        {
            var result = ModifierKeys.None;
            if ((GetKeyState(VK_SHIFT) & 0x8000) != 0)
            {
                result = ModifierKeys.Shift;
            }
            if ((GetKeyState(VK_CONTROL) & 0x8000) != 0)
            {
                result |= ModifierKeys.Control;
            }
            if ((GetKeyState(VK_MENU) & 0x8000) != 0)
            {
                result |= ModifierKeys.Alternate;
            }
            if (((GetKeyState(VK_LWIN) | GetKeyState(VK_RWIN)) & 0x8000) != 0)
            {
                result |= ModifierKeys.Command;
            }
            if ((GetKeyState(VK_CAPITAL) & 1) != 0)
            {
                result |= ModifierKeys.CapsLock;
            }
            return result;
        }

        protected internal override string GetClipboardString()
        {
            if (OpenClipboard(helperWindowHandle) == 0)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.OpenClipboardFailed));
            }

            var data = GetClipboardData(CF_UNICODETEXT);
            if (data == IntPtr.Zero)
            {
                CloseClipboard();
                return string.Empty;
            }

            var result = GlobalHandleToString(data);
            CloseClipboard();
            return result;
        }

        protected internal override void SetClipboardString(string text)
        {
            if (OpenClipboard(helperWindowHandle) == 0)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.OpenClipboardFailed));
            }

            var data = GlobalAllocMoveableWideChar(text);
            EmptyClipboard();
            if (SetClipboardData(CF_UNICODETEXT, data) == IntPtr.Zero)
            {
                GlobalFree(data);
                CloseClipboard();
                throw new InvalidOperationException(Resources.FormatMessage(Resources.Key.SetClipboardFailed, text));
            }
            CloseClipboard();
        }

        protected internal override unsafe string ConvertKeycodeToString(Keycode keycode)
        {
            var scancode = scancodes[(int)keycode];
            var buffer = stackalloc char[16];
            var length = GetKeyNameTextW(scancode << 16, new IntPtr(buffer), 16);
            if (length == 0)
            {
                return string.Empty;
            }
            return Marshal.PtrToStringUni(new IntPtr(buffer), length);
        }

        protected internal override bool WaitAndProcessEvents()
        {
            WaitMessage();
            return ProcessEvents();
        }

        protected internal override unsafe bool WaitAndProcessEvents(double timeout)
        {
            MsgWaitForMultipleObjects(0, null, 0, (uint)(timeout * 1000), QS_ALLEVENTS);
            return ProcessEvents();
        }

        protected internal override bool ProcessEvents()
        {
            var result = false;
            while (PeekMessageW(out var msg, IntPtr.Zero, 0, 0, PM_REMOVE) != 0)
            {
                result = true;
                if (msg.message == WM_QUIT)
                {
                    foreach (var window in new List<Window>(windows.Values))
                    {
                        CloseRequestOccurred(window);
                    }
                }
                else
                {
                    TranslateMessage(ref msg);
                    DispatchMessageW(ref msg);
                    if (eventException != null)
                    {
                        throw eventException;
                    }
                }
            }
            var handle = GetActiveWindow();
            if (handle != IntPtr.Zero)
            {
                if (windows.TryGetValue(handle, out Window? window))
                {
                    if (((GetAsyncKeyState(VK_LSHIFT) >> 15) & 1) == 0 && window.IsKeyPressed(Keycode.LeftShift))
                    {
                        var modifiers = GetAsyncModifierKeys();
                        KeyUpEventOccurred(window, Keycode.LeftShift, modifiers);
                    }
                    else if (((GetAsyncKeyState(VK_RSHIFT) >> 15) & 1) == 0 && window.IsKeyPressed(Keycode.RightShift))
                    {
                        var modifiers = GetAsyncModifierKeys();
                        KeyUpEventOccurred(window, Keycode.RightShift, modifiers);
                    }
                }
            }
            if (freeLookMouseWindow != null)
            {
                var (width, height) = GetWindowSize(freeLookMouseWindow);
                (double x, double y) point = (Math.Truncate(width / 2), Math.Truncate(height / 2));
                var data = GetData(freeLookMouseWindow);
                var (cx, cy) = data.LastCursorPosition;
                var (sx, sy) = data.Scale;
                if (cx != point.x * sx || cy != point.y * sy)
                {
                    SetCursorPosition(freeLookMouseWindow, point);
                }
            }
            return result;
        }

        protected internal override void UnblockProcessEvents() => PostMessage(helperWindowHandle, WM_NULL, UIntPtr.Zero, IntPtr.Zero);

        private unsafe IntPtr WndProc(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
        {
            if (windows.TryGetValue(hWnd, out var window))
            {
                try
                {
                    var data = GetData(window);
                    switch (msg)
                    {
                        case WM_MOVE:
                            window.RenderingContext?.UpdateRenderingData(window);
                            if (freeLookMouseWindow == window)
                            {
                                UpdateCursorClip(window);
                            }
                            MoveEventOccurred(window);
                            return IntPtr.Zero;

                        case WM_SIZE:
                            window.RenderingContext?.UpdateRenderingData(window);
                            if (freeLookMouseWindow == window)
                            {
                                UpdateCursorClip(window);
                            }
                            if (data.SizeState != (ulong)wParam)
                            {
                                data.SizeState = (ulong)wParam;
                                SizeStateChangedEventOccurred(window);
                            }
                            FramebufferResizeEventOccurred(window);
                            ResizeEventOccurred(window);
                            return IntPtr.Zero;

                        case WM_SETFOCUS:
                            GotFocusEventOccurred(window);
                            if (data.SystemButtonInteraction)
                            {
                                break;
                            }
                            if (window.FreeLookMouse && IsCursorInClientArea(window))
                            {
                                SetFreeLookMouseWindow(window, true);
                            }
                            return IntPtr.Zero;

                        case WM_KILLFOCUS:
                            if (window.FreeLookMouse)
                            {
                                SetFreeLookMouseWindow(window, false);
                            }
                            LostFocusEventOccurred(window);
                            return IntPtr.Zero;

                        case WM_CLOSE:
                            CloseRequestOccurred(window);
                            return IntPtr.Zero;

                        case WM_ERASEBKGND:
                            return new IntPtr(1);

                        case WM_SHOWWINDOW:
                            data.Visible = wParam != UIntPtr.Zero;
                            VisibleChangedEventOccurred(window);
                            break;

                        case WM_SETCURSOR:
                            if (LOWORD(lParam) == HTCLIENT && IsWindowFocused(window))
                            {
                                UpdateWindowCursor(window);
                                return new IntPtr(1);
                            }
                            break;

                        case WM_MOUSEACTIVATE:
                            if (HIWORD(lParam) == WM_LBUTTONDOWN)
                            {
                                switch (LOWORD(lParam))
                                {
                                    case HTCLOSE:
                                    case HTMINBUTTON:
                                    case HTMAXBUTTON:
                                        data.SystemButtonInteraction = true;
                                        break;
                                }
                            }
                            break;

                        case WM_GETMINMAXINFO:
                            {
                                var mmi = (MINMAXINFO*)lParam;
                                var rect = new RECT();
                                AdjustWindowRect(data.Handle, ref rect);
                                var frameWidth = rect.right - rect.left;
                                var frameHeight = rect.bottom - rect.top;
                                var (sx, sy) = data.Scale;
                                if (data.MinimumSize.width > 0)
                                {
                                    mmi->ptMinTrackSize.x = (int)Math.Round(data.MinimumSize.width * sx + frameWidth);
                                }
                                if (data.MinimumSize.height > 0)
                                {
                                    mmi->ptMinTrackSize.y = (int)Math.Round(data.MinimumSize.height * sy + frameHeight);
                                }
                                if (data.MaximumSize.width > 0)
                                {
                                    mmi->ptMaxTrackSize.x = (int)Math.Round(data.MaximumSize.width * sx + frameWidth);
                                }
                                if (data.MaximumSize.height > 0)
                                {
                                    mmi->ptMaxTrackSize.y = (int)Math.Round(data.MaximumSize.height * sy + frameHeight);
                                }
                            }
                            return IntPtr.Zero;

                        case WM_INPUT:
                            if (freeLookMouseWindow == window)
                            {
                                uint size = 0;
                                GetRawInputData(lParam, RID_INPUT, IntPtr.Zero, ref size, (uint)sizeof(RAWINPUTHEADER));
                                if (size > rawDataBufferSize)
                                {
                                    Marshal.FreeHGlobal(rawDataBuffer);
                                    rawDataBufferSize = (int)size;
                                    rawDataBuffer = Marshal.AllocHGlobal(rawDataBufferSize);
                                }
                                if (GetRawInputData(lParam, RID_INPUT, rawDataBuffer, ref size, (uint)sizeof(RAWINPUTHEADER)) == 0xFFFFFFFF)
                                {
                                    // ERROR
                                    break;
                                }
                                var raw = (RAWINPUT*)rawDataBuffer;
                                if (raw->header.dwType == RIM_TYPEMOUSE)
                                {
                                    int dx = raw->data.mouse.lLastX;
                                    int dy = raw->data.mouse.lLastY;
                                    var (cx, cy) = data.LastCursorPosition;
                                    if (raw->data.mouse.usFlags == MOUSE_MOVE_ABSOLUTE)
                                    {
                                        dx -= cx;
                                        dy -= cy;
                                    }
                                    var (fx, fy) = window.FreeLookPosition;
                                    var (sx, sy) = data.Scale;
                                    MouseMoveEventOccurred(window, fx + dx / sx, fy + dy / sy);
                                    data.LastCursorPosition = (cx + dy, cy + dy);
                                }
                            }
                            break;

                        case WM_KEYDOWN:
                        case WM_SYSKEYDOWN:
                            {
                                var keycode = TranslateKeycode(wParam, lParam);
                                if (keycode != Keycode.Unknown && keycode != Keycode.Invalid)
                                {
                                    messageTime = GetMessageTime();
                                    if (keycode == Keycode.RightAlternate)
                                    {
                                        KeyDownEventOccurred(window, keycode, GetModifierKeys() & ~(ModifierKeys.Alternate | ModifierKeys.Control));
                                    }
                                    else
                                    {
                                        KeyDownEventOccurred(window, keycode, GetModifierKeys());
                                    }
                                }
                            }
                            break;

                        case WM_KEYUP:
                        case WM_SYSKEYUP:
                            {
                                var keycode = TranslateKeycode(wParam, lParam);
                                if (keycode != Keycode.Unknown && keycode != Keycode.Invalid)
                                {
                                    var modifiers = GetModifierKeys();
                                    messageTime = GetMessageTime();
                                    if ((ulong)wParam == VK_SHIFT)
                                    {
                                        KeyUpEventOccurred(window, Keycode.LeftShift, modifiers);
                                        KeyUpEventOccurred(window, Keycode.RightShift, modifiers);
                                    }
                                    else if ((ulong)wParam == VK_SNAPSHOT)
                                    {
                                        KeyDownEventOccurred(window, keycode, modifiers);
                                        KeyUpEventOccurred(window, keycode, modifiers);
                                    }
                                    else
                                    {
                                        KeyUpEventOccurred(window, keycode, modifiers);
                                    }
                                }
                            }
                            break;

                        case WM_CHAR:
                        case WM_SYSCHAR:
                            messageTime = GetMessageTime();
                            TextInputEventOccurred(window, (int)wParam, GetModifierKeys());
                            return IntPtr.Zero;

                        case WM_UNICHAR:
                            if ((ulong)wParam == UNICODE_NOCHAR)
                            {
                                return (IntPtr)1;
                            }
                            messageTime = GetMessageTime();
                            TextInputEventOccurred(window, (int)wParam, GetModifierKeys());
                            return IntPtr.Zero;

                        case WM_SYSCOMMAND:
                            if (((ulong)wParam & 0xFFF0) == SC_KEYMENU)
                            {
                                return IntPtr.Zero;
                            }
                            break;

                        case WM_MOUSEMOVE:
                            {
                                if (!data.MouseEntered)
                                {
                                    var trackmouseevent = new TRACKMOUSEEVENT
                                    {
                                        cbSize = (uint)sizeof(TRACKMOUSEEVENT),
                                        dwFlags = TME_LEAVE,
                                        hwndTrack = hWnd
                                    };
                                    TrackMouseEvent(ref trackmouseevent);
                                    data.MouseEntered = true;
                                    messageTime = GetMessageTime();
                                    MouseEnterEventOccurred(window);
                                }
                                if (window.FreeLookMouse)
                                {
                                    if (freeLookMouseWindow != window)
                                    {
                                        if (IsCursorInClientArea(window))
                                        {
                                            SetFreeLookMouseWindow(window, true);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                var x = GET_X_LPARAM(lParam);
                                var y = GET_Y_LPARAM(lParam);
                                var (cx, cy) = data.LastCursorPosition;
                                if (x != cx || y != cy)
                                {
                                    data.LastCursorPosition = (x, y);
                                    messageTime = GetMessageTime();
                                    var (sx, sy) = data.Scale;
                                    MouseMoveEventOccurred(window, x / sx, y / sy);
                                }
                            }
                            return IntPtr.Zero;

                        case WM_LBUTTONDOWN:
                            SetCapture(hWnd);
                            messageTime = GetMessageTime();
                            MouseDownEventOccurred(window, MouseButton.Left);
                            return IntPtr.Zero;

                        case WM_LBUTTONUP:
                            ReleaseCapture();
                            messageTime = GetMessageTime();
                            MouseUpEventOccurred(window, MouseButton.Left);
                            return IntPtr.Zero;

                        case WM_RBUTTONDOWN:
                            SetCapture(hWnd);
                            messageTime = GetMessageTime();
                            MouseDownEventOccurred(window, MouseButton.Right);
                            return IntPtr.Zero;

                        case WM_RBUTTONUP:
                            ReleaseCapture();
                            messageTime = GetMessageTime();
                            MouseUpEventOccurred(window, MouseButton.Right);
                            return IntPtr.Zero;

                        case WM_MBUTTONDOWN:
                            SetCapture(hWnd);
                            messageTime = GetMessageTime();
                            MouseDownEventOccurred(window, MouseButton.Middle);
                            return IntPtr.Zero;

                        case WM_MBUTTONUP:
                            ReleaseCapture();
                            messageTime = GetMessageTime();
                            MouseUpEventOccurred(window, MouseButton.Middle);
                            return IntPtr.Zero;

                        case WM_MOUSEWHEEL:
                            messageTime = GetMessageTime();
                            ScrollEventOccurred(window, 0, -HISHORT(wParam) / WHEEL_DELTA);
                            return IntPtr.Zero;

                        case WM_XBUTTONDOWN:
                            SetCapture(hWnd);
                            messageTime = GetMessageTime();
                            MouseDownEventOccurred(window, (int)wParam + MouseButton.Middle + 1);
                            return IntPtr.Zero;

                        case WM_XBUTTONUP:
                            ReleaseCapture();
                            messageTime = GetMessageTime();
                            MouseUpEventOccurred(window, (int)wParam + MouseButton.Middle + 1);
                            return IntPtr.Zero;

                        case WM_MOUSEHWHEEL:
                            messageTime = GetMessageTime();
                            ScrollEventOccurred(window, HISHORT(wParam) / WHEEL_DELTA, 0);
                            return IntPtr.Zero;

                        case WM_ENTERMENULOOP:
                        case WM_ENTERSIZEMOVE:
                            if (window.FreeLookMouse)
                            {
                                SetFreeLookMouseWindow(window, false);
                            }
                            break;

                        case WM_EXITMENULOOP:
                        case WM_EXITSIZEMOVE:
                            if (window.FreeLookMouse)
                            {
                                SetFreeLookMouseWindow(window, true);
                            }
                            break;

                        case WM_CAPTURECHANGED:
                            if (lParam == IntPtr.Zero && data.SystemButtonInteraction)
                            {
                                if (window.FreeLookMouse)
                                {
                                    SetFreeLookMouseWindow(window, true);
                                }
                                data.SystemButtonInteraction = false;
                            }
                            break;

                        case WM_MOUSELEAVE:
                            if (data.MouseEntered)
                            {
                                data.MouseEntered = false;
                                messageTime = GetMessageTime();
                                MouseLeaveEventOccurred(window);
                                return IntPtr.Zero;
                            }
                            break;

                        case WM_DROPFILES:
                            {
                                var hDrop = new IntPtr((long)wParam);

                                var fileCount = DragQueryFileW(hDrop, 0xFFFFFFFF, IntPtr.Zero, 0);
                                var paths = new string[fileCount];
                                var builder = new StringBuilder();
                                for (uint i = 0; i < fileCount; i++)
                                {
                                    var length = DragQueryFileW(hDrop, i, IntPtr.Zero, 0) + 1;
                                    if (builder.Capacity < length)
                                    {
                                        builder.Capacity = (int)length;
                                    }
                                    DragQueryFileW(hDrop, i, builder, length);
                                    paths[i] = builder.ToString();
                                }

                                GetCursorPos(out var point);
                                ScreenToClient(data.Handle, ref point);

                                var (sx, sy) = data.Scale;
                                FileDropEventOccurred(window, point.x / sx, point.y / sy, paths);

                                DragFinish(hDrop);
                            }
                            return IntPtr.Zero;

                        case WM_DPICHANGED:
                            {
                                data.Scale = (LOWORD(wParam) / 96d, HIWORD(wParam) / 96d);
                                var rect = (RECT*)lParam;
                                SetWindowPos(data.Handle, HWND_TOP, rect->left, rect->top,
                                    rect->right - rect->left, rect->bottom - rect->top,
                                    SWP_NOACTIVATE | SWP_NOZORDER);
                            }
                            break;

                        case WM_NCACTIVATE:
                        case WM_NCPAINT:
                            {
                                if (GetWindowBorderStyle(window) == WindowBorderStyle.None)
                                {
                                    return (IntPtr)1;
                                }
                            }
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    eventException = e;
                }
            }
            return DefWindowProcW(hWnd, msg, wParam, lParam);
        }

        private static unsafe string GlobalHandleToString(IntPtr handle)
        {
            var d = GlobalLock(handle);
            if (d == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var result = new string((char*)d);

            GlobalUnlock(handle);
            return result;
        }

        private static unsafe IntPtr GlobalAllocMoveableWideChar(string s)
        {
            var length = (long)s.Length * 2;
            var result = GlobalAlloc(GMEM_MOVEABLE, (ulong)length + 2);
            if (result == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            var d = GlobalLock(result);
            if (d == IntPtr.Zero)
            {
                GlobalFree(result);
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            fixed (char* p = s)
            {
                Buffer.MemoryCopy(p, (void*)d, length, length);
            }
            var bytes = (byte*)d;
            bytes[length] = 0;
            bytes[length + 1] = 0;

            GlobalUnlock(result);
            return result;
        }

        private static bool IsCursorInClientArea(Window window)
        {
            if (GetCursorPos(out var point) == 0)
            {
                return false;
            }

            var handle = GetHandle(window);
            if (WindowFromPoint(point) != handle)
            {
                return false;
            }

            GetClientRect(handle, out var rect);
            unsafe
            {
                ClientToScreen(handle, (POINT*)&rect.left);
                ClientToScreen(handle, (POINT*)&rect.right);
            }
            return PtInRect(ref rect, point) != 0;
        }

        private void UpdateWindowCursor(Window window)
        {
            if (window.Disposed)
            {
                return;
            }
            if (window.FreeLookMouse)
            {
                SetCursor(IntPtr.Zero);
            }
            else
            {
                var cursor = window.Cursor;
                if (cursor == null)
                {
                    SetCursor(LoadCursorW(IntPtr.Zero, IDC_ARROW));
                }
                else
                {
                    SetCursor(GetHandle(cursor));
                }
            }
        }

        private unsafe void UpdateCursorClip(Window? window)
        {
            if (window != null)
            {
                var handle = GetHandle(window);
                GetClientRect(handle, out var rect);
                ClientToScreen(handle, (POINT*)&rect.left);
                ClientToScreen(handle, (POINT*)&rect.right);
                ClipCursor(&rect);
            }
            else
            {
                ClipCursor(null);
            }
        }

        private unsafe IntPtr CreateIcon(byte[] imageData, (int width, int height) size, (double x, double y) hotSpot, bool isIcon)
        {
            var bi = new BITMAPV5HEADER
            {
                bV5Size = (uint)sizeof(BITMAPV5HEADER),
                bV5Width = size.width,
                bV5Height = -size.height,
                bV5Planes = 1,
                bV5BitCount = 32,
                bV5Compression = BI_BITFIELDS,
                bV5RedMask = 0x00ff0000,
                bV5GreenMask = 0x0000ff00,
                bV5BlueMask = 0x000000ff,
                bV5AlphaMask = 0xff000000
            };

            var dc = GetDC(IntPtr.Zero);
            byte* target;
            var color = CreateDIBSection(dc, (BITMAPINFO*)&bi, DIB_RGB_COLORS, (void**)&target, IntPtr.Zero, 0);
            ReleaseDC(IntPtr.Zero, dc);
            if (color == IntPtr.Zero)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.ImageCreationFailed));
            }

            var mask = CreateBitmap(size.width, size.height, 1, 1, IntPtr.Zero);
            if (mask == IntPtr.Zero)
            {
                DeleteObject(color);
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.ImageCreationFailed));
            }

            var source = imageData;
            var pixelCount = size.width * size.height;
            for (int i = 0, index = 0; i < pixelCount; i++, index += 4)
            {
                target[0] = source[index + 2];
                target[1] = source[index + 1];
                target[2] = source[index];
                target[3] = source[index + 3];
                target += 4;
            }

            var ii = new ICONINFO
            {
                fIcon = isIcon ? 1 : 0,
                xHotspot = (uint)hotSpot.x,
                yHotspot = (uint)hotSpot.y,
                hbmMask = mask,
                hbmColor = color
            };

            var handle = CreateIconIndirect(ref ii);
            DeleteObject(color);
            DeleteObject(mask);
            if (handle == IntPtr.Zero)
            {
                throw new InvalidOperationException(Resources.GetMessage(Resources.Key.ImageCreationFailed));
            }
            return handle;
        }

        private static void AdjustWindowRect(IntPtr handle, ref RECT rect)
        {
            var style = GetWindowLongPtrW(handle, GWL_STYLE);
            var exStyle = GetWindowLongPtrW(handle, GWL_EXSTYLE);
            AdjustWindowRectEx(ref rect, (uint)style, 0, (uint)exStyle);
        }

        private static IntPtr CreateNativeWindow()
        {
            if (windowClassNamePtr == IntPtr.Zero)
            {
                throw new InvalidOperationException();
            }
            var style = WS_CLIPSIBLINGS | WS_CLIPCHILDREN | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX;
            var exStyle = WS_EX_APPWINDOW;
            var x = unchecked((int)CW_USEDEFAULT);
            var y = unchecked((int)CW_USEDEFAULT);
            var rect = new RECT
            {
                left = 0,
                top = 0,
                right = DefaultSurfaceWidth,
                bottom = DefaultSurfaceHeight
            };
            AdjustWindowRectEx(ref rect, style, 0, exStyle);
            var width = rect.right - rect.left;
            var height = rect.bottom - rect.top;

            var handle = CreateWindowExW(exStyle, windowClassNamePtr, IntPtr.Zero, style, x, y, width, height,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if (handle == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            GetWindowScale(handle, out var xScale, out var yScale);
            if (xScale != 1d || yScale != 1d)
            {
                rect = new RECT
                {
                    left = 0,
                    top = 0,
                    right = (int)Math.Round(DefaultSurfaceWidth * xScale),
                    bottom = (int)Math.Round(DefaultSurfaceHeight * yScale)
                };
                AdjustWindowRectEx(ref rect, style, 0, exStyle);
                width = rect.right - rect.left;
                height = rect.bottom - rect.top;
                SetWindowPos(handle, HWND_TOP, 0, 0, width, height,
                             SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOMOVE | SWP_NOZORDER);
            }

            unsafe
            {
                ChangeWindowMessageFilterEx(handle, WM_DROPFILES, MSGFLT_ALLOW, null);
                ChangeWindowMessageFilterEx(handle, WM_COPYDATA, MSGFLT_ALLOW, null);
                ChangeWindowMessageFilterEx(handle, WM_COPYGLOBALDATA, MSGFLT_ALLOW, null);
            }
            DragAcceptFiles(handle, 1);

            return handle;
        }

        private static void GetWindowScale(IntPtr handle, out double xScale, out double yScale)
        {
            try
            {
                var monitor = MonitorFromWindow(handle, MONITOR_DEFAULTTONEAREST);
                GetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpiX, out var dpiY);
                xScale = dpiX / 96d;
                yScale = dpiY / 96d;
            }
            catch
            {
                xScale = 1;
                yScale = 1;
            }
        }

        private static WindowData GetData(Window window) =>
            (WindowData?)window.NativeData ?? throw new InvalidOperationException();

        private static IntPtr GetHandle(Window window) => GetData(window).Handle;

        private static IntPtr GetHandle(Cursor cursor) =>
            ((CursorData?)cursor.NativeData ?? throw new InvalidOperationException()).Handle;

        private static int GET_X_LPARAM(IntPtr lParam) => (short)((long)lParam);

        private static int GET_Y_LPARAM(IntPtr lParam) => (short)((long)lParam >> 16);

        private static short HISHORT(UIntPtr wParam) => (short)((ulong)wParam >> 16);

        private static ushort HIWORD(IntPtr lParam) => (ushort)((long)lParam >> 16);

        private static ushort LOWORD(IntPtr lParam) => (ushort)lParam;

        private static ushort HIWORD(UIntPtr wParam) => (ushort)((long)wParam >> 16);

        private static ushort LOWORD(UIntPtr wParam) => (ushort)wParam;

        private static ModifierKeys GetAsyncModifierKeys()
        {
            var result = ModifierKeys.None;
            if ((GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0)
            {
                result = ModifierKeys.Shift;
            }
            if ((GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0)
            {
                result |= ModifierKeys.Control;
            }
            if ((GetAsyncKeyState(VK_MENU) & 0x8000) != 0)
            {
                result |= ModifierKeys.Alternate;
            }
            if (((GetAsyncKeyState(VK_LWIN) | GetAsyncKeyState(VK_RWIN)) & 0x8000) != 0)
            {
                result |= ModifierKeys.Command;
            }
            if ((GetAsyncKeyState(VK_CAPITAL) & 1) != 0)
            {
                result |= ModifierKeys.CapsLock;
            }
            return result;
        }

        private static Keycode TranslateKeycode(UIntPtr wParam, IntPtr lParam)
        {
            switch ((ulong)wParam)
            {
                case VK_CONTROL:
                    if (((long)lParam & 0x01000000) != 0)
                    {
                        return Keycode.RightControl;
                    }
                    var messageTime = GetMessageTime();
                    if (PeekMessageW(out var msg, IntPtr.Zero, 0, 0, PM_NOREMOVE) != 0)
                    {
                        switch (msg.message)
                        {
                            case WM_KEYDOWN:
                            case WM_SYSKEYDOWN:
                            case WM_KEYUP:
                            case WM_SYSKEYUP:
                                if ((ulong)msg.wParam == VK_MENU && ((long)msg.lParam & 0x01000000) != 0 && messageTime == msg.time)
                                {
                                    return Keycode.Invalid;
                                }
                                break;
                        }
                    }
                    return Keycode.LeftControl;

                case VK_PROCESSKEY:
                    return Keycode.Invalid;
            }

            var index = HIWORD(lParam) & 0x1FF;
            return (index < keycodeMappings.Length) ? keycodeMappings[index] : Keycode.Invalid;
        }

        private static void InitializeKeyMappings()
        {
            keycodeMappings[0x001] = Keycode.Escape;
            keycodeMappings[0x002] = Keycode.One;
            keycodeMappings[0x003] = Keycode.Two;
            keycodeMappings[0x004] = Keycode.Three;
            keycodeMappings[0x005] = Keycode.Four;
            keycodeMappings[0x006] = Keycode.Five;
            keycodeMappings[0x007] = Keycode.Six;
            keycodeMappings[0x008] = Keycode.Seven;
            keycodeMappings[0x009] = Keycode.Eight;
            keycodeMappings[0x00A] = Keycode.Nine;
            keycodeMappings[0x00B] = Keycode.Zero;
            keycodeMappings[0x00C] = Keycode.Minus;
            keycodeMappings[0x00D] = Keycode.Equal;
            keycodeMappings[0x00E] = Keycode.Backspace;
            keycodeMappings[0x00F] = Keycode.Tab;
            keycodeMappings[0x010] = Keycode.Q;
            keycodeMappings[0x011] = Keycode.W;
            keycodeMappings[0x012] = Keycode.E;
            keycodeMappings[0x013] = Keycode.R;
            keycodeMappings[0x014] = Keycode.T;
            keycodeMappings[0x015] = Keycode.Y;
            keycodeMappings[0x016] = Keycode.U;
            keycodeMappings[0x017] = Keycode.I;
            keycodeMappings[0x018] = Keycode.O;
            keycodeMappings[0x019] = Keycode.P;
            keycodeMappings[0x01A] = Keycode.LeftBracket;
            keycodeMappings[0x01B] = Keycode.RightBracket;
            keycodeMappings[0x01C] = Keycode.Enter;
            keycodeMappings[0x01D] = Keycode.LeftControl;
            keycodeMappings[0x01E] = Keycode.A;
            keycodeMappings[0x01F] = Keycode.S;
            keycodeMappings[0x020] = Keycode.D;
            keycodeMappings[0x021] = Keycode.F;
            keycodeMappings[0x022] = Keycode.G;
            keycodeMappings[0x023] = Keycode.H;
            keycodeMappings[0x024] = Keycode.J;
            keycodeMappings[0x025] = Keycode.K;
            keycodeMappings[0x026] = Keycode.L;
            keycodeMappings[0x027] = Keycode.SemiColon;
            keycodeMappings[0x028] = Keycode.Apostrophe;
            keycodeMappings[0x029] = Keycode.Backquote;
            keycodeMappings[0x02A] = Keycode.LeftShift;
            keycodeMappings[0x02B] = Keycode.Backslash;
            keycodeMappings[0x02C] = Keycode.Z;
            keycodeMappings[0x02D] = Keycode.X;
            keycodeMappings[0x02E] = Keycode.C;
            keycodeMappings[0x02F] = Keycode.V;
            keycodeMappings[0x030] = Keycode.B;
            keycodeMappings[0x031] = Keycode.N;
            keycodeMappings[0x032] = Keycode.M;
            keycodeMappings[0x033] = Keycode.Comma;
            keycodeMappings[0x034] = Keycode.Dot;
            keycodeMappings[0x035] = Keycode.Slash;
            keycodeMappings[0x036] = Keycode.RightShift;
            keycodeMappings[0x037] = Keycode.KeypadAsterisk;
            keycodeMappings[0x038] = Keycode.LeftAlternate;
            keycodeMappings[0x039] = Keycode.Space;
            keycodeMappings[0x03A] = Keycode.CapsLock;
            keycodeMappings[0x03B] = Keycode.F1;
            keycodeMappings[0x03C] = Keycode.F2;
            keycodeMappings[0x03D] = Keycode.F3;
            keycodeMappings[0x03E] = Keycode.F4;
            keycodeMappings[0x03F] = Keycode.F5;
            keycodeMappings[0x040] = Keycode.F6;
            keycodeMappings[0x041] = Keycode.F7;
            keycodeMappings[0x042] = Keycode.F8;
            keycodeMappings[0x043] = Keycode.F9;
            keycodeMappings[0x044] = Keycode.F10;
            keycodeMappings[0x045] = Keycode.Pause;
            keycodeMappings[0x046] = Keycode.ScrollLock;
            keycodeMappings[0x047] = Keycode.KeypadSeven;
            keycodeMappings[0x048] = Keycode.KeypadEight;
            keycodeMappings[0x049] = Keycode.KeypadNine;
            keycodeMappings[0x04A] = Keycode.KeypadMinus;
            keycodeMappings[0x04B] = Keycode.KeypadFour;
            keycodeMappings[0x04C] = Keycode.KeypadFive;
            keycodeMappings[0x04D] = Keycode.KeypadSix;
            keycodeMappings[0x04E] = Keycode.KeypadPlus;
            keycodeMappings[0x04F] = Keycode.KeypadOne;
            keycodeMappings[0x050] = Keycode.KeypadTwo;
            keycodeMappings[0x051] = Keycode.KeypadThree;
            keycodeMappings[0x052] = Keycode.KeypadZero;
            keycodeMappings[0x053] = Keycode.KeypadDot;
            keycodeMappings[0x056] = Keycode.World1;
            keycodeMappings[0x057] = Keycode.F11;
            keycodeMappings[0x058] = Keycode.F12;
            keycodeMappings[0x064] = Keycode.F13;
            keycodeMappings[0x065] = Keycode.F14;
            keycodeMappings[0x066] = Keycode.F15;
            keycodeMappings[0x067] = Keycode.F16;
            keycodeMappings[0x068] = Keycode.F17;
            keycodeMappings[0x069] = Keycode.F18;
            keycodeMappings[0x06A] = Keycode.F19;
            keycodeMappings[0x06B] = Keycode.F20;
            keycodeMappings[0x06C] = Keycode.F21;
            keycodeMappings[0x06D] = Keycode.F22;
            keycodeMappings[0x06E] = Keycode.F23;
            keycodeMappings[0x076] = Keycode.F24;
            keycodeMappings[0x11C] = Keycode.KeypadEnter;
            keycodeMappings[0x11D] = Keycode.RightControl;
            keycodeMappings[0x135] = Keycode.KeypadSlash;
            keycodeMappings[0x137] = Keycode.PrintScreen;
            keycodeMappings[0x138] = Keycode.RightAlternate;
            keycodeMappings[0x145] = Keycode.NumLock;
            keycodeMappings[0x146] = Keycode.Pause;
            keycodeMappings[0x147] = Keycode.Home;
            keycodeMappings[0x148] = Keycode.Up;
            keycodeMappings[0x149] = Keycode.PageUp;
            keycodeMappings[0x14B] = Keycode.Left;
            keycodeMappings[0x14D] = Keycode.Right;
            keycodeMappings[0x14F] = Keycode.End;
            keycodeMappings[0x150] = Keycode.Down;
            keycodeMappings[0x151] = Keycode.PageDown;
            keycodeMappings[0x152] = Keycode.Insert;
            keycodeMappings[0x153] = Keycode.Delete;
            keycodeMappings[0x15B] = Keycode.LeftCommand;
            keycodeMappings[0x15C] = Keycode.RightCommand;
            keycodeMappings[0x15D] = Keycode.Menu;

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
