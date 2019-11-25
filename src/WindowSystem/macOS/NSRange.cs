using System.Runtime.InteropServices;

namespace NStuff.WindowSystem.macOS
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NSRange
    {
        private readonly ulong location;
        private readonly ulong length;

        internal ulong Location => location;

        internal ulong Length => length;

        internal NSRange(ulong location, ulong length)
        {
            this.location = location;
            this.length = length;
        }
    }
}
