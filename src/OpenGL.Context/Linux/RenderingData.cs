using System;

namespace NStuff.OpenGL.Context.Linux
{
    /// <summary>
    /// Holds the native handle used to manage the rendering context.
    /// </summary>
    public class RenderingData
    {
        /// <summary>
        /// The native X11 handle on the OpenGL context.
        /// </summary>
        /// <value>A handle returned by <c>glXCreateContext()</c>.</value>
        public IntPtr Context { get; set; }
    }
}
