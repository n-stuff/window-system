using NStuff.Runtime.InteropServices.ObjectiveC;

namespace NStuff.WindowSystem.macOS
{
    internal class CursorData
    {
        internal Id Id { get; }

        internal CursorData(Id id) => Id = id;
    }
}
