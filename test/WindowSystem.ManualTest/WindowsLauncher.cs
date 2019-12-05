using NStuff.OpenGL.Context;
using NStuff.RasterGraphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using GLbitfield = System.UInt32;
using GLfloat = System.Single;

namespace NStuff.WindowSystem.ManualTest
{
    class WindowsLauncher
    {
        internal void Launch()
        {
            Console.WriteLine("Windows...");

            using var windowServer = new WindowServer();
            using var renderingContext = new RenderingContext();
            using var window1 = windowServer.CreateWindow(renderingContext);
            using var window2 = windowServer.CreateWindow(renderingContext);

            var eventLogger = new EventLogger();

            window1.Title = "Window 1";
            eventLogger.RegisterWindow(window1);

            window2.Title = "Window 2";
            window2.BorderStyle = WindowBorderStyle.Sizable;
            eventLogger.RegisterWindow(window2);
            var cursor = CreateCursor();
            window2.Cursor = windowServer.CreateCursor(cursor.Data, cursor.Size, (7, 7));

            renderingContext.CurrentWindow = window1;
            var glClear = renderingContext.GetOpenGLEntryPoint<GLClearDelegate>("glClear");
            var glClearColor = renderingContext.GetOpenGLEntryPoint<GLClearColorDelegate>("glClearColor");

            window1.Visible = true;
            window2.Visible = true;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (windowServer.Windows.Count > 0)
            {
                if (!window1.Disposed)
                {
                    renderingContext.CurrentWindow = window1;
                    glClearColor(0.2f, 0.5f, 0.8f, 1.0f);
                    glClear(GLBuffers.Color);
                    renderingContext.SwapBuffers(window1);
                }
                if (!window2.Disposed)
                {
                    renderingContext.CurrentWindow = window2;
                    glClearColor(0.8f, 0.5f, 0.2f, 1.0f);
                    glClear(GLBuffers.Color);
                    renderingContext.SwapBuffers(window2);
                }
                windowServer.ProcessEvents(0.02);
                eventLogger.PrintLog(stopwatch.ElapsedMilliseconds);
            }
            stopwatch.Stop();
            Console.WriteLine("Windows done.");
        }


        private static RasterImage CreateImage(params string[] asciiArt)
        {
            var result = new RasterImage
            {
                Size = (asciiArt.Length, asciiArt.Length)
            };
            var data = new byte[result.Size.width * result.Size.height * 4];
            result.Data = data;
            var index = 0;
            foreach (var l in asciiArt)
            {
                foreach (var c in l)
                {
                    switch (c)
                    {
                        case 'X': // Black
                            index += 3;
                            data[index++] = 255;
                            break;
                        case 'O': // White
                            data[index++] = 255;
                            data[index++] = 255;
                            data[index++] = 255;
                            data[index++] = 255;
                            break;
                        default:
                            index += 4;
                            break;
                    }
                }
            }
            return result;
        }

        private static RasterImage CreateCursor()
        {
            return CreateImage(
                "                ",
                "        XXXXXXX ",
                "       XXXXXXXX ",
                "      XOOOOOOXX ",
                "     OOOOOOOOXX ",
                "    OXXXXXXOOXX ",
                "   OOXOOOOXOOXX ",
                "  XOOXO  OXOOXX ",
                " XXOOXO  OXOOXX ",
                " XXOOXOOOOXOOXX ",
                " XXOOXXXXXXOOXX ",
                " XXOOOOOOOOOOXX ",
                " XXOOOOOOOOOOXX ",
                " XXXXXXXXXXXXXX ",
                " XXXXXXXXXXXXXX ",
                "                "
            );
        }
    }

    class EventLogger
    {
        long milliseconds;
        internal List<LogEntry> entries = new List<LogEntry>();

        internal void RegisterWindow(Window window)
        {
            window.Move += Moved;
            window.MouseEnter += MouseEnter;
            window.MouseLeave += MouseLeave;
            window.MouseMove += MouseMoved;
            window.MouseDown += MouseDown;
            window.MouseUp += MouseUp;
            window.Scroll += Scroll;
            window.KeyDown += KeyDown;
            window.KeyUp += KeyUp;
            window.TextInput += TextInput;
            window.GotFocus += GotFocus;
            window.LostFocus += LostFocus;
            window.Resize += Resize;
            window.FileDrop += FileDrop;
        }

        internal void PrintLog(long milliseconds)
        {
            if (this.milliseconds + 300 > milliseconds)
            {
                return;
            }
            this.milliseconds = milliseconds;
            int i = 0;
            while (i < entries.Count)
            {
                var entry1 = entries[i++];
                Console.WriteLine($"'{entry1.label}' [{entry1.type}] {entry1.message}");
                LogEntry? lastEntry = null;
                int n = 0;
                while (i < entries.Count)
                {
                    var entry2 = entries[i++];
                    if (entry1.label != entry2.label || entry1.type != entry2.type)
                    {
                        --i;
                        break;
                    }
                    n++;
                    lastEntry = entry2;
                }
                if (lastEntry != null)
                {
                    Console.WriteLine($"... {n} more ...");
                    --i;
                }
            }
            entries.Clear();
        }

        void Moved(object? sender, EmptyEventArgs _)
        {
            var window = (Window?)sender ?? throw new InvalidOperationException();
            entries.Add(new LogEntry { label = window.Title, type = "Moved     ", message = window.Location.ToString() });
        }

        void MouseMoved(object? sender, MousePositionEventArgs args)
        {
            var window = (Window?)sender ?? throw new InvalidOperationException();
            entries.Add(new LogEntry { label = window.Title, type = "MouseMoved", message = args.Position.ToString() });
        }

        void MouseEnter(object? sender, EmptyEventArgs _)
        {
            var window = (Window?)sender ?? throw new InvalidOperationException();
            entries.Add(new LogEntry { label = window.Title, type = "MouseEnter", message = "" });
        }

        void MouseLeave(object? sender, EmptyEventArgs _)
        {
            var window = (Window?)sender ?? throw new InvalidOperationException();
            entries.Add(new LogEntry { label = window.Title, type = "MouseLeave", message = "" });
        }

        void MouseDown(object? sender, MouseButtonEventArgs args)
        {
            var window = (Window?)sender ?? throw new InvalidOperationException();
            entries.Add(new LogEntry { label = window.Title, type = "MouseDown ", message = $"Button: {args.ChangedButton}" });
        }

        void MouseUp(object? sender, MouseButtonEventArgs args)
        {
            var window = (Window?)sender ?? throw new InvalidOperationException();
            entries.Add(new LogEntry { label = window.Title, type = "MouseUp   ", message = $"Button: {args.ChangedButton}" });
        }

        void Scroll(object? sender, ScrollEventArgs args)
        {
            var window = (Window?)sender ?? throw new InvalidOperationException();
            entries.Add(new LogEntry { label = window.Title, type = "Scroll    ", message = $"dx: {args.DeltaX}, dy: {args.DeltaY}" });
        }

        void KeyDown(object? sender, KeyEventArgs args)
        {
            var window = (Window?)sender ?? throw new InvalidOperationException();
            entries.Add(new LogEntry
            {
                label = window.Title,
                type = "KeyDown   ",
                message = $"Key: {args.Keycode}, Modifiers: [{args.ModifierKeys}] repeat? {args.IsRepeat}"
            });
        }

        void KeyUp(object? sender, KeyEventArgs args)
        {
            var window = (Window?)sender ?? throw new InvalidOperationException();
            entries.Add(new LogEntry
            {
                label = window.Title,
                type = "KeyUp     ",
                message = $"Key: {args.Keycode}, Modifiers [{args.ModifierKeys}]"
            });
        }

        void TextInput(object? sender, TextInputEventArgs args)
        {
            var window = (Window?)sender ?? throw new InvalidOperationException();
            entries.Add(new LogEntry
            {
                label = window.Title,
                type = "TextInput ",
                message = $"'{char.ConvertFromUtf32(args.CodePoint)}', CodePoint: {args.CodePoint}, Modifiers [{args.ModifierKeys}]"
            });
        }

        void GotFocus(object? sender, EmptyEventArgs _)
        {
            var window = (Window?)sender ?? throw new InvalidOperationException();
            entries.Add(new LogEntry { label = window.Title, type = "GotFocus  ", message = "" });
        }

        void LostFocus(object? sender, EmptyEventArgs _)
        {
            var window = (Window?)sender ?? throw new InvalidOperationException();
            entries.Add(new LogEntry { label = window.Title, type = "LostFocus ", message = "" });
        }

        void Resize(object? sender, EmptyEventArgs _)
        {
            var window = (Window?)sender ?? throw new InvalidOperationException();
            entries.Add(new LogEntry { label = window.Title, type = "Resize    ", message = $"{window.Size}" });
        }

        void FileDrop(object? sender, FileDropEventArgs args)
        {
            var window = (Window?)sender ?? throw new InvalidOperationException();
            var builder = new System.Text.StringBuilder();
            builder.Append(args.Position);
            builder.Append(":\n");
            foreach (var path in args.Paths)
            {
                builder.Append("  - '");
                builder.Append(path);
                builder.Append("'\n");
            }
            entries.Add(new LogEntry { label = window.Title, type = "FileDrop  ", message = builder.ToString() });
        }

        internal class LogEntry
        {
            internal string label = "";
            internal string type = "";
            internal string message = "";
        }
    }

    enum GLBuffers : GLbitfield
    {
        Color = 0x00004000,
        Depth = 0x00000100,
        Stencil = 0x00000400,
    }

    delegate void GLClearDelegate(GLBuffers mask);
    delegate void GLClearColorDelegate(GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha);
}
