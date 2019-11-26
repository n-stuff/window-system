using NStuff.OpenGL.Context;
using NStuff.WindowSystem;
using System;

using GLbitfield = System.UInt32;
using GLfloat = System.Single;

namespace NStuff.ManualTest
{
    class Program
    {
        static void Main(string[] arguments)
        {
            using var windowServer = new WindowServer();
            using var renderingContext = new RenderingContext();
            using var window1 = windowServer.CreateWindow(renderingContext);

            window1.Move += (w, e) => Console.WriteLine($"W1 Move       {window1.Location}");
            window1.MouseEnter += (w, e) => Console.WriteLine("W1 MouseEnter");
            window1.MouseLeave += (w, e) => Console.WriteLine("W1 MouseLeave");
            window1.MouseMove += (w, e) => Console.WriteLine($"W1 MouseMove  {e.Position}");

            renderingContext.CurrentWindow = window1;
            var glClear = renderingContext.GetOpenGLEntryPoint<ClearDelegate>("glClear", true);
            var glClearColor = renderingContext.GetOpenGLEntryPoint<ClearColorDelegate>("glClearColor", true);

            window1.Visible = true;
            while (windowServer.Windows.Count > 0)
            {
                if (!window1.Disposed)
                {
                    renderingContext.CurrentWindow = window1;
                    glClearColor(0.2f, 0.5f, 0.8f, 1.0f);
                    glClear(Buffers.Color);
                    renderingContext.SwapBuffers(window1);
                }
                windowServer.ProcessEvents(0.02);
            }
        }
    }

    internal enum Buffers : GLbitfield
    {
        Color = 0x00004000,
        Depth = 0x00000100,
        Stencil = 0x00000400,
    }

    internal delegate void ClearDelegate(Buffers mask);
    internal delegate void ClearColorDelegate(GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha);
}
