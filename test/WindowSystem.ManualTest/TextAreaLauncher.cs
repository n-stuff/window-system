using NStuff.GraphicsBackend;
using NStuff.OpenGL.Backend;
using NStuff.OpenGL.Context;
using NStuff.Text;
using NStuff.Typography.Font;
using NStuff.WindowSystem.ManualTest.VectorGraphics;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace NStuff.WindowSystem.ManualTest
{
    class TextAreaLauncher
    {
        internal void Launch()
        {
            Console.WriteLine("Text Area...");

            using var windowServer = new WindowServer();
            using var renderingContext = new RenderingContext { Settings = new RenderingSettings { Samples = 8 } };
            using var window = windowServer.CreateWindow(renderingContext);

            window.Title = "Text Area";
            window.BorderStyle = WindowBorderStyle.Sizable;
            window.Size = (900, 900);
            renderingContext.CurrentWindow = window;

            var viewportSize = window.ViewportSize;
            var windowSize = window.Size;
            var backend = new DrawingBackend(new EntryPointLoader(renderingContext))
            {
                PixelScaling = viewportSize.height / windowSize.height
            };

            var openTypeCollection = new OpenTypeCollection();
            var nameId = typeof(BezierLauncher).Namespace + ".Resources.Hack-Regular.ttf";
            openTypeCollection.AddFontResource(nameId, () => Assembly.GetExecutingAssembly().GetManifestResourceStream(nameId)!);

            var drawingContext = new DrawingContext(backend, openTypeCollection)
            {
                ClearColor = new RgbaColor(55, 55, 55, 255)
            };

            var styles = new MonospaceTextStyles(256);
            var fontSubfamily = openTypeCollection.LookupFontSubfamily("Hack", FontSubfamily.Normal);
            styles.GetStyle(fontSubfamily, new RgbaColor(255, 255, 255, 255), new RgbaColor(0, 0, 0, 0));
            var textArea = new TextArea(styles, "Hack", fontSubfamily, 9);
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "..", "..", "..", "TextAreaLauncher.cs");
            textArea.Load(path);
            const int textLeft = 20;
            const int textTop = 30;

            window.Scroll += (sender, e) =>
            {
                textArea.ScrollVertically(e.DeltaY * 30);
            };

            var hideCaret = true;

            window.KeyDown += (sender, e) =>
            {
                switch (e.Keycode)
                {
                    case Keycode.Up:
                        textArea.MoveUp();
                        hideCaret = false;
                        break;
                    case Keycode.Down:
                        textArea.MoveDown();
                        hideCaret = false;
                        break;
                    case Keycode.Left:
                        textArea.MoveLeft();
                        hideCaret = false;
                        break;
                    case Keycode.Right:
                        textArea.MoveRight();
                        hideCaret = false;
                        break;
                    case Keycode.F11:
                        textArea.FontPoints--;
                        break;
                    case Keycode.F12:
                        textArea.FontPoints++;
                        break;
                    case Keycode.Backspace:
                        textArea.Backspace();
                        break;
                    case Keycode.Tab:
                        textArea.Tab();
                        break;
                    case Keycode.Enter:
                        textArea.Enter();
                        break;
                }
            };

            window.TextInput += (sender, e) =>
            {
                textArea.Insert(e.CodePoint);
            };

            window.MouseDown += (sender, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    var (x, y) = window.CursorPosition;
                    textArea.LeftMouseDown(x, y);
                }
            };

            var interruptThread = false;

            void draw()
            {
                renderingContext.CurrentWindow = window;
                backend.PixelScaling = viewportSize.height / windowSize.height;

                drawingContext.StartDrawing();

                textArea.Render(drawingContext);

                drawingContext.FinishDrawing();

                renderingContext.SwapBuffers(window);
            }

            void resize()
            {
                viewportSize = window.ViewportSize;
                windowSize = window.Size;
                renderingContext.CurrentWindow = window;
                backend.WindowSize = windowSize;
                textArea.SetViewRectangle(textLeft, textTop, (windowSize.width - textLeft - 30), (windowSize.height - textTop - 50));
            }

            window.Resize += (sender, e) =>
            {
                resize();
                draw();
            };

            var runLoop = MainRunLoop.Create(windowServer);
            string[]? text = null;
            var version = 0;
            var th = new Thread(() =>
            {
                var regex = new Regex("\\b(break|case|class|const|else|false|float|for|foreach|" +
                    "if|int|internal|public|namespace|new|unsafe|using|string|switch|true|var|void)\\b");
                var style = styles.GetStyle(fontSubfamily,
                    new RgbaColor (81, 153, 213, 255),
                    new RgbaColor(0, 0, 0, 0));

                while (!interruptThread)
                {
                    var t = text;
                    text = null;
                    var v = version;
                    if (t != null)
                    {
                        var txt = new StyledMonospaceText(new DecoratedText<byte>(), styles);
                        (int line, int column) loc = (0, 0);
                        for (int i = 0; i < t.Length; i++)
                        {
                            loc = txt.Insert(loc, t[i]);
                        }
                        for (int i = 0; i < t.Length; i++)
                        {
                            var m = regex.Match(t[i]);
                            while (m.Success)
                            {
                                txt.StyleRange(style, (i, m.Index), (i, m.Index + m.Length));
                                m = m.NextMatch();
                            }
                        }
                        void f(int currentVersion, StyledMonospaceText newText)
                        {
                            runLoop.InvokeLater(0, (lag) =>
                            {
                                if (textArea.Version == currentVersion)
                                {
                                    textArea.SetText(newText);
                                }
                            });
                        }
                        f(v, txt);
                    }
                    Thread.Sleep(500);
                }
            });

            void blink(long lag)
            {
                textArea.HideCaret = hideCaret;
                hideCaret = !hideCaret;
                if (textArea.RequireRestyle)
                {
                    text = textArea.GetText();
                    version = textArea.Version;
                }
                runLoop.InvokeLater(500, blink);
            }

            runLoop.InvokeLater(500, blink);

            window.Closed += (sender, e) =>
            {
                backend.Dispose();
                runLoop.Interrupt();
                interruptThread = true;
            };

            resize();
            runLoop.RecurringActions.Add((time) =>
            {
                if (textArea.RequireRender)
                {
                    draw();
                }
            });
            th.Start();
            window.Visible = true;
            runLoop.Run();

            Console.WriteLine("Text Area done.");
        }
    }
}

