using NStuff.GraphicsBackend;
using NStuff.OpenGL.Backend;
using NStuff.OpenGL.Context;
using NStuff.VectorGraphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace NStuff.WindowSystem.ManualTest
{
    class BezierLauncher
    {
        internal void Launch()
        {
            Console.WriteLine("Bezier...");

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
            var drawingContext = new DrawingContext(backend)
            {
                ClearColor = new RgbaColor(155, 155, 155, 255)
            };
            var redrawRequired = true;

            void draw()
            {
                renderingContext.CurrentWindow = window;
                backend.PixelScaling = viewportSize.height / windowSize.height;

                drawingContext.StartDrawing();

                drawingContext.FinishDrawing();
                renderingContext.SwapBuffers(window);
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
    }
}
