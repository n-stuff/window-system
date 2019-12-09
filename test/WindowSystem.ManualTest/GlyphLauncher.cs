using NStuff.Geometry;
using NStuff.GraphicsBackend;
using NStuff.OpenGL.Backend;
using NStuff.OpenGL.Context;
using NStuff.Tessellation;
using NStuff.Typography.Font;
using NStuff.Typography.Typesetting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace NStuff.WindowSystem.ManualTest
{
    class GlyphLauncher
    {
        internal unsafe void Launch()
        {
            Console.WriteLine("Glyph...");
            Console.WriteLine("    type any key to display the corresponding glyph.");
            Console.WriteLine("The top glyph is rasterized before being rendered as an image.");
            Console.WriteLine("The bottom glyph is tessellated and rendered as a set of triangles.");

            using var windowServer = new WindowServer();
            using var renderingContext = new RenderingContext { Settings = new RenderingSettings { Samples = 8 } };
            using var window = windowServer.CreateWindow(renderingContext);

            window.BorderStyle = WindowBorderStyle.Sizable;
            window.Size = (500, 700);
            renderingContext.CurrentWindow = window;

            var viewportSize = window.ViewportSize;
            var windowSize = window.Size;
            var vertices = new List<(double x, double y)>();
            using var backend = new DrawingBackend(new EntryPointLoader(renderingContext))
            {
                PixelScaling = viewportSize.height / windowSize.height
            };

            var colors = new RgbaColor[]
            {
                        new RgbaColor(255, 255, 255, 255),
                        new RgbaColor(110, 100, 70, 200),
                        new RgbaColor(130, 30, 50, 200),
                        new RgbaColor(150, 90, 80, 200),
                        new RgbaColor(170, 40, 40, 200),
                        new RgbaColor(190, 80, 90, 200),
                        new RgbaColor(210, 50, 30, 200),
                        new RgbaColor(230, 70, 100, 200),
            };

            var bufferTexture = new PointAndImageCoordinates[6];
            var vertexBufferTexture = backend.CreateVertexBuffer(VertexType.PointAndImageCoordinates, 6);
            var buffer = new PointCoordinates[3];
            var vertexBuffer = backend.CreateVertexBuffer(VertexType.PointCoordinates, 3);
            var colorBufferHandle = backend.CreateUniformBuffer(UniformType.RgbaColor, colors.Length);
            backend.UpdateUniformBuffer(colorBufferHandle, colors, 0, colors.Length);

            const int GlyphSize = 300;

            var transformBufferHandle = backend.CreateUniformBuffer(UniformType.AffineTransform, 1);
            var imageHandle = backend.CreateImage(GlyphSize, GlyphSize, ImageFormat.GreyscaleAlpha, ImageComponentType.UnsignedByte);

            var commandBufferInit = backend.CreateCommandBuffer();
            backend.BeginRecordCommands(commandBufferInit);
            backend.AddClearCommand(commandBufferInit, new RgbaColor(55, 55, 55, 255));
            backend.EndRecordCommands(commandBufferInit);

            var displayImageCommandBuffer = backend.CreateCommandBuffer();
            backend.BeginRecordCommands(displayImageCommandBuffer);
            backend.AddBindImageCommand(displayImageCommandBuffer, imageHandle);
            backend.AddUseShaderCommand(displayImageCommandBuffer, ShaderKind.GreyscaleImage);
            backend.AddBindUniformCommand(displayImageCommandBuffer, Uniform.Transform, transformBufferHandle, 0);
            backend.AddBindUniformCommand(displayImageCommandBuffer, Uniform.Color, colorBufferHandle, 0);
            backend.AddBindVertexBufferCommand(displayImageCommandBuffer, vertexBufferTexture);
            backend.AddDrawCommand(displayImageCommandBuffer, DrawingPrimitive.Triangles, 0, 6);
            backend.EndRecordCommands(displayImageCommandBuffer);

            var commandBufferInitDrawTriangle = backend.CreateCommandBuffer();
            backend.BeginRecordCommands(commandBufferInitDrawTriangle);
            backend.AddUseShaderCommand(commandBufferInitDrawTriangle, ShaderKind.PlainColor);
            backend.AddBindUniformCommand(commandBufferInitDrawTriangle, Uniform.Transform, transformBufferHandle, 0);
            backend.EndRecordCommands(commandBufferInitDrawTriangle);

            var bindColorCommandBuffers = new CommandBufferHandle[colors.Length];
            for (int i = 1; i < colors.Length; i++)
            {
                bindColorCommandBuffers[i - 1] = backend.CreateCommandBuffer();
                backend.BeginRecordCommands(bindColorCommandBuffers[i - 1]);
                backend.AddBindUniformCommand(bindColorCommandBuffers[i - 1], Uniform.Color, colorBufferHandle, i);
                backend.EndRecordCommands(bindColorCommandBuffers[i - 1]);
            }

            var commandBufferDrawTriangle = backend.CreateCommandBuffer();
            backend.BeginRecordCommands(commandBufferDrawTriangle);
            backend.AddBindVertexBufferCommand(commandBufferDrawTriangle, vertexBuffer);
            backend.AddDrawCommand(commandBufferDrawTriangle, DrawingPrimitive.Triangles, 0, 3);
            backend.EndRecordCommands(commandBufferDrawTriangle);

            var commandBufferHandles = new CommandBufferHandle[2];
            var transforms = new AffineTransform[1];
            transforms[0] = new AffineTransform(m11: 1, m22: 1);

            var bitmap = new byte[GlyphSize * (GlyphSize + 5)];
            bool glyphAvailable = false;

            void draw()
            {
                renderingContext.CurrentWindow = window;

                backend.BeginRenderFrame();
                backend.UpdateUniformBuffer(transformBufferHandle, transforms, 0, 1);
                commandBufferHandles[0] = commandBufferInit;
                backend.SubmitCommands(commandBufferHandles, 0, 1);

                if (glyphAvailable)
                {
                    commandBufferHandles[0] = displayImageCommandBuffer;
                    backend.UpdateImage(imageHandle, bitmap, 0, 0, GlyphSize, GlyphSize);
                    bufferTexture[0] = new PointAndImageCoordinates(0, 0, 0, 0);
                    bufferTexture[1] = new PointAndImageCoordinates(0, 300, 0, 1);
                    bufferTexture[2] = new PointAndImageCoordinates(300, 300, 1, 1);
                    bufferTexture[3] = new PointAndImageCoordinates(0, 0, 0, 0);
                    bufferTexture[4] = new PointAndImageCoordinates(300, 300, 1, 1);
                    bufferTexture[5] = new PointAndImageCoordinates(300, 0, 1, 0);
                    backend.UpdateVertexBuffer(vertexBufferTexture, bufferTexture, 0, 6);
                    backend.SubmitCommands(commandBufferHandles, 0, 1);

                    commandBufferHandles[0] = commandBufferInitDrawTriangle;
                    backend.SubmitCommands(commandBufferHandles, 0, 1);

                    commandBufferHandles[1] = commandBufferDrawTriangle;
                    for (int i = 0; i < vertices.Count - 2; i += 3)
                    {
                        commandBufferHandles[0] = bindColorCommandBuffers[((i / 3) % (colors.Length - 1))];
                        var (x, y) = vertices[i];
                        buffer[0] = new PointCoordinates(x, y);
                        (x, y) = vertices[i + 1];
                        buffer[1] = new PointCoordinates(x, y);
                        (x, y) = vertices[i + 2];
                        buffer[2] = new PointCoordinates(x, y);

                        backend.UpdateVertexBuffer(vertexBuffer, buffer, 0, 3);
                        backend.SubmitCommands(commandBufferHandles, 0, 2);
                    }

                }

                backend.EndRenderFrame();

                renderingContext.SwapBuffers(window);
            }

            OpenType openType;
            var namePrefix = typeof(GlyphLauncher).Namespace + ".Resources.";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(namePrefix + "ebrima.ttf"))
            {
                openType = OpenType.Load(stream ?? throw new InvalidOperationException())[0];
            }
            var glyphRasterizer = new GlyphRasterizer();
            glyphRasterizer.Setup(openType, GlyphSize);
            var glyphReader = new GlyphReader();
            var yOffset = 320;
            var tessellator = new Tessellator2D<int, int>(new TessellateHandler(vertices))
            {
                OutputKind = OutputKind.TrianglesOnly,
                WindingRule = WindingRule.NonZero
            };
            var bezierApproximator = new BezierApproximator
            {
                PointComputed = (x, y) => tessellator.AddVertex(x, y + yOffset, 0)
            };

            window.TextInput += (sender, e) =>
            {
                Console.WriteLine("Rendering '" + char.ConvertFromUtf32(e.CodePoint) + "'...");
                int glyphIndex = openType.GetGlyphIndex(e.CodePoint);
                if (glyphIndex == 0)
                {
                    Console.WriteLine("Font does not have a glyph for codepoint " + e.CodePoint);
                    return;
                }
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                if (!glyphRasterizer.Rasterize(glyphIndex))
                {
                    Console.WriteLine("Cannot rasterize codepoint" + e.CodePoint);
                    return;
                }
                stopWatch.Stop();
                Array.Clear(bitmap, 0, bitmap.Length);
                glyphRasterizer.DrawGlyph(bitmap, GlyphSize, 0, 0);
                glyphAvailable = true;
                Console.WriteLine("  Glyph rasterized in " + (int)(stopWatch.Elapsed.TotalMilliseconds * 1e3) + "µs");

                if (!glyphReader.Setup(openType, GlyphSize, glyphIndex))
                {
                    Console.WriteLine("Failed to initialize glyph.");
                }
                var boundingBoxLeft = (int)Math.Floor(glyphReader.XMin);
                var boundingBoxTop = (int)Math.Floor(-glyphReader.YMax);
                double xs = 0;
                double ys = 0;

                stopWatch.Reset();
                stopWatch.Start();
                vertices.Clear();
                tessellator.BeginPolygon(0);
                bool started = false;
                while (glyphReader.Move())
                {
                    double x = glyphReader.X - boundingBoxLeft;
                    double y = -glyphReader.Y - boundingBoxTop;
                    switch (glyphReader.PathCommand)
                    {
                        case PathCommand.MoveTo:
                            if (started)
                            {
                                tessellator.EndContour();
                            }
                            else
                            {
                                started = true;
                            }
                            tessellator.BeginContour();
                            tessellator.AddVertex(x, y + yOffset, 0);
                            break;

                        case PathCommand.LineTo:
                            tessellator.AddVertex(x, y + yOffset, 0);
                            break;

                        case PathCommand.QuadraticBezierTo:
                            bezierApproximator.ApproximateQuadratic(xs, ys, glyphReader.Cx - boundingBoxLeft,
                                -glyphReader.Cy - boundingBoxTop, x, y);
                            break;

                        case PathCommand.CubicBezierTo:
                            bezierApproximator.ApproximateCubic(xs, ys, glyphReader.Cx - boundingBoxLeft, -glyphReader.Cy - boundingBoxTop,
                                glyphReader.Cx1 - boundingBoxLeft, -glyphReader.Cy1 - boundingBoxTop, x, y);
                            break;
                    }
                    xs = x;
                    ys = y;
                }
                if (started)
                {
                    tessellator.EndContour();
                }
                tessellator.EndPolygon();

                stopWatch.Stop();
                Console.WriteLine("  Glyph tessellated in " + (int)(stopWatch.Elapsed.TotalMilliseconds * 1e3) + "µs ("
                    + (vertices.Count / 3) + " triangles)");
            };

            void resize()
            {
                viewportSize = window.ViewportSize;
                windowSize = window.Size;
                renderingContext.CurrentWindow = window;
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
                backend.Dispose();
                runLoop.Interrupt();
            };

            resize();
            runLoop.RecurringActions.Add((time) =>
            {
                draw();
            });
            window.Visible = true;
            runLoop.Run();

            Console.WriteLine("Glyph done.");
        }
    }
}
