using NStuff.WindowSystem;
using System;

namespace NStuff.ManualTest
{
    class Program
    {
        static void Main(string[] arguments)
        {
            using var windowServer = new WindowServer();
            using var window1 = windowServer.CreateWindow(new RenderingContext());

            window1.Move += (w, e) => Console.WriteLine($"W1 Move       {window1.Location}");
            window1.MouseEnter += (w, e) => Console.WriteLine("W1 MouseEnter");
            window1.MouseLeave += (w, e) => Console.WriteLine("W1 MouseLeave");
            window1.MouseMove += (w, e) => Console.WriteLine($"W1 MouseMove  {e.Position}");

            window1.Visible = true;
            while (windowServer.Windows.Count > 0)
            {
                windowServer.ProcessEvents(0.02);
            }
        }
    }

    class RenderingContext : IRenderingContext
    {
        void IRenderingContext.AttachRenderingData(WindowServer server, Window window)
        {
        }

        void IRenderingContext.DetachRenderingData(Window window)
        {
        }

        void IRenderingContext.SetupRenderingData(WindowServer server, Window window)
        {
        }

        void IRenderingContext.UpdateRenderingData(Window window)
        {
        }
    }
}
