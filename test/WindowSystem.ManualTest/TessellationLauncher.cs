using NStuff.GraphicsBackend;
using NStuff.OpenGL.Backend;
using NStuff.OpenGL.Context;
using NStuff.Tessellation;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NStuff.WindowSystem.ManualTest
{
    class TessellationLauncher
    {
        internal unsafe void Launch()
        {
            Console.WriteLine("Tessellation...");
            Console.WriteLine("    Left click to add a point.");
            Console.WriteLine("    'c' to close the current polyline and start a new one.");
            Console.WriteLine("    'r' reset the polygon.");
            Console.WriteLine("    't' to tessellate the polygon.");

            using var windowServer = new WindowServer();
            using var renderingContext = new RenderingContext { Settings = new RenderingSettings { Samples = 8 } };

            using var window = windowServer.CreateWindow(renderingContext);
            window.Title = "Tessellation";
            window.BorderStyle = WindowBorderStyle.Sizable;
            window.Size = (800, 800);
            renderingContext.CurrentWindow = window;

            var viewportSize = window.ViewportSize;
            var windowSize = window.Size;

            var backend = new DrawingBackend(new EntryPointLoader(renderingContext))
            {
                PixelScaling = viewportSize.height / windowSize.height
            };

            var vertices = new List<(double x, double y)>();
            List<List<(double x, double y)>>? polygon = new List<List<(double x, double y)>>();
            var contour = default(List<(double x, double y)>);
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

            var buffer = new PointCoordinates[6];
            var vertexBuffer = backend.CreateVertexBuffer(VertexType.PointCoordinates, 6);
            var colorBufferHandle = backend.CreateUniformBuffer(UniformType.RgbaColor, colors.Length);

            var transformBufferHandle = backend.CreateUniformBuffer(UniformType.AffineTransform, 1);

            var commandBufferInit = backend.CreateCommandBuffer();
            backend.BeginRecordCommands(commandBufferInit);
            backend.AddClearCommand(commandBufferInit, new RgbaColor(55, 55, 55, 255));
            backend.AddUseShaderCommand(commandBufferInit, ShaderKind.PlainColor);
            backend.AddBindUniformCommand(commandBufferInit, Uniform.Color, colorBufferHandle, 0);
            backend.AddBindUniformCommand(commandBufferInit, Uniform.Transform, transformBufferHandle, 0);
            backend.AddBindVertexBufferCommand(commandBufferInit, vertexBuffer);
            backend.EndRecordCommands(commandBufferInit);

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
            backend.AddDrawCommand(commandBufferDrawTriangle, DrawingPrimitive.Triangles, 0, 3);
            backend.EndRecordCommands(commandBufferDrawTriangle);

            var commandBufferDrawTriangles = backend.CreateCommandBuffer();
            backend.BeginRecordCommands(commandBufferDrawTriangles);
            backend.AddDrawCommand(commandBufferDrawTriangles, DrawingPrimitive.Triangles, 0, 6);
            backend.EndRecordCommands(commandBufferDrawTriangles);

            var drawLineStripArgsBufferHandle = backend.CreateVertexRangeBuffer(1);
            var drawLineStripCommandBuffer = backend.CreateCommandBuffer();
            backend.BeginRecordCommands(drawLineStripCommandBuffer);
            backend.AddDrawIndirectCommand(drawLineStripCommandBuffer, DrawingPrimitive.LineStrip, drawLineStripArgsBufferHandle, 0);
            backend.EndRecordCommands(drawLineStripCommandBuffer);


            var vertexRanges = new VertexRange[1];
            var commandBufferHandles = new CommandBufferHandle[2];
            var transforms = new AffineTransform[1];
            transforms[0] = new AffineTransform(m11: 1, m22: 1);

            var drawLock = new object();
            var interruptRendering = false;
            var requireRendering = true;
            void draw()
            {
                while (!interruptRendering)
                {
                    if (requireRendering)
                    {
                        requireRendering = false;
                        renderingContext.CurrentWindow = window;
                        backend.PixelScaling = viewportSize.height / windowSize.height;
                        backend.WindowSize = windowSize;
                        backend.UpdateUniformBuffer(colorBufferHandle, colors, 0, colors.Length);

                        backend.BeginRenderFrame();
                        backend.UpdateUniformBuffer(transformBufferHandle, transforms, 0, 1);
                        commandBufferHandles[0] = commandBufferInit;
                        backend.SubmitCommands(commandBufferHandles, 0, 1);

                        if (polygon != null)
                        {
                            for (int j = 0; j < polygon.Count; j++)
                            {
                                var c = polygon[j];
                                if (c.Count == 1)
                                {
                                    commandBufferHandles[0] = commandBufferDrawTriangles;

                                    var (x, y) = c[0];
                                    var x0 = x - 2;
                                    var y0 = y - 2;
                                    var x1 = x + 2;
                                    var y1 = y - 2;
                                    var x2 = x + 2;
                                    var y2 = y + 2;
                                    var x3 = x - 2;
                                    var y3 = y + 2;

                                    buffer[0] = new PointCoordinates(x0, y0);
                                    buffer[1] = new PointCoordinates(x1, y1);
                                    buffer[2] = new PointCoordinates(x2, y2);
                                    buffer[3] = new PointCoordinates(x2, y2);
                                    buffer[4] = new PointCoordinates(x3, y3);
                                    buffer[5] = new PointCoordinates(x0, y0);

                                    backend.UpdateVertexBuffer(vertexBuffer, buffer, 0, 6);
                                    backend.SubmitCommands(commandBufferHandles, 0, 1);
                                }
                                else
                                {
                                    commandBufferHandles[0] = drawLineStripCommandBuffer;
                                    vertexRanges[0] = new VertexRange(0, 6);
                                    backend.UpdateVertexRangeBuffer(drawLineStripArgsBufferHandle, vertexRanges, 0, 1);
                                    var n = 0;
                                    for (int i = 0; i < c.Count; i++)
                                    {
                                        var (x, y) = c[i];
                                        buffer[n] = new PointCoordinates(x, y);

                                        if (++n == 6)
                                        {
                                            backend.UpdateVertexBuffer(vertexBuffer, buffer, 0, 6);
                                            backend.SubmitCommands(commandBufferHandles, 0, 1);
                                            buffer[0] = new PointCoordinates(x, y);
                                            n = 1;
                                        }
                                    }
                                    if (j < polygon.Count - 1 || contour == null)
                                    {
                                        var (x, y) = c[0];
                                        buffer[n] = new PointCoordinates(x, y);
                                        ++n;
                                    }
                                    if (n > 1)
                                    {
                                        backend.UpdateVertexBuffer(vertexBuffer, buffer, 0, n);
                                        vertexRanges[0] = new VertexRange(0, n);
                                        backend.UpdateVertexRangeBuffer(drawLineStripArgsBufferHandle, vertexRanges, 0, 1);
                                        backend.SubmitCommands(commandBufferHandles, 0, 1);
                                    }
                                }
                            }
                        }
                        else
                        {
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
                        renderingContext.CurrentWindow = null;
                    }

                    lock (drawLock)
                    {
                        Monitor.Wait(drawLock, 50);
                    }
                }
            }

            window.TextInput += (sender, e) =>
            {
                switch (e.CodePoint)
                {
                    case 'c':
                        if (contour != null && contour.Count > 2)
                        {
                            contour.Add(contour[0]);
                        }
                        contour = null;
                        requireRendering = true;
                        break;

                    case 'r':
                        polygon = new List<List<(double x, double y)>>();
                        contour = null;
                        requireRendering = true;
                        break;

                    case 't':
                        if (vertices.Count == 0)
                        {
                            var tessellator = new Tessellator2D<int, int>(new TessellateHandler(vertices))
                            {
                                OutputKind = OutputKind.TrianglesOnly,
                                WindingRule = WindingRule.NonZero
                            };

                            tessellator.BeginPolygon(0);
                            foreach (var c in polygon)
                            {
                                tessellator.BeginContour();
                                foreach (var (x, y) in c)
                                {
                                    tessellator.AddVertex(x, y, 0);
                                }
                                tessellator.EndContour();
                            }
                            tessellator.EndPolygon();
                            vertices.Clear();

                            tessellator.OutputKind = OutputKind.TriangleEnumerator;

                            tessellator.BeginPolygon(0);
                            foreach (var c in polygon)
                            {
                                tessellator.BeginContour();
                                foreach (var (x, y) in c)
                                {
                                    tessellator.AddVertex(x, y, 0);
                                }
                                tessellator.EndContour();
                            }
                            var sw = new System.Diagnostics.Stopwatch();
                            var f = System.Diagnostics.Stopwatch.Frequency;
                            sw.Start();
                            tessellator.EndPolygon();

                            while (tessellator.Move())
                            {
                                var (x, y, _) = tessellator.Vertex;
                                vertices.Add((x, y));
                            }

                            sw.Stop();
                            Console.WriteLine("Tessellated " + (vertices.Count / 3) + " triangles in " +
                                (sw.ElapsedTicks * 1e3 / f).ToString("0.000") + "ms");
                            polygon = null;
                        }
                        requireRendering = true;
                        break;
                }
            };

            window.MouseDown += (sender, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && polygon != null)
                {
                    if (contour == null)
                    {
                        contour = new List<(double x, double y)>();
                        polygon.Add(contour);
                    }
                    else if (window.CursorPosition.x == contour[contour.Count - 1].x
                          && window.CursorPosition.y == contour[contour.Count - 1].y)
                    {
                        return;
                    }
                    contour.Add(window.CursorPosition);
                    requireRendering = true;
                }
            };

            void resize()
            {
                viewportSize = window.ViewportSize;
                windowSize = window.Size;
                requireRendering = true;
                lock (drawLock)
                {
                    Monitor.Pulse(drawLock);
                }
            }

            var runLoop = MainRunLoop.Create(windowServer);

            var drawThread = new Thread(draw);
            window.Closed += (sender, e) =>
            {
                interruptRendering = true;
                drawThread.Join();
                runLoop.Interrupt();
                backend.Dispose();
            };
            window.Resize += (sender, e) => resize();
            window.FramebufferResize += (sender, e) => resize();

            renderingContext.CurrentWindow = null;
            drawThread.Start();
            window.Visible = true;
            runLoop.Run();

            Console.WriteLine("Tessellation done.");
        }
    }

    class EntryPointLoader : IEntryPointLoader
    {
        private readonly RenderingContext renderingContext;

        internal EntryPointLoader(RenderingContext renderingContext) => this.renderingContext = renderingContext;

        public TDelegate LoadEntryPoint<TDelegate>(string command) where TDelegate : class =>
            renderingContext.GetOpenGLEntryPoint<TDelegate>(command);
    }

    class TessellateHandler : ITessellateHandler<int, int>
    {
        private readonly List<(double x, double y)> vertices;

        public TessellateHandler(List<(double x, double y)> vertices)
        {
            this.vertices = vertices;
        }

        public void Begin(PrimitiveKind primitiveKind, int data)
        {
        }

        public void End(int data)
        {
        }

        public void AddVertex(double x, double y, double z, int data)
        {
            vertices.Add((x, y));
        }

        public int CombineEdges(double x, double y, double z,
            (int data, double weight) origin1, (int data, double weight) destination1,
            (int data, double weight) origin2, (int data, double weight) destination2,
            int polygonData)
        {
            return 0;
        }

        public void FlagEdges(bool onPolygonBoundary)
        {
        }
    }
}
