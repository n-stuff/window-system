using System.Collections.Generic;

namespace NStuff.WindowSystem
{
    /// <summary>
    /// Holds the arguments of an event involving a files drag and drop.
    /// </summary>
    public struct FileDropEventArgs
    {
        /// <summary>
        /// The position of the mouse cursor.
        /// </summary>
        public (double x, double y) Position { get; }

        /// <summary>
        /// The paths associated with this event.
        /// </summary>
        public ICollection<string> Paths { get; }

        /// <summary>
        /// Initializes a new instance of the <c>FileDropEvenArgs</c> struct using supplied <paramref name="position"/> and <paramref name="paths"/>.
        /// </summary>
        /// <param name="position">The position of the cursor when the file was dropped.</param>
        /// <param name="paths">The paths to the files that were dropped.</param>
        public FileDropEventArgs((double x, double y) position, string[] paths)
        {
            Position = position;
            Paths = paths;
        }
    }
}
