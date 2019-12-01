using System;

namespace NStuff.OpenGL.Backend
{
    internal partial class GraphicsLibrary
    {
        private IEntryPointLoader? entryPointLoader;

        internal GraphicsLibrary(IEntryPointLoader entryPointLoader)
        {
            this.entryPointLoader = entryPointLoader;
            Initialize();
            this.entryPointLoader = null;
        }

        internal TDelegate GetOpenGLEntryPoint<TDelegate>(string command) where TDelegate : class =>
            (entryPointLoader ?? throw new InvalidOperationException()).LoadEntryPoint<TDelegate>(command);
    }
}
