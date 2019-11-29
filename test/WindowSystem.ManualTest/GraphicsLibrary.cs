using NStuff.OpenGL.Context;

namespace NStuff.WindowSystem.ManualTest
{
    internal partial class GraphicsLibrary
    {
        private RenderingContext RenderingContext { get; }

        internal GraphicsLibrary(RenderingContext renderingContext)
        {
            RenderingContext = renderingContext;
            Initialize();
        }

        internal TDelegate GetOpenGLEntryPoint<TDelegate>(string command) where TDelegate : class
        {
            return RenderingContext.GetOpenGLEntryPoint<TDelegate>(command);
        }
    }
}
