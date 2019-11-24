using System.Collections.Generic;

namespace NStuff.WindowSystem
{
    /// <summary>
    /// Provides a base class to implement a platform-specific window server.
    /// </summary>
    public abstract class NativeWindowServerBase
    {
        /// <summary>
        /// The preferred surface width when a window is created.
        /// </summary>
        protected const int DefaultSurfaceWidth = 480;

        /// <summary>
        /// The preferred surface height when a window is created.
        /// </summary>
        protected const int DefaultSurfaceHeight = 300;

        /// <summary>
        /// Tells whether the server is currently running.
        /// </summary>
        /// <returns><c>true</c> if the server is running.</returns>
        public abstract bool IsRunning();

        /// <summary>
        /// Stops the server.
        /// </summary>
        public abstract void Shutdown();

        /// <summary>
        /// Creates and attaches the native data corresponding to the window.
        /// It is called during window creation just before <see cref="IRenderingContext.AttachRenderingData(WindowServer, Window)"/>.
        /// </summary>
        /// <param name="window">The window to initalize.</param>
        public abstract void CreateWindowData(Window window);

        /// <summary>
        /// Creates the native window corresponding to the supplied managed window.
        /// It is called during window creation after <see cref="IRenderingContext.AttachRenderingData(WindowServer, Window)"/>
        /// and just before <see cref="IRenderingContext.SetupRenderingData(WindowServer, Window)"/>.
        /// </summary>
        /// <param name="window">The window to initialize.</param>
        public abstract void CreateWindow(Window window);

        /// <summary>
        /// Destroys the native window corresponding to the supplied managed window.
        /// </summary>
        /// <param name="window">The window to destroy.</param>
        public abstract void DestroyWindow(Window window);

        /// <summary>
        /// Recreates if needed the native window corresponding to the supplied window in response to a graphic mode modification.
        /// </summary>
        /// <param name="window">The window to recreate.</param>
        public abstract void RecreateNativeWindow(Window window);

        /// <summary>
        /// Gets the windows currently alive.
        /// </summary>
        /// <returns>A list of managed windows.</returns>
        public abstract ICollection<Window> GetWindows();

        /// <summary>
        /// Attempts to bring the window to the foreground and activates it.
        /// </summary>
        /// <param name="window">The window to activate.</param>
        public abstract void ActivateWindow(Window window);

        /// <summary>
        /// Tells whether the supplied window is currently visible.
        /// </summary>
        /// <param name="window">The window to test.</param>
        /// <returns><c>true</c> if the window is visible.</returns>
        public abstract bool IsWindowVisible(Window window);

        /// <summary>
        /// Changes the visibilty of the supplied window.
        /// </summary>
        /// <param name="window">The window to modify.</param>
        /// <param name="visible"><c>true</c> to make the window visible.</param>
        public abstract void SetWindowVisible(Window window, bool visible);

        /// <summary>
        /// Tells whether the supplied window is currently focused.
        /// </summary>
        /// <param name="window">The window to test.</param>
        /// <returns><c>true</c> if the window is focused.</returns>
        public abstract bool IsWindowFocused(Window window);

        /// <summary>
        /// Tells whether the supplied window should be displayed as the top-most window.
        /// </summary>
        /// <param name="window">The window to test.</param>
        /// <returns><c>true</c> if the window should be displayed as the top-most window.</returns>
        public abstract bool IsWindowTopMost(Window window);

        /// <summary>
        /// Changes the state of the supplied window by making it top-most or not.
        /// </summary>
        /// <param name="window">The window to modify.</param>
        /// <param name="topMost"><c>true</c> to make the window a top-most window.</param>
        public abstract void SetWindowTopMost(Window window, bool topMost);

        /// <summary>
        /// Gets the size of each of the four borders of the supplied window.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <returns>The size of each of the four borders of the window.</returns>
        public abstract (double top, double left, double bottom, double right) GetWindowBorderSize(Window window);

        /// <summary>
        /// Updates the cursor of the supplied window using the value of <see cref="Window.Cursor"/>.
        /// </summary>
        /// <param name="window">The window to update.</param>
        public abstract void SetWindowCursor(Window window);

        /// <summary>
        /// Gets the style of the supplied window borders.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <returns>The style of the borders of the window.</returns>
        public abstract WindowBorderStyle GetWindowBorderStyle(Window window);

        /// <summary>
        /// Sets the style of the supplied window borders.
        /// </summary>
        /// <param name="window">The window to modify.</param>
        /// <param name="borderStyle">The border style to apply to the window.</param>
        public abstract void SetWindowBorderStyle(Window window, WindowBorderStyle borderStyle);

        /// <summary>
        /// Gets the location of the upper-left corner of the supplied window.
        /// </summary>
        /// <param name="window">The window to locate.</param>
        /// <returns>The coordinates of the upper-left corner of the window.</returns>
        public abstract (double x, double y) GetWindowLocation(Window window);

        /// <summary>
        /// Sets the location of the upper-left corner of the supplied window.
        /// </summary>
        /// <param name="window">The window to move.</param>
        /// <param name="location">The coordinates of the upper-left corner of the window.</param>
        public abstract void SetWindowLocation(Window window, (double x, double y) location);

        /// <summary>
        /// Gets the maximum size the supplied window can be resized to.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <returns>The maximum size of the window.</returns>
        public abstract (double width, double height) GetWindowMaximumSize(Window window);

        /// <summary>
        /// Sets the maximum size the supplied window can be resized to.
        /// </summary>
        /// <param name="window">The window to update.</param>
        /// <param name="size">The maximum size of the window.</param>
        public abstract void SetWindowMaximumSize(Window window, (double width, double height) size);

        /// <summary>
        /// Gets the minimum size the supplied window can be resized to.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <returns>The minimum size of the window.</returns>
        public abstract (double width, double height) GetWindowMinimumSize(Window window);

        /// <summary>
        /// Sets the minimum size the supplied window can be resized to.
        /// </summary>
        /// <param name="window">The window to update.</param>
        /// <param name="size">The minimum size of the window.</param>
        public abstract void SetWindowMinimumSize(Window window, (double width, double height) size);

        /// <summary>
        /// Gets a value representing the opacity of the supplied window.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <returns>A value between 0.0 (fully transparent) and 1.0 (fully opaque).</returns>
        public abstract double GetWindowOpacity(Window window);

        /// <summary>
        /// Sets a value representing the opacity of the supplied window.
        /// </summary>
        /// <param name="window">The window to modify.</param>
        /// <param name="opacity">A value between 0.0 (fully transparent) and 1.0 (fully opaque).</param>
        public abstract void SetWindowOpacity(Window window, double opacity);

        /// <summary>
        /// Gets the size of the supplied window.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <returns>The size of the window.</returns>
        public abstract (double width, double height) GetWindowSize(Window window);

        /// <summary>
        /// Sets the size of the supplied window.
        /// </summary>
        /// <param name="window">The window to modify.</param>
        /// <param name="size">The size to assign to the window.</param>
        public abstract void SetWindowSize(Window window, (double width, double height) size);

        /// <summary>
        /// Gets the size state of the supplied window.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <returns>The state of the window.</returns>
        public abstract WindowSizeState GetWindowSizeState(Window window);

        /// <summary>
        /// Sets the size state of the supplied window.
        /// </summary>
        /// <param name="window">The window to modify.</param>
        /// <param name="sizeState">The state to assign to the window.</param>
        public abstract void SetWindowSizeState(Window window, WindowSizeState sizeState);

        /// <summary>
        /// Gets the title of the supplied window.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <returns>A string representing the title of the window.</returns>
        public abstract string GetWindowTitle(Window window);

        /// <summary>
        /// Sets the title of the supplied window.
        /// </summary>
        /// <param name="window">The window to modify.</param>
        /// <param name="title">A string representing the title of the window.</param>
        public abstract void SetWindowTitle(Window window, string title);

        /// <summary>
        /// Sets the supplied window free look mode.
        /// </summary>
        /// <param name="window">The window to modify.</param>
        /// <param name="enable"><c>true</c> to enable the free look mode.</param>
        public abstract void SetFreeLookMouseWindow(Window window, bool enable);

        /// <summary>
        /// Converts the supplied point from screen coordinate space to window coordinate space.
        /// </summary>
        /// <param name="window">The reference window.</param>
        /// <param name="point">The point to convert.</param>
        /// <returns>The point converted to screen coordinate space.</returns>
        public abstract (double x, double y) ConvertFromScreen(Window window, (double x, double y) point);

        /// <summary>
        /// Converts the supplied point from window coordinate space to screen coordinate space.
        /// </summary>
        /// <param name="window">The reference window.</param>
        /// <param name="point">The point to convert.</param>
        /// <returns>The point converted to window coordinate space.</returns>
        public abstract (double x, double y) ConvertToScreen(Window window, (double x, double y) point);

        /// <summary>
        /// Requests the attention of the user if the window in not visible.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        public abstract void RequestWindowAttention(Window window);

        /// <summary>
        /// Creates the native data and attaches it to the managed cursor using the supplied predefined shape.
        /// </summary>
        /// <param name="cursor">The cursor to initialize.</param>
        /// <param name="shape">The predefined shape of the created cursor.</param>
        public abstract void CreateCursor(Cursor cursor, CursorShape shape);

        /// <summary>
        /// Creates the native data and attaches it to the managed cursor using the supplied image.
        /// </summary>
        /// <param name="cursor">The cursor to initialize.</param>
        /// <param name="imageData">The bytes of the image representing the cursor.</param>
        /// <param name="size">The size of the image.</param>
        /// <param name="hotSpot">The hot spot of the cursor in the supplied image.</param>
        public abstract void CreateCursor(Cursor cursor, byte[] imageData, (int width, int height) size, (double x, double y) hotSpot);

        /// <summary>
        /// Destroys the native data of the supplied cursor.
        /// </summary>
        /// <param name="cursor">The cursor to destroy.</param>
        public abstract void DestroyCursor(Cursor cursor);

        /// <summary>
        /// Gets the position of the cursor relative to the supplied window.
        /// </summary>
        /// <param name="window">The reference window.</param>
        /// <returns>A point in window coordinate space.</returns>
        public abstract (double x, double y) GetCursorPosition(Window window);

        /// <summary>
        /// Sets the position of the cursor relative to the supplied window.
        /// </summary>
        /// <param name="window">The reference window.</param>
        /// <param name="position">A point in window coordinate space.</param>
        public abstract void SetCursorPosition(Window window, (double x, double y) position);

        /// <summary>
        /// Gets the size of the viewport of the supplied window.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <returns>A size dependent of the resolution of the drawing surface.</returns>
        public abstract (double x, double y) GetWindowViewportSize(Window window);

        /// <summary>
        /// Gets the contents of the clipboard.
        /// </summary>
        /// <returns>A string representing the clipboard.</returns>
        public abstract string GetClipboardString();

        /// <summary>
        /// Sets the contents of the clipboard.
        /// </summary>
        /// <param name="text">A string to send to the clipboard.</param>
        public abstract void SetClipboardString(string text);

        /// <summary>
        /// Gets the set of pressed modifier keys.
        /// </summary>
        /// <returns>The flags representing the pressed modifier keys.</returns>
        public abstract ModifierKeys GetModifierKeys();

        /// <summary>
        /// Gets a human readable representation of the supplied keycode.
        /// </summary>
        /// <param name="keycode">The keycode to encode.</param>
        /// <returns>A string representing the keycode.</returns>
        public abstract string ConvertKeycodeToString(Keycode keycode);

        /// <summary>
        /// Gets a value in seconds with millisecond precision representing the time of the last key or mouse event.
        /// </summary>
        /// <returns>A timestamp.</returns>
        public abstract double GetEventTime();

        /// <summary>
        /// Blocks until an event occurs, and then process the queued events.
        /// </summary>
        /// <returns><c>true</c> if at least one event was processed.</returns>
        public abstract bool WaitAndProcessEvents();

        /// <summary>
        /// Blocks until the supplied timeout expires or an event occurs.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns><c>true</c> if at least one event was processed.</returns>
        public abstract bool WaitAndProcessEvents(double timeout);

        /// <summary>
        /// Processes the queued events if any.
        /// </summary>
        /// <returns><c>true</c> if at least one event was processed.</returns>
        public abstract bool ProcessEvents();

        /// <summary>
        /// Interrupts <see cref="WaitAndProcessEvents"/> or <see cref="WaitAndProcessEvents(double)"/> if the methods
        /// are actually waiting for events.
        /// </summary>
        public abstract void UnblockProcessEvents();

        /// <summary>
        /// To be called by the native server to signal a close request.
        /// </summary>
        /// <param name="window">The window to close.</param>
        protected static void CloseRequestOccurred(Window window)
        {
            var args = new CancelEventArgs(false);
            window.OnClosing(ref args);
            if (!args.Cancel)
            {
                window.Close();
            }
        }

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.FileDrop"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        /// <param name="x">The x position of the cursor.</param>
        /// <param name="y">The y position of the cursor.</param>
        /// <param name="paths">The list of file paths.</param>
        protected static void FileDropEventOccurred(Window window, double x, double y, string[] paths) =>
            window.OnFileDrop(new FileDropEventArgs((x, y), paths));

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.FramebufferResize"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        protected static void FramebufferResizeEventOccurred(Window window) => window.OnFramebufferResize(new EmptyEventArgs());

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.GotFocus"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        protected static void GotFocusEventOccurred(Window window) => window.OnGotFocus(new EmptyEventArgs());

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.KeyDown"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        /// <param name="keycode">The keycode of the key.</param>
        /// <param name="modifiers">The pressed modifier keys.</param>
        protected static void KeyDownEventOccurred(Window window, Keycode keycode, ModifierKeys modifiers)
        {
            var repeated = window.PressedKeys[(int)keycode];
            window.PressedKeys[(int)keycode] = true;
            window.OnKeyDown(new KeyEventArgs(keycode, modifiers, repeated));
        }

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.KeyUp"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        /// <param name="keycode">The keycode of the key.</param>
        /// <param name="modifiers">The pressed modifier keys.</param>
        protected static void KeyUpEventOccurred(Window window, Keycode keycode, ModifierKeys modifiers)
        {
            if (window.PressedKeys[(int)keycode])
            {
                window.PressedKeys[(int)keycode] = false;
                window.OnKeyUp(new KeyEventArgs(keycode, modifiers, false));
            }
        }

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.LostFocus"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        protected static void LostFocusEventOccurred(Window window)
        {
            window.OnLostFocus(new EmptyEventArgs());
            for (int i = 1; i < window.PressedKeys.Length; i++)
            {
                if (window.PressedKeys[i])
                {
                    KeyUpEventOccurred(window, (Keycode)i, ModifierKeys.None);
                }
            }
        }

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.MouseDown"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        /// <param name="mouseButton">The pressed mouse button.</param>
        protected static void MouseDownEventOccurred(Window window, MouseButton mouseButton) =>
            window.OnMouseDown(new MouseButtonEventArgs(MouseButtonState.Pressed, mouseButton));

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.MouseEnter"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        protected static void MouseEnterEventOccurred(Window window) => window.OnMouseEnter(new EmptyEventArgs());

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.MouseLeave"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        protected static void MouseLeaveEventOccurred(Window window) => window.OnMouseLeave(new EmptyEventArgs());

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.MouseMove"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        /// <param name="x">The x position of the mouse.</param>
        /// <param name="y">The y position of the mouse.</param>
        protected static void MouseMoveEventOccurred(Window window, double x, double y)
        {
            if (!window.Disposed && window.FreeLookMouse)
            {
                if (window.FreeLookPosition.x == x && window.FreeLookPosition.y == y)
                {
                    return;
                }
                window.FreeLookPosition = (x, y);
            }
            window.OnMouseMove(new MousePositionEventArgs((x, y)));
        }

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.MouseUp"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        /// <param name="mouseButton">The released mouse button.</param>
        protected static void MouseUpEventOccurred(Window window, MouseButton mouseButton) =>
            window.OnMouseUp(new MouseButtonEventArgs(MouseButtonState.Released, mouseButton));

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.Move"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        protected static void MoveEventOccurred(Window window) => window.OnMove(new EmptyEventArgs());

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.Resize"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        protected static void ResizeEventOccurred(Window window) => window.OnResize(new EmptyEventArgs());

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.SizeStateChanged"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        protected static void SizeStateChangedEventOccurred(Window window) => window.OnSizeStateChanged(new EmptyEventArgs());

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.Scroll"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        /// <param name="deltaX">The horizontal scroll amount.</param>
        /// <param name="deltaY">The vertical scroll amount.</param>
        protected static void ScrollEventOccurred(Window window, double deltaX, double deltaY) =>
            window.OnScroll(new ScrollEventArgs(deltaX, deltaY));

        protected static void TextInputEventOccurred(Window window, int codePoint, ModifierKeys modifierKeys)
        {
            if (codePoint >= 32 && (codePoint <= 126 || codePoint >= 160))
            {
                window.OnTextInput(new TextInputEventArgs(codePoint, modifierKeys));
            }
        }

        /// <summary>
        /// To be called by the native server to signal a <see cref="Window.VisibleChanged"/> event.
        /// </summary>
        /// <param name="window">The window to signal.</param>
        protected static void VisibleChangedEventOccurred(Window window) => window.OnVisibleChanged(new EmptyEventArgs());
    }
}
