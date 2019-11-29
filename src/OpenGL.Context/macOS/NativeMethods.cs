namespace NStuff.OpenGL.Context.macOS
{
    internal static class NativeMethods
    {
        internal const string Libobjc = "/usr/lib/libobjc.dylib";

        internal const uint NSOpenGLPFADoubleBuffer = 5;
        internal const uint NSOpenGLPFAStereo = 6;
        internal const uint NSOpenGLPFAAuxBuffers = 7;
        internal const uint NSOpenGLPFAColorSize = 8;
        internal const uint NSOpenGLPFAAlphaSize = 11;
        internal const uint NSOpenGLPFADepthSize = 12;
        internal const uint NSOpenGLPFAStencilSize = 13;
        internal const uint NSOpenGLPFAAccumSize = 14;
        internal const uint NSOpenGLPFASampleBuffers = 55;
        internal const uint NSOpenGLPFASamples = 56;
        internal const uint NSOpenGLPFAClosestPolicy = 74;
        internal const uint NSOpenGLPFAOpenGLProfile = 99;

        internal const uint NSOpenGLProfileVersion4_1Core = 0x4100;

        internal const ulong NSOpenGLCPSwapInterval = 222;
    }
}
