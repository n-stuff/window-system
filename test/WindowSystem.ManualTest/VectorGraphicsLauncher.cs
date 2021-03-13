#nullable disable
using NStuff.GraphicsBackend;
using NStuff.OpenGL.Backend;
using NStuff.OpenGL.Context;
using NStuff.RasterGraphics;
using NStuff.Typography.Font;
using NStuff.VectorGraphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace NStuff.WindowSystem.ManualTest
{
    class VectorGraphicsLauncher
    {
        internal void Launch()
        {
            Console.WriteLine("Bezier...");
            Console.WriteLine("    'i': Image.");
            Console.WriteLine("    's': SVG tiger.");
            Console.WriteLine("    't': Text.");

            using var windowServer = new WindowServer();
            using var renderingContext = new RenderingContext { Settings = new RenderingSettings { Samples = 8 } };
            using var window = windowServer.CreateWindow(renderingContext);

            window.Title = "Bézier";
            window.BorderStyle = WindowBorderStyle.Sizable;
            window.Size = (900, 900);
            renderingContext.CurrentWindow = window;

            var viewportSize = window.ViewportSize;
            var windowSize = window.Size;
            var backend = new DrawingBackend(new EntryPointLoader(renderingContext));
            var openTypeCollection = new OpenTypeCollection();
            var nameId = typeof(VectorGraphicsLauncher).Namespace + ".Resources.ebrima.ttf";
            openTypeCollection.AddFontResource(nameId, () => Assembly.GetExecutingAssembly().GetManifestResourceStream(nameId)!);
            using var sharedContext = new SharedDrawingContext(backend, openTypeCollection);
            using var drawingContext = new DrawingContext(sharedContext)
            {
                ClearColor = new RgbaColor(155, 155, 155, 255)
            };
            var redrawRequired = true;

            var images = new List<ImageDrawing>();
            var paths = new List<PathDrawing>();
            var labels = new List<LabelDrawing>();

            window.TextInput += (sender, e) =>
            {
                switch (e.CodePoint)
                {
                    case 'i':
                        {
                            var image = new RasterImage();
                            var namePrefix = typeof(DrawTextureLauncher).Namespace + ".Resources.";
                            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(namePrefix + "kitten.png"))
                            {
                                image.LoadPng(stream ?? throw new InvalidOperationException());
                            }
                            var imageDrawing = new ImageDrawing()
                            {
                                Image = image
                            };
                            images.Add(imageDrawing);
                            redrawRequired = true;
                        }
                        break;
                    case 's':
                        {
                            images.Clear();
                            paths.Clear();
                            labels.Clear();

                            var sw = new System.Diagnostics.Stopwatch();
                            sw.Start();

                            var pathReader = new SvgPathReader();
                            var transform = new AffineTransform(m11: 1.76, m22: 1.76, m31: 325, m32: 255);

                            var namePrefix = typeof(VectorGraphicsLauncher).Namespace + ".Resources.";
                            XDocument document;
                            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(namePrefix + "Ghostscript_Tiger.svg"))
                            {
                                document = XDocument.Load(stream);
                            }
                            XNamespace ns = "http://www.w3.org/2000/svg";
                            var rootG = document.Element(ns + "svg").Element(ns + "g");
                            foreach (var g in rootG.Elements(ns + "g"))
                            {
                                var path = new PathDrawing
                                {
                                    Transform = transform
                                };
                                paths.Add(path);
                                foreach (var a in g.Attributes())
                                {
                                    switch (a.Name.LocalName)
                                    {
                                        case "fill":
                                            path.FillColor = ParseSvgColor(a.Value);
                                            break;
                                        case "stroke":
                                            path.StrokeColor = ParseSvgColor(a.Value);
                                            break;
                                        case "stroke-width":
                                            path.StrokeWidth = float.Parse(a.Value);
                                            break;
                                    }
                                }
                                pathReader.PathData = g.Element(ns + "path").Attribute("d").Value;
                                var x = 0f;
                                var y = 0f;
                                var initialX = 0f;
                                var initialY = 0f;
                                (double x, double y) c2 = (0, 0);
                                bool hasPreviousC2 = false;
                                while (pathReader.Read())
                                {
                                    switch (pathReader.PathCommand)
                                    {
                                        case PathCommandType.MoveTo:
                                            x = pathReader.X;
                                            y = pathReader.Y;
                                            path.Move((x, y));
                                            initialX = x;
                                            initialY = y;
                                            hasPreviousC2 = false;
                                            break;
                                        case PathCommandType.MoveToRelative:
                                            x += pathReader.X;
                                            y += pathReader.Y;
                                            path.Move((x, y));
                                            initialX = x;
                                            initialY = y;
                                            hasPreviousC2 = false;
                                            break;
                                        case PathCommandType.LineToRelative:
                                            x += pathReader.X;
                                            y += pathReader.Y;
                                            path.AddLine((x, y));
                                            hasPreviousC2 = false;
                                            break;
                                        case PathCommandType.CurveToRelative:
                                            {
                                                var c1 = ((double)x + pathReader.X1, (double)y + pathReader.Y1);
                                                c2 = (x + pathReader.X2, y + pathReader.Y2);
                                                x += pathReader.X;
                                                y += pathReader.Y;
                                                path.AddCubicBezier(c1, c2, (x, y));
                                                hasPreviousC2 = true;
                                            }
                                            break;
                                        case PathCommandType.SmoothCurveToRelative:
                                            {
                                                (double x, double y) c1;
                                                if (hasPreviousC2)
                                                {
                                                    c1 = (2 * x - c2.x, 2 * y - c2.y);
                                                }
                                                else
                                                {
                                                    c1 = (x, y);
                                                }
                                                c2 = (x + pathReader.X2, y + pathReader.Y2);
                                                x += pathReader.X;
                                                y += pathReader.Y;
                                                path.AddCubicBezier(c1, c2, (x, y));
                                                hasPreviousC2 = true;
                                            }
                                            break;
                                        case PathCommandType.VerticalLineToRelative:
                                            y += pathReader.Y;
                                            path.AddLine((x, y));
                                            hasPreviousC2 = false;
                                            break;
                                        case PathCommandType.Closepath:
                                            path.AddLine((initialX, initialY));
                                            hasPreviousC2 = false;
                                            break;
                                        default:
                                            Console.WriteLine("Unhandled command: " + pathReader.PathCommand);
                                            break;
                                    }
                                }
                            }
                            sw.Stop();
                            Console.WriteLine($"SVG file decoded in {sw.ElapsedMilliseconds}ms ({paths.Count} paths).");
                            redrawRequired = true;
                        }
                        break;
                    case 't':
                        {
                            images.Clear();
                            paths.Clear();
                            labels.Clear();

                            var label = new LabelDrawing
                            {
                                Transform = new AffineTransform(m11: 4, m22: 4, m31: 20, m32: 50),
                                FontFamily = "Ebrima",
                                FontSubfamily = FontSubfamily.Normal,
                                FontPoints = 8
                            };
                            label.AppendString("The quick brown fox jumps over the lazy dog.");
                            labels.Add(label);

                            for (int i = 0; i < 15; i++)
                            {
                                label = new LabelDrawing
                                {
                                    Transform = new AffineTransform(m11: 1, m22: 1, m31: 20, m32: 100 + i * 30),
                                    FontFamily = "Ebrima",
                                    FontSubfamily = FontSubfamily.Normal,
                                    FontPoints = 32 - i * 2
                                };
                                label.AppendString("The quick brown fox jumps over the lazy dog.");
                                labels.Add(label);
                            }
                            redrawRequired = true;
                        }
                        break;
                }
            };

            void draw()
            {
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                renderingContext.CurrentWindow = window;
                drawingContext.SharedContext.PixelScaling = viewportSize.height / windowSize.height;

                drawingContext.StartDrawing();

                foreach (var image in images)
                {
                    image.Draw(drawingContext);
                }
                foreach (var path in paths)
                {
                    path.Draw(drawingContext);
                }
                foreach (var label in labels)
                {
                    label.Draw(drawingContext, 0, 0);
                }

                drawingContext.FinishDrawing();
                renderingContext.SwapBuffers(window);

                sw.Stop();
                Console.WriteLine($"Drawn in {sw.ElapsedMilliseconds}ms.");
            }

            void resize()
            {
                viewportSize = window.ViewportSize;
                windowSize = window.Size;
                backend.WindowSize = windowSize;
            }

            window.Resize += (sender, e) =>
            {
                resize();
                draw();
            };

            var runLoop = MainRunLoop.Create(windowServer);
            window.Closed += (sender, e) =>
            {
                drawingContext.Dispose();
                backend.Dispose();
                runLoop.Interrupt();
            };

            resize();
            runLoop.RecurringActions.Add((delay) =>
            {
                if (redrawRequired)
                {
                    draw();
                    redrawRequired = false;
                }
            });

            window.Visible = true;
            runLoop.Run();
            Console.WriteLine("Bezier done.");
        }

        private static RgbaColor ParseSvgColor(string value)
        {
            byte red;
            byte green;
            byte blue;
            if (value.Length == 4)
            {
                red = (byte)(GetValue(value[1]) * 17);
                green = (byte)(GetValue(value[2]) * 17);
                blue = (byte)(GetValue(value[3]) * 17);
            }
            else
            {
                red = (byte)((GetValue(value[1]) << 4) | GetValue(value[2]));
                green = (byte)((GetValue(value[3]) << 4) | GetValue(value[4]));
                blue = (byte)((GetValue(value[5]) << 4) | GetValue(value[6]));
            }
            return new RgbaColor(red, green, blue, 255);
        }

        private static int GetValue(char hexa)
        {
            switch (hexa)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'a':
                case 'A': return 10;
                case 'b':
                case 'B': return 11;
                case 'c':
                case 'C': return 12;
                case 'd':
                case 'D': return 13;
                case 'e':
                case 'E': return 14;
                case 'f':
                case 'F': return 15;
                default:
                    throw new Exception();
            }
        }
    }
}
