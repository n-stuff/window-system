using NStuff.Runtime.InteropServices.ObjectiveC;

namespace NStuff.OpenGL.Context.macOS
{
    /// <summary>
    /// Holds the native objects used to manage the rendering context.
    /// </summary>
    public class RenderingData
    {
        /// <summary>
        /// The native OpenGL context.
        /// </summary>
        /// <value>An instance of the <c>NSOpenGLContext</c> class.</value>
        public Id Context { get; set; }
    }
}
