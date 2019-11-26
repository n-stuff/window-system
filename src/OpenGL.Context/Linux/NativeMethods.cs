using System;
using System.Runtime.InteropServices;

using BOOL = System.Int32;
using DRAWABLE = System.UInt64;
using GLXCONTEXT = System.IntPtr;
using PDISPLAY = System.IntPtr;
using PVISUAL = System.IntPtr;
using VISUALID = System.UInt64;

namespace NStuff.OpenGL.Context.Linux
{
    internal static class NativeMethods
    {
        private const string OpenGLLibrary = "libGL.so.1";
        private const string X11Library = "libX11.so.6";

        internal const int GLX_RGBA = 4;
        internal const int GLX_DOUBLEBUFFER = 5;
        internal const int GLX_STEREO = 6;
        internal const int GLX_AUX_BUFFERS = 7;
        internal const int GLX_RED_SIZE = 8;
        internal const int GLX_GREEN_SIZE = 9;
        internal const int GLX_BLUE_SIZE = 10;
        internal const int GLX_ALPHA_SIZE = 11;
        internal const int GLX_DEPTH_SIZE = 12;
        internal const int GLX_STENCIL_SIZE = 13;
        internal const int GLX_ACCUM_RED_SIZE = 14;
        internal const int GLX_ACCUM_GREEN_SIZE = 15;
        internal const int GLX_ACCUM_BLUE_SIZE = 16;
        internal const int GLX_ACCUM_ALPHA_SIZE = 17;
        internal const int GLX_SAMPLES_ARB = 100001;

        internal const int None = 0;

        [StructLayout(LayoutKind.Sequential)]
        internal struct XVisualInfo
        {
            internal PVISUAL visual;
            internal VISUALID visualid;
            internal int screen;
            internal int depth;
            internal int @class;
            internal ulong red_mask;
            internal ulong green_mask;
            internal ulong blue_mask;
            internal int colormap_size;
            internal int bits_per_rgb;
        }

        internal delegate int PFNGLXSWAPINTERVALEXTPROC(PDISPLAY dpy, DRAWABLE drawable, int interval);
        internal delegate int PFNGLXSWAPINTERVALMESAPROC(uint interval);

        [DllImport(X11Library)]
        internal static extern int XDefaultScreen(PDISPLAY display);

#pragma warning disable IDE1006 // Naming Styles
        [DllImport(OpenGLLibrary)]
        internal static extern unsafe XVisualInfo* glXChooseVisual(PDISPLAY dpy, int screen, int* attribList);

        [DllImport(OpenGLLibrary)]
        internal static extern unsafe GLXCONTEXT glXCreateContext(PDISPLAY dpy, XVisualInfo* vis, GLXCONTEXT shareList, BOOL direct);

        [DllImport(OpenGLLibrary)]
        internal static extern void glXDestroyContext(PDISPLAY dpy, GLXCONTEXT ctx);

        [DllImport(OpenGLLibrary)]
        internal static extern IntPtr glXGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string procname);

        [DllImport(OpenGLLibrary)]
        internal static extern BOOL glXMakeCurrent(PDISPLAY dpy, DRAWABLE drawable, GLXCONTEXT ctx);

        [DllImport(OpenGLLibrary)]
        internal static extern IntPtr glXQueryExtensionsString(PDISPLAY dpy, int screen);

        [DllImport(OpenGLLibrary)]
        internal static extern void glXSwapBuffers(PDISPLAY dpy, DRAWABLE drawable);
    }
}
