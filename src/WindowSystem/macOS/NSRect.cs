using System.Runtime.InteropServices;

namespace NStuff.WindowSystem.macOS
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NSRect
    {
        private NSPoint location;
        private NSSize size;

        public NSPoint Location => location;

        public NSSize Size => size;

        public double X => location.X;

        public double Y => location.Y;

        public double Width => size.Width;

        public double Height => size.Height;

        public NSRect(NSPoint location, NSSize size)
        {
            this.location = location;
            this.size = size;
        }

        public NSRect(double x, double y, double width, double height)
        {
            location = new NSPoint(x, y);
            size = new NSSize(width, height);
        }
    }
}
