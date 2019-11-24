using System;

namespace NStuff.WindowSystem.Windows
{
    internal class CursorData
    {
        internal IntPtr Handle { get; }

        internal CursorData(IntPtr handle) => Handle = handle;
    }
}
