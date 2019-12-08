using System;

namespace NStuff.OpenGL.Context.Windows
{
    /// <summary>
    /// Holds the native handles used to manage the rendering context.
    /// </summary>
    public class RenderingData
    {
        /// <summary>
        /// Gets the native handle of the OpenGL context.
        /// </summary>
        /// <value>A native handle.</value>
        public IntPtr Handle { get; internal set; }

        /// <summary>
        /// Gets the native handle of the device context.
        /// </summary>
        /// <value>A native handle.</value>
        public IntPtr DeviceContext { get; internal set; }
    }
}
