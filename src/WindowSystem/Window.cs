using System;

namespace NStuff.WindowSystem
{
    /// <summary>
    /// Represents a main window of the windowing system.
    /// </summary>
    public sealed class Window : IDisposable
    {
        private NativeWindowServerBase? nativeWindowServer;
        private bool freeLookMouse;
        private Cursor? cursor;

        /// <summary>
        /// The style of this window's borders.
        /// </summary>
        public WindowBorderStyle BorderStyle {
            get => GetNativeWindowServer().GetWindowBorderStyle(this);
            set => GetNativeWindowServer().SetWindowBorderStyle(this, value);
        }

        /// <summary>
        /// The size of each border of this window.
        /// </summary>
        public (double top, double left, double bottom, double right) BorderSize => GetNativeWindowServer().GetWindowBorderSize(this);

        /// <summary>
        /// The cursor displayed when this window is active.
        /// </summary>
        public Cursor? Cursor {
            get {
                if (Disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return cursor;
            }
            set {
                if (nativeWindowServer == null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                cursor = value;
                nativeWindowServer.SetWindowCursor(this);
            }
        }

        /// <summary>
        /// The position of the cursor relative to the window.
        /// </summary>
        public (double x, double y) CursorPosition {
            get {
                if (nativeWindowServer == null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return freeLookMouse ? FreeLookPosition : nativeWindowServer.GetCursorPosition(this);
            }
            set {
                if (nativeWindowServer == null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                if (freeLookMouse)
                {
                    FreeLookPosition = value;
                }
                else
                {
                    nativeWindowServer.SetCursorPosition(this, value);
                }
            }
        }

        /// <summary>
        /// A value indicating whether the cursor's <see cref="Dispose"/> method was called.
        /// </summary>
        /// <value><c>true</c> if <c>Dispose</c> was called.</value>
        public bool Disposed => NativeData == null;

        /// <summary>
        /// A value indicating whether the control has input focus.
        /// </summary>
        public bool Focused => GetNativeWindowServer().IsWindowFocused(this);

        /// <summary>
        /// A value indicating whether the mouse is currently in free look mode.
        /// </summary>
        public bool FreeLookMouse {
            get {
                if (Disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return freeLookMouse;
            }
            set {
                if (nativeWindowServer == null)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                freeLookMouse = value;
                FreeLookPosition = nativeWindowServer.GetCursorPosition(this);
                nativeWindowServer.SetFreeLookMouseWindow(this, value);
            }
        }

        internal (double x, double y) FreeLookPosition { get; set; }

        /// <summary>
        /// The point that represents the upper-left corner of the window in screen coordinates.
        /// </summary>
        public (double x, double y) Location {
            get => GetNativeWindowServer().GetWindowLocation(this);
            set => GetNativeWindowServer().SetWindowLocation(this, value);
        }

        /// <summary>
        /// The maximum size the window can be resized to.
        /// </summary>
        public (double width, double height) MaximumSize {
            get => GetNativeWindowServer().GetWindowMaximumSize(this);
            set => GetNativeWindowServer().SetWindowMaximumSize(this, value);
        }

        /// <summary>
        /// The minimum size the window can be resized to.
        /// </summary>
        public (double width, double height) MinimumSize {
            get => GetNativeWindowServer().GetWindowMinimumSize(this);
            set => GetNativeWindowServer().SetWindowMinimumSize(this, value);
        }

        /// <summary>
        /// A platform-dependent object set by the window server.
        /// </summary>
        public object? NativeData { get; internal set; }

        /// <summary>
        /// The opacity of this window. When the opacity is 0.0, the window is completely transparent,
        /// when the opacity is 1.0, the window is opaque.
        /// </summary>
        public double Opacity {
            get => GetNativeWindowServer().GetWindowOpacity(this);
            set => GetNativeWindowServer().SetWindowOpacity(this, value);
        }

        internal bool[] PressedKeys { get; } = new bool[(uint)Keycode.Invalid];

        internal IRenderingContext? RenderingContext { get; private set; }

        /// <summary>
        /// The size of this window.
        /// </summary>
        public (double width, double height) Size {
            get => GetNativeWindowServer().GetWindowSize(this);
            set => GetNativeWindowServer().SetWindowSize(this, value);
        }

        /// <summary>
        /// A value indicating the size state of the window, for example whether it is maximized or minimized.
        /// </summary>
        public WindowSizeState SizeState {
            get => GetNativeWindowServer().GetWindowSizeState(this);
            set => GetNativeWindowServer().SetWindowSizeState(this, value);
        }

        /// <summary>
        /// The title of the window.
        /// </summary>
        public string Title {
            get => GetNativeWindowServer().GetWindowTitle(this);
            set => GetNativeWindowServer().SetWindowTitle(this, value);
        }

        /// <summary>
        /// A value indicating whether the window should be displayed as the top-most window.
        /// </summary>
        public bool TopMost {
            get => GetNativeWindowServer().IsWindowTopMost(this);
            set => GetNativeWindowServer().SetWindowTopMost(this, value);
        }

        /// <summary>
        /// The size of the viewport of this window.
        /// </summary>
        public (double width, double height) ViewportSize => GetNativeWindowServer().GetWindowViewportSize(this);

        /// <summary>
        /// A value indicating whether the window is displayed.
        /// </summary>
        public bool Visible {
            get => GetNativeWindowServer().IsWindowVisible(this);
            set => GetNativeWindowServer().SetWindowVisible(this, value);
        }

        /// <summary>
        /// Occurs when the window was closed. When a window is closed it is also disposed.
        /// </summary>
        public event EventHandler<EmptyEventArgs>? Closed;

        /// <summary>
        /// Occurs after the window is being closed. The even handler can cancel the closing.
        /// </summary>
        public event MutableEventHandler<CancelEventArgs>? Closing;

        /// <summary>
        /// Occurs when one or more files are dropped inside the window.
        /// </summary>
        public event EventHandler<FileDropEventArgs>? FileDrop;

        /// <summary>
        /// Occurs when the framebuffer is resized.
        /// </summary>
        public event EventHandler<EmptyEventArgs>? FramebufferResize;

        /// <summary>
        /// Occurs when the window receives focus.
        /// </summary>
        public event EventHandler<EmptyEventArgs>? GotFocus;

        /// <summary>
        /// Occurs when a key is pressed.
        /// </summary>
        public event EventHandler<KeyEventArgs>? KeyDown;

        /// <summary>
        /// Occurs when a key is released.
        /// </summary>
        public event EventHandler<KeyEventArgs>? KeyUp;

        /// <summary>
        /// Occurs when the window loses focus.
        /// </summary>
        public event EventHandler<EmptyEventArgs>? LostFocus;

        /// <summary>
        /// Occurs when a mouse button is pressed.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs>? MouseDown;

        /// <summary>
        /// Occurs when the mouse pointer enters the window.
        /// </summary>
        public event EventHandler<EmptyEventArgs>? MouseEnter;

        /// <summary>
        /// Occurs when the mouse pointer leaves the window.
        /// </summary>
        public event EventHandler<EmptyEventArgs>? MouseLeave;

        /// <summary>
        /// Occurs when the mouse cursor position changed.
        /// </summary>
        public event EventHandler<MousePositionEventArgs>? MouseMove;

        /// <summary>
        /// Occurs when a mouse button is released.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs>? MouseUp;

        /// <summary>
        /// Occurs when the window is moved.
        /// </summary>
        public event EventHandler<EmptyEventArgs>? Move;

        /// <summary>
        /// Occurs when the window is resized.
        /// </summary>
        public event EventHandler<EmptyEventArgs>? Resize;

        /// <summary>
        /// Occurs when the <see cref="SizeState"/> property value changed.
        /// </summary>
        public event EventHandler<EmptyEventArgs>? SizeStateChanged;

        /// <summary>
        /// Occurs when a scroll gesture occurred.
        /// </summary>
        public event EventHandler<ScrollEventArgs>? Scroll;

        /// <summary>
        /// Occurs when a text input occurred.
        /// </summary>
        public event EventHandler<TextInputEventArgs>? TextInput;

        /// <summary>
        /// Occurs when the <see cref="Visible"/> property value changed.
        /// </summary>
        public event EventHandler<EmptyEventArgs>? VisibleChanged;

        internal Window(WindowServer windowServer, IRenderingContext renderingContext)
        {
            nativeWindowServer = windowServer.NativeWindowServer;
            if (nativeWindowServer == null)
            {
                throw new InvalidOperationException();
            }
            RenderingContext = renderingContext;
            nativeWindowServer.CreateWindowData(this);
            renderingContext.AttachRenderingData(windowServer, this);
            nativeWindowServer.CreateWindow(this);
            renderingContext.SetupRenderingData(windowServer, this);
        }

        /// <summary>
        /// Ensures that resources are freed and other cleanup operations are performed when the garbage collector
        /// reclaims the <see cref="Window"/>.
        /// </summary>
        ~Window() => FreeResources();

        /// <summary>
        /// Releases the resources associated with this object. After calling this method, calling the other methods of this object
        /// is throwing an <c>ObjectDisposedException</c>.
        /// </summary>
        public void Dispose()
        {
            FreeResources();
            GC.SuppressFinalize(this);
        }

        private void FreeResources()
        {
            if (nativeWindowServer != null)
            {
                RenderingContext?.DetachRenderingData(this);
                if (FreeLookMouse)
                {
                    FreeLookMouse = false;
                }
                nativeWindowServer.DestroyWindow(this);
                nativeWindowServer = null;
                RenderingContext = null;
            }
        }

        /// <summary>
        /// Attempts to bring the window to the foreground and activates it.
        /// </summary>
        public void Activate() => GetNativeWindowServer().ActivateWindow(this);

        /// <summary>
        /// Closes the window. It is also disposed.
        /// </summary>
        public void Close()
        {
            if (nativeWindowServer == null)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            OnClosed(new EmptyEventArgs());
            Dispose();
        }

        /// <summary>
        /// Tells whether the key corresponding to the supplied keycode is pressed.
        /// </summary>
        /// <param name="keycode">A keycode that identifies a keyboard key.</param>
        /// <returns><c>true</c> if the key is in a pressed state.</returns>
        public bool IsKeyPressed(Keycode keycode)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            return PressedKeys[(int)keycode];
        }

        /// <summary>
        /// Converts the supplied point from screen coordinate space to window coordinate space.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        /// <returns>A point in window coordinate space.</returns>
        public (double x, double y) PointFromScreen((double x, double y) point) => GetNativeWindowServer().ConvertFromScreen(this, point);

        /// <summary>
        /// Converts the supplied point from window coordinate space to screen coordinate space.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        /// <returns>A point in screen coordinate space.</returns>
        public (double x, double y) PointToScreen((double x, double y) point) => GetNativeWindowServer().ConvertToScreen(this, point);

        /// <summary>
        /// Requests the attention of the user if the window in not visible. For example the icon of the window in the taskbar flashes in Windows.
        /// </summary>
        public void RequestAttention() => GetNativeWindowServer().RequestWindowAttention(this);

        internal void OnClosed(EmptyEventArgs e) => Closed?.Invoke(this, e);
        internal void OnClosing(ref CancelEventArgs e) => Closing?.Invoke(this, ref e);
        internal void OnFileDrop(FileDropEventArgs e) => FileDrop?.Invoke(this, e);
        internal void OnFramebufferResize(EmptyEventArgs e) => FramebufferResize?.Invoke(this, e);
        internal void OnGotFocus(EmptyEventArgs e) => GotFocus?.Invoke(this, e);
        internal void OnKeyDown(KeyEventArgs e) => KeyDown?.Invoke(this, e);
        internal void OnKeyUp(KeyEventArgs e) => KeyUp?.Invoke(this, e);
        internal void OnLostFocus(EmptyEventArgs e) => LostFocus?.Invoke(this, e);
        internal void OnMouseDown(MouseButtonEventArgs e) => MouseDown?.Invoke(this, e);
        internal void OnMouseEnter(EmptyEventArgs e) => MouseEnter?.Invoke(this, e);
        internal void OnMouseLeave(EmptyEventArgs e) => MouseLeave?.Invoke(this, e);
        internal void OnMouseMove(MousePositionEventArgs e) => MouseMove?.Invoke(this, e);
        internal void OnMouseUp(MouseButtonEventArgs e) => MouseUp?.Invoke(this, e);
        internal void OnMove(EmptyEventArgs e) => Move?.Invoke(this, e);
        internal void OnResize(EmptyEventArgs e) => Resize?.Invoke(this, e);
        internal void OnSizeStateChanged(EmptyEventArgs e) => SizeStateChanged?.Invoke(this, e);
        internal void OnScroll(ScrollEventArgs e) => Scroll?.Invoke(this, e);
        internal void OnTextInput(TextInputEventArgs e) => TextInput?.Invoke(this, e);
        internal void OnVisibleChanged(EmptyEventArgs e) => VisibleChanged?.Invoke(this, e);

        private NativeWindowServerBase GetNativeWindowServer() => nativeWindowServer ?? throw new ObjectDisposedException(GetType().FullName);
    }
}
