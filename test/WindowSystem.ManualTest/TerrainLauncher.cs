using NStuff.OpenGL.Context;
using NStuff.RasterGraphics;
using NStuff.WindowSystem.ManualTest.Terrain;
using System;
using System.Diagnostics;
using System.Reflection;

namespace NStuff.WindowSystem.ManualTest
{
    internal class TerrainLauncher
    {
        internal void Launch()
        {
            Console.WriteLine("Terrain...");
            Console.WriteLine("  Press Esc to exit.");
            Console.WriteLine("  Press W, Q, S, D, Space to move.");

            using var windowServer = new WindowServer();
            using var renderingContext = new RenderingContext { Settings = new RenderingSettings { Samples = 4 } };

            using var window = windowServer.CreateWindow(renderingContext);
            window.Title = "Terrain";
            window.BorderStyle = WindowBorderStyle.Sizable;

            const double WindowWidth = 1024;
            const double WindowHeight = 768;
            window.Size = (WindowWidth, WindowHeight);

            renderingContext.CurrentWindow = window;
            var gl = new GraphicsLibrary(renderingContext);
            gl.ClearColor(0, 0, 0, 1);
            using (var scene = new Scene(gl))
            {
                var imageLength = 16 * 16 * 3;
                var buffer = new byte[imageLength * 3];
                var image = new RasterImage();
                var namePrefix = typeof(TerrainLauncher).Namespace + ".Resources.";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(namePrefix + "grass.png"))
                {
                    image.LoadPng(stream ?? throw new InvalidOperationException());
                }
                Array.Copy(image.Data, 0, buffer, 0, imageLength);
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(namePrefix + "grass_side.png"))
                {
                    image.LoadPng(stream ?? throw new InvalidOperationException());
                }
                Array.Copy(image.Data, 0, buffer, imageLength, imageLength);
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(namePrefix + "dirt.png"))
                {
                    image.LoadPng(stream ?? throw new InvalidOperationException());
                }
                Array.Copy(image.Data, 0, buffer, imageLength * 2, imageLength);
                scene.AddTextureMap(buffer, 3);

                var viewportSize = window.ViewportSize;
                scene.SetViewport(0, 0, (int)viewportSize.width, (int)viewportSize.height);

                (double x, double y) previousPoint = (-1d, -1d);
                window.MouseMove += (sender, e) =>
                {
                    if (previousPoint.x != -1d || previousPoint.y != -1d)
                    {
                        scene.Rotate((float)(e.Position.x - previousPoint.x), (float)(e.Position.y - previousPoint.y));
                    }
                    previousPoint = e.Position;
                };


                void draw()
                {
                    scene.Render();
                    renderingContext.SwapBuffers(window);
                }

                window.Resize += (sender, e) =>
                {
                    var (width, height) = window.ViewportSize;
                    scene.SetViewport(0, 0, (int)width, (int)height);
                    draw();
                };

                window.FramebufferResize += (sender, e) =>
                {
                    var (width, height) = window.ViewportSize;
                    scene.SetViewport(0, 0, (int)width, (int)height);
                };

                window.Closed += (sender, e) =>
                {
                    scene.Dispose();
                };

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var lastTime = stopWatch.ElapsedMilliseconds / 1e3f;
                var deltaTime = 0.0f;

                window.FreeLookMouse = true;
                //renderingContext.SyncWithVerticalBlank(window, false);
                Console.WriteLine("Press 'Esc' to quit.");
                window.Visible = true;
                while (windowServer.Windows.Count > 0)
                {
                    var currentTime = stopWatch.ElapsedMilliseconds / 1e3f;
                    deltaTime = currentTime - lastTime;
                    lastTime = currentTime;

                    if (window.IsKeyPressed(Keycode.W))
                    {
                        scene.MoveForward(deltaTime);
                    }
                    if (window.IsKeyPressed(Keycode.S))
                    {
                        scene.MoveBackward(deltaTime);
                    }
                    if (window.IsKeyPressed(Keycode.A))
                    {
                        scene.MoveLeft(deltaTime);
                    }
                    if (window.IsKeyPressed(Keycode.D))
                    {
                        scene.MoveRight(deltaTime);
                    }
                    if (window.IsKeyPressed(Keycode.Space))
                    {
                        scene.MoveUp(deltaTime);
                    }
                    if (window.IsKeyPressed(Keycode.Escape))
                    {
                        break;
                    }

                    draw();

                    windowServer.ProcessEvents(0.0);
                }
                stopWatch.Stop();
            }
            Console.WriteLine("Terrain done.");
        }
    }
}
