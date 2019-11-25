using System.Runtime.InteropServices;

namespace NStuff.WindowSystem.macOS
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NSSize
    {
        public static implicit operator (double width, double height)(NSSize size) => (size.width, size.height);

        private double width;
        private double height;

        public double Width { get => width; set => width = value; }

        public double Height { get => height; set => height = value; }

        public NSSize(double width, double height)
        {
            this.width = width;
            this.height = height;
        }
    }
}
