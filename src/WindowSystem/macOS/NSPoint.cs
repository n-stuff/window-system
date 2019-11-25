using System.Runtime.InteropServices;

namespace NStuff.WindowSystem.macOS
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NSPoint
    {
        public static implicit operator (double x, double y)(NSPoint size) => (size.x, size.y);

        private double x;
        private double y;

        public double X { get => x; set => x = value; }

        public double Y { get => y; set => y = value; }

        public NSPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
